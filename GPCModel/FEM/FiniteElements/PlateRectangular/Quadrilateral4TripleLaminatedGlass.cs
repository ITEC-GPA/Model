using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using System;
using System.Collections.Generic;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.FiniteElements
{
    public class Quadrilateral4TripleLaminatedGlass : FiniteElement
    {
        #region variables
        Quad4TripleLaminatedGlass _rectangular;
        #endregion

        public Quadrilateral4TripleLaminatedGlass(Node[] nodes, double G0, double h0, double h1, double h2, double EGlass, double niGlass) : base(nodes)
        { 
            Node[] nodesTransformed = new Node[4];
            nodesTransformed[0] = new Node(new Geometry.Point3d(0, 0, 0));
            nodesTransformed[1] = new Node(new Geometry.Point3d(2, 0, 0));
            nodesTransformed[2] = new Node(new Geometry.Point3d(2, 2, 0));
            nodesTransformed[3] = new Node(new Geometry.Point3d(0, 2, 0));

            _rectangular = new Quad4TripleLaminatedGlass(nodesTransformed, G0, h0, h1, h2, EGlass, niGlass);
            _rectangular.BuildMatrix();
        }

        public override void BuildMatrix()
        {
            _dofGlobalToLocal = mnl.Matrix<double>.Build.DenseIdentity(24); //TODO: aggiornare

            var jacob = FEMUtilities.J2D(Quad4Element.GetdNdCsi, Quad4Element.GetdNdEta, _nodesGlobal);

            Console.WriteLine("detJ(csi=0,eta=0)=" + jacob(0, 0).Determinant());

            /*mnl.Matrix<double> f(double x, double y)
            {
                return mnl.Matrix<double>.Build.DenseIdentity(24);
            }
            
            _kElementLocalCoord = _rectangular.KElementLocalCoord * GaussIntegration.IntegrationQuadrilateral(f, jacob, 1);*/
        }

        public override FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> lcAttributes, List<FreedomCaseAttribute> fcAttributes)
        {
            throw new NotImplementedException();
        }

        public override FiniteElement Duplicate()
        {
            throw new NotImplementedException();
        }

        protected override mnl.Vector<double> BuildFLocalCoord()
        {
            return mnl.Vector<double>.Build.Dense(24);
        }
    }
}
