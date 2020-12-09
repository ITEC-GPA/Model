using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;

namespace GPC.Model.Sections
{
    public class Plate
    {
        public enum TypePlate
        {
            inner,
            outer
        }

        Point2d _initialPoint;
        Point2d _endPoint;
        double _t;
        double _B;
        TypePlate _type;

        Point2d _pInitialEff1;
        Point2d _pFinalEff1;
        //double _sigmaInitialEff1;
        //double _sigmaFinalEff1;

        Point2d _pInitialEff2;
        Point2d _pFinalEff2;
        //double _sigmaInitialEff2;
        //double _sigmaFinalEff2;

        //Point2d _pInitialTraction;
        //double _sigmaInitialTraction = 0;
        //Point2d _pFinalTraction;
        //double _sigmaFinaTraction;

        public Plate(double t, Point2d initialPoint, Point2d endPoint, TypePlate typePlate)
        {
            _initialPoint = initialPoint;
            _endPoint = endPoint;
            _t = t;
            _type = typePlate;
            _B = B;

            if (_type == TypePlate.inner) {
                _pInitialEff1 = _initialPoint;
                _pFinalEff1 = new Point2d((_endPoint.X - _initialPoint.X) / 2.0, (_endPoint.Y - _initialPoint.Y) / 2.0);

                _pFinalEff2 = _pFinalEff1;
                _pInitialEff2 = _endPoint;

                /*_pInitialTraction = _endPoint;
                _pFinalTraction = _endPoint;*/
            } else
            {
                _pInitialEff1 = _initialPoint;
                _pFinalEff1 = _endPoint;

                /*_pInitialTraction = _endPoint;
                _pFinalTraction = _endPoint;*/
            }
        }

        public Point2d[] CentroidEffPosition {
            get {
                Point2d[] centroids = new Point2d[2];
                centroids[0] = new Point2d((_pInitialEff1.X + _pFinalEff1.X) / 2.0, (_pInitialEff1.Y + _pFinalEff1.Y) / 2.0);
                centroids[1] = new Point2d((_pInitialEff2.X + _pFinalEff2.X) / 2.0, (_pInitialEff2.Y + _pFinalEff2.Y) / 2.0);
                //centroids[2] = new Point2d((_pInitialTraction.X + _pFinalTraction.X) / 2.0, (_pInitialTraction.Y + _pFinalTraction.Y) / 2.0);
                return centroids;
            }
        }

        public double Area {
            get {
                return _t* (Beff1 + Beff2);
            }
        }

        public double J2Centroid
        {
            get
            {
                double beff1 = Beff1;
                double beff2 = Beff2;
                double J1 = 1.0 / 12.0 * _t * Math.Pow(beff1, 3.0);
                double J2 = 1.0 / 12.0 * _t * Math.Pow(beff2, 3.0);

                Point2d centroid0 = new Point2d((_endPoint.X - _initialPoint.X) / 2.0, (_endPoint.Y - _initialPoint.Y) / 2.0);

                Point2d[] centroids = CentroidEffPosition;
                double d1 = Math.Sqrt(Math.Pow(centroid0.X - centroids[0].X, 2.0) + Math.Pow(centroid0.Y - centroids[0].Y, 2.0));
                double d2 = Math.Sqrt(Math.Pow(centroid0.X - centroids[1].X, 2.0) + Math.Pow(centroid0.Y - centroids[1].Y, 2.0));

                return J1 + (beff1 * _t) * Math.Pow(d1, 2.0) + J2 + (beff2 * _t) * Math.Pow(d2, 2.0);
            }
        }

        public double Beff1
        {
            get {
                return Math.Sqrt(Math.Pow(_pFinalEff1.X - _pInitialEff1.X, 2.0) + Math.Pow(_pFinalEff1.Y - _pInitialEff1.Y, 2.0));
            }
        }
        
        public double Beff2 {
            get {
                return Math.Sqrt(Math.Pow(_pFinalEff2.X - _pInitialEff2.X, 2.0) + Math.Pow(_pFinalEff2.Y - _pInitialEff2.Y, 2.0));
            }
        }

        public double B
        {
            get
            {
                return Math.Sqrt(Math.Pow(_endPoint.X - _initialPoint.X, 2.0) + Math.Pow(_endPoint.Y - _initialPoint.Y, 2.0));
            }
        }

