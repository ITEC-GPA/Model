using GPC.Model.ElementProperties;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Glass
{
    /// <summary>
    /// This represent a generic glazing panel composed by a generic number of glass panels separated by air.
    /// </summary>
    [Serializable]
    public class GlassPlateProperty : PlateProperty, IEquatable<GlassPlateProperty>
    {
        #region Variables

        /// <summary>
        /// The layers, from external to internal
        /// </summary>
        protected List<IGlassLayer> _glassLayer;

        #endregion

        #region Properties

        /// <summary>
        /// The layers (glasses, interlayers and air chambers), from external to internal
        /// </summary>
        public List<IGlassLayer> GlassLayers { get => _glassLayer; set => _glassLayer = value; }

        /// <summary>
        /// The total thickness of the layers
        /// </summary>
        public double TotalThickness => _glassLayer.Select(i => i.Thickness).Sum();

        #endregion

        #region Public constructor

        /// <summary>
        /// Initialize the empty insulating glass
        /// Used in UI to create an empty laminated that the user will interactively define.
        /// </summary>
        /// <param name="name">The name</param>
        /// <remarks>Order of the glass panels is from external to internal</remarks>
        public GlassPlateProperty(string name)
            : base(name)
        {
            _glassLayer = new List<IGlassLayer>();
        }

        /// <summary>
        /// Create a Insulating glass
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="glassPanel">The layers (glasses, interlayers and air chambers)</param>
        /// <remarks>Order of the glass panels is from external to internal</remarks>
        public GlassPlateProperty(string name, IEnumerable<IGlassLayer> glassPanel)
            : base(name)
        {
            _glassLayer = glassPanel.ToList();
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private GlassPlateProperty(SerializationInfo info, StreamingContext context)
           : base(info, context)
        {
            _glassLayer = (List<IGlassLayer>)info.GetValue("GlassLayers", typeof(List<IGlassLayer>));
        }

        #endregion

        #region PUBLIC METHODS
        ///// <remarks>Order of the glass panels is from external to internal</remarks>
        //public IGlassPackage[][] GetGlassPackage()
        //{
        //    IGlassPackage[][] package = new IGlassPackage[_glassLayer.Count][];

        //    for (int i = 0; i < _glassLayer.Count; i++)
        //    {
        //        package[2 * i] = _glassLayer[i].GetGlassPackage();
        //    }

        //    return package;
        //}

        #endregion

        #region Equals - HashCode - Operators

        /// <summary>
        /// Serializes the property
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GlassLayers", _glassLayer);
        }

        /// <summary>
        /// Equality of the layers (in the same order) and of the base
        /// </summary>
        /// <param name="other">The property to compare</param>
        /// <returns>True if the propertys are equal</returns>
        public bool Equals(GlassPlateProperty other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) &&
                other._glassLayer.SequenceEqual(_glassLayer) &&
                base.Equals(other);
        }

        /// <summary>
        /// Equality with another property (see <see cref="Equals(GlassPlateProperty)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal property</returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as GlassPlateProperty);
        }

        /// <summary>
        /// The hash code of the base and of the layers
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                for (int i = 0; i < _glassLayer.Count; i++)
                    hashCode = hashCode * -17 + _glassLayer[i].GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(GlassPlateProperty)"/>)
        /// </summary>
        /// <param name="obj1">The first property</param>
        /// <param name="obj2">The second property</param>
        /// <returns>True if the propertys are equal</returns>
        public static bool operator ==(GlassPlateProperty obj1, GlassPlateProperty obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(GlassPlateProperty)"/>)
        /// </summary>
        /// <param name="obj1">The first property</param>
        /// <param name="obj2">The second property</param>
        /// <returns>True if the propertys are different</returns>
        public static bool operator !=(GlassPlateProperty obj1, GlassPlateProperty obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
