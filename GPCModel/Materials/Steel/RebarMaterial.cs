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
        public static RebarMaterial B450A => new RebarMaterial("B450A", 200000, 450, 450, 0.03);
        public static RebarMaterial B450AHardening => new RebarMaterial("B450A Hardening", 200000, 450, 540, 0.03);

        public static RebarMaterial B450C => new RebarMaterial("B450C", 200000, 450, 450, 0.075);
        public static RebarMaterial B450CHardening => new RebarMaterial("B450C Hardening", 200000, 450, 540, 0.075);

        public static RebarMaterial B500A => new RebarMaterial("B500A", 200000, 500, 500, 0.03);
        public static RebarMaterial B500AHardening => new RebarMaterial("B500A Hardening", 200000, 500, 525, 0.03);

        public static RebarMaterial B500B => new RebarMaterial("B500B", 200000, 500, 500, 0.05);
        public static RebarMaterial B500BHardening => new RebarMaterial("B500B Hardening", 200000, 500, 550, 0.05);

        public static RebarMaterial B500C => new RebarMaterial("B500C", 200000, 500, 500, 0.075);
        public static RebarMaterial B500CHardening => new RebarMaterial("B500C Hardening", 200000, 500, 575, 0.075);

        public static RebarMaterial Y1570C => new RebarMaterial("Y1570", 195000, 1420, 1420, 0.075);
        public static RebarMaterial Y1570CHardening => new RebarMaterial("Y1570 Hardening", 195000, 1420, 1570, 0.075);

        public static RebarMaterial Y1620C => new RebarMaterial("Y1620", 195000, 1420, 1420, 0.075);
        public static RebarMaterial Y1620CHardening => new RebarMaterial("Y1620 Hardening", 195000, 1420, 1620, 0.075);

        public static RebarMaterial Y1670C => new RebarMaterial("Y1670", 195000, 1480, 1480, 0.075);
        public static RebarMaterial Y1670CHardening => new RebarMaterial("Y1670 Hardening", 195000, 1480, 1670, 0.075);

        public static RebarMaterial Y1770C => new RebarMaterial("Y1770", 195000, 1560, 1560, 0.075);
        public static RebarMaterial Y1770CHardening => new RebarMaterial("Y1770 Hardening", 195000, 1560, 1770, 0.075);

        public static RebarMaterial Y1860C => new RebarMaterial("Y1860", 195000, 1640, 1640, 0.075);
        public static RebarMaterial Y1860CHardening => new RebarMaterial("Y1860 Hardening", 195000, 1640, 1860, 0.075);

        public static RebarMaterial Y1960C => new RebarMaterial("Y1960", 195000, 1740, 1740, 0.075);
        public static RebarMaterial Y1960CHardening => new RebarMaterial("Y1960 Hardening", 195000, 1740, 1960, 0.075);

        public static RebarMaterial Y2060C => new RebarMaterial("Y2060C", 195000, 1850, 1820, 0.075);
        public static RebarMaterial Y2060CHardening => new RebarMaterial("Y2060C Hardening", 195000, 1820, 2060, 0.075);

        public static RebarMaterial Grade40 => new RebarMaterial("Grade 40", 199947.9615, 413.6854, 413.6854, 0.010);
        public static RebarMaterial Grade40Hardening => new RebarMaterial("Grade 40 Hardening", 199947.9615, 413.6854, 455.05398, 0.010);

        public static RebarMaterial Grade60 => new RebarMaterial("Grade 60", 199947.9615, 551.58058, 551.58058, 0.010);
        public static RebarMaterial Grade60Hardening => new RebarMaterial("Grade 60 Hardening", 199947.9615, 551.58058, 606.73864, 0.010);

        public static RebarMaterial Grade80 => new RebarMaterial("Grade 80", 199947.9615, 689.47573, 689.47573, 0.010);
        public static RebarMaterial Grade80Hardening => new RebarMaterial("Grade 80 Hardening", 199947.9615, 689.47573, 758.423302, 0.010);

        public static RebarMaterial Grade100 => new RebarMaterial("Grade 100", 199947.9615, 792.897089, 792.897089, 0.010);
        public static RebarMaterial Grade100Hardening => new RebarMaterial("Grade 100 Hardening", 199947.9615, 792.897089, 872.186798, 0.010);

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
            : base(name, elasticModulus, poisson, fy, fu, strainU, density, alfaThermalExpansion, new Guid())
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
