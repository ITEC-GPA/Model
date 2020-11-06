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
    public class FEMGaussIntegrationTri : FEMGaussIntegration
    {
        #region Variables 
        /// <summary>
        /// <param name="_numX"> Total number of Gauss points in X dimension </param>
        /// <param name="_numY"> Total number of Gauss points in Y dimension </param>
        /// <param name="_numZ"> Total number of Gauss points in Z dimension </param>
        /// <param name="_numPoints"> Total number of Gauss Point</param>
        /// </summary>
        protected int _order;
        protected int _dimension;
        protected int _numPoints;
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMGaussIntegrationTri(int order, int dim)
        {
            //if (_numX < 0 && _numX > 3) { throw new ArgumentException($"{nameof(_numX)} Gauss Point Numbers - Cannot be zero, less than 1 or higher than 3"); }
            //if (_numY < 0 && _numY > 3) { throw new ArgumentException($"{nameof(_numY)} Gauss Point Numbers - Cannot be zero, less than 1 or higher than 3"); }
            //if (_numZ < 0 && _numZ > 3) { throw new ArgumentException($"{nameof(_numZ)} Gauss Point Numbers - Cannot be zero, less than 1 or higher than 3"); }

            SetNumPoint(order, dim);
            _coords = new Point3d[_numPoints];
            _weights = new double[_numPoints];
            SetValue(order, dim);
        }

        protected FEMGaussIntegrationTri(SerializationInfo info, StreamingContext context)
        {
        }
        #endregion

        #region Public Methods Specific
        #endregion

        #region Protected Methods virtual
        public override void SetValue(int order, int dim)
        {
            double a, b;

            if (dim == 2)
            {
                if (order == 2)
                { a = -.5; b = .5; }
                else
                { a = .4; b = .2; }
            }
            else
            {
                if (order == 2)
                { a = .4472136; b = .1381966; }
                else
                { a = 1.0/ 3.0; b = 1.0/ 6.0; }
            }

            double eq = 1.0/ (dim + 1.0);
            double factorW = (dim == 2 ? 1.0/ 2.0 : 1.0/ 6.0);

            for (int i = 0; i < _numPoints; i++)
            {
                switch (order)
                {
                    //case 1:
                    //    m_Coord[0].SetValue(eq, eq, eq);
                    //    m_Weight[0] = factorW;
                    //    break;

                    //default:
                    //case 2:
                    //    m_Coord[i].SetValue(b + (i == 1) * a, b + (i == 2) * a, b + (i == 3) * a);
                    //    m_Weight[i] = eq * factorW;
                    //    break;

                    //case 3:
                    //    if (!i)
                    //    {
                    //        m_Coord[0].SetValue(eq, eq, eq);
                    //        m_Weight[0] = (dim == 2 ? -(27./ 48.) : -(4./ 5.)) * factorW;
                    //    }
                    //    else
                    //    {
                    //        _coords[i].SetValue(b + (i == 1) * a, b + (i == 2) * a, b + (i == 3) * a);
                    //        _weights[i] = (dim == 2 ? (25./ 48.) : (9./ 20.)) * factorW;
                    //    }
                    //    break;
                }
            }
        }

        private void SetNumPoint(int order, int dim)
        {
            if (order < 2)
            {
                _numPoints = 1;
            }
            else
            {
                _numPoints = order + dim - 1;
            }
        }
        #endregion
    }
}
