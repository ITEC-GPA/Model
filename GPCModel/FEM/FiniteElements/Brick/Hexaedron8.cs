using System;
using GPC.Model.Fem.Properties;
using GPC.Utilities.Fem;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.Fem.FiniteElements
{
    /// <summary>
    /// Refecente to Finite Element Method by Rao §11.3
    /// </summary>
    public class Hexaedron : Brick
    {
        internal Hexaedron(Node[] globalNodes, BrickProperty brickProperty) : base(globalNodes)
        {
            _DOF.Add(LinearSolver.DOF.DX);
            _DOF.Add(LinearSolver.DOF.DY);
            _DOF.Add(LinearSolver.DOF.DZ);
            //a displacement in Local coordinate plane (Dx, Dy) can be a DX, DY, DZ in Global space!

            SetProperty(brickProperty);

            #region
            //Controllo che per ogni nodo I vengano visti gli altri 3 in senso antiorario.
            //TODO : assicurarsi che ordine nodi sia corretto
            //Uso formula per trovare area del triangolo, se area è positiva -> punti in senso orario, altrimenti in senso antiorario
            /*for (int i = 1; i <= 4; i++)
            {
                List<Node> check = OrderNode(i, globalNodes).ToList();
                check.RemoveAt(0);

                //Mi sposto nelle coordinate locali della faccia
                Node[] localFaceNode = Tri3Element.LocalNodes(check.ToArray(), out CoordinateSystem sys);

                if (Tri3Element.GetArea(localFaceNode) < 0)
                {
                    Console.WriteLine("Ordine non corretto dei nodi con vista dal nodo " + i);
                    check.ForEach(p => Console.WriteLine(p.Position.ToString()));
                    throw new Exception("Ordine non corretto dei nodi");
                }
            }*/
            #endregion
        }

        public override void BuildMatrix()
        {
            //For this element there is not advantage in setting up a local coordinate system
            // -> local axis coincide with global axis -> ref. Finite Element Method - by Rao
            _dofGlobalToLocal = mnl.Matrix<double>.Build.DenseDiagonal(4 * 2 * 3, 1.0);

            /*double E = ((BrickProperty)_property).GetE();
            double ni = ((BrickProperty)_property).GetNi();*/
            _d = ((BrickProperty)_property).Material.Get3DSolidStress();

            Func<double, double, double, mnl.Matrix<double>> kFunc = (double csi, double eta, double zeta) =>
            {
                mnl.Matrix<double> b = GetB(csi, eta, zeta);
                return b.Transpose() * _d * b;
            };

            var jacob = FemUtilities.J3D(LinearShapeFunctionHexaedron8.DNdCsi, LinearShapeFunctionHexaedron8.DNdEta, LinearShapeFunctionHexaedron8.DNdZeta, _nodesGlobal);

            _kElementLocalCoord = OldGaussIntegration.IntegrationHexaedron(kFunc, jacob, 8);
        }

        /// <summary>
        /// Equations 3.42 -> 3.46 + eq. 11.8 of "Finite Element Method by Rao
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="zeta"></param>
        /// <returns></returns>
        public override mnl.Matrix<double> GetB(double csi, double eta, double zeta)
        {
            mnl.Matrix<double> b = mnl.Matrix<double>.Build.Dense(6, 0);

            for (int i = 1; i <= 8; i++)
            {
                b = b.Append(GetBi(i, csi, eta, zeta, _nodesGlobal));
            }
            /*Console.WriteLine("B");
            Util.WriteMatrix(b);*/
            return b;
        }

        #region Results
        //TODO: Da ottimizzare/scrivere
        /*public void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            base.GetNodesResults(globalDisplacementsNodes, out localDisplacements, out gloabalPseudoDeformation, out localPseudoDeformation, out globalForces, out localForces, out globalStress, out localStress, out globalEpsilon, out localEpsilon);
        }*/
        #endregion

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            //read attribute and convert
            mnl.Vector<double> local = mnl.Vector<double>.Build.Dense(8 * 3);
            return local;
        }
        /// <summary>
        /// Eq. 11.21 - Finite element method by Rao
        /// </summary>
        /// <param name="i"></param>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="zeta"></param>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private static mnl.Matrix<double> GetBi(int i, double csi, double eta, double zeta, Node[] nodes)
        {
            Func<double, double, double, mnl.Matrix<double>> jacob = FemUtilities.J3D(LinearShapeFunctionHexaedron8.DNdCsi, LinearShapeFunctionHexaedron8.DNdEta, LinearShapeFunctionHexaedron8.DNdZeta, nodes);

            Func<double, double, double, double> FdNdCsi = (double r, double s, double t) =>
            {
                return LinearShapeFunctionHexaedron8.DNdCsi(i, r, s, t);
            };

            Func<double, double, double, double> FdNdEta = (double r, double s, double t) =>
            {
                return LinearShapeFunctionHexaedron8.DNdEta(i, r, s, t);
            };

            Func<double, double, double, double> FdNdZeta = (double r, double s, double t) =>
            {
                return LinearShapeFunctionHexaedron8.DNdZeta(i, r, s, t);
            };

            mnl.Vector<double> dNdLocal = FemUtilities.GetdNdLocalFromdNdNatural3D(csi, eta, zeta, FdNdCsi, FdNdEta, FdNdZeta, jacob);
            double dNdX = dNdLocal[0];
            double dNdY = dNdLocal[1];
            double dNdZ = dNdLocal[2];

            mnl.Matrix<double> bi = mnl.Matrix<double>.Build.Dense(6, 3);
            bi[0, 0] = dNdX;

            bi[1, 1] = dNdY;

            bi[2, 2] = dNdZ;

            bi[3, 0] = dNdY;
            bi[3, 1] = dNdX;

            bi[4, 1] = dNdZ;
            bi[4, 2] = dNdY;

            bi[5, 0] = dNdZ;
            bi[5, 2] = dNdX;
            /*Console.WriteLine("b"+i);
            Util.WriteMatrix(bi,"F3");*/
            return bi;
        }
    }
}
