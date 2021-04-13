using System;
using System.Collections.Generic;
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

        #endregion 

        #region PROPERTIES

        public double E => _elasticModulus;
        public double Ni => _ni;
        public double AlfaThermalExpansion => _alfaThermalExpansion;
        public double Density => _density;

        #endregion

        #region PUBLIC CONSTRUCTOR

        /// <summary>
        /// </summary>
        /// <param name="elasticModulus"> Elastic Modulus [MPa]</param>
        /// <param name="poisson"> Poisson modulus </param>
        /// <param name="alfaThermalExpansion"> Thermal expansion constant</param>
        /// <param name="density"> Density [T/mm^3]</param>
        public Material(string name, double elasticModulus, double poisson, double density, double alfaThermalExpansion)
            : this(name, elasticModulus, poisson, density, alfaThermalExpansion, Guid.NewGuid())
        {
        }

        /// <summary>
        /// </summary>
        /// <param name="elasticModulus"> Elastic Modulus [MPa]</param>
        /// <param name="poisson"> Poisson modulus </param>
        /// <param name="alfaThermalExpansion"> Thermal expansion constant</param>
        /// <param name="density"> Density [T/mm^3]</param>
        public Material(string name, double elasticModulus, double poisson, double density, double alfaThermalExpansion, Guid guid) 
            : base(guid, name)
        {
            _elasticModulus = elasticModulus < 0 ? throw new ArgumentException($"{nameof(elasticModulus)} cannot be lower than zero") : elasticModulus;

            if (poisson > 1)
                throw new ArgumentException($"{nameof(poisson)} cannot be greater than 1");

            _ni = poisson < 0 ? throw new ArgumentException($"Poisson cannot be lower than zero") : poisson;

            _alfaThermalExpansion = alfaThermalExpansion < 0 ? throw new ArgumentException($"{nameof(alfaThermalExpansion)} cannot be lower than zero") : alfaThermalExpansion;
            
            _density = density < 0 ? throw new ArgumentException($"{nameof(density)} cannot be lower than zero") : density;
        }

        public Material(string name, double elasticModulus, double ni, Guid guid) 
            : this(name, elasticModulus, ni, 0, 0, guid)
        { 
        }


        protected Material(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _alfaThermalExpansion = info.GetDouble("AlfaThermalExpansion");
            _density = info.GetDouble("Density");
            _elasticModulus = info.GetDouble("ElasticModulus");
            _ni = info.GetDouble("Ni");
        }

        #endregion 

        public virtual double GetShearModule()
        {
            return E / (2.0 * (1.0 + Ni));
        }


        #region Equals - HashCode - Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("AlfaThermalExpansion", _alfaThermalExpansion);
            info.AddValue("Density", _density);
            info.AddValue("ElasticModulus", _elasticModulus);
            info.AddValue("Ni", _ni);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            Material objCasted = obj as Material;
            return !(objCasted is null) && objCasted._elasticModulus.Equals(_elasticModulus) &&
                                           objCasted._ni.Equals(_ni) &&
                                           objCasted._alfaThermalExpansion.Equals(_alfaThermalExpansion) &&
                                           objCasted._density.Equals(_density) &&
                                           base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _elasticModulus.GetHashCode();
            hashCode = hashCode * -17 + _ni.GetHashCode();
            hashCode = hashCode * -17 + _alfaThermalExpansion.GetHashCode();
            hashCode = hashCode * -17 + _density.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Material obj1, Material obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }
        public static bool operator !=(Material obj1, Material obj2)
        {
            return !(obj1 == obj2);
        }



        #endregion
    }
}