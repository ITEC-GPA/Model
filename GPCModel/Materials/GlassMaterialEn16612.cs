using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Glass Material according to EN 16612 - 2019 standard
    /// </summary>
    [Serializable]
    public sealed class GlassMaterialEn16612 : GlassMaterial, IEquatable<GlassMaterialEn16612>
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
        /// <param name="elasticModulus">Elastic modulus of the glass [MPa]</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="fgk">Characeristic value of bending strength of annealed glass [MPa]</param>
        /// <param name="density">Density of the material [T/mm^3]</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        public GlassMaterialEn16612(string name, double elasticModulus, double poisson, double fgk, GlassType glassType, SurfaceTreatment surfaceTreatment, PrestressType prestressType, 
                                    ManufactoringProcess manufactoringProcess, double density, double alfaThermalExpansion)
            : this(name, elasticModulus, poisson, fgk, glassType, surfaceTreatment, prestressType, manufactoringProcess, density, alfaThermalExpansion, Guid.NewGuid())
        {
            // TODO: ke factors
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Elastic modulus of the glass [MPa]</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="fgk">Characeristic value of bending strength of annealed glass [MPa]</param>
        /// <param name="density">Density of the material [T/mm^3]</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        public GlassMaterialEn16612(string name, double elasticModulus, double poisson, double fgk, GlassType glassType, SurfaceTreatment surfaceTreatment, PrestressType prestressType, 
                                    ManufactoringProcess manufactoringProcess, double density, double alfaThermalExpansion, Guid guid)
            : base(name, elasticModulus, poisson, density, alfaThermalExpansion, guid)
        {
            _fgk = fgk < 0.001 ? throw new ArgumentException($"{nameof(fgk)} cannot be zero or lower") : fgk;

            this._glassType = glassType;
            this._surfaceTreatment = surfaceTreatment;
            this._prestressType = prestressType;
            this._manufactoringProcess = manufactoringProcess;
        }

        public GlassMaterialEn16612(SerializationInfo info, StreamingContext context)
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

        public bool Equals(GlassMaterialEn16612 other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._fgk.Equals(_fgk) &&
                                        other._glassType.Equals(_glassType) &&
                                        other._surfaceTreatment.Equals(_surfaceTreatment) &&
                                        other._prestressType.Equals(_prestressType) &&
                                        other._manufactoringProcess.Equals(_manufactoringProcess) &&
                                        base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return Equals(obj as GlassMaterialEn16612);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _fgk.GetHashCode();
            hashCode = hashCode * -17 + _glassType.GetHashCode();
            hashCode = hashCode * -17 + _surfaceTreatment.GetHashCode();
            hashCode = hashCode * -17 + _prestressType.GetHashCode();
            hashCode = hashCode * -17 + _manufactoringProcess.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(GlassMaterialEn16612 obj1, GlassMaterialEn16612 obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(GlassMaterialEn16612 obj1, GlassMaterialEn16612 obj2)
        {
            return !(obj1 == obj2);
        }
    }
}