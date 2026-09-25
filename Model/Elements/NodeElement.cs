using GPC.Geometry;
using GPC.Model.Results.ElementResults;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements
{
    /// <summary>
    /// A node of the model: a point
    /// </summary>
    [Serializable]
    public class NodeElement : Element, ISerializable, IEquatable<NodeElement>
    {
        #region Variables

        /// <summary>
        /// The position
        /// </summary>
        protected Point3d _position;

        #endregion

        #region Properties

        /// <summary>
        /// The position (the instance of the node)
        /// </summary>
        public Point3d Position { get => _position; set => _position = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a node (the point instance is kept)
        /// </summary>
        /// <param name="position">The position</param>
        /// <param name="CoordinateSystem">The local coordinate system; null: the global one</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        public NodeElement(Point3d position, CoordinateSystem CoordinateSystem = null, string name = "", int id = IDUNASSIGNED)
            : base(id, name, CoordinateSystem)
        {
            _position = position;
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Element"/>, the version "NodeElementVersion" and the point
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected NodeElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version = info.GetInt32("NodeElementVersion");
            _position = (Point3d)info.GetValue("Point", typeof(Point3d));
        }

        #endregion

        /// <summary>
        /// Adds a result of the node (a null result is ignored)
        /// </summary>
        /// <param name="result">The result</param>
        public void AddResult(NodeResult result)
        {
            if (result != null)
                _results.Add(result);
        }


        #region Equals, hascode, operators

        /// <summary>
        /// Equality with another node (see the implementation of <see cref="IEquatable{NodeElement}"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal node</returns>
        /// <remarks>It calls this same method (the implementation of the interface is explicit): infinite recursion (see the list of the defects found)</remarks>
        public override bool Equals(object obj)
        {
            return Equals(obj as NodeElement);
        }

        /// <summary>
        /// Equality of the positions (within the tolerance of <see cref="Point3d"/>)
        /// </summary>
        /// <param name="other">The node to compare</param>
        /// <returns>True if the nodes have the same position</returns>
        bool IEquatable<NodeElement>.Equals(NodeElement other)
        {
            return !(other is null) &&
                _position.Equals(other.Position);
        }

        /// <summary>
        /// Serializes the data of <see cref="Element"/>, a version "BeamVersion" (the constructor reads "NodeElementVersion") and the point
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 0;
            info.AddValue("BeamVersion", version);
            info.AddValue("Point", _position);
        }

        /// <summary>
        /// The hash code of the name, of the coordinate system and of the exact position
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null nodes are equal
        /// </summary>
        /// <param name="left">The first node</param>
        /// <param name="right">The second node</param>
        /// <returns>True if the nodes are equal</returns>
        public static bool operator ==(NodeElement left, NodeElement right)
        {
            if (left is null)
                return right is null;
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first node</param>
        /// <param name="right">The second node</param>
        /// <returns>True if the nodes are different</returns>
        public static bool operator !=(NodeElement left, NodeElement right)
        {
            return !(left == right);
        }

        #endregion
    }
}
