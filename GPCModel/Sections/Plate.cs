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

        double _t;
        double _B;
        TypePlate _type;

        double _removeLengthSide1;
        double _removeLengthSide2;

        double _fy;

        Point2d _initialPoint;
        Point2d _endPoint;
        
        public Plate(double t, double x0, double y0, double x1, double y1, double fy, TypePlate typePlate, double removeLengthSide1, double removeLengthSide2) : this(t, new Point2d(x0, y0), new Point2d(x1, y1), fy, typePlate, removeLengthSide1, removeLengthSide2)
        {
        }
        public Plate(double t, Point2d initialPoint, Point2d endPoint, double fy, TypePlate typePlate, double removeLengthSide1, double removeLengthSide2)
        {
            _initialPoint = initialPoint;
            _endPoint = endPoint;
            _t = t;
            _type = typePlate;
            _B = Math.Sqrt(Math.Pow(_endPoint.X - _initialPoint.X, 2.0) + Math.Pow(_endPoint.Y - _initialPoint.Y, 2.0));
            _fy = fy;

            _removeLengthSide1 = removeLengthSide1;
            _removeLengthSide2 = removeLengthSide2;
        }

        public Point2d Centroid
        {
            get
            {
                double xg = (_endPoint.X + _initialPoint.X) / 2.0;
                double yg = (_endPoint.Y + _initialPoint.Y) / 2.0;

                return new Point2d(xg, yg);
            }
        }

        public double Thickness => _t;

        public double Area {
            get {
                return _t * _B;
            }
        }

        public double B
        {
            get
            {
                return _B;
            }
        }

        public double J1Centroid
        {
            get
            {
                return 1.0 / 12.0 * _B * Math.Pow(_t, 3.0);
            }
        }

        public double J2Centroid
        {
            get
            {
                return 1.0 / 12.0 * _t * Math.Pow(_B, 3.0);
            }
        }

        public double JyCentroid
        {
            get
            {
                double dy = _endPoint.Y - _initialPoint.Y;
                double dx = _endPoint.X - _initialPoint.X;

                if (dy != 0 && dx == 0) //vertical plate
                {
                    return J2Centroid;
                }
                else if (dx != 0 && dy == 0) //horizontal plate
                {
                    return J1Centroid;
                }
                else
                {
                    throw new Exception("Oblique plate not yet supported");
                }
            }
        }

        public double JzCentroid
        {
            get
            {
                double dy = _endPoint.Y - _initialPoint.Y;
                double dx = _endPoint.X - _initialPoint.X;

                if (dy != 0 && dx == 0) //vertical plate
                {
                    return J1Centroid;
                }
                else if (dx != 0 && dy == 0) //horizontal plate
                {
                    return J2Centroid;
                }
                else
                {
                    throw new Exception("Oblique plate not yet supported");
                }
            }
        }

        public Point2d InitialPoint => _initialPoint;
        public Point2d EndPoint => _endPoint;
        public double Fyk => _fy;
        public TypePlate GetType => _type;
        public double RemoveLengthSide1 => _removeLengthSide1;
        public double RemoveLengthSide2 => _removeLengthSide2;

    }
}
