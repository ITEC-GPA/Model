using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// Monolithic glass. This represent the simpler glass panel. It is composed by a single layer of glass
    /// </summary>
    [Serializable]
    public class MonolithicGlass : GlassPanel
    {
        #region VARIABLES
        private double _thickness;
        #endregion

        public double Thickness => _thickness;

        #region CONSTRUCTORS

        /// <summary>
        ///
        /// </summary>
        /// <param name="thickness">The minimum thickness of the panel (the one used for calculation)</param>
        /// <param name="glassMaterial"></param>
        public MonolithicGlass(double thickness, GlassMaterial glassMaterial)
            : this(thickness, glassMaterial, Guid.Empty)
        {

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="guid">The guid of the glass</param>
        /// <param name="thickness">The minimum thickness of the panel (the one used for calculation)</param>
        /// <param name="glassMaterial"></param>
        public MonolithicGlass(double thickness, GlassMaterial glassMaterial, Guid guid)
            : base(glassMaterial, guid)
        {
            if (thickness <= 0)
            {
                throw new ArgumentException($"{nameof(thickness)} cannot be zero or lower");
            }

            this._thickness = thickness;
        }

        public MonolithicGlass(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion CONSTRUCTORS

        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        #endregion PUBLIC METHODS
    }
}