using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;

namespace GPC.Model.FEM.FiniteElements
{
    public class VoidElement : FiniteElement
    {

        public VoidElement(Node[] nodes, int id) : base(nodes, id)
        {
            //recalled base(nodes)
            _DOF[FEM.DOF.DX] = true;
            _DOF[FEM.DOF.DY] = true;
            _DOF[FEM.DOF.DZ] = true;
            _DOF[FEM.DOF.RX] = true;
        }

        public override void BuildMatrix()
        {
            
        }

        public override void BuildF()
        {
            // implement force equivalent to node due to prestress, or temperature etc
            throw new NotImplementedException();
        }
    }
}
