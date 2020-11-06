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
        public FEMShapeQuad4() :
            base (4, 2, 2)
        {
            _localNodes = new List<Geometry.Point2d>(_numNodes);
            _localNodes.Add(new Point2d(-1, -1));
            _localNodes.Add(new Point2d(+1, -1));
            _localNodes.Add(new Point2d(+1, +1));
            _localNodes.Add(new Point2d(-1, +1));
        }

        protected FEMShapeQuad4(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        public override void SetValue(Point2d coord)
        {
            double csi = coord.X;
            double eta = coord.Y;

            double cm = 1 - csi;
            double cp = 1 + csi;
            double em = 1 - eta;
            double ep = 1 + eta;

            _NMat[0] = cm * em / 4.0;
            _NMat[1] = cp * em / 4.0;
            _NMat[2] = cp * ep / 4.0;
            _NMat[3] = cm * ep / 4.0;

            _dNMat[0, 0] = -em / 4.0;
            _dNMat[0, 1] = +em / 4.0;
            _dNMat[0, 2] = +ep / 4.0;
            _dNMat[0, 3] = -ep / 4.0;
            _dNMat[1, 0] = -cm / 4.0;
            _dNMat[1, 1] = -cp / 4.0;
            _dNMat[1, 2] = +cp / 4.0;
            _dNMat[1, 3] = +cm / 4.0;
        }
        #endregion
    }
}
