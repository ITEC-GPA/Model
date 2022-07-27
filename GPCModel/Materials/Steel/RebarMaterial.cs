using GPC.Model.Fem.Materials;
using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Rebar", Group = "Materials", Kind = "Material")]
    [Obsolete("Deprecated, use SteelMaterial (with SteelType = Rebar) instead.")]
    public class RebarMaterial : SteelMaterial
    {
        #region Constructor

        /// <summary>
        /// Default rebar material constructor
        /// </summary>
        /// <param name="name">Name of material</param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="strainU">Ultimate strain</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="density"></param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        /// <remarks>Name is empty</remarks>
        public RebarMaterial(string name, double elasticModulus, double fy, double fu, double strainU = 0.075, 
            double poisson = 0.28, double density = 0.007850, double alfaThermalExpansion = 12 * 1e-6)
            : base(name, elasticModulus, poisson, fy, fu, strainU, SteelTypes.Rebar, density, alfaThermalExpansion)
        {
        }

        /// <param name="name">Name of material</param>
        /// <param name="fyk">Yielding stress</param>        
        /// <remarks>StressStrainDiagram is set to ElastoPlastic. alfaThermalExpansion setted to 0. EpsilonY equal to fy / E
        /// E = 200GPa, ni = 0.28. Epsilon U is set as 0.075 and fu is set as fyk</remarks>
        public RebarMaterial(string name, double fyk)
            : this(name, 200000, fyk, fyk)
		{
		}

        protected RebarMaterial(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
        }

        #endregion 

        #region Public override Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
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
