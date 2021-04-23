using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    /// <summary>
    /// Refecente to Finite Element Method by Rao
    /// </summary>
    public class Tethraedron4 : Brick
    {
        internal Tethraedron4(Node[] globalNodes, BrickProperty brickProperty) :base(globalNodes)
        {
            _DOF.Add(LinearSolver.DOF.DX);
            _DOF.Add(LinearSolver.DOF.DY);
            _DOF.Add(LinearSolver.DOF.DZ);
            //a displacement in Local coordinate plane (Dx, Dy) can be a DX, DY, DZ in Global space!

            SetProperty(brickProperty);

            #region Controllo
            //Controllo che per ogni nodo I vengano visti gli altri 3 in senso antiorario.
            //Uso formula per trovare area del triangolo, se area è positiva -> punti in senso orario, altrimenti in senso antiorario
            for (int i = 1; i <= 4; i++)
            {
                List<Node> check = OrderNode(i, globalNodes).ToList();
                check.RemoveAt(0);

                //Mi sposto nelle coordinate locali della faccia
                Point3d[] localFaceNode = Tri3Element.LocalNodes(check.ToArray(), out CoordinateSystem sys).Select(x => x.Position).ToArray();

                if (Tri3Element.GetArea(localFaceNode) < 0)
                {
                    Console.WriteLine("Ordine non corretto dei nodi con vista dal nodo " + i);
                    check.ForEach(p => Console.WriteLine(p.Position.ToString()));
                    throw new Exception("Ordine non corretto dei nodi");
                }
            }
            #endregion
        }

        public override void BuildMatrix()
        {
            //For this element there is not advantage in setting up a local coordinate system
            // -> local axis coincide with global axis -> ref. Finite Element Method - by Rao §11.2
            _dofGlobalToLocal = mnl.Matrix<double>.Build.DenseDiagonal(4*3, 1.0);

            double E = ((BrickProperty)_property).GetE();
            double ni = ((BrickProperty)_property).GetNi();
            _d = Brick.GetD(E, ni);

            mnl.Matrix<double> b = GetB();
            double volume = GetVolume(_nodesGlobal);
            _kElementLocalCoord = volume * b.Transpose() * _d * b;
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
            double volume = GetVolume(_nodesGlobal);
            mnl.Matrix<double> b = mnl.Matrix<double>.Build.Dense(6, 12);
            b[0, 0] = GetCoefficientShapeFunction(1, "b", _nodesGlobal);
            b[0, 3] = GetCoefficientShapeFunction(2, "b", _nodesGlobal);
            b[0, 6] = GetCoefficientShapeFunction(3, "b", _nodesGlobal);
            b[0, 9] = GetCoefficientShapeFunction(4, "b", _nodesGlobal);

            b[1, 1] = GetCoefficientShapeFunction(1, "c", _nodesGlobal);
            b[1, 4] = GetCoefficientShapeFunction(2, "c", _nodesGlobal);
            b[1, 7] = GetCoefficientShapeFunction(3, "c", _nodesGlobal);
            b[1, 10] = GetCoefficientShapeFunction(4, "c", _nodesGlobal);

            b[2, 2] = GetCoefficientShapeFunction(1, "d", _nodesGlobal);
            b[2, 5] = GetCoefficientShapeFunction(2, "d", _nodesGlobal);
            b[2, 8] = GetCoefficientShapeFunction(3, "d", _nodesGlobal);
            b[2, 11] = GetCoefficientShapeFunction(4, "d", _nodesGlobal);

            b[3, 0] = GetCoefficientShapeFunction(1, "c", _nodesGlobal);
            b[3, 1] = GetCoefficientShapeFunction(1, "b", _nodesGlobal);
            b[3, 3] = GetCoefficientShapeFunction(2, "c", _nodesGlobal);
            b[3, 4] = GetCoefficientShapeFunction(2, "b", _nodesGlobal);
            b[3, 6] = GetCoefficientShapeFunction(3, "c", _nodesGlobal);
            b[3, 7] = GetCoefficientShapeFunction(3, "b", _nodesGlobal);
            b[3, 9] = GetCoefficientShapeFunction(4, "c", _nodesGlobal);
            b[3, 10] = GetCoefficientShapeFunction(4, "b", _nodesGlobal);

            b[4, 1] = GetCoefficientShapeFunction(1, "d", _nodesGlobal);
            b[4, 2] = GetCoefficientShapeFunction(1, "c", _nodesGlobal);
            b[4, 4] = GetCoefficientShapeFunction(2, "d", _nodesGlobal);
            b[4, 5] = GetCoefficientShapeFunction(2, "c", _nodesGlobal);
            b[4, 7] = GetCoefficientShapeFunction(3, "d", _nodesGlobal);
            b[4, 8] = GetCoefficientShapeFunction(3, "c", _nodesGlobal);
            b[4, 10] = GetCoefficientShapeFunction(4, "d", _nodesGlobal);
            b[4, 11] = GetCoefficientShapeFunction(4, "c", _nodesGlobal);

            b[5, 0] = GetCoefficientShapeFunction(1, "d", _nodesGlobal);
            b[5, 2] = GetCoefficientShapeFunction(1, "b", _nodesGlobal);
            b[5, 3] = GetCoefficientShapeFunction(2, "d", _nodesGlobal);
            b[5, 5] = GetCoefficientShapeFunction(2, "b", _nodesGlobal);
            b[5, 6] = GetCoefficientShapeFunction(3, "d", _nodesGlobal);
            b[5, 8] = GetCoefficientShapeFunction(3, "b", _nodesGlobal);
            b[5, 9] = GetCoefficientShapeFunction(4, "d", _nodesGlobal);
            b[5, 11] = GetCoefficientShapeFunction(4, "b", _nodesGlobal);

            return b / (6.0 * volume);
        }

        #region Results
        //TODO: Da ottimizzare/scrivere
        /*public override void GetNodesResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon)
        {
            base.GetNodesResults(globalDisplacementsNodes, out localDisplacements, out gloabalPseudoDeformation, out localPseudoDeformation, out globalForces, out localForces, out globalStress, out localStress, out globalEpsilon, out localEpsilon);
        }*/
        #endregion

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            /*
             * Finite Element by Rao: Equation (11.13) shows that the body force is distributed equally between the four nodes of the element.
             */

            //ripartire secondo V/4 ed eventualmente per pressioni su facce come A/3

            return mnl.Vector<double>.Build.Dense(4*3);
        }

        /// <summary>
        /// Get volume of the tethraedron from nodes position
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        internal static double GetVolume(Node[] nodes)
        {
            mnl.Vector<double> row(Node n)
            {
                mnl.Vector<double> vector = mnl.Vector<double>.Build.Dense(4);
                vector[0] = 1.0;
                vector[1] = n.Position.X;
                vector[2] = n.Position.Y;
                vector[3] = n.Position.Z;

                return vector;
            }

            var rows = nodes.ToList().Select(node => row(node)).ToList();

            mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(0, 4);
            int i = 0;
            rows.ForEach(r => {
                m = m.InsertRow(i, r);
                i++;
            });

            return 1.0 / 6.0 * m.Determinant();
        }

        internal static Node[] OrderNode(int i, Node[] nodes)
        {
            Node[] list = new Node[4];
            switch(i)
            {
                case 1:
                    list[0] = nodes[1-1]; //I
                    list[1] = nodes[2-1]; //J
                    list[2] = nodes[3-1]; //K
                    list[3] = nodes[4-1]; //L
                    break;
                case 2:
                    list[0] = nodes[2-1];
                    list[1] = nodes[1-1];
                    list[2] = nodes[3-1];
                    list[3] = nodes[4-1];
                    break;
                case 3:
                    list[0] = nodes[3-1];
                    list[1] = nodes[1-1];
                    list[2] = nodes[2-1];
                    list[3] = nodes[4-1];
                    break;
                case 4:
                    list[0] = nodes[4-1];
                    list[1] = nodes[1-1];
                    list[2] = nodes[2-1];
                    list[3] = nodes[3-1];
                    break;
            }
            return list;
        }

        internal static double GetCoefficientShapeFunction(int index, string nameCoefficient, Node[] nodes)
        {
            Node[] nodeOrdered = OrderNode(index,nodes);
            /*nodeOrdered.ToList().ForEach(x => Console.WriteLine(x));
            Console.WriteLine();*/

            var listNodes = nodeOrdered.ToList();
            listNodes.RemoveAt(0); //remove node
            /*listNodes.ForEach(x => Console.WriteLine(x));
            Console.WriteLine();*/

            mnl.Vector<double> a(Node n)
            {
                mnl.Vector<double> vector = mnl.Vector<double>.Build.Dense(3);
                vector[0] = n.Position.X;
                vector[1] = n.Position.Y;
                vector[2] = n.Position.Z;

                return vector;
            }

            mnl.Vector<double> b(Node n)
            {
                mnl.Vector<double> vector = mnl.Vector<double>.Build.Dense(3);
                vector[0] = 1.0;
                vector[1] = n.Position.Y;
                vector[2] = n.Position.Z;

                return vector;
            }

            mnl.Vector<double> c(Node n)
            {
                mnl.Vector<double> vector = mnl.Vector<double>.Build.Dense(3);
                vector[0] = n.Position.X;
                vector[1] = 1.0;
                vector[2] = n.Position.Z;

                return vector;
            }

            mnl.Vector<double> d(Node n)
            {
                mnl.Vector<double> vector = mnl.Vector<double>.Build.Dense(3);
                vector[0] = n.Position.X;
                vector[1] = n.Position.Y;
                vector[2] = 1.0;

                return vector;
            }

            Func<Node, mnl.Vector<double>> f = (Node nd) =>
            {
                switch (nameCoefficient)
                {
                    case "a":
                        return a(nd);
                    case "b":
                        return b(nd);
                    case "c":
                        return c(nd);
                    case "d":
                        return d(nd);
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            };

            var rows = listNodes.Select(node => f(node)).ToList();

            mnl.Matrix<double> m = mnl.Matrix<double>.Build.Dense(0, 3);
            int i = 0;
            rows.ForEach(r => {
                m = m.InsertRow(i, r);
                i++;
            });
            //Console.WriteLine(nameCoefficient + " = " + m);

            double factor;
            switch (nameCoefficient)
            {
                case "a":
                    factor = 1.0;
                    break;
                case "b":
                    factor = -1.0;
                    break;
                case "c":
                    factor = -1.0;
                    break;
                case "d":
                    factor = -1.0;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (index == 2 || index == 4)
            {
                factor = -1.0 * factor;
            }

            return factor * m.Determinant();
        }
    }
}
