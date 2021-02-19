using GPC.Geometry;
using GPC.Model.FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEMOld
{
    public class FEMGaussIntegrationBeam : FEMGaussIntegration
    {
        #region Variables 
        /// <summary>
        /// <param name="_numX"> Total number of Gauss points in X dimension </param>
        /// <param name="_numY"> Total number of Gauss points in Y dimension </param>
        /// <param name="_numZ"> Total number of Gauss points in Z dimension </param>
        /// <param name="_numPoints"> Total number of Gauss Point</param>
        /// </summary>
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMGaussIntegrationBeam(int order)
        {
            //if (_numX < 0 && _numX > 3) { throw new ArgumentException($"{nameof(_numX)} Gauss Point Numbers - Cannot be zero, less than 1 or higher than 3"); }
            //if (_numY < 0 && _numY > 3) { throw new ArgumentException($"{nameof(_numY)} Gauss Point Numbers - Cannot be zero, less than 1 or higher than 3"); }
            //if (_numZ < 0 && _numZ > 3) { throw new ArgumentException($"{nameof(_numZ)} Gauss Point Numbers - Cannot be zero, less than 1 or higher than 3"); }

            _order = order;
            _dimension = 1;
            _numPoints = order;
            _coords = new Point3d[_numPoints];
            _weights = new double[_numPoints];
            SetValue(order, _dimension);
        }

        protected FEMGaussIntegrationBeam(SerializationInfo info, StreamingContext context)
        {
        }
        #endregion

        #region Public Methods Specific
        #endregion

        #region Protected Methods virtual
        public override void SetValue(int order, int dim)
        {
            if (order == 1)
            {
                _coords[0] = new Point3d(0.0, 0.0, 0.0);

                _weights[0] = 2.0;
            }
            if(order == 2)
            {
                _coords[0] = new Point3d(-1.0 / Math.Sqrt(3.0), 0.0, 0.0);
                _coords[1] = new Point3d(+1.0 / Math.Sqrt(3.0), 0.0, 0.0);

                _weights[0] = 1.0;
                _weights[1] = 1.0;
            }
            if (order == 3)
            {
                _coords[0] = new Point3d(-Math.Sqrt(-(3.0 / 5.0)), 0.0, 0.0);
                _coords[1] = new Point3d(0.0, 0.0, 0.0);
                _coords[1] = new Point3d(+Math.Sqrt(-(3.0 / 5.0)), 0.0, 0.0);

                _weights[0] = 5.0 / 9.0;
                _weights[1] = 8.0 / 9.0;
                _weights[1] = 5.0 / 9.0;
            }
        }

        private void SetNumPoint(int order, int dim)
        {
        }
        #endregion
    }
}
