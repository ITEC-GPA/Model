using System;
using GPC.Model.Elements;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    public class VoidElement : FiniteElement
    {

        public VoidElement(Node[] nodes, ElementProperty p, int id) : base(nodes, p, id)
        {
            //recalled base(nodes)
            _DOF.Add(LinearSolver.DOF.RX);
            _DOF.Add(LinearSolver.DOF.DX);
            _DOF.Add(LinearSolver.DOF.DY);
            _DOF.Add(LinearSolver.DOF.DZ);
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
