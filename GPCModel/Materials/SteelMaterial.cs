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
        public static SteelMaterial S235 => new SteelMaterial("S235", StressStrainDiagrams.ElastoPlastic, 210000, 0.3, 235, 360, 0.05, 0.007850, 12 * 1e-6, new Guid());

        /// <summary>
        /// Default Steel S275 according to EN1993
        /// </summary>
        public static SteelMaterial S275 => new SteelMaterial("S275", StressStrainDiagrams.ElastoPlastic, 210000, 0.3, 275, 430, 0.05, 0.007850, 12 * 1e-6, new Guid());

        /// <summary>
        /// Default Steel S355 according to EN1993
        /// </summary>
        public static SteelMaterial S355 => new SteelMaterial("S355", StressStrainDiagrams.ElastoPlastic, 210000, 0.3, 355, 510, 0.05, 0.007850, 12 * 1e-6, new Guid());

        #region Enumerator

        public enum StressStrainDiagrams
        {
            ElastoPlastic,
            ElastoPlasticWithLinearHardening,
        }

        #endregion

        #region Variables

        protected double _fyk;
        protected double _fu;
        protected double _epsilonU;
        protected StressStrainDiagrams _stressStrainDiagrams;

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
        public double EpsilonY => _fyk / _elasticModulus;

        /// <summary>
        /// Ultimate strain
        /// </summary>
        public double EpsilonU => _epsilonU;

        /// <summary>
        /// The stress-strain relationship
        /// </summary>
        public StressStrainDiagrams StressStrainDiagram => _stressStrainDiagrams;

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
            : this(name, StressStrainDiagrams.ElastoPlastic, elasticModulus, poisson, fyk, fu, 0.05, density, alfaThermalExpansion, new Guid())
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
            : this(name, StressStrainDiagrams.ElastoPlastic, elasticModulus, poisson, fyk, fu, 0.05, density, 12 * 1e-6, Guid.NewGuid())
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
            : this(name, StressStrainDiagrams.ElastoPlastic, 210000.0, 0.30, fyk, fu, 0.05, density, 0, Guid.NewGuid())
        {

        }

        /// <summary>
        /// Protected steelMaterial constructor 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="stressStrainDiagrams">The stress Strain Diagrams</param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fyk">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="epsilonU">The ultimate strain</param>
        /// <param name="density">Density of material</param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        protected SteelMaterial(string name, StressStrainDiagrams stressStrainDiagrams, double elasticModulus, double poisson, double fyk, 
            double fu, double epsilonU, double density, double alfaThermalExpansion, Guid guid)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion, guid)
        {
            if (elasticModulus == 0)
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be equal to zero");

            _fu = fu <= 0 ? throw new ArgumentException($"{nameof(fu)} cannot be zero or lower") : fu;
            _fyk = fyk <= 0 ? throw new ArgumentException($"{nameof(fyk)} cannot be zero or lower") : fyk;

            _stressStrainDiagrams = stressStrainDiagrams;
            _epsilonU = epsilonU;
        }

        public SteelMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _fu = info.GetDouble("Fu");
            _fyk = info.GetDouble("Fyk");
            _epsilonU = info.GetDouble("EpsilonU");
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
            info.AddValue("EpsilonU", _epsilonU);
            info.AddValue("Fyk", _fyk);
            info.AddValue("Fu", _fu);
        }

        #endregion 
    }
}
