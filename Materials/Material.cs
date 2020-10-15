using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    public abstract class Material
    {
        #region VARIABLES

        protected Guid _guid;
        protected double _alfaThermalExpansion;
        protected double _density;

        #endregion VARIABLES

        #region PROPERTIES

        public Guid Guid => _guid;
        protected double AlfaThermalExpansion => _alfaThermalExpansion;
        protected double Density => _density;

        #endregion PROPERTIES

        #region PUBLIC CONSTRUCTOR

        public Material(double density, double alfaThermalExpansion, Guid guid)
        {
            this._guid = guid;
            this._alfaThermalExpansion = alfaThermalExpansion;
            this._density = density;
        }

        public Material(SerializationInfo info, StreamingContext context)
        {
            _guid = (Guid)info.GetValue("Guid", typeof(Guid));
            _alfaThermalExpansion = info.GetDouble("AlfaThermalExpansion");
            _density = info.GetDouble("Density");
        }

        #endregion PUBLIC CONSTRUCTOR

        #region PUBLIC METHODS

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Guid", _guid);
            info.AddValue("AlfaThermalExpansion", _alfaThermalExpansion);
            info.AddValue("Density", _density);
        }

        #endregion PUBLIC METHODS
    }
}