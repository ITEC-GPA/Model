using GPC.Utilities.Attributes;
using GPC.Utilities.Converters;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Glass Material according to EN 16612 - 2019 standard
    /// </summary>
    [Serializable]
    [UI(Description = "Glass EN 16612", Group = "Materials", Kind = "Material")]
    public sealed class GlassMaterialEn16612 : GlassMaterial, IEquatable<GlassMaterialEn16612>
    {
        #region PUBLIC ENUMS

        [Serializable]
        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum GlassTypes
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
        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum SurfaceTreatments
        {
            [Description("As produced")] AsProduced = 0,
            [Description("Sand blasted")] Sandblasted = 1
        }

        [Serializable]
        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum PrestressTypes
        {
            [Description("Annealed glass")] Annealed = 0,
            [Description("Thermally toughened glass")] ThermallyToughened = 1,
            [Description("Heat strengthened glass")] HeatStrengthened = 2,
            [Description("Chemically strengthened glass")] ChemicallyStrengthened = 3,
        }

        [Serializable]
        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum ManufactoringProcesses
        {
            [Description("None")] None = 0,
            [Description("Horizontal toughening")] HorizontalToughening = 1,
            [Description("Vertical toughening")] VerticalToughening = 2,
        }
        #endregion

        #region VARIABLES
        private double _fgk;
        private GlassTypes _glassType;
        private SurfaceTreatments _surfaceTreatment;
        private PrestressTypes _prestressType;
        private ManufactoringProcesses _manufactoringProcess;

        #endregion

        #region PROPERTIES

        public double Fgk => _fgk;

        public GlassTypes GlassType => _glassType;

        public SurfaceTreatments SurfaceTreatment => _surfaceTreatment;

        public PrestressTypes PrestressType => _prestressType;

        public ManufactoringProcesses ManufactoringProcess => _manufactoringProcess;

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
        public GlassMaterialEn16612(string name, double elasticModulus, double poisson, double fgk, GlassTypes glassType, SurfaceTreatments surfaceTreatment, PrestressTypes prestressType, 
                                    ManufactoringProcesses manufactoringProcess, double density, double alfaThermalExpansion)
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
        public GlassMaterialEn16612(string name, double elasticModulus, double poisson, double fgk, GlassTypes glassType, SurfaceTreatments surfaceTreatment, PrestressTypes prestressType, 
                                    ManufactoringProcesses manufactoringProcess, double density, double alfaThermalExpansion, Guid guid)
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
            _glassType = (GlassTypes)info.GetValue("GlassType", typeof(GlassTypes));
            _surfaceTreatment = (SurfaceTreatments)info.GetValue("SurfaceTreatment", typeof(SurfaceTreatments));
            _prestressType = (PrestressTypes)info.GetValue("PrestressType", typeof(PrestressTypes));
            _manufactoringProcess = (ManufactoringProcesses)info.GetValue("ManufactoringProcess", typeof(ManufactoringProcesses));
        }

        #endregion PUBLIC CONSTRUCTORS


        #region Public method override 

        /// <summary>
        /// 
        /// </summary>
        /// <param name="edgeResistance">if true give the resistance on edge</param>
        /// <param name="loadDuration">load duration [seconds]</param>
        /// <returns>The glass resistance according to NCSEA §3.5</returns>
        /// <exception cref="ArgumentException">If <paramref name="loadDuration"/> is lower than zero</exception>
        public override double GetGlassResistance(bool edgeResistance, double loadDuration)
        {
            // TODO: implementare verifica
            return _fgk;
        }


        #endregion

        #region Equals - haschode - operators - serialization

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

        #endregion
    }
}