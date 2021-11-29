using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Utilities.Extensions;

namespace GPC.Model.Sections
{
    public abstract class ThinWallSection : Section
    {
        #region Variables

        protected ThinWall[] _thinWalls;
        protected Point2d[] _points;

        #endregion


        #region Public Constructors

        /// <summary>
        /// The default constructor of generic ThinWallSection
        /// </summary>
        /// <param name="material">The <see cref="Materials"/> of the section </param>
        /// <param name="name">The name of the section</param>
        internal ThinWallSection(Material material, string name)
            : base(material, name)
        {

        }

        protected ThinWallSection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _thinWalls = (ThinWall[])info.GetValue("ThinWall", typeof(ThinWall));
            _points = (Point2d[])info.GetValue("Points", typeof(Point2d));
        }

        #endregion

        #region Protected methods

        protected void SetThinWalls(ThinWall[] thinWalls, Point2d[] points)
        {

            _thinWalls = thinWalls ?? throw new ArgumentNullException(nameof(thinWalls));
            _points = points ?? throw new ArgumentNullException(nameof(points));

            if (thinWalls.Length != points.Length)
                throw new ArgumentException();

        }


        #endregion

        #region Public abstract method

        protected abstract override double CalculateJw();

        protected abstract override Point2d CalculateShearCenter();

        #endregion


        #region Public method


        protected override abstract Shape2d GetShape();


        /// <inheritdoc cref="Section.CalculateCentroid()"/>
        protected override Point2d CalculateCentroid()
        {
            double xSum = 0;
            double ySum = 0;
            double area = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
            {
                xSum += _thinWalls[i].Area * _points[i].X;
                ySum += _thinWalls[i].Area * _points[i].Y;
                area += _thinWalls[i].Area;
            }

            return new Point2d((xSum / area), (ySum / area));
        }

        protected override double CalculateJt()
        {
            double jt = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
            {
                jt += _thinWalls[i].CalculateJt();
            }

            return jt;
        }

        /// <summary>
        /// Calculate the first moment of inertia respect the X-axis (the Y-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        protected override double CalculateJ11()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJx(Centroid.Y - _points[i].Y);

