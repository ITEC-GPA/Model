using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    public class SteelMaterial : Material
    {
        #region VARIABLES

        protected double _fyk;
        protected double _fu;
        protected double _epsilon0;

        #endregion 

        #region PROPERTIES

        public double Fyk { get => _fyk; set { _fyk = value; } }

        public double Fu { get => _fu; set { _fu = value; } }

        public double Epsilon0 { get => _epsilon0; set { _epsilon0 = value; } }

        #endregion 

        #region CONSTRUCTORS

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="epsilon0">Yielding strain</param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        public SteelMaterial(string name, double elasticModulus, double poisson, double fyk, double fu, double epsilon0, double density, double alfaThermalExpansion, Guid guid)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion, guid)
        {
            if (elasticModulus == 0)
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be equal to zero");

            if (poisson == 0)
                throw new ArgumentException($"{nameof(poisson)} cannot be equal to zero");

            this._fu = fu <= 0 ? throw new ArgumentException($"{nameof(fu)} cannot be zero or lower") : fu ;
            this._fyk = fyk <= 0 ? throw new ArgumentException($"{nameof(fyk)} cannot be zero or lower") : fyk;
            this._epsilon0 = epsilon0 <= 0 ? throw new ArgumentException($"{nameof(epsilon0)} cannot be zero or lower") : epsilon0;
        }

        /// <summary>
        /// Guid setted to empty, alfaThermalExpansion setted to 0
        /// </summary>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="epsilon0">Yielding strain</param>
        public SteelMaterial(string name, double elasticModulus, double poisson, double fy, double fu, double epsilon0, double density)
            : this(name, elasticModulus, poisson, fy, fu, epsilon0, density, 0, Guid.Empty)
        {

        }

        /// <summary>
        /// Guid setted to empty, alfaThermalExpansion setted to 0. Epsilon0 equal to fy / E
        /// </summary>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        public SteelMaterial(string name, double elasticModulus, double poisson, double fy, double fu, double density)
            : this(name, elasticModulus, poisson, fy, fu, fy / elasticModulus, density, 0, Guid.Empty)
        {

        }

        public SteelMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _fu = info.GetDouble("Fu");
            _fyk = info.GetDouble("Fyk");
            _epsilon0 = info.GetDouble("Epsilon0");
        }

        #endregion 

        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Epsilon0", _epsilon0);
            info.AddValue("Fyk", _fyk);
            info.AddValue("Fu", _fu);
        }

        #endregion 
    }
}