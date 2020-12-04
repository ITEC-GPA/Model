using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;

namespace GPC.Model.Sections
{
    class Plate
    {
        enum TypePlate
        {
            inner,
            outer
        }

        Point2d _initialPoint;
        Point2d _endPoint;
        double _t;

        Point2d _pInitialEff1;
        double _sigmaIinitialEff1;
        Point2d _pEndEff1;
        double _sigmaEndEff1;

        Point2d _pInitialEff2;
        double _sigmaIinitialEff2;
        Point2d _pEndEff2;
        double _sigmaEndEff2;

        TypePlate _typePlate;

        public Plate(double t, Point2d initialPoint, Point2d endPoint)
        {
            _initialPoint = initialPoint;
            _endPoint = endPoint;
            _t = t;
        }

        public Point2d[] CentroidEffPosition {
            get {
                Point2d[] centroids = new Point2d[2];
                centroids[0] = new Point2d((_pInitialEff1.X + _pEndEff1.X)/2.0, (_pInitialEff1.Y + _pEndEff1.Y) / 2.0);
                centroids[0] = new Point2d((_pInitialEff2.X + _pEndEff2.X) / 2.0, (_pInitialEff2.Y + _pEndEff2.Y) / 2.0);
                return centroids;
            }
        }

        public double GetKSigma()
        {
            double psi = _sigmaIinitialEff1 / _sigmaIinitialEff2;
            if (psi > 1)
            {
                psi = 1.0 / psi;
            }

            if (_typePlate == TypePlate.inner)
            {  
                if (psi == 1)
                {
                    return 4;
                } else if (psi > 0 && psi < 1)
                {
                    return 8.2 / (1.05 + psi);
                } else if (psi == 0)
                {
                    return 7.81;
                } else if (psi < 0 && psi > -1)
                {
                    return 7.81 - 6.29 * psi + 9.78 * psi * psi;
                } else if (psi == -1)
                {
                    return 23.9;
                } else if (psi > -3 && psi < -1)
                {
                    return 5.98 * (1 - psi) * (1 - psi);
                } else
                {
                    throw new Exception("out of range");
                }
            } else //Outer
            {
                if (psi == 1)
                {
                    return 0.43;
                }
                else if (psi > 0 && psi < 1)
                {
                    return 0.578/(0.34+psi);
                }
                else if (psi == 0)
                {
                    return 1.70;
                }
                else if (psi < 0 && psi > -1)
                {
                    return 1.7-5*psi+17.1*psi*psi;
                }
                else if (psi == -1)
                {
                    return 23.8;
                }
                else if (psi > -3 && psi < -1)
                {
                    return 0.57 -0.21 * psi + 0.07 * psi * psi;
                }
                else
                {
                    throw new Exception("out of range");
                }
            }
        }

        public double GetLambdaP(double b, double t, double ksigma, double fy)
        {
            double epsilon = Math.Sqrt(235/fy);
            return b/t / (28.4 * epsilon * Math.Sqrt(ksigma));
        }
    }
}
