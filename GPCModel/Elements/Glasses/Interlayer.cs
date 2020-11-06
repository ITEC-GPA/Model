using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// Abstract class that represent the interlayer between two monolithic glasses to compose a laminated glass
    /// </summary>
    [Serializable]
    public abstract class Interlayer : Element
    {
        #region VARIABLES
        protected double _thickness;
        protected InterlayerMaterial _interlayerMaterial;
        #endregion

        #region PROPERTIES
        protected double Thickness => _thickness;
        protected InterlayerMaterial InterlayerMaterial => _interlayerMaterial;
        #endregion

        /// <summary>
        ///
        /// </summary>
        /// <param name="guid">The guid of the element</param>
        protected Interlayer(double thickness, InterlayerMaterial interlayerMaterial, Guid guid)
            : base(guid)
        {
            this._thickness = thickness;
            this._interlayerMaterial = interlayerMaterial;
        }

        public Interlayer(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}