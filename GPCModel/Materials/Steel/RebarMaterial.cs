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
        public static RebarMaterial B450C => new RebarMaterial("B450C", 200000, 0.28, 450, 510, 0.075, 0.007850, 12 * 1e-6, new Guid());

        #region Constructor

        /// <summary>
        /// Default rebar material constructor
        /// </summary>
        /// <param name="name">Name of material</param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="strainU">Ultimate strain</param>
        /// <param name="density"></param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        public RebarMaterial(string name, double elasticModulus, double poisson, 
            double fy, double fu, double strainU, double density, double alfaThermalExpansion, Guid guid)
            : base(name, elasticModulus, poisson, fy, fu, strainU, density, alfaThermalExpansion, guid)
        {
        }


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


        /// <param name="fyk">Yielding stress</param>        
        /// <remarks>Guid setted to new guid. StressStrainDiagram is set to ElastoPlastic. alfaThermalExpansion setted to 0. Epsilon0 equal to fy / E
        /// E = 205GPa, ni = 0.28. Epsilon U is set as 0.075 and fu is set as fyk</remarks>
        public RebarMaterial(double fyk)
            : this(205000, fyk, fyk)
		{
		}

        RebarMaterial(SerializationInfo info, StreamingContext context) 
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
