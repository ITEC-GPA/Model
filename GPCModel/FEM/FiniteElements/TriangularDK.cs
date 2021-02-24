using GPC.Model.FEM.Properties;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    class TriangularDK : FiniteElement
    {
        public TriangularDK(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
        {

        }

        public override void BuildMatrix()
        {
            throw new System.NotImplementedException();
        }

        protected override Vector<double> BuildFLocalCoord()
        {
            throw new System.NotImplementedException();
        }

        private double N1(double csi, double eta)
        {
            return 2.0 * (1.0 - csi - eta) * (0.5 - csi - eta);
        }

    }
}
