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
    public class FEMShapeDilling4 : FEMShape
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMShapeDilling4(int intgrPts, int numNodes = 4, int maxNumDoF = 2, int dim = 2, int order = 1) :
            base (intgrPts, maxNumDoF, dim, order)
        {
            _localNodes = new List<Geometry.Point2d>(numNodes);
            _NShape = Vector<double>.Build.Dense(numNodes);
            _dNShape = Matrix<double>.Build.Dense(dim, numNodes);
        }

        protected FEMShapeDilling4(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        public override void SetValue(Point3d coord)
        {
            double csi = coord.X;
            double eta = coord.Y;

            //double cm = 1 - csi;
            //double cp = 1 + csi;
            //double em = 1 - eta;
            //double ep = 1 + eta;
            //double c2 = cm * cp;
            //double e2 = em * ep;

            //_NShape[0] = e2 * cm;
            //_NShape[1] = c2 * em;
            //_NShape[2] = e2 * cm;
            //_NShape[3] = c2 * cp;

            //_dNShape[0, 0] = -e2;
            //_dNShape[0, 1] = -2.0 * csi * em;
            //_dNShape[0, 2] = e2;
            //_dNShape[0, 3] = -2.0 * csi * ep;

            //_dNShape[1, 0] = -2.0 * eta * cm;
            //_dNShape[1, 1] = -c2;
            //_dNShape[1, 2] = -2.0 * eta * cp;
            //_dNShape[1, 3] = c2;


            _NShape[0] = 0.5 * (1 - csi * csi) * (1 - eta);
            _NShape[1] = 0.5 * (1 + csi) * (1 - eta * eta);
            _NShape[2] = 0.5 * (1 - csi * csi) * (1 + eta);
            _NShape[3] = 0.5 * (1 - csi) * (1 - eta * eta);

            _dNShape[0, 0] = -csi * (1 - eta);
            _dNShape[0, 1] = 0.5 * (1 - eta * eta);
            _dNShape[0, 2] = -csi * (1 + eta);
            _dNShape[0, 3] = 0.5 * (1 - eta * eta);

            _dNShape[1, 0] = -0.5 * (1 - csi * csi);
            _dNShape[1, 1] = -eta * (1 + csi);
            _dNShape[1, 2] = 0.5 * (1 - csi * csi);
            _dNShape[1, 3] = -eta * (1 - csi);


            //double c = coord.X;
            //double e = coord.Y;

            //double cc = c * c;
            //double ee = e * e;
            //double c2 = c * 2.0;
            //double e2 = e * 2.0;


            //_NShape[0] = (1.0 - cc) * (1.0 - e) / 2.0;
            //_NShape[1] = (1.0 + c) * (1.0 - ee) / 2.0;
            //_NShape[2] = (1.0 - cc) * (1.0 + e) / 2.0;
            //_NShape[3] = (1.0 - c) * (1.0 - ee) / 2.0;

            //_dNShape[0, 0] = -(1.0 - e) * c;
            //_dNShape[0, 1] = (1.0 - ee) / 2.0;
            //_dNShape[0, 2] = -(1.0 + e) * c;
            //_dNShape[0, 3] = -(1.0 - ee) / 2.0;

            //_dNShape[1, 0] = -(1.0 - cc) / 2.0;
            //_dNShape[1, 1] = -(1.0 + c) * e;
            //_dNShape[1, 2] = (1.0 - cc) / 2.0;
            //_dNShape[1, 3] = -(1.0 - c) * e;
        }
        #endregion
    }
}
