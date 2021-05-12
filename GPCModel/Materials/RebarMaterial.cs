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
        #region CONSTRUCTORS

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="epsilon0">Yielding strain</param>
        /// <param name="density"></param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        public RebarMaterial(double elasticModulus, double poisson, double fy, double fu, double epsilon0, double density, double alfaThermalExpansion, Guid guid)
            : base("", elasticModulus, poisson, fy, fu, epsilon0, density, alfaThermalExpansion, guid)
        {
            if (fu == 0)
            {
                throw new ArgumentException($"{nameof(fu)} cannot be zero");
            }
            if (fy == 0)
            {
                throw new ArgumentException($"{nameof(fy)} cannot be zero");
            }
            if (elasticModulus == 0)
            {
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be zero");
            }
            if (poisson == 0)
            {
                throw new ArgumentException($"{nameof(poisson)} cannot be zero");
            }
            if (epsilon0 == 0)
            {
                throw new ArgumentException($"{nameof(epsilon0)} cannot be zero");
            }
        }

        /// <summary>
        /// Guid setted to empty, alfaThermalExpansion setted to 0
        /// </summary>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="epsilon0">Yielding strain</param>
        /// <param name="density"></param>
        public RebarMaterial(double elasticModulus, double poisson, double fy, double fu, double epsilon0, double density)
            : this(elasticModulus, poisson, fy, fu, epsilon0, density, 0, Guid.NewGuid())
        {
        }

        /// <summary>
        /// Guid setted to empty, alfaThermalExpansion setted to 0. Epsilon0 equal to fy / E
        /// </summary>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="density"></param>
        public RebarMaterial(double elasticModulus, double poisson, double fy, double fu, double density)
            : this(elasticModulus, poisson, fy, fu, fy / elasticModulus, density, 0, Guid.NewGuid())
        {
            if (fy == 0)
            {
                throw new ArgumentException($"{nameof(fy)} cannot be zero");
            }
            if (elasticModulus == 0)
            {
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be zero");
            }
        }

        public RebarMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
        }

        #endregion CONSTRUCTORS

        #region PUBLIC METHODS

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

        #endregion PUBLIC METHODS
    }
}
