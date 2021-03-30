using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Refecente to Finite Element Method by Rao §11.3
    /// </summary>
    public class Hexaedron : Brick
    {
        public Hexaedron(Node[] globalNodes, BrickProperty brickProperty, int id) :base(globalNodes, brickProperty, id)
        {
            _DOF.Add(LinearSolver.DOF.DX);
            _DOF.Add(LinearSolver.DOF.DY);
            _DOF.Add(LinearSolver.DOF.DZ);
            //a displacement in Local coordinate plane (Dx, Dy) can be a DX, DY, DZ in Global space!

            #region
            //Controllo che per ogni nodo I vengano visti gli altri 3 in senso antiorario.
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

        public override mnl.Matrix<double> KElementGlobalCoord => base.KElementGlobalCoord;

        public override void BuildMatrix()
        {
            //For this element there is not advantage in setting up a local coordinate system
            // -> local axis coincide with global axis -> ref. Finite Element Method - by Rao §11.2
            _dofGlobalToLocal = mnl.Matrix<double>.Build.DenseDiagonal(4*3, 1.0);

            double E = ((BrickProperty)_property).GetE();
            double ni = ((BrickProperty)_property).GetNi();
            _d = Brick.GetD(E, ni);

            mnl.Matrix<double> b = GetB();
            _kElementLocalCoord = b.Transpose() * _d * b;
        }

        /// <summary>
        /// Equations 3.42 -> 3.46 + eq. 11.8 of "Finite Element Method by Rao
        /// </summary>
        /// <param name="csi"></param>
        /// <param name="eta"></param>
        /// <param name="zeta"></param>
        /// <returns></returns>
        public override mnl.Matrix<double> GetB(double csi = 0, double eta = 0, double zeta = 0)
        {
            
            mnl.Matrix<double> b = mnl.Matrix<double>.Build.Dense(6, 12);
            
            return b;
        }

        #region Results
        public override void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            base.GetNodesResults(globalDisplacementsNodes, out localDisplacements, out gloabalPseudoDeformation, out localPseudoDeformation, out globalForces, out localForces, out globalStress, out localStress, out globalEpsilon, out localEpsilon);
        }


        public override void GetResultPositionNaturalCoordinates(double csi, double eta, double zeta, double[] globalDisplacementsNodes, out double x, out double y, out double z, out double[] localDisplacements, out mnl.Matrix<double> gloabalPseudoDeformation, out mnl.Matrix<double> localPseudoDeformation, out mnl.Matrix<double> globalForces, out mnl.Matrix<double> localForces, out mnl.Matrix<double> globalStress, out mnl.Matrix<double> localStress, out mnl.Matrix<double> globalEpsilon, out mnl.Matrix<double> localEpsilon)
        {
            base.GetResultPositionNaturalCoordinates(csi, eta, zeta, globalDisplacementsNodes, out x, out y, out z, out localDisplacements, out gloabalPseudoDeformation, out localPseudoDeformation, out globalForces, out localForces, out globalStress, out localStress, out globalEpsilon, out localEpsilon);
        }
        #endregion

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            return base.BuildFLocalCoord();
        }
    }
}
