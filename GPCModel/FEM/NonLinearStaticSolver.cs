using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Costrains;
using GPC.Model.FEM.FiniteElements;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM
{
    public class NonLinearStaticSolver : Solver
    {
        #region variables

        #endregion

        #region Properties

        #endregion

        public NonLinearStaticSolver(FiniteElement[] inputElements) : this(inputElements, new MultiPointsCostrain[0])
        {

        }

        public NonLinearStaticSolver(FiniteElement[] inputElements, MultiPointsCostrain[] costrains)
        {
            int nrElements = inputElements.Length;

            #region Assegno/Calcolo passo k-1
            Console.WriteLine("Passo -1 ########################################################");
            LinearSolver femkm1 = new LinearSolver(inputElements, costrains);
            //mnl.Matrix<double> Kkm1 = femkm1.KGlobalRestrains;
            mnl.Vector<double> ukm1 = femkm1.NodeGlobalDisplacements;
            //mnl.Vector<double> fkm1 = femkm1.FRestrains;
            #endregion

            #region passo k
            Console.WriteLine("Passo -1 ########################################################");
            //creo copia e muovo nodi nella nuova configurazione per ogni elemento
            FiniteElement[] inputElementsk = new FiniteElement[nrElements];
            for (int i = 0; i < inputElements.Length; i++)
            {
                inputElementsk[i] = inputElements[i].Duplicate();
                for (int j = 0; j < inputElementsk[i].Nodes.Count(); j++)
                {
                    Node node = inputElementsk[i].Nodes[j];
                    var originalPosNode = inputElementsk[i].Nodes[j].Position;
                    double DX = femkm1.GetNodeDisplacementGlobalCoordinates(node, DOF.DX);
                    double DY = femkm1.GetNodeDisplacementGlobalCoordinates(node, DOF.DY);
                    double DZ = femkm1.GetNodeDisplacementGlobalCoordinates(node, DOF.DZ);

                    inputElementsk[i].Nodes[j].Position.Move(originalPosNode.X + DX, originalPosNode.Y + DY, originalPosNode.Z + DZ);
                }
            }

            LinearSolver femk = new LinearSolver(inputElementsk, costrains);
            mnl.Matrix<double> Kk = femk.KGlobalRestrains;
            mnl.Vector<double> uk = femk.NodeGlobalDisplacements;
            mnl.Vector<double> fk = femk.FRestrains;
            #endregion

            double normDispl = normDispl = (uk - ukm1) * (uk - ukm1) / (uk * uk);
            Console.WriteLine("Norm displ. " + normDispl);

            #region passo k + 1
            int iter = 0;
            while (normDispl > 1e-3 && iter < 100)
            {
                Console.WriteLine("Passo " + iter++ + " ########################################################");
                //mnl.Vector<double> ukp1 = GetNewU(uk, ukm1, Kk, Kkm1, fk, fkm1);

                //creo copia e muovo nodi nella nuova configurazione per ogni elemento
                FiniteElement[] inputElementskp1 = new FiniteElement[nrElements];
                for (int i = 0; i < inputElementskp1.Length; i++)
                {
                    inputElementskp1[i] = inputElementsk[i].Duplicate();
                    for (int j = 0; j < inputElementskp1[i].Nodes.Count(); j++)
                    {
                        Node node = inputElementskp1[i].Nodes[j];
                        var originalPosNode = inputElementskp1[i].Nodes[j].Position;

                        /*int posDX = femk.GetPositionInKGlobal(node, DOF.DX);
                        int posDY = femk.GetPositionInKGlobal(node, DOF.DX);
                        int posDZ = femk.GetPositionInKGlobal(node, DOF.DX);
                        double DX = ukp1[posDX];
                        double DY = ukp1[posDY];
                        double DZ = ukp1[posDZ];*/

                        double DX = femk.GetNodeDisplacementGlobalCoordinates(node, DOF.DX);
                        double DY = femk.GetNodeDisplacementGlobalCoordinates(node, DOF.DY);
                        double DZ = femk.GetNodeDisplacementGlobalCoordinates(node, DOF.DZ);

                        inputElementskp1[i].Nodes[j].Position.Move(originalPosNode.X + DX, originalPosNode.Y + DY, originalPosNode.Z + DZ);
                    }
                }

                LinearSolver femkp1 = new LinearSolver(inputElementskp1, costrains);
                femk = femkp1;
                inputElementsk = inputElementskp1;

                //Aggorno variabili
                //Kkm1 = Kk;
                ukm1 = uk;
                //fkm1 = fk;

                //Kk = femkp1.KGlobalRestrains;
                uk = femkp1.NodeGlobalDisplacements;
                //fk = femkp1.FRestrains;

                normDispl = (uk - ukm1) * (uk - ukm1) / (uk * uk);
                Console.WriteLine("Norm displ. " + normDispl);
            }
            #endregion
        }

        #region PublicFuction

        #endregion

        #region PrivateFunction
        //funzione da minimizzare
        internal mnl.Vector<double> GetResidual(mnl.Matrix<double> Kp1, mnl.Vector<double> uk, mnl.Vector<double> fkp1)
        {
            return Kp1 * uk - fkp1;
        }

        //Regula Falsi
        internal mnl.Vector<double> GetNewU(mnl.Vector<double> uk, mnl.Vector<double> ukm1, mnl.Matrix<double> Kkp1, mnl.Matrix<double> Kk, mnl.Matrix<double> Kkm1, mnl.Vector<double> fkp1, mnl.Vector<double> fk, mnl.Vector<double> fkm1)
        {
            mnl.Vector<double> rkm1 = GetResidual(Kk, ukm1, fk);
            mnl.Vector<double> rk = GetResidual(Kkp1, uk, fkp1);
            return uk - rk * (uk - ukm1) / (rk - rkm1);
        }
        #endregion       
    }
}
