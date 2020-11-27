using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using GPC.Geometry;

namespace GPC.Model.FEM
{
    public class FEMShapeBeamQuadratic3 : FEMShape
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMShapeBeamQuadratic3(int intgrPts, int numNodes = 3, int maxNumDoF = 1, int dim = 1, int order = 0) :
            base(intgrPts, maxNumDoF, dim, order)
        {
            _localNodes = new List<Geometry.Point2d>(numNodes);
            _localNodes.Add(new Point2d(-1, 0));
            _localNodes.Add(new Point2d(0, 1));

            _numIntgrPts = intgrPts;
            _NShape = Vector<double>.Build.Dense((order + 1) * numNodes);
            _dNShape = Matrix<double>.Build.Dense(dim, (order + 1) * numNodes);
        }

        protected FEMShapeBeamQuadratic3(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        public override void SetValue(Point3d coord)
        {
            double csi = coord.X;

            _NShape[0] = -0.5 * csi * (1.0 - csi);
            _NShape[1] = +(1.0 - csi * csi);
            _NShape[2] = +0.5 * csi * (1.0 + csi);

            _dNShape[0, 0] = csi - 0.5;
            _dNShape[0, 1] = -2.0 * csi; 
            _dNShape[0, 2] = csi + 0.5;
        }
        #endregion
    }
}
