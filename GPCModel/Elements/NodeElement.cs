using GPC.Geometry;
using GPC.Model.Results;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    [Serializable]
    public class NodeElement : Element, ISerializable, IEquatable<NodeElement>
    {
        #region Variables

        protected Point3d _position;

        #endregion

        #region Properties

        public Point3d Position { get => _position; set => _position = value; }

        #endregion

        #region Constructor

        public NodeElement(Point3d position, CoordinateSystem CoordinateSystem = null, string name = "", int id = IDUNASSIGNED)
            : base(id, name, CoordinateSystem)
        {
            _position = position;
        }

        protected NodeElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version = info.GetInt32("NodeElementVersion");
            _position = (Point3d)info.GetValue("Point", typeof(Point3d));
        }

        #endregion

        public void AddResult(NodeResult result)
        {
            if (result != null)
                _results.Add(result);
        }


        #region Equals, hascode, operators

        public override bool Equals(object obj)
        {
            return Equals(obj as NodeElement);
        }

        bool IEquatable<NodeElement>.Equals(NodeElement other)
        {
            return !(other is null) &&
                _position.Equals(other.Position);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 0;
            info.AddValue("BeamVersion", version);
            info.AddValue("Point", _position);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _position.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(NodeElement left, NodeElement right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        public static bool operator !=(NodeElement left, NodeElement right)
        {
            return !(left == right);
        }

        #endregion
    }
}
