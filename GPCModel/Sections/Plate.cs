using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;

namespace GPC.Model.Sections
{
    /*
    public class Plate
    {
        public enum TypePlate
        {
            inner,
            outer
        }

        //double _t;
        //double _B;
        TypePlate _type;

        double _fy;

        Point2d _initialPoint;
        Point2d _endPoint;
        
        public Plate(double t, double x0, double y0, double x1, double y1, double fy, TypePlate typePlate) 
            : this(t, new Point2d(x0, y0), new Point2d(x1, y1), fy, typePlate)
        {
        }
        public Plate(double t, Point2d initialPoint, Point2d endPoint, double fy, TypePlate typePlate)
        {
            _initialPoint = initialPoint;
            _endPoint = endPoint;
            _t = t;
            _type = typePlate;
            _B = Math.Sqrt(Math.Pow(_endPoint.X - _initialPoint.X, 2.0) + Math.Pow(_endPoint.Y - _initialPoint.Y, 2.0));
            _fy = fy;
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

        public bool IsVertical
        {
            get
            {
                if ((_endPoint.Y - _initialPoint.Y != 0) && (_endPoint.X - _initialPoint.X == 0)) //vertical plate  
                    return true;
                else
                    return false;
            }
        }

        public bool IsOrizzontal
        {
            get
            {
                if ((_endPoint.Y - _initialPoint.Y == 0) && (_endPoint.X - _initialPoint.X != 0)) //vertical plate  
                    return true;
                else
                    return false;
            }
        }

        public double GetB()
        {
            return Math.Sqrt(Math.Pow(_endPoint.X - _initialPoint.X, 2.0) + Math.Pow(_endPoint.Y - _initialPoint.Y, 2.0));
        }

        public double GetThickness()
        {

        }

        public double Thickness => _t;

        public double Area {
            get 
            {
                return _t * _B;
            }
        }

        public double B => _B;

        public double J1Centroid => 1.0 / 12.0 * _B * Math.Pow(_t, 3.0);

        public double J2Centroid => 1.0 / 12.0 * _t * Math.Pow(_B, 3.0);

        public double JyCentroid
        {
            get
            {
                double dy = _endPoint.Y - _initialPoint.Y;
                double dx = _endPoint.X - _initialPoint.X;

                if (dy != 0 && dx == 0) //vertical plate                
                    return J2Centroid;
                
                else if (dx != 0 && dy == 0) //horizontal plate                
                    return J1Centroid;
                
                else                
                    throw new Exception("Oblique plate not yet supported");
                
            }
        }

        public double JzCentroid
        {
            get
            {
                double dy = _endPoint.Y - _initialPoint.Y;
                double dx = _endPoint.X - _initialPoint.X;

                if (dy != 0 && dx == 0) //vertical plate                
                    return J1Centroid;
                
                else if (dx != 0 && dy == 0) //horizontal plate                
                    return J2Centroid;
                
                else                
                    throw new Exception("Oblique plate not yet supported");
                
            }
        }
    }*/
}
