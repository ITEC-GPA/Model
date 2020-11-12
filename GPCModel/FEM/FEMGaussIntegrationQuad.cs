using GPC.Geometry;
using GPC.Model.FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM
{
    public class FEMGaussIntegrationQuad : FEMGaussIntegration
    {
        #region Variables 
        /// <summary>
        /// <param name="_numX"> Total number of Gauss points in X dimension </param>
        /// <param name="_numY"> Total number of Gauss points in Y dimension </param>
        /// <param name="_numZ"> Total number of Gauss points in Z dimension </param>
        /// <param name="_numPoints"> Total number of Gauss Point</param>
        /// </summary>
        protected int _numX;
        protected int _numY;
        protected int _numZ;
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMGaussIntegrationQuad(int numX = 1, int numY = 0, int numZ = 0)
        {
            _numX = numX;
            _numY = numY;
            _numZ = numZ;


            if (_numX < 0 && _numX > 3) { throw new ArgumentException($"{nameof(_numX)} Gauss Point Numbers - Cannot be zero, less than 1 or higher than 3"); }
            if (_numY < 0 && _numY > 3) { throw new ArgumentException($"{nameof(_numY)} Gauss Point Numbers - Cannot be zero, less than 1 or higher than 3"); }
            if (_numZ < 0 && _numZ > 3) { throw new ArgumentException($"{nameof(_numZ)} Gauss Point Numbers - Cannot be zero, less than 1 or higher than 3"); }

            _numPoints = numX * (numY == 0 ? 1 : numY) * (numZ == 0 ? 1 : numZ);

            _coords = new Point3d[_numPoints];
            _weights = new double[_numPoints];
            SetValue(0, _numPoints);
        }

        protected FEMGaussIntegrationQuad(SerializationInfo info, StreamingContext context)
        {
        }
        #endregion

        #region Public Methods Specific
        #endregion

        #region Protected Methods virtual
        public override void SetValue(int order, int dim)
        {
            int rowX = _numX - 1;
            int rowY = _numY - 1;
            int rowZ = _numZ - 1;

            double[][] GP = new double[3][] { new double[3] { 0.0, 0.0, 0.0},
                                              new double[3] { -Math.Sqrt(1.0/ 3.0), Math.Sqrt(1.0 / 3.0), 0.0},
                                              new double[3] { -Math.Sqrt(1.0 / 3.0), 0.0, Math.Sqrt(1.0 / 3.0) }};

            double[][] WP = new double[3][] { new double[3] { 2.0, 0.0, 0.0},
                                              new double[3] { 1.0, 1.0, 0.0},
                                              new double[3] { 5.0/ 9.0, 8.0/ 9.0, 5.0/ 9.0 }};

            for (int iz = 0; iz < (_numZ == 0 ? 1 : _numZ); iz++)
            {
                for (int iy = 0; iy < (_numY == 0 ? 1 : _numY); iy++)
                {
                    for (int ix = 0; ix < _numX; ix++)
                    {
                        int i = ix + iy * _numX + iz * _numX * (_numY == 0 ? 1 : _numY);

                        double wpx = WP[rowX][ix];
                        //double wpy = _numY == 0 ? 1 : WP[rowY][iy];
                        //double wpz = _numZ == 0 ? 1 : WP[rowZ][iz];

                        double wpy = _numY == 0 ? 1 : WP[rowY][iy];
                        double wpz = _numZ == 0 ? 1 : WP[rowZ][iz];

                        _weights[i] = wpx * wpy * wpz;

                        //double y = GP[rowY][iy];
                        //double z = GP[rowZ][iz];

                        double x = GP[rowX][ix];
                        double y = _numY == 0 ? 0.0 : GP[rowY][iy];
                        double z = _numZ == 0 ? 0.0 : GP[rowZ][iz];

                        _coords[i] = new Point3d(x, y, z);
                    }
                }
            }
        }
        #endregion
    }
}
