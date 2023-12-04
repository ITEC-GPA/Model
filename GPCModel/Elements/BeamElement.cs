using GPC.Geometry;
using GPC.Model.ElementProperties;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    [Serializable]
    public class BeamElement : Element, ISerializable, IEquatable<BeamElement>
    {
        #region Variables

        protected Point3d _startNode;
        protected Point3d _endNode;
        protected BeamProperty _beamProperty;
        protected double _axisAngleRadians;

        #endregion

        #region Properties

        public Point3d StartPoint { get => _startNode; set => _startNode = value; }

        public Point3d EndPoint { get => _endNode; set => _endNode = value; }

        public Line3d Line { get => new Line3d(StartPoint, EndPoint); }

        public BeamProperty BeamProperty { get => _beamProperty; set => _beamProperty = value; }

        public double RotationAroundFirstAxis { get => _axisAngleRadians; set => _axisAngleRadians = value; }

        public double Length => Line.Length;

        #endregion

        #region Constructor

        public BeamElement(Point3d startPoint, Point3d endPoint, BeamProperty beamProperty, CoordinateSystem coordinateSystem = null, string name = "", int id = IDUNASSIGNED)
            : base(id, name, coordinateSystem)
        {
            _startNode = startPoint;
            _endNode = endPoint;
            _beamProperty = beamProperty;
        }

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

        public override bool Equals(object obj)
        {
            return Equals(obj as BeamElement);
        }

        bool IEquatable<BeamElement>.Equals(BeamElement other)
        {
            return !(other is null) &&
                _startNode.Equals(other._startNode) &&
                _endNode.Equals(other._endNode) &&
                _beamProperty == other.BeamProperty;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 0;
            info.AddValue("BeamVersion", version);
            info.AddValue("EndNode", _endNode);
            info.AddValue("StartNode", _startNode);
            info.AddValue("BeamProperty", _beamProperty);
        }

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

        public static bool operator ==(BeamElement left, BeamElement right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(BeamElement left, BeamElement right)
        {
            return !(left == right);
        }

        #endregion
    }
}