        public double GetKSigma(double psi)
        {
            //double psi = sigma1 / sigma2;
            //compression stress in denominator
            if (psi > 1)
            {
                psi = 1.0 / psi;
            }

            if (_type == TypePlate.inner)
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

        public double GetLambdaP(double ksigma, double fy)
        {
            double epsilon = Math.Sqrt(235/fy);
            return _B/_t / (28.4 * epsilon * Math.Sqrt(ksigma));
        }

        public void CalcBEff(double lambdaP, double psi)
        {
            if (_type == TypePlate.inner)
            {
                double lambdaPLimit = 0.5 + Math.Sqrt(0.085-0.055*psi);
                if (lambdaP <= lambdaPLimit)
                {
                    double rho = 1;
                    //-> no change
                } else
                {
                    double rho = Math.Min((lambdaP-0.055*(3+psi))/(lambdaP * lambdaP),1);
                    if (psi == 1)
                    {
                        double beff = rho * _B;
                        double beff1 = 0.5 * beff;
                        double beff2 = 0.5 * beff;

                        double xP = (_endPoint.X - _initialPoint.X) / _B * beff1 + _initialPoint.X;
                        double yP = (_endPoint.Y - _initialPoint.Y) / _B * beff1 + _initialPoint.Y;
                        _pFinalEff1 = new Point2d(xP, yP);

                        xP = -(_endPoint.X - _initialPoint.X) / _B * beff2 + _endPoint.X;
                        yP = -(_endPoint.Y - _initialPoint.Y) / _B * beff2 + _endPoint.Y;
                        _pFinalEff2 = new Point2d(xP, yP);
                    } else if (psi >= 0 && psi < 1)
                    {
                        double beff = rho * _B;
                        //se psi è nella end {
                        double beff1 = (2.0/(5.0-psi)) * beff;
                        double beff2 = beff - beff1;
                        // } else {
                        //double beff2 = (2.0/(5.0-psi)) * beff;
                        //double beff1 = beff - beff2;
                        // }

                        double xP = (_endPoint.X - _initialPoint.X) / _B * beff1 + _initialPoint.X;
                        double yP = (_endPoint.Y - _initialPoint.Y) / _B * beff1 + _initialPoint.Y;
                        _pFinalEff1 = new Point2d(xP, yP);

                        xP = -(_endPoint.X - _initialPoint.X) / _B * beff2 + _endPoint.X;
                        yP = -(_endPoint.Y - _initialPoint.Y) / _B * beff2 + _endPoint.Y;
                        _pFinalEff2 = new Point2d(xP, yP);
                    } else if (psi < 0)
                    {
                        //se psi è nel lato 2
                        double bT = _B/(1.0 + Math.Abs(psi)) * Math.Abs(psi);
                        double bC = _B - bT;

                        double beff = rho * bC;
                        double beff1 = 0.4 * beff;
                        double beff2 = 0.6 * beff;

                        double xP = (_endPoint.X - _initialPoint.X) / _B * beff1 + _initialPoint.X;
                        double yP = (_endPoint.Y - _initialPoint.Y) / _B * beff1 + _initialPoint.Y;
                        _pFinalEff1 = new Point2d(xP, yP);

                        xP = -(_endPoint.X - _initialPoint.X) / _B * (beff2+bT) + _endPoint.X;
                        yP = -(_endPoint.Y - _initialPoint.Y) / _B * (beff2+bT) + _endPoint.Y;
                        _pFinalEff2 = new Point2d(xP, yP);
                    } else
                    {
                        throw new Exception("psi not supported");
                    }
                }
            } else if (_type == TypePlate.outer)
            {
                if (lambdaP <= 0.748)
                {
                    double rho = 1;
                    //--> no changes
                }
                else
                {
                    double rho = Math.Min((lambdaP - 0.188) / (lambdaP * lambdaP), 1);
                    if (psi >= 0 && psi < 1)
                    {
                        double beff = rho * _B;
                        double xP = (_endPoint.X - _initialPoint.X) / _B * beff + _initialPoint.X;
                        double yP = (_endPoint.Y - _initialPoint.Y) / _B * beff + _initialPoint.Y;
                        _pFinalEff1 = new Point2d(xP, yP);
                    } else if (psi < 0) {
                        //se trazione vicino vincolo {
                        double bT = _B / (1.0 + Math.Abs(psi)) * Math.Abs(psi);
                        double bC = _B - bT;

                        double beff = rho * bC;
                        double xP = (_endPoint.X - _initialPoint.X) / _B * (beff+bT) + _initialPoint.X;
                        double yP = (_endPoint.Y - _initialPoint.Y) / _B * (beff) + _initialPoint.Y;
                        //} se trazione vicina lato libero:
                        //
                    }
                }
            } else
            {
                throw new Exception("type of plate not supported");
            }
        }

        public void SetSigma(double sigma0, double sigma1)
        {
            double fy = 275;
            //sigma > 0 compression
            double psi = -sigma0 / -sigma1;
            double ksigma = GetKSigma(psi);
            double lambdap = GetLambdaP(ksigma, fy);
            CalcBEff(lambdap, psi);
        }
    }
}
