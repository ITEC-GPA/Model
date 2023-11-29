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

        protected List<IGlassLayer> _glassLayer;

        #endregion

        #region Properties

        public List<IGlassLayer> GlassLayers { get => _glassLayer; set => _glassLayer = value; }

        public double TotalThickness => _glassLayer.Select(i => i.Thickness).Sum();

        #endregion

        #region Public constructor

        /// <summary>
        /// Initialize the empty insulating glass 
        /// Used in UI to create an empty laminated that the user will interactively define.
        /// </summary>
        /// <param name="name"></param>
        /// <remarks>Order of the glass panels is from external to internal</remarks>
        public GlassPlateProperty(string name)
            : base(name)
        {
            _glassLayer = new List<IGlassLayer>();
        }

        /// <summary>
        /// Create a Insulating glass
        /// </summary>
        /// <param name="name"></param>
        /// <param name="glassPanel"></param>
        /// <param name="airChamber"></param>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>Order of the glass panels is from external to internal</remarks>
        public GlassPlateProperty(string name, IEnumerable<IGlassLayer> glassPanel)
            : base(name)
        {
            _glassLayer = glassPanel.ToList();
        }

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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("GlassLayers", _glassLayer);
        }

        public bool Equals(GlassPlateProperty other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) &&
                other._glassLayer.Equals(_glassLayer) &&
                base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as GlassPlateProperty);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _glassLayer.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(GlassPlateProperty obj1, GlassPlateProperty obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(GlassPlateProperty obj1, GlassPlateProperty obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
