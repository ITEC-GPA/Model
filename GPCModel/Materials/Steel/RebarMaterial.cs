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
        public static RebarMaterial B450A => new RebarMaterial("B450A", 200000, 0.28, 450, 450, 0.03, 0.007850, 12 * 1e-6, new Guid());
        public static RebarMaterial B450AHardening => new RebarMaterial("B450A Hardening", 200000, 0.28, 450, 540, 0.03, 0.007850, 12 * 1e-6, new Guid());

        public static RebarMaterial B450C => new RebarMaterial("B450C", 200000, 0.28, 450, 450, 0.075, 0.007850, 12 * 1e-6, new Guid());
        public static RebarMaterial B450CHardening => new RebarMaterial("B450C Hardening", 200000, 0.28, 450, 540, 0.075, 0.007850, 12 * 1e-6, new Guid());

        public static RebarMaterial B500A => new RebarMaterial("B500A", 200000, 0.28, 500, 500, 0.03, 0.007850, 12 * 1e-6, new Guid());
        public static RebarMaterial B500AHardening => new RebarMaterial("B500A Hardening", 200000, 0.28, 500, 525, 0.03, 0.007850, 12 * 1e-6, new Guid());

        public static RebarMaterial B500B => new RebarMaterial("B500B", 200000, 0.28, 500, 500, 0.05, 0.007850, 12 * 1e-6, new Guid());
        public static RebarMaterial B500BHardening => new RebarMaterial("B500B Hardening", 200000, 0.28, 500, 550, 0.05, 0.007850, 12 * 1e-6, new Guid());

        public static RebarMaterial B500C => new RebarMaterial("B500C", 200000, 0.28, 500, 500, 0.075, 0.007850, 12 * 1e-6, new Guid());
        public static RebarMaterial B500CHardening => new RebarMaterial("B500C Hardening", 200000, 0.28, 500, 575, 0.075, 0.007850, 12 * 1e-6, new Guid());

        public static RebarMaterial Y1570C => new RebarMaterial("Y1570", 195000, 0.28, 1420, 1420, 0.075, 0.007850, 12 * 1e-6, new Guid());
        public static RebarMaterial Y1570CHardening => new RebarMaterial("Y1570 Hardening", 195000, 0.28, 1420, 1570, 0.075, 0.007850, 12 * 1e-6, new Guid());

        public static RebarMaterial Y1620C => new RebarMaterial("Y1620", 195000, 0.28, 1420, 1420, 0.075, 0.007850, 12 * 1e-6, new Guid());
        public static RebarMaterial Y1620CHardening => new RebarMaterial("Y1620 Hardening", 195000, 0.28, 1420, 1620, 0.075, 0.007850, 12 * 1e-6, new Guid());

        public static RebarMaterial Y1670C => new RebarMaterial("Y1670", 195000, 0.28, 1480, 1480, 0.075, 0.007850, 12 * 1e-6, new Guid());
        public static RebarMaterial Y1670CHardening => new RebarMaterial("Y1670 Hardening", 195000, 0.28, 1480, 1670, 0.075, 0.007850, 12 * 1e-6, new Guid());

        public static RebarMaterial Y1770C => new RebarMaterial("Y1770", 195000, 0.28, 1560, 1560, 0.075, 0.007850, 12 * 1e-6, new Guid());
        public static RebarMaterial Y1770CHardening => new RebarMaterial("Y1770 Hardening", 195000, 0.28, 1560, 1770, 0.075, 0.007850, 12 * 1e-6, new Guid());

        public static RebarMaterial Y1860C => new RebarMaterial("Y1860", 195000, 0.28, 1640, 1640, 0.075, 0.007850, 12 * 1e-6, new Guid());
        public static RebarMaterial Y1860CHardening => new RebarMaterial("Y1860 Hardening", 195000, 0.28, 1640, 1860, 0.075, 0.007850, 12 * 1e-6, new Guid());

        public static RebarMaterial Y1960C => new RebarMaterial("Y1960", 195000, 0.28, 1740, 1740, 0.075, 0.007850, 12 * 1e-6, new Guid());
        public static RebarMaterial Y1960CHardening => new RebarMaterial("Y1960 Hardening", 195000, 0.28, 1740, 1960, 0.075, 0.007850, 12 * 1e-6, new Guid());

        public static RebarMaterial Y2060C => new RebarMaterial("Y2060C", 195000, 0.28, 1850, 1820, 0.075, 0.007850, 12 * 1e-6, new Guid());
        public static RebarMaterial Y2060CHardening => new RebarMaterial("Y2060C Hardening", 195000, 0.28, 1820, 2060, 0.075, 0.007850, 12 * 1e-6, new Guid());

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

        /// <param name="name">Name of material</param>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="density"></param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        /// <remarks>Name is empty</remarks>
        public RebarMaterial(string name, double elasticModulus, double fy, double fu, double poisson = 0.28, double density = 0.007850, double alfaThermalExpansion = 12 * 1e-6)
            : this(name, elasticModulus, poisson, fy, fu, 0.075, density, alfaThermalExpansion, new Guid())
        {
        }

        /// <param name="name">Name of material</param>
        /// <param name="fyk">Yielding stress</param>        
        /// <remarks>Guid setted to new guid. StressStrainDiagram is set to ElastoPlastic. alfaThermalExpansion setted to 0. Epsilon0 equal to fy / E
        /// E = 205GPa, ni = 0.28. Epsilon U is set as 0.075 and fu is set as fyk</remarks>
        public RebarMaterial(string name, double fyk)
            : this(name, 205000, fyk, fyk)
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
