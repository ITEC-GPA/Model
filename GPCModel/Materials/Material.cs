using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    public abstract class Material : ModelObject
    {
        #region VARIABLES
        protected double _elasticModulus;
        protected double _ni;
        protected double _alfaThermalExpansion;
        protected double _density;
        #endregion VARIABLES

        #region PROPERTIES
        public double E => _elasticModulus;
        public double Ni => _ni;
        public double AlfaThermalExpansion => _alfaThermalExpansion;
        public double Density => _density;

        #endregion PROPERTIES

        #region PUBLIC CONSTRUCTOR

        /// <summary>
        /// </summary>
        /// <param name="_elasticModulus"> Elastic Modulus [MPa]</param>
        /// <param name="_poisson"> Poisson modulus </param>
        /// <param name="_alfaThermalExpansion"> Thermal expansion constant</param>
        /// <param name="_density"> Density [T/mm^3]</param>
        public Material(string name, double elasticModulus, double ni, double density, double alfaThermalExpansion, Guid guid) 
            : base(guid, name)
        {
            _elasticModulus = elasticModulus < 0 ? throw new ArgumentException($"{nameof(elasticModulus)} cannot be zero or lower") : elasticModulus;
            
            _ni = ni < 0 ? throw new ArgumentException($"Poisson cannot be zero or lower") : ni;

            _alfaThermalExpansion = alfaThermalExpansion < 0 ? throw new ArgumentException($"{nameof(alfaThermalExpansion)} cannot be zero or lower") : alfaThermalExpansion;
            
            _density = density < 0 ? throw new ArgumentException($"{nameof(density)} cannot be zero or lower") : density;
        }

        public Material(string name, double elasticModulus, double ni, Guid guid) 
            : this(name, elasticModulus, ni, 0, 0, guid)
        { 

        }

        public Material(Guid guid) 
            : this("", 0, 0, 0, 0, guid) 
        { 

        }

        protected Material(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _alfaThermalExpansion = info.GetDouble("AlfaThermalExpansion");
            _density = info.GetDouble("Density");
            _elasticModulus = info.GetDouble("ElasticModulus");
            _ni = info.GetDouble("Ni");
        }

        #endregion PUBLIC CONSTRUCTOR

        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("AlfaThermalExpansion", _alfaThermalExpansion);
            info.AddValue("Density", _density);
            info.AddValue("ElasticModulus", _elasticModulus);
            info.AddValue("Ni", _ni);
        }

        #endregion PUBLIC METHODS
    }
}