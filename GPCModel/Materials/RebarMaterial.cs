using GPC.Model.FEM.Materials;
using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Rebar", Group = "Materials", Kind = "Material")]
    public class RebarMaterial : SteelMaterial
    {
        #region Constructor

        /// <summary>
        /// Default rebar material constructor
        /// </summary>
        /// <param name="name">Name of material</param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="epsilonU">Ultimate strain</param>
        /// <param name="density"></param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        public RebarMaterial(string name, double elasticModulus, double poisson, 
            double fy, double fu, double epsilonU, double density, double alfaThermalExpansion, Guid guid)
            : base(name, elasticModulus, poisson, fy, fu, epsilonU, density, alfaThermalExpansion, guid)
        {
            if (fu == 0)            
                throw new ArgumentException($"{nameof(fu)} cannot be zero");
            
            if (fy == 0)            
                throw new ArgumentException($"{nameof(fy)} cannot be zero");
            
            if (elasticModulus == 0)            
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be zero");
                        
            if (poisson == 0)            
                throw new ArgumentException($"{nameof(poisson)} cannot be zero");
            
            if (poisson > 0.5)            
                throw new ArgumentException($"{nameof(poisson)} cannot be major than 0.5");

			if (epsilonU == 0)
				throw new ArgumentException($"{nameof(epsilonU)} cannot be zero");

            if (density <= 0)
                throw new ArgumentException($"{nameof(density)} cannot be minor than zero");

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="density"></param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        /// <remarks>Name is empty</remarks>
        public RebarMaterial(double elasticModulus, double fy, double fu, double poisson = 0.28, double density = 0.007850, double alfaThermalExpansion = 12 * 1e-6)
            : this("", elasticModulus, poisson, fy, fu, 0.075, density, alfaThermalExpansion, new Guid())
        {
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fyk">Yielding stress</param>        
        /// <remarks>Guid setted to new guid. StressStrainDiagram is set to ElastoPlastic. alfaThermalExpansion setted to 0. Epsilon0 equal to fy / E
        /// E = 200GPa, ni = 0.28. Epsilon U is set as 0.075 and fu is set as fyk</remarks>
        public RebarMaterial(double fyk)
            : this(200000, fyk, fyk)
		{
		}

        public RebarMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
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
