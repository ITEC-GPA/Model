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
    public class FEMShapeTri6 : FEMShape
    {
        #region Variables 
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMShapeTri6() :
            base(6, 2, 2)
        {
            _localNodes = new List<Geometry.Point2d>(_numNodes);

            _localNodes.Add(new Point2d(0, 0));
            _localNodes.Add(new Point2d(0.5, 0));
            _localNodes.Add(new Point2d(1, 0));
            _localNodes.Add(new Point2d(0.5, 0.5));
            _localNodes.Add(new Point2d(0, 1));
            _localNodes.Add(new Point2d(0, 0.5));
        }

        protected FEMShapeTri6(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }
        #endregion

        #region Public Methods Override
        public override void SetValue(Point2d coord)
        {
            double csi = coord.X;
            double eta = coord.Y;

            _NMat[0] = 1 - csi - eta;
            _NMat[1] = csi;
            _NMat[2] = eta;

            _dNMat[0, 0] = -1.0;
            _dNMat[0, 1] = 1.0;
            _dNMat[0, 2] = 0.0;
            _dNMat[1, 0] = -1.0;
            _dNMat[1, 1] = 0.0;
            _dNMat[1, 2] = 1.0;
        }
        #endregion
    }
}
