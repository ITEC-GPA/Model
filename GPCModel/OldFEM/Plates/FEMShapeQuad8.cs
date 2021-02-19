using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using GPC.Geometry;



namespace GPC.Model.FEMOld
{
    //8 node Serendipity Element
    public class FEMShapeQuad8 : FEMShape
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMShapeQuad8(int intgrPts, int numNodes = 8, int maxNumDoF = 2, int dim = 2, int order = 0) :
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

            _numIntgrPts = intgrPts;
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
            double c = coord.X;
            double e = coord.Y;

            double cc = c * c;
            double ee = e * e;
            double c2 = c * 2.0;
            double e2 = e * 2.0;

            _NShape[0] = -(1.0 - c) * (1.0 - e) * (1.0 + c + e) / 4.0;
            _NShape[1] = -(1.0 + c) * (1.0 - e) * (1.0 - c + e) / 4.0;
            _NShape[2] = -(1.0 + c) * (1.0 + e) * (1.0 - c - e) / 4.0;
            _NShape[3] = -(1.0 - c) * (1.0 + e) * (1.0 + c - e) / 4.0;
            _NShape[4] = (1.0 - cc) * (1.0 - e) / 2.0;
            _NShape[5] = (1.0 + c) * (1.0 - ee) / 2.0;
            _NShape[6] = (1.0 - cc) * (1.0 + e) / 2.0;
            _NShape[7] = (1.0 - c) * (1.0 - ee) / 2.0;

            _dNShape[0, 0] = (1.0 - e) * (c2 + e) / 4.0;
            _dNShape[0, 1] = (1.0 - e) * (c2 - e) / 4.0;
            _dNShape[0, 2] = (1.0 + e) * (c2 + e) / 4.0;
            _dNShape[0, 3] = (1.0 + e) * (c2 - e) / 4.0;
            _dNShape[0, 4] = -(1.0 - e) * c;
            _dNShape[0, 5] = (1.0 - ee) / 2.0;
            _dNShape[0, 6] = -(1.0 + e) * c;
            _dNShape[0, 7] = -(1.0 - ee) / 2.0;

            _dNShape[1, 0] = (1.0 - c) * (c + e2) / 4.0;
            _dNShape[1, 1] = -(1.0 + c) * (c - e2) / 4.0;
            _dNShape[1, 2] = (1.0 + c) * (c + e2) / 4.0;
            _dNShape[1, 3] = -(1.0 - c) * (c - e2) / 4.0;
            _dNShape[1, 4] = -(1.0 - cc) / 2.0;
            _dNShape[1, 5] = -(1.0 + c) * e;
            _dNShape[1, 6] = (1.0 - cc) / 2.0;
            _dNShape[1, 7] = -(1.0 - c) * e;
        }
        #endregion
    }
}
