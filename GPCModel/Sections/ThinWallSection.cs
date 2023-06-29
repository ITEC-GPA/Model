using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    public enum EdgeType
    {
        Sharp = 0, // Without working, simple corner. Default.
        Fillet = 1, // Rounded corners with circumference arc.
        Chamfer = 2 // Straight line.
    }

    /// <summary>
    /// List of rectangular thin wall.
    /// Useful for approximating thin steel profiles. They do not use fillets between wall elements.
    /// 2023-06-20: Refactoring, moved points from ThinWallSection to ThinWall. Added version=2 in serialization.
    /// </summary>
    [Serializable]
    public abstract class ThinWallSection : Section, ISerializable
    {
        #region Variables

        protected ThinWall[] _thinWalls;
        protected EdgeType _edgeWorking;

        #endregion

        #region Properties

        public override double Height { get; set; }

        public override ThinWall[] ThinWalls => _thinWalls;

        /// <summary>
        /// Edge workings, used to define the type of workings for inside corners.
        /// For steel, EdgeType.Chamfer can be used to represent welds
        /// or EdgeType.Fillet for simple arc fillets.
        /// </summary>
        internal EdgeType EdgeWorking => _edgeWorking;

        #endregion

        #region Public Constructors

        /// <summary>
        /// The default constructor of generic ThinWallSection
        /// </summary>
        /// <param name="name">The name of the section</param>
        internal ThinWallSection(string name)
            : base(name)
        {
        }

        protected ThinWallSection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("ThinWallSectionVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            if (version == 1)
            {
                var thinWallsWithoutPoint = (ThinWall[])info.GetValue("ThinWalls", typeof(ThinWall[]));
                var points = (Point2d[])info.GetValue("Points", typeof(Point2d[]));

                if (thinWallsWithoutPoint.Length != points.Length)
                    throw new ArgumentException("Different lenght between thin walls and points.");

                _thinWalls = new ThinWall[thinWallsWithoutPoint.Length];
                for (int i = 0; i < thinWallsWithoutPoint.Length; i++)
                    _thinWalls[i] = new ThinWall(thinWallsWithoutPoint[i].L, thinWallsWithoutPoint[i].T, thinWallsWithoutPoint[i].Angle, points[i]);
            }
            else if (version == 2)
            {
                _thinWalls = (ThinWall[])info.GetValue("ThinWalls", typeof(ThinWall[]));
            }
        }

        #endregion

        #region Protected methods

        protected void SetThinWalls(ThinWall[] thinWalls)
        {
            _thinWalls = thinWalls ?? throw new ArgumentNullException(nameof(thinWalls));
        }

        public override void SetEdgeTypeFromSteelType(SectionTypes sectionType)
        {
            if (sectionType == SectionTypes.Rolled)
                _edgeWorking = EdgeType.Fillet;
            else if (sectionType == SectionTypes.Welded)
                _edgeWorking = EdgeType.Chamfer;
        }

        #endregion

        #region Mesh

        public Mesh GetMesh()
        {
            Mesh mesh = new Mesh();

            for (int i = 0; i < _thinWalls.Count(); i++)
            {
                Polygon2d a = ((Polygon2d)_thinWalls[i].GetPerimeter().Clone());
                Point3d[] ps = new Point3d[a.Count];
                for (int j = 0; j < ps.Length; j++)
                    ps[j] = new Point2d(a[j].X, a[j].Y);
                mesh.AddFaceMesh(ps);
            }

            return mesh;
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
                xSum += _thinWalls[i].CalculateSy();
                ySum += _thinWalls[i].CalculateSx();
                area += _thinWalls[i].Area;
            }

            return new Point2d(xSum / area, ySum / area);
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
        protected override double CalculateJ11() => SectionHelper.CalculateJ11(_jxx, _jyy, _jxy);

        /// <summary>
        /// Calculate the first moment of inertia respect the Y-axis (the Z-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        protected override double CalculateJ22() => SectionHelper.CalculateJ22(_jxx, _jyy, _jxy);

        private double CalculateAreaThinWallSection()
        {
            double area = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                area += _thinWalls[i].Area;

            return area;
        }

        protected override double CalculateArea()
        {
            return CalculateAreaThinWallSection();
        }

        /// <summary>
        /// Moment of inertia with respect to the X axis passing through the center of gravity
        /// of the section. Contributions to the moment of inertia only thin walls.
        /// </summary>
        /// <returns></returns>
        private double CalculateJxxThinWall()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJx();

            j -= CalculateAreaThinWallSection() * _centroid.Y * _centroid.Y;

            return j;
        }

        protected override double CalculateJxx() => CalculateJxxThinWall();

        /// <summary>
        /// Moment of inertia with respect to the Y axis passing through the center of gravity
        /// of the section. Contributions to the moment of inertia only thin walls.
        /// </summary>
        /// <returns></returns>
        private double CalculateJyyThinWall()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJy();

            j -= CalculateAreaThinWallSection() * _centroid.X * _centroid.X;

            return j;
        }

        protected override double CalculateJyy() => CalculateJyyThinWall();

        /// <summary>
        /// Product of inertia with respect to the X and Y axes passing through the center of
        /// gravity of the section. Contributions to the moment of inertia only thin walls.
        /// </summary>
        /// <returns></returns>
        protected override double CalculateJxy()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
                j += _thinWalls[i].CalculateJxy();

            j -= CalculateAreaThinWallSection() * _centroid.X * _centroid.Y;

            return j;
        }

        protected abstract override double CalculateWpl1();

        protected abstract override double CalculateWpl2();

        protected abstract override double CalculateWel1Max();

        protected abstract override double CalculateWel1Min();

        protected abstract override double CalculateWel2Max();

        protected abstract override double CalculateWel2Min();

        public override Point2d[] GetSectionPoints()
        {
            List<Point2d> points = new List<Point2d>();

            for (int i = 0; i < _thinWalls.Length; i++)
            {
                List<Point2d> _pointBuffer = _thinWalls[i].GetPerimeter().Select(j => j).ToList();

                points.AddRange(_pointBuffer.Select(k => (Point2d)k.Clone()));
            }

            return points.ToArray();
        }

        #endregion

        #region Equals, hashcode, operators

        public override bool Equals(object obj)
        {

            return obj is ThinWallSection section &&
                   base.Equals(obj) &&
                   _thinWalls.SequenceEqual(section._thinWalls);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + _thinWalls.GetHashCodeSequence();
                return hashCode;
            }
        }

        public static bool operator ==(ThinWallSection left, ThinWallSection right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ThinWallSection left, ThinWallSection right)
        {
            return !(left == right);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            int version = 2;
            info.AddValue("ThinWallSectionVersion", version);
            info.AddValue("ThinWalls", _thinWalls, typeof(ThinWall[]));
        }

        #endregion

        #region Nested classes ThinWall

        /// With _angle = 0:
        ///    ┌-----------------┐
        /// _t |                 |
        ///    └-----------------┘
        ///            _l
        /// </summary>
        [Serializable]
        public class ThinWall
        {
            #region Variables

            private readonly double _t;
            private readonly double _l;
            private readonly double _angle;
            private readonly Point2d _point;

            #endregion

            #region Properties

            /// <summary>
            /// The _thickness of the wall
            /// </summary>
            internal double T => _t;

            /// <summary>
            /// The lenght of the wall
            /// </summary>
            internal double L => _l;

            /// <summary>
            /// The angle of rotation of the principal axis. Angle = 0 is the X-axis, PI.GRECO/2 is the y-axis
            /// Angle in radians, counterclockwise, is zero for the x-positive direction.
            /// </summary>
            internal double Angle => _angle;

            /// <summary>
            /// The position of the thin wall, the center of gravity point.
            /// </summary>
            internal Point2d Point => _point;

            /// <summary>
            /// The area og the thin wal
            /// </summary>
            public double Area => CalculateArea();

            #endregion

            #region Protected constructor

            /// <summary>
            /// Older version of the constructor, kept for compatibility.
            /// </summary>
            /// <param name="length"></param>
            /// <param name="thickness"></param>
            /// <param name="angle"></param>
            internal ThinWall(double length, double thickness, double angle)
                : this(length, thickness, angle, Point2d.Origin)
            {
            }

            /// <summary>
            /// The default constructor of generic ThinWall
            /// </summary>
            /// <param name="length">The length of the ThinWall</param>
            /// <param name="thickness">The _thickness of the ThinWall</param>
            /// <param name="angle">The angle of the ThinWall. 0 is orizontal, Math.PI / 2.0 is vertical</param>
            /// <param name="point">Position, barycenter/centroid of rectangular.</param>
            internal ThinWall(double length, double thickness, double angle, Point2d point)
                : base()
            {
                _t = thickness < 0 ? throw new ArgumentException($"Thickness cannot be lower than zero") : thickness;
                _l = length < 0 ? throw new ArgumentException($"Lenght cannot be lower than zero") : length;
                _angle = angle;
                _point = point ?? throw new ArgumentException($"Position cannot be null.");
            }

            protected ThinWall(SerializationInfo info, StreamingContext _)
            {
                _t = info.GetDouble("T");
                _l = info.GetDouble("L");
                _angle = info.GetDouble("Angle");
                try
                {
                    _point = (Point2d)info.GetValue("Point", typeof(Point2d));
                }
                catch (Exception)
                {
                    _point = Point2d.Origin;
                }
            }

            #endregion

            #region Internal method

            internal Polygon2d GetPerimeter()
            {
                var sinAngle = Math.Sin(_angle);
                var cosAngle = Math.Cos(_angle);
                var lHalf = _l / 2.0;
                var tHalf = _t / 2.0;

                var poly = new Polygon2d(new Point2d[] {
                    new Point2d(- lHalf * cosAngle - tHalf * sinAngle, - lHalf * sinAngle - tHalf * cosAngle),
                    new Point2d(lHalf * cosAngle - tHalf * sinAngle, lHalf * sinAngle - tHalf * cosAngle),
                    new Point2d(lHalf * cosAngle + tHalf * sinAngle, lHalf * sinAngle + tHalf * cosAngle),
                    new Point2d(- lHalf * cosAngle + tHalf * sinAngle, - lHalf * sinAngle + tHalf * cosAngle)
                    });
                poly.Move(_point.X, _point.Y);
                return poly;
            }

            internal Point2d[] GetMiddleLine()
            {
                var sinAngle = Math.Sin(_angle);
                var cosAngle = Math.Cos(_angle);
                var lHalf = _l / 2.0;
                return new Point2d[] {  _point + new Point2d(-lHalf * cosAngle, -lHalf * sinAngle),
                                        _point + new Point2d(lHalf * cosAngle, lHalf * sinAngle) };
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
            /// Moment of inertia with respect to the X and Y axes.
            /// </summary>
            /// <returns></returns>
            internal double CalculateJxy()
            {
                double Jxx = _l * Math.Pow(_t, 3) / 12.0;
                double Jyy = _t * Math.Pow(_l, 3) / 12.0;
                double Jxy = 0.0;

                return SectionHelper.CalculateJxyAlpha(Jxx, Jyy, Jxy, -_angle) + _point.X * _point.Y * Area;
            }

            /// <summary>
            /// Moment of inertia with respect to the Y-axis.
            /// </summary>
            /// <returns></returns>
            internal double CalculateJy()
            {
                double momentTranslation = Area * _point.X * _point.X;
                if (Math.Abs(_angle) < GeometryBase.GetDefaultAngularTolerance() || Math.Abs(_angle - Math.PI) < GeometryBase.GetDefaultAngularTolerance())
                    return momentTranslation + _t * Math.Pow(_l, 3) / 12.0;

                else if (Math.Abs(_angle - Math.PI / 2) < GeometryBase.GetDefaultAngularTolerance())
                    return momentTranslation + _l * Math.Pow(_t, 3) / 12.0;

                else
                {
                    double Jxx = _l * Math.Pow(_t, 3) / 12.0;
                    double Jyy = _t * Math.Pow(_l, 3) / 12.0;
                    double Jxy = 0.0;
                    return momentTranslation + SectionHelper.CalculateJAlpha(Jxx, Jyy, Jxy, -_angle + 0.5 * Math.PI);
                }
            }

            /// <summary>
            /// Moment of inertia with respect to the X-axis.
            /// </summary>
            /// <returns></returns>
            internal double CalculateJx()
            {
                double momentTranslation = Area * _point.Y * _point.Y;
                if (Math.Abs(_angle) < GeometryBase.GetDefaultAngularTolerance() || Math.Abs(_angle - Math.PI) < GeometryBase.GetDefaultAngularTolerance())
                    return momentTranslation + _l * Math.Pow(_t, 3) / 12.0;

                else if (Math.Abs(_angle - Math.PI / 2) < GeometryBase.GetDefaultAngularTolerance())
                    return momentTranslation + _t * Math.Pow(_l, 3) / 12.0;

                else
                {
                    double Jxx = _l * Math.Pow(_t, 3) / 12.0;
                    double Jyy = _t * Math.Pow(_l, 3) / 12.0;
                    double Jxy = 0.0;
                    return momentTranslation + SectionHelper.CalculateJAlpha(Jxx, Jyy, Jxy, -_angle);
                }
            }

            /// <summary>
            /// Static moment with respect to X-axis.
            /// </summary>
            /// <returns></returns>
            internal double CalculateSx() => Area * _point.Y;

            /// <summary>
            /// Static moment with respect to Y-axis.
            /// </summary>
            /// <returns></returns>
            internal double CalculateSy() => Area * _point.X;

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

            public override bool Equals(object obj)
            {
                return obj is ThinWall wall &&
                       _t == wall._t &&
                       _l == wall._l &&
                       _angle == wall._angle &&
                       _point == wall._point;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    int hashCode = -23;
                    hashCode = hashCode * -17 + _t.GetHashCode();
                    hashCode = hashCode * -17 + _l.GetHashCode();
                    hashCode = hashCode * -17 + _angle.GetHashCode();
                    hashCode = hashCode * -17 + _point.GetHashCode();
                    return hashCode;
                }
            }

            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("T", _t);
                info.AddValue("L", _l);
                info.AddValue("Angle", _angle);
                info.AddValue("Point", _point);
            }

            #endregion
        }

        #endregion
    }
}
