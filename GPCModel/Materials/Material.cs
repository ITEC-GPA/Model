using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    public class Material : ModelObject
    {
        #region VARIABLES
        /// <summary>
        /// </summary>
        /// <param name="_elasticModulus"> Elastic Modulus [MPa]</param>
        /// <param name="_poisson"> Poisson modulus </param>
        /// <param name="_alfaThermalExpansion"> Thermal expansion constant</param>
        /// <param name="_density"> Effective area [mm2]</param>
        protected double _e;
        protected double _ni;
        protected double _alfaThermalExpansion;
        protected double _density;
        #endregion VARIABLES

        #region PROPERTIES
        public double E => _e;
        public double Ni => _ni;
        public double AlfaThermalExpansion => _alfaThermalExpansion;
        public double Density => _density;

        #endregion PROPERTIES

        #region PUBLIC CONSTRUCTOR
        public Material(string name, double e, double ni, double density, double alfaThermalExpansion, Guid guid) : base(guid, name)
        {
            _e = e;
            _ni = ni;
            _alfaThermalExpansion = alfaThermalExpansion;
            _density = density;
        }
        public Material(string name, double density, double alfaThermalExpansion, Guid guid) : base(guid, name)
        {
            _alfaThermalExpansion = alfaThermalExpansion;
            _density = density;
        }

        protected Material(double density, double alfaThermalExpansion, Guid guid) 
            : this("", density, alfaThermalExpansion, guid)
        {
        }

        protected Material(Guid guid) : this("", 0.0, 0.0, guid)
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