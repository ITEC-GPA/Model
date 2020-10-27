using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    public abstract class Material : ModelObject
    {
        #region VARIABLES

        protected double _alfaThermalExpansion;
        protected double _density;

        #endregion VARIABLES

        #region PROPERTIES

        public double AlfaThermalExpansion => _alfaThermalExpansion;
        public double Density => _density;

        #endregion PROPERTIES

        #region PUBLIC CONSTRUCTOR

        protected Material(double density, double alfaThermalExpansion, Guid guid) : base(guid)
        {
            this._alfaThermalExpansion = alfaThermalExpansion;
            this._density = density;
        }

        protected Material(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _alfaThermalExpansion = info.GetDouble("AlfaThermalExpansion");
            _density = info.GetDouble("Density");
        }

        #endregion PUBLIC CONSTRUCTOR

        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("AlfaThermalExpansion", _alfaThermalExpansion);
            info.AddValue("Density", _density);
        }

        #endregion PUBLIC METHODS
    }
}