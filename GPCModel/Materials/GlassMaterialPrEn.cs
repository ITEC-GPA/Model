using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    public class GlassMaterialPrEn : GlassMaterial
    {
        #region PUBLIC ENUMS
        [Serializable]
        public enum GlassType
        {
            [Description("Float")] FloatGlass = 0,
            [Description("Drawn sheet")] DrawnSheetGlass = 1,
            [Description("Enamelled float or drawn sheet")] EnamelledFloatOrDrawn = 2,
            [Description("Patterned")] PatternedGlass = 3,
            [Description("Enamelled patterned")] EnamelledPatternedGlass = 4,
            [Description("Polished wired")] PolishedWiredGlass = 5,
            [Description("Patterned wired")] PatternedWiredGlass = 6
        }

        [Serializable]
        public enum SurfaceTreatment
        {
            [Description("As produced")] AsProduced = 0,
            [Description("Sand blasted")] Sandblasted = 1
        }

        [Serializable]
        public enum PrestressType
        {
            [Description("Annealed glass")] Annealed = 0,
            [Description("Thermally toughened glass")] ThermallyToughened = 1,
            [Description("Heat strengthened glass")] HeatStrengthened = 2,
            [Description("Chemically strengthened glass")] ChemicallyStrengthened = 3,
        }

        [Serializable]
        public enum ManufactoringProcess
        {
            [Description("None")] None = 0,
            [Description("Horizontal toughening")] HorizontalToughening = 1,
            [Description("Vertical toughening")] VerticalToughening = 2,
        }
        #endregion

        #region VARIABLES
        private double _fgk;
        private GlassType _glassType;
        private SurfaceTreatment _surfaceTreatment;
        private PrestressType _prestressType;
        private ManufactoringProcess _manufactoringProcess;
        #endregion

        #region PROPERTIES
        public double Fgk => _fgk;
        public GlassType GetGlassType => _glassType;
        public SurfaceTreatment GetSurfaceTreatment => _surfaceTreatment;
        public PrestressType GetPrestressType => _prestressType;
        public ManufactoringProcess GetManufactoringProcess => _manufactoringProcess;
        #endregion

        #region PUBLIC CONSTRUCTORS

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elasticModulus">Elastic modulus of the glass</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="fgk">Characeristic value of bending strength of annealed glass</param>
        /// <param name="density">Density of the material</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        public GlassMaterialPrEn(double elasticModulus, double poisson, double fgk, GlassType glassType, SurfaceTreatment surfaceTreatment, PrestressType prestressType, ManufactoringProcess manufactoringProcess, 
                                double density, double alfaThermalExpansion)
            : this(elasticModulus, poisson, fgk, glassType, surfaceTreatment, prestressType, manufactoringProcess, density, alfaThermalExpansion, Guid.Empty)
        {
            // TODO: ke factors
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Elastic modulus of the glass</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="fgk">Characeristic value of bending strength of annealed glass</param>
        /// <param name="density">Density of the material</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        public GlassMaterialPrEn(double elasticModulus, double poisson, double fgk, GlassType glassType, SurfaceTreatment surfaceTreatment, PrestressType prestressType, ManufactoringProcess manufactoringProcess,
                                 double density, double alfaThermalExpansion, Guid guid)
            : base(elasticModulus, poisson, density, alfaThermalExpansion, guid)
        {
            if (fgk <= 0.001)
            {
                throw new ArgumentException($"{nameof(fgk)} cannot be zero or lower");
            }
            this._fgk = fgk;

            this._glassType = glassType;
            this._surfaceTreatment = surfaceTreatment;
            this._prestressType = prestressType;
            this._manufactoringProcess = manufactoringProcess;
        }

        public GlassMaterialPrEn(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _fgk = info.GetDouble("Fgk");
            _glassType = (GlassType)info.GetValue("GlassType", typeof(GlassType));
            _surfaceTreatment = (SurfaceTreatment)info.GetValue("SurfaceTreatment", typeof(SurfaceTreatment));
            _prestressType = (PrestressType)info.GetValue("PrestressType", typeof(PrestressType));
            _manufactoringProcess = (ManufactoringProcess)info.GetValue("ManufactoringProcess", typeof(ManufactoringProcess));
        }

        #endregion PUBLIC CONSTRUCTORS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Fgk", _fgk);
            info.AddValue("GlassType", _glassType);
            info.AddValue("SurfaceTreatment", _surfaceTreatment);
            info.AddValue("PrestressType", _prestressType);
            info.AddValue("ManufactoringProcess", _manufactoringProcess);
        }
    }
}