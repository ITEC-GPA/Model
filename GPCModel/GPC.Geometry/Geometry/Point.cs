using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Geometry
{
    public class Point : IGeometryBase
    {
        #region FIELD_CONSTRUCTORS
        public Point() { }
        #endregion

        #region FIELD_DECONSTRUCTORS
        #endregion

        #region FIELD_COMMANDS
        #endregion

        #region FIELD_METHODS
        #endregion

        #region FIELD_VARIABLES
        #endregion

        #region FIELD_PROPERTIES
        #endregion
    }

    [Serializable]
    [DebuggerDisplay("X={X}, Y={Y}")]
    public class Point2d : Point, ISerializable
    {

        #region FIELD_CONSTRUCTORS
        public Point2d(double x, double y) : base()
        {
            _x = x;
            _y = y;
        }
        public Point2d(Point2d p) : base()
        {
            _x = p.X;
            _y = p.Y;
        }
        #endregion

        #region FIELD_DECONSTRUCTORS
        #endregion

        #region FIELD_COMMANDS
        #endregion

        #region FIELD_METHODS
        public void Move(double newX, double newY)
        {
            _x = newX;
            _y = newY;
        }

        public void Pan(double dX, double dY)
        {
            _x += dX;
            _y += dY;
        }

        public void Scale(double factor)
        {
            _x *= factor;
            _y *= factor;
        }

        /// <summary>
        /// Get the mirror point about the straight line defined by the equation ax + by + c = 0
        /// </summary>
        /// <param name="a">The 'a' parameter of the equation</param>
        /// <param name="b">The 'b' parameter of the equation</param>
        /// <param name="c">The 'c' parameter of the equation</param>
        /// <returns></returns>
        public Point2d Mirror(double a, double b, double c)
        {
            double k = -2 * (a * _x + b * _y + c) / (a * a + b * b);
            double x = k * a + _x;
            double y = k * b + _y;
            return new Point2d(x, y);
        }
        #endregion

        #region FIELD_OPERATORS
        public static Point2d operator +(Point2d a, Point2d b)
        {
            return new Point2d(a.X + b.X, a.Y + b.Y);
        }

        public static Point2d operator -(Point2d a, Point2d b)
        {
            return new Point2d(a.X - b.X, a.Y - b.Y);
        }

        public static double operator *(Point2d a, Point2d b)
        {
            return a.X * b.X + a.Y * b.Y;
        }

        public static Point2d operator *(Point2d a, double b)
        {
            return new Point2d(a.X * b, a.Y * b);
        }

        public static Point2d operator *(double a, Point2d b)
        {
            return b * a;
        }

        public static Point2d operator /(Point2d a, double b)
        {
            return a * (1 / b);
        }

        public static double operator ^(Point2d a, Point2d b)
        {
            return a.X * b.Y - a.Y * b.X;
        }
        #endregion


        #region FIELD_SERIALIZATION
        private static readonly int SERIALIZATION_RELEASE = 1;
        protected Point2d(SerializationInfo info, StreamingContext context)
        {
            _x = info.GetDouble("X");
            _y = info.GetDouble("Y");
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", SERIALIZATION_RELEASE);
            info.AddValue("X", _x);
            info.AddValue("Y", _y);
        }
        #endregion 

        #region FIELD_VARIABLES
        private double _x;
        private double _y;
        #endregion

        #region FIELD_PROPERTIES
        public static Point2d Origin = new Point2d(0, 0);
        public static Point2d XDir = new Point2d(1, 0);
        public static Point2d YDir = new Point2d(0, 1);
        public double X  => _x; 
        public double Y => _y; 
        #endregion
    }


    [Serializable]
    [DebuggerDisplay("X={X}, Y={Y}, Z={Z}")]
    public class Point3d : Point, ISerializable
    {

        #region FIELD_CONSTRUCTORS
        public Point3d(double x, double y, double z)
        {
            _x = x;
            _y = y;
            _z = z;
        }
        public Point3d(Point3d p) : this(p.X, p.Y, p.Z)
        { }
        #endregion

        #region FIELD_DECONSTRUCTORS
        #endregion

        #region FIELD_COMMANDS
        #endregion

        #region FIELD_METHODS
        public void Move(double newX, double newY, double newZ)
        {
            _x = newX;
            _y = newY;
            _z = newZ;
        }
        public void Pan(double dX, double dY, double dz)
        {
            _x += dX;
            _y += dY;
            _z += dz;
        }
        /// <summary>
        /// Get the mirror point about the plane defined by the equation ax + by + cz + d = 0
        /// </summary>
        /// <param name="a">The 'a' parameter of the equation</param>
        /// <param name="b">The 'b' parameter of the equation</param>
        /// <param name="c">The 'c' parameter of the equation</param>
        /// <param name="d">The 'd' parameter of the equation</param>
        /// <returns></returns>
        public Point3d Mirror(double a, double b, double c, double d)
        {
            double k = (-a * _x - b * _y - c * _z - d) / (a * a + b * b + c * c);
            double x2 = a * k + _x;
            double y2 = b * k + _y;
            double z2 = c * k + _z;
            double x3 = 2 * x2 - _x;
            double y3 = 2 * y2 - _y;
            double z3 = 2 * z2 - _z;
            return new Point3d(x3, y3, z3);
        }
        #endregion

        #region FIELD_OPERATORS

        public static Point3d operator +(Point3d a, Point3d b)
        {
            return new Point3d(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        }

        public static Point3d operator -(Point3d a, Point3d b)
        {
            return new Point3d(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        }

        public static double operator *(Point3d a, Point3d b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        public static Point3d operator *(Point3d a, double b)
        {
            return new Point3d(a.X * b, a.Y * b, a.Z * b);
        }

        public static Point3d operator *(double a, Point3d b)
        {
            return b * a;
        }

        public static Point3d operator /(Point3d a, double b)
        {
            return a * (1 / b);
        }

        public static Point3d operator ^(Point3d a, Point3d b)
        {
            return new Point3d(a.Y * b.Z - a.Z * b.Y, -(a.X * b.Z - a.Z * b.X), a.X * b.Y - a.Y * b.X);
        }
        #endregion 

        #region FIELD_SERIALIZATION
        private static int SERIALIZATION_RELEASE = 1;
        protected Point3d(SerializationInfo info, StreamingContext context)
        {
            _x = info.GetDouble("X");
            _y = info.GetDouble("Y");
            _z = info.GetDouble("Z");
        }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Version", SERIALIZATION_RELEASE);
            info.AddValue("X", _x);
            info.AddValue("Y", _y);
            info.AddValue("Z", _z);
        }
        #endregion 

        #region FIELD_VARIABLES
        protected double _x;
        protected double _y;
        protected double _z;
        #endregion

        #region FIELD_PROPERTIES
        public static Point3d Origin = new Point3d(0, 0, 0);
        public static Point3d XDir = new Point3d(1, 0, 0);
        public static Point3d YDir = new Point3d(0, 1, 0);
        public static Point3d ZDir = new Point3d(0, 0, 1);
        public double X => _x;
        public double Y => _y;
        public double Z => _z;
        #endregion

    }
}
