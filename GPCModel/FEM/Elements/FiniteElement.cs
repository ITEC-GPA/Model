using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace FEM.Elements
{
    /// <summary>
    /// Each finite element should derive from this
    /// </summary>
    public abstract class FiniteElement
    {
        protected int _DofActivePerNode; //example: for plane stress: UX, UY, UZ -> 3

        protected int[] _positionGlobalDisplElementInGlobalDisplSystemVectorResult; //position of interesting displacements of gdl of element in global displacement vector in all system
        protected double[] _nodeDisplacementGlobalCoordinates;
        protected double[] _nodeDisplacementLocalCoordinates;

        /// <summary>
        /// Node with in gloabal coordinate system
        /// </summary>
        public Node[] GlobalNodesElement { get; set; }

        /// <summary>
        /// Nodes in local coordinates
        /// </summary>
        public Node[] LocalNodesElement { get; set; }

        /// <summary>
        /// used for KeGlobal = DofGlobalToLocal^T [KeLocal] [DofGlobaltoLocal] or for UlocalCoord = DofGlobalToLocal UglobalCoord; NOTE: DofLocalToGlobal = DofGlobalToLocal^TRASPOSTE
        /// </summary>
        public Matrix<double> DofGlobalToLocal { get; set; }

        public Polynome[] ShapeFunctions { get; set; }

        /// <summary>
        /// B : derivative of ShapeFunctions, need for epsilon = [B] * q with q = node displacements vector
        /// </summary>
        public Matrix<double> B { get; set; }
        /// <summary>
        /// sigma = [D] * epsilon
        /// </summary>
        public Matrix<double> D { get; set; }

        /// <summary>
        /// ke = int [B]^T [D] [B] dV (stiffness matrix in local coordinates)
        /// </summary>
        public Matrix<double> KElementLocalCoord { get; set; }

        /// <summary>
        /// Stiffness Matrix of element in global coordinates, KeGlobal = LocalToGlobal^T [KeLocal] [LocalToGlobal])
        /// </summary>
        public Matrix<double> KElementGlobalCoord { get; set; }

        /// <summary>
        /// Bind uniquely KeGlobal (stiffness matrix of element in Global coords) to KGlobal (stiffness matrix of entire system). example: PositionToGlobal<0,0> goes to 5,5 of global system matrix. Or KeGlobal[0,0] -> goes to KGlobalsystemMatix[5,5] 
        /// </summary>
        public Dictionary<Position,Position> PositionToGlobalSystemK { get; set; }

        /// <summary>
        /// position of interesting displacements of gdl of element in global displacement vector in all system
        /// </summary>
        //public int[] PositionGlobalDisplElementInGlobalDisplSystemVectorResult => _positionGlobalDisplElementInGlobalDisplSystemVectorResult;        

        /// <summary>
        /// Used as pointer in matrices
        /// </summary>
        public struct Position
        {
            public int row;
            public int col;

            public Position(int r, int c)
            {
                row = r;
                col = c;
            }

            public override string ToString()
            {
                return "row = " + row + " column = " + col;
            }
        }

        

        /// <summary>
        /// Constructor 
        /// </summary>
        /// <param name="nodes">Set the nodes of element</param>
        public FiniteElement(IEnumerable<Node> nodes)
        {
            GlobalNodesElement = nodes.ToArray();
        }

        /// <summary>
        /// Build Stiffness Matrix etc
        /// </summary>
        public abstract void BuildMatrix();

        /// <summary>
        /// Build vector of Forces in nodes due to internal action applied (shear stress, prestress etc)
        /// </summary>
        public abstract void BuildF();

        public virtual void CalcResults(double[] Displacements)
        {
            #region SelectDisplacementsInGlobalCoordinates
            Console.WriteLine("Displacement in Global coordinates:");
            _nodeDisplacementGlobalCoordinates = new double[FEMModel.MAXGDLPERNODE * GlobalNodesElement.Length];
            for (int i = 0; i < _positionGlobalDisplElementInGlobalDisplSystemVectorResult.Length; i++)
            {
                Console.WriteLine(Displacements[_positionGlobalDisplElementInGlobalDisplSystemVectorResult[i]]);
                _nodeDisplacementGlobalCoordinates[i] = Displacements[_positionGlobalDisplElementInGlobalDisplSystemVectorResult[i]];
            }
            #endregion

            #region ConvertGlobalDisplacementsInLocalDisplacements
            Vector<double> vecGlobalDispl = Vector<double>.Build.Dense(_nodeDisplacementGlobalCoordinates);
            Vector<double> vecLocalDispl = Vector<double>.Build.Dense(_nodeDisplacementGlobalCoordinates.Length);
            vecLocalDispl = DofGlobalToLocal * vecGlobalDispl;
            _nodeDisplacementLocalCoordinates = vecLocalDispl.ToArray();

            Console.WriteLine("Displacement in Local coordinates:" + vecLocalDispl.ToString());
            #endregion
        }

        public double GetGlobalDisplacement(string labelNode, FEMModel.DOF gdl)
        {
            int pos = -1;
            for (int i = 0; i < GlobalNodesElement.Count(); i++)
            {
                if (labelNode == GlobalNodesElement[i].Label)
                {
                    pos = _positionGlobalDisplElementInGlobalDisplSystemVectorResult[i + (int) gdl];
                    break;
                }
            }
            return _nodeDisplacementGlobalCoordinates[pos];
        }

        public double GetLocalDisplacement(string labelNode, FEMModel.DOF gdl)
        {
            int pos = -1;
            for (int i = 0; i < GlobalNodesElement.Count(); i++)
            {
                if (labelNode == GlobalNodesElement[i].Label)
                {
                    pos = _positionGlobalDisplElementInGlobalDisplSystemVectorResult[i + (int)gdl];
                    break;
                }
            }
            return _nodeDisplacementLocalCoordinates[pos];
        }
    }
}
