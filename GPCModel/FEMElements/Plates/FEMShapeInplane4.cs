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
    public class FEMShapeInplane4 : FEMShape
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMShapeInplane4(int intgrPts, int numNodes = 4, int maxNumDoF = 2, int dim = 2, int order = 1) :
            base (intgrPts, maxNumDoF, dim, order)
        {
            _localNodes = new List<Geometry.Point2d>(numNodes);
            _NShape = Vector<double>.Build.Dense(numNodes);
            _dNShape = Matrix<double>.Build.Dense(dim, numNodes);
        }

        protected FEMShapeInplane4(SerializationInfo info, StreamingContext context) :
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
            double c2 = cm * cp;
            double e2 = em * ep;

            _NShape[0] = e2 * cm;
            _NShape[1] = c2 * em;
            _NShape[2] = e2 * cp;
            _NShape[3] = c2 * cp;

           _dNShape[0, 0] = -e2;
           _dNShape[0, 1] = -2.0 * csi * em;
           _dNShape[0, 2] = e2;
           _dNShape[0, 3] = -2.0 * csi * ep;

           _dNShape[1, 0] = -2.0 * eta * cm;
           _dNShape[1, 1] = -c2;
           _dNShape[1, 2] = -2.0 * eta * cp;
           _dNShape[1, 3] = c2;
        }
        #endregion
    }
}
