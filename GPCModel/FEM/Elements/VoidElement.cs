using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;

namespace GPC.Model.FEM.Elements
{
    public class VoidElement : FiniteElement
    {

        public VoidElement(IEnumerable<Node> nodes, int id) : base(nodes, id)
        {
            //recalled base(nodes)
            _DOF[FEMModel.DOF.DX] = true;
            _DOF[FEMModel.DOF.DY] = true;
            _DOF[FEMModel.DOF.DZ] = true;
            _DOF[FEMModel.DOF.RX] = true;
        }

        public override void BuildMatrix()
        {
            
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
