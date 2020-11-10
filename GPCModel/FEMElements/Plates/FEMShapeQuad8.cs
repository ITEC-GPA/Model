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
    public class FEMShapeQuad8 : FEMShape
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMShapeQuad8(int intgrPts, int numNodes = 8, int maxNumDoF = 2, int dim = 2, int order = 1) :
            base (intgrPts, maxNumDoF, dim, order)
        {
            _localNodes = new List<Geometry.Point2d>(numNodes);
            _localNodes.Add(new Point2d(-1.0, -1.0));
            _localNodes.Add(new Point2d(+1.0, -1.0));
            _localNodes.Add(new Point2d(+1.0, +1.0));
            _localNodes.Add(new Point2d(-1.0, +1.0));
            _localNodes.Add(new Point2d(0.0, -1.0));
            _localNodes.Add(new Point2d(+1.0, 0.0));
            _localNodes.Add(new Point2d(0.0, +1.0));
            _localNodes.Add(new Point2d(-1.0, 0.0));

            _NShape = Vector<double>.Build.Dense(numNodes);
            _dNShape = Matrix<double>.Build.Dense(dim, numNodes);
        }

        protected FEMShapeQuad8(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        public override void SetValue(Point3d coord)
        {
            double g = coord.X;
            double h = coord.Y;

            double gg = g * g;
            double hh = h * h;
            double g2 = g * 2.0;
            double h2 = h * 2.0;

            _NShape[0] = -(1.0 - g) * (1.0 - h) * (1.0 + g + h) / 4.0;
            _NShape[1] = -(1.0 + g) * (1.0 - h) * (1.0 - g + h) / 4.0;
            _NShape[2] = -(1.0 + g) * (1.0 + h) * (1.0 - g - h) / 4.0;
            _NShape[3] = -(1.0 - g) * (1.0 + h) * (1.0 + g - h) / 4.0;
            _NShape[4] = (1.0 - gg) * (1.0 - h) / 2.0;
            _NShape[5] = (1.0 + g) * (1.0 - hh) / 2.0;
            _NShape[6] = (1.0 - gg) * (1.0 + h) / 2.0;
            _NShape[7] = (1.0 - g) * (1.0 - hh) / 2.0;

            _dNShape[0, 0] = (1.0 - h) * (g2 + h) / 4.0;
            _dNShape[0, 1] = (1.0 - h) * (g2 - h) / 4.0;
            _dNShape[0, 2] = (1.0 + h) * (g2 + h) / 4.0;
            _dNShape[0, 3] = (1.0 + h) * (g2 - h) / 4.0;
            _dNShape[0, 4] = -(1.0 - h) * g;
            _dNShape[0, 5] = (1.0 - hh) / 2.0;
            _dNShape[0, 6] = -(1.0 + h) * g;
            _dNShape[0, 7] = -(1.0 - hh) / 2.0;

            _dNShape[1, 0] = (1.0 - g) * (g + h2) / 4.0;
            _dNShape[1, 1] = -(1.0 + g) * (g - h2) / 4.0;
            _dNShape[1, 2] = (1.0 + g) * (g + h2) / 4.0;
            _dNShape[1, 3] = -(1.0 - g) * (g - h2) / 4.0;
            _dNShape[1, 4] = -(1.0 - gg) / 2.0;
            _dNShape[1, 5] = -(1.0 + g) * h;
            _dNShape[1, 6] = (1.0 - gg) / 2.0;
            _dNShape[1, 7] = -(1.0 - g) * h;
        }
        #endregion
    }
}
