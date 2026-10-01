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
        public GPC.Model.PostProcessing.BeamAssignments Assignments { get; private set; } = new GPC.Model.PostProcessing.BeamAssignments();
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
        private NodeElement _nodeI;
        private NodeElement _nodeJ;
        public NodeElement NodeI => _nodeI;
        public NodeElement NodeJ => _nodeJ;

        #endregion

        #region Properties

        /// <summary>
        /// The start point
        /// </summary>
        public Point3d StartPoint { get => _nodeI is null ? _startNode : _nodeI.Position; set { if (_nodeI is null) _startNode = value; else _nodeI.Position = value; } }

        /// <summary>
        /// The end point
        /// </summary>
        public Point3d EndPoint { get => _nodeJ is null ? _endNode : _nodeJ.Position; set { if (_nodeJ is null) _endNode = value; else _nodeJ.Position = value; } }

        /// <summary>
        /// A new segment from the start to the end point
        /// </summary>
        public Line3d Line { get => new Line3d(StartPoint, EndPoint); }

        /// <summary>
        /// The section
        /// </summary>
        public BeamProperty BeamProperty { get => _beamProperty; set => _beamProperty = value; }

        /// <summary>
        /// The legacy scalar rotation in radians; SectionAxes carries the authoritative full section orientation.
        /// </summary>
        public double RotationAroundFirstAxis { get => _axisAngleRadians; set => _axisAngleRadians = NumericGuard.Finite(value, nameof(value)); }

        /// <summary>
        /// The length of the beam
        /// </summary>
        public double Length => Line.Length;

        #endregion

        #region Constructor

        /// <summary>Connects existing registry nodes explicitly. No coordinate matching is performed.</summary>
        public void ConnectNodes(NodeElement nodeI, NodeElement nodeJ)
        {
            if (nodeI is null) throw new ArgumentNullException(nameof(nodeI));
            if (nodeJ is null) throw new ArgumentNullException(nameof(nodeJ));
            if (ReferenceEquals(nodeI, nodeJ)) throw new ArgumentException("Two distinct nodes are required.");
            _nodeI = nodeI;
            _nodeJ = nodeJ;
            _startNode = null;
            _endNode = null;
        }

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
            Assignments = SerializationFields.Read(info, "BeamAssignments", new GPC.Model.PostProcessing.BeamAssignments());
            int version = info.GetInt32("BeamVersion");

            _startNode = (Point3d)info.GetValue("StartNode", typeof(Point3d));
            _endNode = (Point3d)info.GetValue("EndNode", typeof(Point3d));
            _beamProperty = (BeamProperty)info.GetValue("BeamProperty", typeof(BeamProperty));
            RotationAroundFirstAxis = SerializationFields.Read<double>(info, "AxisAngleRadians");
            _nodeI = SerializationFields.Read<NodeElement>(info, "NodeI");
            _nodeJ = SerializationFields.Read<NodeElement>(info, "NodeJ");
        }

        #endregion

        #region Equals, hascode, operators

        /// <summary>
        /// Equality with another beam (see the implementation of <see cref="IEquatable{BeamElement}"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal beam</returns>
        /// <remarks>Geometry and section changes do not change FEM identity.</remarks>
        public override bool Equals(object obj)
        {
            return obj is BeamElement && base.Equals(obj);
        }

        /// <summary>
        /// Equality of FEM identities.
        /// </summary>
        /// <param name="other">The beam to compare</param>
        /// <returns>True if the beams have the same identity.</returns>
        bool IEquatable<BeamElement>.Equals(BeamElement other)
        {
            return Equals((object)other);
        }

        /// <summary>
        /// Serializes the data of <see cref="Element"/>, the version, the points and the section
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("BeamAssignments", Assignments);

            double version = 1;
            info.AddValue("BeamVersion", version);
            info.AddValue("EndNode", _endNode);
            info.AddValue("StartNode", _startNode);
            info.AddValue("BeamProperty", _beamProperty);
            info.AddValue("AxisAngleRadians", _axisAngleRadians);
            info.AddValue("NodeI", _nodeI);
            info.AddValue("NodeJ", _nodeJ);
        }

        /// <summary>
        /// The hash code of the name, of the coordinate system, of the exact points and of the section
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode();
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
