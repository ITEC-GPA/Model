using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// Monolithic glass. This represent the simpler glass panel. It is composed by a single layer of glass
    /// </summary>
    [Serializable]
    public class MonolithicGlass : Glass, IGlassPanel
    {
        #region Variables
        protected GlassMaterial _material;

        protected double _thickness;

        #endregion

        #region Properties

        public double Thickness => _thickness;

        public GlassMaterial Material => _material; 

        #endregion

        #region Constructors

        /// <summary>
        ///
        /// </summary>
        /// <param name="thickness">The minimum thickness of the panel (the one used for calculation)</param>
        /// <param name="glassMaterial"></param>
        public MonolithicGlass(string name, double thickness, GlassMaterial glassMaterial)
            : this(name, thickness, glassMaterial, Guid.NewGuid())
        {

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="guid">The guid of the glass</param>
        /// <param name="thickness">The minimum thickness of the panel (the one used for calculation)</param>
        /// <param name="glassMaterial"></param>
        public MonolithicGlass(string name, double thickness, GlassMaterial glassMaterial, Guid guid)
            : base(guid, name)
        {
            if (thickness <= 0.001)
            {
                throw new ArgumentException($"{nameof(thickness)} cannot be zero or lower");
            }

            this._thickness = thickness;
            this._material = glassMaterial;
        }

        public MonolithicGlass(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _thickness = info.GetDouble("Thickness");
            _material = (GlassMaterial)info.GetValue("Material", typeof(GlassMaterial));
        }

        #endregion 


        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Thickness", _thickness);
            info.AddValue("Material", _material);
        }

        #endregion 
    }
}