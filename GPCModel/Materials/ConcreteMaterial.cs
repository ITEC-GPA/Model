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

        #endregion

        #region Properties

        public double Fck => _fck; 

        #endregion

        #region Constructor

        /// <summary>
        ///
        /// </summary>
        /// <param name="name"></param>
        /// <param name="elasticModulus">Elastic secant modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fck">Concrete compression resistance reference value (28 days)</param>
        /// <param name="density"></param>
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
        /// <param name="density"></param>
        public ConcreteMaterial(double elasticModulus, double poisson, double fck, double density = 2500.0)
            : this("", elasticModulus, poisson, fck, density, 0, Guid.NewGuid())
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
