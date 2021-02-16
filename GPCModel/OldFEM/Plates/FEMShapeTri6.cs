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
    public class FEMShapeTri6 : FEMShape
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMShapeTri6(int intgrPts, int numNodes = 6, int maxNumDoF = 2, int dim = 2, int order = 1) :
            base(intgrPts, maxNumDoF, dim, order)
        {
            _localNodes = new List<Geometry.Point2d>(numNodes);

            _localNodes.Add(new Point2d(0, 0));
            _localNodes.Add(new Point2d(0.5, 0));
            _localNodes.Add(new Point2d(1, 0));
            _localNodes.Add(new Point2d(0.5, 0.5));
            _localNodes.Add(new Point2d(0, 1));
            _localNodes.Add(new Point2d(0, 0.5));

            _numIntgrPts = intgrPts;
            _NShape = Vector<double>.Build.Dense(numNodes);
            _dNShape = Matrix<double>.Build.Dense(dim, numNodes);
        }

        protected FEMShapeTri6(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        public override void SetValue(Point3d coord)
        {
            double csi = coord.X;
            double eta = coord.Y;
            double lam = 1 - csi - eta;

            _dNShape[0, 0] = -1.0;
            _dNShape[0, 1] = 1.0;
            _dNShape[0, 2] = 0.0;
            _dNShape[1, 0] = -1.0;
            _dNShape[1, 1] = 0.0;
            _dNShape[1, 2] = 1.0;


            _NShape[0] = -lam * (1.0 - 2.0 * lam);
            _NShape[1] = 4.0 * csi * lam;
            _NShape[2] = -csi * (1.0 - 2.0 * csi);
            _NShape[3] = 4.0 * csi * eta;
            _NShape[4] = -eta * (1.0 - 2.0 * eta);
            _NShape[5] = 4.0 * eta * lam;

            _dNShape[0, 0] = 1.0 - 4.0 * lam;
            _dNShape[0, 1] = 4.0 * (lam - csi);
            _dNShape[0, 2] = 4.0 * csi - 1.0;
            _dNShape[0, 3] = 4.0 * eta;
            _dNShape[0, 4] = 0.0;
            _dNShape[0, 5] = -4.0 * eta;

            _dNShape[1, 0] = 1.0 - 4.0 * lam;
            _dNShape[1, 1] = -4.0 * csi;
            _dNShape[1, 2] = 0.0;
            _dNShape[1, 3] = 4.0 * csi;
            _dNShape[1, 4] = -1.0 + 4.0 * eta;
            _dNShape[1, 5] = 4.0 * (lam - eta);
        }
    }
        #endregion
}
