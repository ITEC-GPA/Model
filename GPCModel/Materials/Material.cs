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
        protected string _name;

        #endregion VARIABLES

        #region PROPERTIES

        public double AlfaThermalExpansion => _alfaThermalExpansion;
        public double Density => _density;

        public string Name { get => _name; set { _name = value; } }

        #endregion PROPERTIES

        #region PUBLIC CONSTRUCTOR

        protected Material(string name, double density, double alfaThermalExpansion, Guid guid) : base(guid)
        {
            this._alfaThermalExpansion = alfaThermalExpansion;
            this._density = density;
        }

        protected Material(double density, double alfaThermalExpansion, Guid guid) 
            : this("", density, alfaThermalExpansion, guid)
        {
        }

        protected Material(Guid guid) : this(0.0, 0.0, guid)
        {
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