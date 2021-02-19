using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Elements;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;

namespace GPC.Model.FEM.FiniteElements
{
    public class VoidElement : FiniteElement
    {

        public VoidElement(Node[] nodes, ElementProperty p, int id) : base(nodes, p, id)
        {
            //recalled base(nodes)
            _DOF.Add(FEMModel.DOF.RX);
            _DOF.Add(FEMModel.DOF.DX);
            _DOF.Add(FEMModel.DOF.DY);
            _DOF.Add(FEMModel.DOF.DZ);
        }

        public override void BuildMatrix()
        {
            
        }

        protected override Vector<double> BuildFLocalCoord()
        {
            // implement force equivalent to node due to prestress, or temperature etc
            throw new NotImplementedException();
        }
    }
}
