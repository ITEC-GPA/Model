using GPC.Utilities.Attributes;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
	/// <summary>
	/// Glass material according to EN 16612:2019: fgk and the classification of the glass (type, surface treatment, prestress, toughening process)
	/// </summary>
	[Serializable]
	[UI(Description = "Glass EN 16612", Group = "Materials", Kind = "Material")]
	public sealed class GlassMaterialEn16612 : GlassMaterial, IEquatable<GlassMaterialEn16612>
	{
		#region PUBLIC ENUMS

		/// <summary>
		/// The types of glass (EN 16612 table 3)
		/// </summary>
		[Serializable]
		public enum GlassTypes
		{
			/// <summary>Float glass</summary>
			[Description("Float")] FloatGlass = 0,
			/// <summary>Drawn sheet glass</summary>
			[Description("Drawn sheet")] DrawnSheetGlass = 1,
			/// <summary>Enamelled float or drawn sheet glass</summary>
			[Description("Enamelled float or drawn sheet")] EnamelledFloatOrDrawn = 2,
			/// <summary>Patterned glass</summary>
			[Description("Patterned")] PatternedGlass = 3,
			/// <summary>Enamelled patterned glass</summary>
			[Description("Enamelled patterned")] EnamelledPatternedGlass = 4,
			/// <summary>Polished wired glass</summary>
			[Description("Polished wired")] PolishedWiredGlass = 5,
			/// <summary>Patterned wired glass</summary>
			[Description("Patterned wired")] PatternedWiredGlass = 6
		}

		/// <summary>
		/// The surface treatments
		/// </summary>
		[Serializable]
		public enum SurfaceTreatments
		{
			/// <summary>As produced</summary>
			[Description("As produced")] AsProduced = 0,
			/// <summary>Sand blasted</summary>
			[Description("Sand blasted")] Sandblasted = 1
		}

		/// <summary>
		/// The prestress of the glass
		/// </summary>
		[Serializable]
		public enum PrestressTypes
		{
			/// <summary>Annealed glass</summary>
			[Description("Annealed glass")] Annealed = 0,
			/// <summary>Thermally toughened glass</summary>
			[Description("Thermally toughened glass")] ThermallyToughened = 1,
			/// <summary>Heat strengthened glass</summary>
			[Description("Heat strengthened glass")] HeatStrengthened = 2,
			/// <summary>Chemically strengthened glass</summary>
			[Description("Chemically strengthened glass")] ChemicallyStrengthened = 3,
		}

		/// <summary>
		/// The toughening processes
		/// </summary>
		[Serializable]
		public enum ManufactoringProcesses
		{
			/// <summary>None</summary>
			[Description("None")] None = 0,
			/// <summary>Horizontal toughening</summary>
			[Description("Horizontal toughening")] HorizontalToughening = 1,
			/// <summary>Vertical toughening</summary>
			[Description("Vertical toughening")] VerticalToughening = 2,
		}

		#endregion

		#region VARIABLES

		/// <summary>
		/// The characteristic bending strength of annealed glass
		/// </summary>
		private double _fgk;
		/// <summary>
		/// The type of glass
		/// </summary>
		private GlassTypes _glassType;
		/// <summary>
		/// The surface treatment
		/// </summary>
		private SurfaceTreatments _surfaceTreatment;
		/// <summary>
		/// The prestress
		/// </summary>
		private PrestressTypes _prestressType;
		/// <summary>
		/// The toughening process
		/// </summary>
		private ManufactoringProcesses _manufactoringProcess;

		#endregion

		#region PROPERTIES

		/// <summary>
		/// Characteristic value of bending strength of annealed glass [MPa]
		/// </summary>
		public double Fgk { get => _fgk; set => _fgk = value; }

		/// <summary>
		/// The type of glass
		/// </summary>
		public GlassTypes GlassType { get => _glassType; set => _glassType = value; }

		/// <summary>
		/// The surface treatment
		/// </summary>
		public SurfaceTreatments SurfaceTreatment { get => _surfaceTreatment; set => _surfaceTreatment = value; }

		/// <summary>
		/// The prestress
		/// </summary>
		public PrestressTypes PrestressType { get => _prestressType; set => _prestressType = value; }

		/// <summary>
		/// The toughening process
		/// </summary>
		public ManufactoringProcesses ManufactoringProcess { get => _manufactoringProcess; set => _manufactoringProcess = value; }

		#endregion

		#region PUBLIC CONSTRUCTORS

		/// <summary>
		/// Creates a glass of EN 16612 (float, as produced, annealed and horizontal toughening by default)
		/// </summary>
		/// <param name="name">The name</param>
		/// <param name="elasticModulus">Elastic modulus of the glass [MPa]</param>
		/// <param name="poisson">Poisson's ratio of the glass</param>
		/// <param name="fgk">Characteristic value of bending strength of annealed glass [MPa]</param>
		/// <param name="glassType">The type of glass</param>
		/// <param name="surfaceTreatment">The surface treatment</param>
		/// <param name="prestressType">The prestress</param>
		/// <param name="manufactoringProcess">The toughening process</param>
		/// <param name="density">Density of the material [T/mm^3]</param>
		/// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
		/// <exception cref="ArgumentException">If fgk is lower than 0.001 (or the elastic constants are invalid)</exception>
		public GlassMaterialEn16612(string name, double elasticModulus, double poisson, double fgk, double density, double alfaThermalExpansion,
			GlassTypes glassType = GlassTypes.FloatGlass, SurfaceTreatments surfaceTreatment = SurfaceTreatments.AsProduced,
			PrestressTypes prestressType = PrestressTypes.Annealed, ManufactoringProcesses manufactoringProcess = ManufactoringProcesses.HorizontalToughening)
			: this(name, elasticModulus, poisson, fgk, glassType, surfaceTreatment, prestressType, manufactoringProcess, density, alfaThermalExpansion)
		{

		}

		/// <summary>
		/// Creates a glass of EN 16612
		/// </summary>
		/// <param name="name">The name</param>
		/// <param name="elasticModulus">Elastic modulus of the glass [MPa]</param>
		/// <param name="poisson">Poisson's ratio of the glass</param>
		/// <param name="fgk">Characteristic value of bending strength of annealed glass [MPa]</param>
		/// <param name="glassType">The type of glass</param>
		/// <param name="surfaceTreatment">The surface treatment</param>
		/// <param name="prestressType">The prestress</param>
		/// <param name="manufactoringProcess">The toughening process</param>
		/// <param name="density">Density of the material [T/mm^3]</param>
		/// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
		/// <exception cref="ArgumentException">If fgk is lower than 0.001 (or the elastic constants are invalid)</exception>
		public GlassMaterialEn16612(string name, double elasticModulus, double poisson, double fgk, GlassTypes glassType, SurfaceTreatments surfaceTreatment,
			PrestressTypes prestressType, ManufactoringProcesses manufactoringProcess, double density, double alfaThermalExpansion)
			: base(name, elasticModulus, poisson, density, alfaThermalExpansion)
		{
			_fgk = fgk < 0.001 ? throw new ArgumentException($"{nameof(fgk)} cannot be zero or lower") : fgk;

			_glassType = glassType;
			_surfaceTreatment = surfaceTreatment;
			_prestressType = prestressType;
			_manufactoringProcess = manufactoringProcess;
		}


		/// <summary>
		/// Deserialization constructor: reads the data of <see cref="Material"/>, fgk and the classification
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		private GlassMaterialEn16612(SerializationInfo info, StreamingContext context)
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
		/// The strength of the glass: fgk (the rules of EN 16612 are not implemented yet)
		/// </summary>
		/// <param name="edgeResistance">Not used</param>
		/// <param name="loadDuration">Not used</param>
		/// <returns>fgk</returns>
		public override double GetGlassResistance(bool edgeResistance, double loadDuration)
		{
			// TODO: implementare verifica
			return _fgk;
		}

		#endregion

		#region Equals - haschode - operators - serialization

		/// <summary>
		/// Serializes the data of <see cref="Material"/>, fgk and the classification
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Fgk", _fgk);
			info.AddValue("GlassType", _glassType);
			info.AddValue("SurfaceTreatment", _surfaceTreatment);
			info.AddValue("PrestressType", _prestressType);
			info.AddValue("ManufactoringProcess", _manufactoringProcess);
		}

		/// <summary>
		/// Equality of fgk, classification and the data of <see cref="Material"/>
		/// </summary>
		/// <param name="other">The glass to compare</param>
		/// <returns>True if the materials are equal</returns>
		public bool Equals(GlassMaterialEn16612 other)
		{
			if (ReferenceEquals(this, other))
				return true;

			return !(other is null) &&
				other._fgk.Equals(_fgk) &&
				other._glassType.Equals(_glassType) &&
				other._surfaceTreatment.Equals(_surfaceTreatment) &&
				other._prestressType.Equals(_prestressType) &&
				other._manufactoringProcess.Equals(_manufactoringProcess) &&
				base.Equals(other);
		}

		/// <summary>
		/// Equality with another object (see <see cref="Equals(GlassMaterialEn16612)"/>)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal glass</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;
			return Equals(obj as GlassMaterialEn16612);
		}

		/// <summary>
		/// The hash code of the data of <see cref="Material"/>, fgk and classification
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			unchecked
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
		}

		/// <summary>
		/// Equality operator (see <see cref="Equals(object)"/>); two null materials are equal
		/// </summary>
		/// <param name="obj1">The first material</param>
		/// <param name="obj2">The second material</param>
		/// <returns>True if the materials are equal</returns>
		public static bool operator ==(GlassMaterialEn16612 obj1, GlassMaterialEn16612 obj2)
		{
			if (ReferenceEquals(obj1, obj2))
				return true;

			if (obj1 is null || obj2 is null)
				return false;

			return obj1.Equals(obj2);
		}

		/// <summary>
		/// Inequality operator (see <see cref="Equals(object)"/>)
		/// </summary>
		/// <param name="obj1">The first material</param>
		/// <param name="obj2">The second material</param>
		/// <returns>True if the materials are different</returns>
		public static bool operator !=(GlassMaterialEn16612 obj1, GlassMaterialEn16612 obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion
	}
}
