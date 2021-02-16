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
    public class FEMShapeBeamCubic2 : FEMShape
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMShapeBeamCubic2(int intgrPts, int numNodes = 2, int maxNumDoF = 1, int dim = 2, int order = 0) :
            base(intgrPts, maxNumDoF, dim, order)
        {
            _localNodes = new List<Geometry.Point2d>(numNodes);
            _localNodes.Add(new Point2d(-1, 0));
            _localNodes.Add(new Point2d(0, 1));

            _numIntgrPts = intgrPts;
            _NShape = Vector<double>.Build.Dense((order+1)*numNodes);
            _dNShape = Matrix<double>.Build.Dense(dim, (order + 1)*numNodes);
        }

        protected FEMShapeBeamCubic2(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        public override void SetValue(Point3d coord)
        {
            double csi = coord.X;
            double csi2 = Math.Pow(coord.X, 2.0);
            double csi3 = Math.Pow(coord.X, 3.0);
            //double csi1 = 1 + csi;
            //double csi_1 = 1 - csi;
            //double csi2 = csi1 * csi_1;


            _NShape[0] = 0.25 * (csi3 - 3 * csi + 2);
            _NShape[1] = 0.25 * (csi3 - csi2 - csi + 1);
            _NShape[2] = - 0.25 * (csi3 - 3 * csi - 2);
            _NShape[3] = 0.25 * (csi3 + csi2 - csi - 1);

            _dNShape[0, 0] = 0;
            _dNShape[0, 1] = 0;
            _dNShape[0, 2] = 0;
            _dNShape[0, 3] = 0;

            //_NShape[0] = -0.5 * csi * (1.0 - csi);
            //_NShape[1] = +(1.0 - csi * csi);
            //_NShape[2] = +0.5 * csi * (1.0 + csi);

            //_dNShape[0, 0] = csi - 0.5;
            //_dNShape[0, 1] = -2.0 * csi; 
            //_dNShape[0, 2] = csi + 0.5;
        }
        #endregion
    }
}
