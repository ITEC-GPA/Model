using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    /// <summary>
    /// A pressure varying over a planar triangle or quadrilateral (e.g. hydrostatic or earth pressure): one value at each vertex, linear over the
    /// triangle and bilinear (isoparametric) over the quadrilateral, acting along the Z axis of the coordinate system of the load. The vertices are
    /// a copy of the global points at creation, so the values keep their physical positions when the element connectivity is reordered
    /// </summary>
    [Serializable]
    public class NonUniformPlatePressure : Load, IAreaLoad
    {
        #region Variables

        /// <summary>
        /// The vertices, in the global coordinates
        /// </summary>
        protected Point3d[] _vertices;
        /// <summary>
        /// The pressure at each vertex (positive along the Z axis of the coordinate system)
        /// </summary>
        protected double[] _pressures;

        #endregion

        #region Properties

        /// <summary>
        /// The vertices, in the global coordinates (copies)
        /// </summary>
        public IReadOnlyList<Point3d> Vertices => _vertices.Select(p => new Point3d(p.X, p.Y, p.Z)).ToArray();

        /// <summary>
        /// The pressure at each vertex, in the order of <see cref="Vertices"/> (positive along the Z axis of the coordinate system)
        /// </summary>
        public IReadOnlyList<double> Pressures => (double[])_pressures.Clone();

        #endregion

        #region Public constructors

        /// <summary>
        /// Creates the pressure
        /// </summary>
        /// <param name="vertices">Three or four coplanar vertices of a convex area, in the global coordinates (copied)</param>
        /// <param name="pressures">The pressure at each vertex (copied)</param>
        /// <param name="loadCase">The load case</param>
        /// <param name="coordinateSystem">The direction of the pressure is its Z axis</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentException">If the vertices, the values or the coordinate system are missing, not finite or not consistent</exception>
        public NonUniformPlatePressure(IReadOnlyList<Point3d> vertices, IReadOnlyList<double> pressures, LoadCaseBase loadCase, CoordinateSystem coordinateSystem,
            string name = "", int id = IDUNASSIGNED)
            : base(loadCase, coordinateSystem, name, id)
        {
            if (vertices == null || pressures == null || (vertices.Count != 3 && vertices.Count != 4) || pressures.Count != vertices.Count)
                throw new ArgumentException("Three or four vertices with one pressure each are required.");
            if (coordinateSystem == null) throw new ArgumentNullException(nameof(coordinateSystem));
            _vertices = vertices.Select(p => p == null ? throw new ArgumentException("Missing vertex.") : new Point3d(p.X, p.Y, p.Z)).ToArray();
            _pressures = pressures.ToArray();
            Validate();
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Load"/>, the vertices and the pressures
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected NonUniformPlatePressure(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _vertices = (Point3d[])info.GetValue("Vertices", typeof(Point3d[]));
            _pressures = (double[])info.GetValue("Pressures", typeof(double[]));
        }

        #endregion

        #region Methods

        /// <summary>
        /// The area of the load
        /// </summary>
        /// <returns>A new shape on copies of the vertices</returns>
        public Shape GetGeometry() => new Shape(new Polygon3d(Vertices));

        /// <summary>
        /// The area of the load (see <see cref="GetGeometry"/>)
        /// </summary>
        /// <returns>The shape</returns>
        public override GeometryBase GetGeometryBase() => GetGeometry();

        /// <summary>
        /// The pressure integrated over the area (force along the Z axis of the coordinate system)
        /// </summary>
        /// <returns>The integral of the pressure</returns>
        public double GetTotalPressure() => Integrate(new Point3d(0, 0, 0)).Force;

        /// <summary>
        /// The total load in the global system
        /// </summary>
        /// <returns>The resultant force</returns>
        public Vector3d GetGlobalLoadVector() => Direction() * GetTotalPressure();

        /// <summary>
        /// The exact resultant in the global system: Gauss integration of degree 3 (three mid-edge points on the triangle, 2 x 2 points on the
        /// quadrilateral), exact for the linear/bilinear pressure, the area and the moment arms of a planar element
        /// </summary>
        /// <param name="pole">The pole of the moment</param>
        /// <returns>The force and the moment about <paramref name="pole"/></returns>
        public (Vector3d Force, Vector3d Moment) GetGlobalResultant(Point3d pole)
        {
            if (pole == null) throw new ArgumentNullException(nameof(pole));
            var (total, firstMoment) = Integrate(pole);
            var direction = Direction();
            return (direction * total, firstMoment.CrossProduct(direction));
        }

        /// <summary>
        /// Integral of the pressure (scalar) and of the pressure times the arm from the pole (vector)
        /// </summary>
        private (double Force, Vector3d Arm) Integrate(Point3d pole)
        {
            Validate();
            double total = 0; var arm = new Vector3d(0, 0, 0);
            void Add(double weight, double[] n)
            {
                double p = 0, x = 0, y = 0, z = 0;
                for (int i = 0; i < n.Length; i++) { p += n[i] * _pressures[i]; x += n[i] * _vertices[i].X; y += n[i] * _vertices[i].Y; z += n[i] * _vertices[i].Z; }
                total += weight * p; arm = arm + new Vector3d(x - pole.X, y - pole.Y, z - pole.Z) * (weight * p);
            }
            if (_vertices.Length == 3)
            {
                double area = Normal(_vertices[0], _vertices[1], _vertices[2]).Length / 2;
                Add(area / 3, new[] { .5, .5, 0 }); Add(area / 3, new[] { 0, .5, .5 }); Add(area / 3, new[] { .5, 0, .5 });
            }
            else
            {
                double g = 1 / Math.Sqrt(3);
                foreach (var xi in new[] { -g, g })
                    foreach (var eta in new[] { -g, g })
                    {
                        // Bilinear map on vertices 1..4 at (-1,-1), (1,-1), (1,1), (-1,1); weight = |dx/dxi x dx/deta|.
                        var n = new[] { (1 - xi) * (1 - eta) / 4, (1 + xi) * (1 - eta) / 4, (1 + xi) * (1 + eta) / 4, (1 - xi) * (1 + eta) / 4 };
                        var dxi = Combine(new[] { -(1 - eta) / 4, (1 - eta) / 4, (1 + eta) / 4, -(1 + eta) / 4 });
                        var deta = Combine(new[] { -(1 - xi) / 4, -(1 + xi) / 4, (1 + xi) / 4, (1 - xi) / 4 });
                        Add(dxi.CrossProduct(deta).Length, n);
                    }
            }
            return (total, arm);
        }

        private Vector3d Combine(double[] weights)
        {
            double x = 0, y = 0, z = 0;
            for (int i = 0; i < weights.Length; i++) { x += weights[i] * _vertices[i].X; y += weights[i] * _vertices[i].Y; z += weights[i] * _vertices[i].Z; }
            return new Vector3d(x, y, z);
        }

        private static Vector3d Normal(Point3d a, Point3d b, Point3d c) =>
            new Vector3d(b.X - a.X, b.Y - a.Y, b.Z - a.Z).CrossProduct(new Vector3d(c.X - a.X, c.Y - a.Y, c.Z - a.Z));

        private Vector3d Direction()
        {
            var z = _coordinateSystem.V3; double length = z.Length;
            return new Vector3d(z.X / length, z.Y / length, z.Z / length);
        }

        private void Validate()
        {
            if (_vertices == null || _pressures == null || (_vertices.Length != 3 && _vertices.Length != 4) || _pressures.Length != _vertices.Length)
                throw new InvalidOperationException("Inconsistent vertices and pressures.");
            if (_vertices.Any(p => !Finite(p.X) || !Finite(p.Y) || !Finite(p.Z)) || _pressures.Any(p => !Finite(p)))
                throw new ArgumentException("Vertices and pressures must be finite.");
            if (_coordinateSystem?.V3 == null || !(_coordinateSystem.V3.Length > 0)) throw new ArgumentException("A coordinate system with a Z axis is required.");
            var normal = Normal(_vertices[0], _vertices[1], _vertices[2]); double size = normal.Length;
            if (!(size > 0)) throw new ArgumentException("Degenerate pressure area.");
            normal = normal / size;
            for (int i = 0; i < _vertices.Length; i++)
            {
                var a = _vertices[i]; var b = _vertices[(i + 1) % _vertices.Length]; var c = _vertices[(i + 2) % _vertices.Length];
                var turn = Normal(a, b, c);
                if (turn.X * normal.X + turn.Y * normal.Y + turn.Z * normal.Z <= 0) throw new ArgumentException("The pressure area must be convex.");
            }
            if (_vertices.Length == 4)
            {
                var d = new Vector3d(_vertices[3].X - _vertices[0].X, _vertices[3].Y - _vertices[0].Y, _vertices[3].Z - _vertices[0].Z);
                double scale = Math.Max(1, new Vector3d(_vertices[2].X - _vertices[0].X, _vertices[2].Y - _vertices[0].Y, _vertices[2].Z - _vertices[0].Z).Length);
                if (Math.Abs(d.X * normal.X + d.Y * normal.Y + d.Z * normal.Z) > 1e-7 * scale) throw new ArgumentException("The pressure area must be planar.");
            }
        }

        private static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);

        #endregion

        #region Equals, HasCode and operators

        /// <summary>
        /// Serializes the base load, the vertices and the pressures
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Vertices", _vertices, typeof(Point3d[]));
            info.AddValue("Pressures", _pressures, typeof(double[]));
        }

        /// <summary>
        /// Equality of name, load case, coordinate system, vertices and pressures (exact)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal pressure</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return obj is NonUniformPlatePressure other &&
                _pressures.SequenceEqual(other._pressures) &&
                _vertices.Length == other._vertices.Length &&
                _vertices.Zip(other._vertices, (a, b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z).All(same => same) &&
                base.Equals(other);
        }

        /// <summary>
        /// The hash code of name, load case, coordinate system and pressures
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -29;
                hashCode = hashCode * -17 + base.GetHashCode();
                foreach (var p in _pressures) hashCode = hashCode * -17 + p.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null loads are equal
        /// </summary>
        /// <param name="obj1">The first load</param>
        /// <param name="obj2">The second load</param>
        /// <returns>True if the loads are equal</returns>
        public static bool operator ==(NonUniformPlatePressure obj1, NonUniformPlatePressure obj2)
        {
            if (obj1 is null)
                return obj2 is null;
            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first load</param>
        /// <param name="obj2">The second load</param>
        /// <returns>True if the loads are different</returns>
        public static bool operator !=(NonUniformPlatePressure obj1, NonUniformPlatePressure obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
