using GPC.Model.FEM.Materials;
using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Concrete", Group = "Materials", Kind = "Material")]
    public abstract class ConcreteMaterial : Material
    {
        #region Variables

        protected double _fck;
        protected double _epsilonCompressionY;
        protected double _epsilonCompressionU;

        #endregion

        #region Properties

        /// <summary>
        /// Characteristic compressive cylinder strength of concrete at 28 days
        /// </summary>
        public double Fck => _fck;

        /// <summary>
        /// Strain in the concrete at the peak compressive stress fc
        /// </summary>
        public double EpsilonCompressionY => _epsilonCompressionY;

        /// <summary>
        /// Ultimate strain in compression
        /// </summary>
        public double EpsilonCompressionU => _epsilonCompressionU;

        #endregion

        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Elastic secant modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fck">Concrete compression resistance reference value (28 days)</param>
        /// <param name="density">Density of concrete</param>
        /// <param name="guid">Guid of the material</param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        public ConcreteMaterial(string name, double elasticModulus, double poisson, double fck, double density, double alfaThermalExpansion, Guid guid)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion, guid)
        {
            _fck = fck;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Elastic secant modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fck">Concrete compression resistance reference value (28 days)</param>
        /// <param name="density">Density of concrete. Default value = 0.0025 T/mm^2</param>
        /// <remarks>alfaThermalExpansion = 1e-6</remarks>
        public ConcreteMaterial(double elasticModulus, double poisson, double fck, double density = 0.0025)
            : this("", elasticModulus, poisson, fck, density, 1e-6, Guid.NewGuid())
        {

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="fck">Concrete compression resistance reference value (28 days)</param>        
        protected ConcreteMaterial(string name, double fck)
            :base(name)
        {
            _fck = fck;
        }

        public ConcreteMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _fck = info.GetDouble("Fck");
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Fck", _fck);
        }

        public override IsotropicFemMaterial GetIsotropicFemMaterial()
        {
            return new IsotropicFemMaterial(E, Ni, AlfaThermalExpansion, Density);
        }

        public override OrthotropicFemMaterial GetOrthotropicFemMaterial()
        {
            return new OrthotropicFemMaterial(E, E, E, Ni, Ni, Ni, GetShearModule(), GetShearModule(), GetShearModule(), AlfaThermalExpansion, AlfaThermalExpansion, AlfaThermalExpansion, Density);
        }

        #endregion

    }
}
