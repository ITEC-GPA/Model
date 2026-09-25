using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    /// <summary>
    /// A 2D shape with a material (e.g. a part of a composite section)
    /// </summary>
    [Serializable]
    public class ShapeEx : Shape2d, ISerializable
    {
        #region Variables

        /// <summary>
        /// The material
        /// </summary>
        protected Material _material;

        #endregion

        #region Properties

        /// <summary>
        /// The material
        /// </summary>
        public Material Material => _material;

        /// <summary>
        /// This shape
        /// </summary>
        public virtual Shape2d Shape => this;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the shape
        /// </summary>
        /// <param name="fill">The outer polygon</param>
        /// <param name="material">The material</param>
        /// <param name="holes">The holes</param>
        /// <param name="childs">The shapes inside the holes</param>
        /// <param name="tolerance">The geometric tolerance</param>
        public ShapeEx(Polygon2d fill, Material material, Polygon2d[] holes = null, ShapeEx[] childs = null, double tolerance = GeometryBase.Tolerance)
            : base(fill, holes, childs, tolerance)
        {
            _material = material;
        }

        /// <summary>
        /// Creates the shape as a copy of another one
        /// </summary>
        /// <param name="shape">The shape to copy</param>
        /// <param name="material">The material</param>
        /// <param name="tolerance">The geometric tolerance</param>
        public ShapeEx(Shape2d shape, Material material, double tolerance = GeometryBase.Tolerance)
            : base(shape, tolerance)
        {
            _material = material;
        }

        /// <summary>
        /// Creates the shape as a copy of another one, with the default tolerance
        /// </summary>
        /// <param name="shape">The shape to copy</param>
        /// <param name="material">The material</param>
        public ShapeEx(Shape2d shape, Material material)
            : this(shape, material, GeometryBase.Tolerance)
        {
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ShapeEx(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("ShapeExVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// This shape
        /// </summary>
        /// <returns>This instance</returns>
        public Shape2d GetShape()
        {
            return this;
        }

        #endregion

        #region Equals - hashcode - Operators

        /// <summary>
        /// Serializes the shape (version 2, with the material)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("ShapeExVersion", version);

            info.AddValue("Material", _material);
        }

        /// <summary>
        /// Equality of the shapes and of the materials (a null material throws <see cref="NullReferenceException"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal shape</returns>
        public override bool Equals(object obj)
        {
            return obj is ShapeEx ex &&
                   base.Equals(obj) &&
                   _material.Equals(ex._material);
        }

        /// <summary>
        /// The hash code of the shape and of the material
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _material.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>; a null <paramref name="left"/> throws <see cref="NullReferenceException"/>)
        /// </summary>
        /// <param name="left">The first shape</param>
        /// <param name="right">The second shape</param>
        /// <returns>True if the shapes are equal</returns>
        public static bool operator ==(ShapeEx left, ShapeEx right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first shape</param>
        /// <param name="right">The second shape</param>
        /// <returns>True if the shapes are different</returns>
        public static bool operator !=(ShapeEx left, ShapeEx right)
        {
            return !(left == right);
        }

        #endregion
    }
}

