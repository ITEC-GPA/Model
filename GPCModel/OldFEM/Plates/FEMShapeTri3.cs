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
    public class FEMShapeTri3 : FEMShape
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMShapeTri3(int intgrPts, int numNodes = 3, int maxNumDoF = 2, int dim = 2, int order = 1) :
            base(intgrPts, maxNumDoF, dim, order)
        {
            _localNodes = new List<Geometry.Point2d>(numNodes);
            _localNodes.Add(new Point2d(0, 0));
            _localNodes.Add(new Point2d(1, 0));
            _localNodes.Add(new Point2d(0, 1));

            _numIntgrPts = intgrPts;
            _NShape = Vector<double>.Build.Dense(numNodes);
            _dNShape = Matrix<double>.Build.Dense(dim, numNodes);
        }

        protected FEMShapeTri3(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        public override void SetValue(Point3d coord)
        {
            double csi = coord.X;
            double eta = coord.Y;

            _NShape[0] = 1 - csi - eta;
            _NShape[1] = csi;
            _NShape[2] = eta;

            _dNShape[0, 0] = -1.0;
            _dNShape[0, 1] = 1.0;
            _dNShape[0, 2] = 0.0;
            _dNShape[1, 0] = -1.0;
            _dNShape[1, 1] = 0.0;
            _dNShape[1, 2] = 1.0;
        }
        #endregion
    }
}
