using GPC.Model.FEM.Materials;
using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Concrete", Group = "Materials", Kind = "Material")]
    public class ConcreteMaterial : Material
    {
        #region VARIABLES
        protected double _fck;

        // Aggiungere i moduli elastici mancanti, dipende da normativa 

        #endregion VARIABLES

        #region PROPERTIES

        // TODO aggiungere le altre proprietà del calcestruzzo derivate fa fck
        public double Fck { get => _fck; set { _fck = value; } }

        #endregion PROPERTIES

        #region CONSTRUCTORS

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
            : base(name, elasticModulus, 0.0, density, alfaThermalExpansion, guid)
        {
            _fck = fck;
        }

        public ConcreteMaterial(double elasticModulus, double poisson, double fck, double density, double alfaThermalExpansion, Guid guid)
            : this("", elasticModulus, poisson, fck, density, alfaThermalExpansion, guid)
        {

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Elastic secant modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fck">Concrete compression resistance reference value (28 days)</param>
        /// <param name="density"></param>
        public ConcreteMaterial(double elasticModulus, double poisson, double fck, double density)
            : this(elasticModulus, poisson, fck, density, 0, Guid.NewGuid())
        {

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Elastic secant modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fck">Concrete compression resistance reference value (28 days)</param>
        /// <param name="density"></param>
        /// <param name="alfaThermalExpansion"></param>
        public ConcreteMaterial(double elasticModulus, double poisson, double fck, double density, double alfaThermalExpansion)
            : this(elasticModulus, poisson, fck, density, alfaThermalExpansion, Guid.NewGuid())
        {

        }

        public ConcreteMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _fck = info.GetDouble("Fck");
        }

        #endregion CONSTRUCTORS

        #region PUBLIC METHODS

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
            return new OrthotropicFemMaterial(E, E, E, GetShearModule(), GetShearModule(), GetShearModule(), Ni, Ni, Ni, AlfaThermalExpansion, AlfaThermalExpansion, AlfaThermalExpansion, Density);
        }

        #endregion
    }
}
