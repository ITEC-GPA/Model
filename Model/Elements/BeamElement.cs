using GPC.Geometry;
using GPC.Model.ElementProperties;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// A beam element: a segment with a section (<see cref="BeamProperty"/>) and a rotation of the section around the axis
    /// </summary>
    [Serializable]
    public class BeamElement : Element, ISerializable, IEquatable<BeamElement>
    {
        #region Variables

        /// <summary>
        /// The start point
        /// </summary>
        protected Point3d _startNode;
        /// <summary>
        /// The end point
        /// </summary>
        protected Point3d _endNode;
        /// <summary>
        /// The section
        /// </summary>
        protected BeamProperty _beamProperty;
        /// <summary>
        /// The rotation of the section around the axis of the beam, in radians
        /// </summary>
        protected double _axisAngleRadians;

        #endregion

        #region Properties

        /// <summary>
        /// The start point
        /// </summary>
        public Point3d StartPoint { get => _startNode; set => _startNode = value; }

        /// <summary>
        /// The end point
        /// </summary>
        public Point3d EndPoint { get => _endNode; set => _endNode = value; }

        /// <summary>
        /// A new segment from the start to the end point
        /// </summary>
        public Line3d Line { get => new Line3d(StartPoint, EndPoint); }

        /// <summary>
        /// The section
        /// </summary>
        public BeamProperty BeamProperty { get => _beamProperty; set => _beamProperty = value; }

        /// <summary>
        /// The rotation of the section around the axis of the beam, in radians (not serialized)
        /// </summary>
        public double RotationAroundFirstAxis { get => _axisAngleRadians; set => _axisAngleRadians = value; }

        /// <summary>
        /// The length of the beam
        /// </summary>
        public double Length => Line.Length;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a beam (the point instances are kept)
        /// </summary>
        /// <param name="startPoint">The start point</param>
        /// <param name="endPoint">The end point</param>
        /// <param name="beamProperty">The section</param>
        /// <param name="coordinateSystem">The local coordinate system; null: the global one</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public BeamElement(Point3d startPoint, Point3d endPoint, BeamProperty beamProperty, CoordinateSystem coordinateSystem = null, string name = "", int id = IDUNASSIGNED)
            : base(id, name, coordinateSystem)
        {
            _startNode = startPoint;
            _endNode = endPoint;
            _beamProperty = beamProperty;
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Element"/>, the version, the points and the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected BeamElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version = info.GetInt32("BeamVersion");

            _startNode = (Point3d)info.GetValue("StartNode", typeof(Point3d));
            _endNode = (Point3d)info.GetValue("EndNode", typeof(Point3d));
            _beamProperty = (BeamProperty)info.GetValue("BeamProperty", typeof(BeamProperty));
        }

        #endregion

        #region Equals, hascode, operators

        /// <summary>
        /// Equality with another beam (see the implementation of <see cref="IEquatable{BeamElement}"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal beam</returns>
        /// <remarks>It calls this same method (the implementation of the interface is explicit): infinite recursion (see the list of the defects found)</remarks>
        public override bool Equals(object obj)
        {
            return Equals(obj as BeamElement);
        }

        /// <summary>
        /// Equality of the points and of the sections
        /// </summary>
        /// <param name="other">The beam to compare</param>
        /// <returns>True if the beams have the same points and section</returns>
        bool IEquatable<BeamElement>.Equals(BeamElement other)
        {
            return !(other is null) &&
                _startNode.Equals(other._startNode) &&
                _endNode.Equals(other._endNode) &&
                _beamProperty == other.BeamProperty;
        }

        /// <summary>
        /// Serializes the data of <see cref="Element"/>, the version, the points and the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 0;
            info.AddValue("BeamVersion", version);
            info.AddValue("EndNode", _endNode);
            info.AddValue("StartNode", _startNode);
            info.AddValue("BeamProperty", _beamProperty);
        }

        /// <summary>
        /// The hash code of the name, of the coordinate system, of the exact points and of the section
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _endNode.GetHashCode();
                hashCode = hashCode * -17 + _startNode.GetHashCode();
                hashCode = hashCode * -17 + _beamProperty.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null beams are equal
        /// </summary>
        /// <param name="left">The first beam</param>
        /// <param name="right">The second beam</param>
        /// <returns>True if the beams are equal</returns>
        public static bool operator ==(BeamElement left, BeamElement right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first beam</param>
        /// <param name="right">The second beam</param>
        /// <returns>True if the beams are different</returns>
        public static bool operator !=(BeamElement left, BeamElement right)
        {
            return !(left == right);
        }

        #endregion
    }
}
