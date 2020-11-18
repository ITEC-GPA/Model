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
    public class FEMShapeQuad4 : FEMShape
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMShapeQuad4(int intgrPts, int numNodes = 4, int maxNumDoF = 2, int dim = 2, int order = 1) :
            base (intgrPts, maxNumDoF, dim, order)
        {
            _localNodes = new List<Geometry.Point2d>(numNodes);
            _localNodes.Add(new Point2d(-1, -1));
            _localNodes.Add(new Point2d(+1, -1));
            _localNodes.Add(new Point2d(+1, +1));
            _localNodes.Add(new Point2d(-1, +1));

            _numIntgrPts = intgrPts;
            _NShape = Vector<double>.Build.Dense(numNodes);
            _dNShape = Matrix<double>.Build.Dense(dim, numNodes);
        }

        protected FEMShapeQuad4(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        public override void SetValue(Point3d coord)
        {
            double csi = coord.X;
            double eta = coord.Y;

            double cm = 1 - csi;
            double cp = 1 + csi;
            double em = 1 - eta;
            double ep = 1 + eta;

            _NShape[0] = cm * em / 4.0;
            _NShape[1] = cp * em / 4.0;
            _NShape[2] = cp * ep / 4.0;
            _NShape[3] = cm * ep / 4.0;

            _dNShape[0, 0] = -em / 4.0;
            _dNShape[0, 1] = +em / 4.0;
            _dNShape[0, 2] = +ep / 4.0;
            _dNShape[0, 3] = -ep / 4.0;
            _dNShape[1, 0] = -cm / 4.0;
            _dNShape[1, 1] = -cp / 4.0;
            _dNShape[1, 2] = +cp / 4.0;
            _dNShape[1, 3] = +cm / 4.0;
        }
        #endregion
    }
}
