using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.Elements
{
    /// <summary>
    /// Each finite element should derive from this
    /// </summary>
    public abstract class FiniteElement //: ModelObject //COMMENTATO PERCHE' EQUALS NON FUNZIONA +
    {
        int _ID;
        protected Dictionary<FEMModel.DOF, bool> _DOF = new Dictionary<FEMModel.DOF, bool>(Enum.GetNames(typeof(FEMModel.DOF)).Length);

        protected int[] _positionGlobalDisplElementInGlobalDisplSystemVectorResult; //position of interesting displacements of gdl of element in global displacement vector in all system
        protected double[] _nodeDisplacementGlobalCoordinates;
        protected double[] _nodeDisplacementLocalCoordinates;


        /// <summary>
        /// DOF[degree of freedom] = true if active, false if unactive
        /// </summary>
        public Dictionary<FEMModel.DOF, bool> DOF => _DOF;

        /// <summary>
        /// Nr of degree of freedom active
        /// </summary>
        public int NrDOFActive
        {
            get
            {
                int counter = 0;
                for (int i = 0; i < DOF.Count; i++)
                {
                    if (DOF[(FEMModel.DOF)i] == true) {
                        counter++;
                    }
                }
                return counter;
            }
        }

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
        /// Constructor 
        /// </summary>
        /// <param name="nodes">Set the nodes of element</param>
        public FiniteElement(IEnumerable<Node> nodes, int id)
        {
            _ID = id;
            GlobalNodesElement = nodes.ToArray();
            _DOF[FEMModel.DOF.DX] = false;
            _DOF[FEMModel.DOF.DY] = false;
            _DOF[FEMModel.DOF.DZ] = false;
            _DOF[FEMModel.DOF.RX] = false;
            _DOF[FEMModel.DOF.RY] = false;
            _DOF[FEMModel.DOF.RZ] = false;
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
            _nodeDisplacementGlobalCoordinates = new double[NrDOFActive * GlobalNodesElement.Length];
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