            return j;
        }

        /// <summary>
        /// Calculate the first moment of inertia respect the Y-axis (the Z-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        protected override double CalculateJ22()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJy(Centroid.X - _points[i].X);

            return j;
        }

        protected override double CalculateArea()
        {
            double area = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                area += _thinWalls[i].Area;

            return area;
        }

        protected override double CalculateJxx()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJx(Centroid.Y - _points[i].Y);

            return j;
        }

        protected override double CalculateJyy()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJy(Centroid.X - _points[i].X);

            return j;
        }

        protected override double CalculateJxy()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJxy(Centroid.X - _points[i].X, Centroid.Y - _points[i].Y);

            return j;
        }

        protected abstract override double CalculateWpl1();

        protected abstract override double CalculateWpl2();

        protected abstract override double CalculateWel1();

        protected abstract override double CalculateWel2();

        internal Point2d[] GetSectionPoints()
        {
            List<Point2d> points = new List<Point2d>();

            for (int i = 0; i < _thinWalls.Length; i++)
            {
                List<Point2d> _pointBuffer = _thinWalls[i].GetPerimeter().Select(j => j).ToList();
                
                points.AddRange(_pointBuffer.Select(k => k.CloneAndMove(new Vector2d(_points[i].X, _points[i].Y))));
            }

            return points.ToArray();
        }


        #endregion

        #region Equals, hashcode, operators

        public override bool Equals(object obj)
        {

            return obj is ThinWallSection section &&
                   base.Equals(obj) &&
                   _thinWalls.SequenceEqual(section._thinWalls) &&
                   _points.SequenceEqual(section._points);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + _thinWalls.SequenceHashCode();
                hashCode = hashCode * -23 + _points.SequenceHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ThinWallSection left, ThinWallSection right)
        {
            return left.Equals(left);
        }

        public static bool operator !=(ThinWallSection left, ThinWallSection right)
        {
            return !(left == right);
        }

        #endregion

        #region Nested classes ThinWall

        protected class ThinWall
        {
            #region Variables

            private readonly double _t;
            private readonly double _l;
            private readonly double _angle;

            #endregion

            #region Protected constructor

            /// <summary>
            /// The default constructor of generic ThinWall
            /// </summary>
            /// <param name="length">The length of the ThinWall</param>
            /// <param name="thickness">The thickness of the ThinWall</param>
            /// <param name="angle">The angle of the ThinWall. 0 is orizontal, Math.PI / 2.0 is vertical</param>
            internal ThinWall(double length, double thickness, double angle)
                : base()
            {
                _t = thickness < 0 ? throw new ArgumentException($"Thickness cannot be lower than zero") : thickness;
                _l = length < 0 ? throw new ArgumentException($"Lenght cannot be lower than zero") : length;
                _angle = angle;
            }

            #endregion

            #region Properties

            /// <summary>
            /// The thickness of the wall
            /// </summary>
            internal double T => _t;

            /// <summary>
            /// The lenght of the wall
            /// </summary>
            internal double L => _l;

            /// <summary>
            /// The angle of rotation of the principal axis. Angle = 0 is the Y-axis, 90° is the Z-axis
            /// </summary>
            internal double Angle => _angle;

            /// <summary>
            /// The area og the thin wal
            /// </summary>
            public double Area => CalculateArea();

            #endregion

            #region Internal method

            internal Polygon2d GetPerimeter()
            {
                return new Polygon2d(new Point2d[] {new Point2d(_l / 2.0 * Math.Cos(_angle) + _t / 2.0 * Math.Sin(_angle), _l / 2.0 * Math.Sin(_angle) + _t / 2.0 * Math.Cos(_angle)),
                    new Point2d(_l / 2.0 * Math.Cos(_angle) - _t / 2.0 * Math.Sin(_angle), _l / 2.0 * Math.Sin(_angle) - _t / 2.0 * Math.Cos(_angle)),
                    new Point2d(- _l / 2.0 * Math.Cos(_angle) + _t / 2.0 * Math.Sin(_angle), - _l / 2.0 * Math.Sin(_angle) + _t / 2.0 * Math.Cos(_angle)),
                    new Point2d(- _l / 2.0 * Math.Cos(_angle) - _t / 2.0 * Math.Sin(_angle), - _l / 2.0 * Math.Sin(_angle) - _t / 2.0 * Math.Cos(_angle))
                    });
            }

            /// <summary>
            /// Calculate the area of the wall 
            /// </summary>
            /// <returns></returns>
            internal double CalculateArea()
            {
                return _t * _l;
            }

            /// <summary>
            /// Calculate the first moment of inertia of the wall respect the X-axis passing throw the <paramref name="point"/>
            /// </summary>
            /// <returns></returns>
            internal double CalculateJx(Point2d point)
            {
                return CalculateJx() + CalculateArea() * Math.Pow((point.Y), 2);
            }

            /// <summary>
            /// Calculate the first moment of inertia of the wall respect the Y-axis passing throw the <paramref name="point"/>
            /// </summary>
            /// <returns></returns>
            internal double CalculateJy(Point2d point)
            {
                return CalculateJy() + CalculateArea() * Math.Pow((point.X), 2);
            }

            internal double CalculateJx(double distance)
            {
                return CalculateJx() + CalculateArea() * Math.Pow(distance, 2);
            }

            internal double CalculateJy(double distance)
            {
                return CalculateJy() + CalculateArea() * Math.Pow(distance, 2);
            }

            internal double CalculateJxy()
            {
                double Jx = _t * Math.Pow(_l, 3) / 12.0;
                double Jy = _l * Math.Pow(_t, 3) / 12.0;

                return (Jx - Jy) / 2.0 * Math.Sin(2.0 * _angle);
            }

            internal double CalculateJxy(Point2d point)
            {
                double Jx = _t * Math.Pow(_l, 3) / 12.0;
                double Jy = _l * Math.Pow(_t, 3) / 12.0;

                return (Jx - Jy) / 2.0 * Math.Sin(2.0 * _angle) + point.X * point.Y * Area;
            }

            internal double CalculateJxy(double distanceX, double distanceY)
            {
                double Jx = _t * Math.Pow(_l, 3) / 12.0;
                double Jy = _l * Math.Pow(_t, 3) / 12.0;

                return (Jx - Jy) / 2.0 * Math.Sin(2.0 * _angle) + distanceX * distanceY * Area;
            }

            internal double CalculateJy()
            {
                if (Math.Abs(_angle) < GeometryBase.GetDefaultAngularTolerance() || Math.Abs(_angle - Math.PI) < GeometryBase.GetDefaultAngularTolerance())
                    return _t * Math.Pow(_l, 3) / 12.0;

                else if (Math.Abs(_angle - Math.PI / 2) < GeometryBase.GetDefaultAngularTolerance())
                    return _l * Math.Pow(_t, 3) / 12.0;

                else
                {
                    double Jx = _t * Math.Pow(_l, 3) / 12.0;
                    double Jy = _l * Math.Pow(_t, 3) / 12.0;

                    return (Jx + Jy) / 2.0 - (Jx - Jy) / 2.0 * Math.Cos(2.0 * _angle);
                }
            }

            internal double CalculateJx()
            {
                if (Math.Abs(_angle) < GeometryBase.GetDefaultAngularTolerance() || Math.Abs(_angle - Math.PI) < GeometryBase.GetDefaultAngularTolerance())
                    return _l * Math.Pow(_t, 3) / 12.0;

                else if (Math.Abs(_angle - Math.PI / 2) < GeometryBase.GetDefaultAngularTolerance())
                    return _t * Math.Pow(_l, 3) / 12.0;

                else
                {
                    double Jx = _t * Math.Pow(_l, 3) / 12.0;
                    double Jy = _l * Math.Pow(_t, 3) / 12.0;

                    return (Jx + Jy) / 2.0 + (Jx - Jy) / 2.0 * Math.Cos(2.0 * _angle);
                }
            }

            internal virtual double CalculateJt()
            {
                return L * Math.Pow(T, 3) / GetAlpha();
            }

            internal virtual double CalculateJw()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Calculate the polar moment of inertia 
            /// </summary>
            /// <returns></returns>
            internal virtual double CalculateJpolar()
            {
                return CalculateJx() + CalculateJy();
            }

            internal double GetAlpha()
            {
                return 3 + 1.8 * T / L;
            }

            #endregion

        }

        #endregion
    }
}
