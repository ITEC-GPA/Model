using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Elements;
using GPC.Model.FEM.Properties;
using GPC.Model.Materials;
using MathNet.Numerics.LinearAlgebra;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    class Brick : FiniteElement
    {
        public Brick(Node[] nodes, BrickProperty property, int id) : base(nodes, property, id) { }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            throw new NotImplementedException();
        }

        public override void BuildMatrix()
        {
            throw new NotImplementedException();
        }

        public override mnl.Matrix<double> GetB(double csi, double eta, double zeta)
        {
            throw new NotImplementedException();
        }

        public override void GetResults(double[] globalDisplacementsNodes, out double[] localDisplacements, out Matrix<double>[] gloabalPseudoDeformation, out Matrix<double>[] localPseudoDeformation, out Matrix<double>[] globalForces, out Matrix<double>[] localForces, out Matrix<double>[] globalStress, out Matrix<double>[] localStress, out Matrix<double>[] globalEpsilon, out Matrix<double>[] localEpsilon)
        {
            throw new NotImplementedException();
        }
    }
}
