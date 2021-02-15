using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;

namespace GPC.FEM.Elements
{
    public class VoidElement : FiniteElement
    {

        public VoidElement(IEnumerable<Node> nodes) : base(nodes)
        {
            //recalled base(nodes)
            _DOF[FEMModel.DOF.DX] = true;
            _DOF[FEMModel.DOF.DY] = true;
            _DOF[FEMModel.DOF.DZ] = true;
        }

        public override void BuildMatrix()
        {
            PositionToGlobalSystemK = new Dictionary<Position, Position>();
            
        }

        public override void BuildF()
        {
            // implement force equivalent to node due to prestress, or temperature etc
            throw new NotImplementedException();
        }

        public override void CalcResults(double[] Displacements)
        {
           
        }
    }
}
