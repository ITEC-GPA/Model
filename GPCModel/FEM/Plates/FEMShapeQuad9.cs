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
    public class FEMShapeQuad9 : FEMShape
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMShapeQuad9(int intgrPts, int numNodes = 9, int maxNumDoF = 2, int dim = 2, int order = 1) :
            base (intgrPts, maxNumDoF, dim, order)
        {
            _localNodes = new List<Geometry.Point2d>(numNodes);

            _localNodes.Add(new Point2d(-1, -1));
            _localNodes.Add(new Point2d(0, -1));
            _localNodes.Add(new Point2d(1, -1));
            _localNodes.Add(new Point2d(1, 0));
            _localNodes.Add(new Point2d(1, 1));
            _localNodes.Add(new Point2d(0, 1));
            _localNodes.Add(new Point2d(-1, 1));
            _localNodes.Add(new Point2d(-1, 0));
            _localNodes.Add(new Point2d(0, 0));

            _numIntgrPts = intgrPts;
            _NShape = Vector<double>.Build.Dense(numNodes);
            _dNShape = Matrix<double>.Build.Dense(dim, numNodes);
        }

        protected FEMShapeQuad9(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        public override void SetValue(Point3d coord)
        {
            double csi = coord.X;
            double eta = coord.Y;

            double cm = 1.0 - csi;
            double cp = 1.0 + csi;
            double em = 1.0 - eta;
            double ep = 1.0 + eta;

            double c2m = 1.0 - 2.0 * csi;
            double c2p = 1.0 + 2.0 * csi;
            double e2m = 1.0 - 2.0 * eta;
            double e2p = 1.0 + 2.0 * eta;

            double omc2 = cm * cp;
            double ome2 = em * ep;

            _NShape[0] = cm * em * csi * eta / 4.0;
            _NShape[1] = -omc2 * em * eta / 2.0;
            _NShape[2] = -cp * em * csi * eta / 4.0;
            _NShape[3] = cp * ome2 * csi / 2.0;
            _NShape[4] = cp * ep * csi * eta / 4.0;
            _NShape[5] = omc2 * ep * eta / 2.0;
            _NShape[6] = -cm * ep * csi * eta / 4.0;
            _NShape[7] = -cm * ome2 * csi / 2.0;
            _NShape[8] = omc2 * ome2;

            _dNShape[0, 0] = c2m * em * eta / 4.0;
            _dNShape[0, 1] = em * eta * csi;
            _dNShape[0, 2] = -c2p * em * eta / 4.0;
            _dNShape[0, 3] = c2p * ome2 / 2.0;
            _dNShape[0, 4] = c2p * ep * eta / 4.0;
            _dNShape[0, 5] = -ep * eta * csi;
            _dNShape[0, 6] = -c2m * ep * eta / 4.0;
            _dNShape[0, 7] = -c2m * ome2 / 2.0;
            _dNShape[0, 8] = -2.0 * ome2 * csi;

            _dNShape[1, 0] = cm * e2m * csi / 4.0;
            _dNShape[1, 1] = -omc2 * e2m / 2.0;
            _dNShape[1, 2] = -cp * e2m * csi / 4.0;
            _dNShape[1, 3] = -cp * csi * eta;
            _dNShape[1, 4] = cp * e2p * csi / 4.0;
            _dNShape[1, 5] = omc2 * e2p / 2.0;
            _dNShape[1, 6] = -cm * e2p * csi / 4.0;
            _dNShape[1, 7] = cm * csi * eta;
            _dNShape[1, 8] = -2.0 * omc2 * eta;
        }
        #endregion
    }
}
