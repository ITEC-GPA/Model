using GPC.Model.FEM.Materials;
using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Steel", Group = "Materials", Kind = "Material")]
    public class SteelMaterial : Material
    {

        /// <summary>
        /// Default Steel S235 according to EN1993
        /// </summary>
        public static SteelMaterial S235 => new SteelMaterial("S235", 210000, 0.3, 235, 360, 0.05, 0.007850, 12 * 1e-6, new Guid());

        /// <summary>
        /// Default Steel S275 according to EN1993
        /// </summary>
        public static SteelMaterial S275 => new SteelMaterial("S275", 210000, 0.3, 275, 430, 0.05, 0.007850, 12 * 1e-6, new Guid());

        /// <summary>
        /// Default Steel S355 according to EN1993
        /// </summary>
        public static SteelMaterial S355 => new SteelMaterial("S355", 210000, 0.3, 355, 510, 0.05, 0.007850, 12 * 1e-6, new Guid());

        #region Variables

        protected double _fyk;
        protected double _fu;
        protected double _strainU;

        #endregion 

        #region Properties

        /// <summary>
        /// Characteristic yield strength
        /// </summary>
        public double Fyk => _fyk;

        /// <summary>
        /// Ultimate strength
        /// </summary>
        public double Fu => _fu;

        /// <summary>
        /// Yielding strain
        /// </summary>
        public double StrainY => _fyk / _elasticModulus;

        /// <summary>
        /// Ultimate strain
        /// </summary>
        public double StrainU => _strainU;

        /// <summary>
        /// Strain hardening modulus
        /// </summary>
        public double Et => GetEt();

        #endregion

        #region Constructor

        /// <summary>
        /// Default SteelMaterial constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="density">Density of material</param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>        
        public SteelMaterial(string name, double elasticModulus, double poisson, double fyk, double fu, double density, double alfaThermalExpansion)
            : this(name, elasticModulus, poisson, fyk, fu, 0.05, density, alfaThermalExpansion, new Guid())
        {
            if (elasticModulus == 0)
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be equal to zero");

            _fu = fu <= 0 ? throw new ArgumentException($"{nameof(fu)} cannot be zero or lower") : fu ;
            _fyk = fyk <= 0 ? throw new ArgumentException($"{nameof(fyk)} cannot be zero or lower") : fyk;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="density">Density of material</param>
        /// <remarks>Guid setted to new guid, alfaThermalExpansion setted to 12 * 1e-6</remarks>
        public SteelMaterial(string name, double elasticModulus, double poisson, double fyk, double fu, double density)
            : this(name, elasticModulus, poisson, fyk, fu, 0.05, density, 12 * 1e-6, Guid.NewGuid())
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="density">Density of material</param>
        /// <remarks>Guid setted to empty, alfaThermalExpansion setted to 12 * 1e-6. Epsilon0 equal to fy / E</remarks>
        public SteelMaterial(string name, double fyk, double fu, double density = 0.007850)
            : this(name, 210000.0, 0.30, fyk, fu, 0.05, density, 0, Guid.NewGuid())
        {

        }

        /// <summary>
        /// Protected steelMaterial constructor 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="strainU">The ultimate strain</param>
        /// <param name="density">Density of material</param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        protected SteelMaterial(string name, double elasticModulus, double poisson, double fyk,
            double fu, double strainU, double density, double alfaThermalExpansion, Guid guid)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion, guid)
        {
            if (elasticModulus == 0)
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be zero");

            if (poisson == 0)
                throw new ArgumentException($"{nameof(poisson)} cannot be zero");

            if (poisson > 0.5)
                throw new ArgumentException($"{nameof(poisson)} cannot be major than 0.5");

            if (density <= 0)
                throw new ArgumentException($"{nameof(density)} cannot be minor than zero");

            _fu = fu <= 0 ? throw new ArgumentException($"{nameof(fu)} cannot be zero or lower") : fu;
            _fyk = fyk <= 0 ? throw new ArgumentException($"{nameof(fyk)} cannot be zero or lower") : fyk;
            _strainU = strainU <= 0 ? throw new ArgumentException($"{nameof(fyk)} cannot be zero or lower") : strainU;
        }

        SteelMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _fu = info.GetDouble("Fu");
            _fyk = info.GetDouble("Fyk");
            _strainU = info.GetDouble("EpsilonU");
        }

        #endregion

        #region Public Methods

        public override IsotropicFemMaterial GetIsotropicFemMaterial()
        {
            return new IsotropicFemMaterial(E, Ni, AlfaThermalExpansion, Density);
        }

        public override OrthotropicFemMaterial GetOrthotropicFemMaterial()
        {
            return new OrthotropicFemMaterial(E, E, E, Ni, Ni, Ni, GetShearModule(), GetShearModule(), GetShearModule(), AlfaThermalExpansion, AlfaThermalExpansion, AlfaThermalExpansion, Density);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("EpsilonU", _strainU);
            info.AddValue("Fyk", _fyk);
            info.AddValue("Fu", _fu);
        }

        public virtual double CalculateStress(double strain)
		{
            if (strain >= 0)
            {
                if (Math.Abs(strain) <= StrainY)
                    return strain * Fyk / StrainY;
                else
                {
                    if (Et == 0)
                        return Fyk;
                    else
                        return Fyk + (strain - StrainY) * Et;
                }
            }
            else
			{
                if (Math.Abs(strain) <= StrainY)
                    return strain * Fyk / StrainY;
                else
                {
                    if (Et == 0)
                        return - Fyk;
                    else
                        return - Fyk - Math.Abs(Math.Abs(strain) - Math.Abs(StrainY)) * Et;
                }
            }
        }

        #endregion 

        #region Protected Methods

        protected double GetEt()
		{
            if (Math.Abs(Fu - Fyk) < Geometry.GeometryBase.GetDefaultTolerance())
                return 0.0;
            else
                return (Fu - Fyk) / (StrainU - StrainY);
		}

		#endregion
	}
}
