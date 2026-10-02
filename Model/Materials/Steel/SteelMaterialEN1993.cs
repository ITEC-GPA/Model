using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
	/// <summary>
	/// A structural steel of EN 1993-1-1
	/// </summary>
	[Serializable]
	[UI(Description = "Steel", Group = "Materials", Kind = "Material")]
	public class SteelMaterialEN1993 : SteelMaterial
	{
		#region Constructor

		/// <summary>
		/// Creates the material with the tables of the given curve (see <see cref="SteelMaterial"/>)
		/// </summary>
		/// <param name="name">The name</param>
		/// <param name="elasticModulus">Elastic modulus</param>
		/// <param name="fyk">Characteristic yield strength</param>
		/// <param name="fu">Ultimate strength</param>
		/// <param name="strainU">Ultimate strain</param>
		/// <param name="stressStrainCurveType">The shape of the stress-strain curve</param>
		/// <param name="steelType">The kind of steel</param>
		/// <param name="poisson">The Poisson's ratio</param>
		/// <param name="density">The density</param>
		/// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
		public SteelMaterialEN1993(string name, double elasticModulus, double fyk, double fu, double strainU = 0.1, StressStrainCurveType stressStrainCurveType = StressStrainCurveType.ElasticPerfectPlastic,
			SteelTypes steelType = SteelTypes.Structural, double poisson = 0.3, double density = SteelDensity, double alfaThermalExpansion = 1.2E-05) 
			: base(name, elasticModulus, fyk, fu, strainU, stressStrainCurveType, steelType, poisson, density, alfaThermalExpansion)
		{
		}

		/// <summary>
		/// Creates a structural steel S235 (see <see cref="SteelMaterial(string, SteelMaterial.SteelTypes)"/>)
		/// </summary>
		/// <param name="name">The name</param>
		public SteelMaterialEN1993(string name)
			: base(name, SteelTypes.Structural)
		{
		}

        /// <summary>
        /// Creates the material from all the properties (see <see cref="SteelMaterial"/>)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="elasticModulusCompression">The elastic modulus in compression</param>
        /// <param name="elasticModulusTension">The elastic modulus in tension</param>
        /// <param name="strainYCompression">The strain at the yield stress in compression</param>
        /// <param name="strainUCompression">The ultimate strain in compression</param>
        /// <param name="strainYTension">The strain at the yield stress in tension</param>
        /// <param name="strainUTension">The ultimate strain in tension</param>
        /// <param name="stressYCompression">The yield stress in compression</param>
        /// <param name="stressUCompression">The ultimate stress in compression</param>
        /// <param name="stressYTension">The yield stress in tension</param>
        /// <param name="stressUTension">The ultimate stress in tension</param>
        /// <param name="stressStrainTableCompression">The characteristic stress-strain table in compression</param>
        /// <param name="stressStrainTableTensio">The characteristic stress-strain table in tension</param>
        /// <param name="stressStrainCurveType">Not used (see the constructor of <see cref="SteelMaterial"/>)</param>
        /// <param name="steelType">The kind of steel</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density (see the defect of the constructor of <see cref="SteelMaterial"/>)</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        public SteelMaterialEN1993(string name, double elasticModulusCompression, double elasticModulusTension, double strainYCompression, double strainUCompression, 
			double strainYTension, double strainUTension, double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
			StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio, StressStrainCurveType stressStrainCurveType = StressStrainCurveType.ElasticHardening, SteelTypes steelType = SteelTypes.Structural, 
			double poisson = 0.3, double density = SteelDensity, double alfaThermalExpansion = 1.2E-05)
			: base(name, elasticModulusCompression, elasticModulusTension, strainYCompression, strainUCompression, 
				  strainYTension, strainUTension, stressYCompression, stressUCompression, stressYTension, stressUTension, 
				  stressStrainTableCompression, stressStrainTableTensio, stressStrainCurveType, steelType, poisson, density, alfaThermalExpansion)
		{
		}

		/// <summary>
		/// Deserialization constructor (see <see cref="SteelMaterial"/>)
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected SteelMaterialEN1993(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}

		/// <summary>
		/// Protected constructor (see <see cref="SteelMaterial"/>)
		/// </summary>
		/// <param name="name">The name</param>
		/// <param name="elasticModulus">Elastic modulus</param>
		/// <param name="poisson">Poisson's ratio</param>
		/// <param name="fyk">Characteristic yield strength</param>
		/// <param name="fu">Ultimate strength</param>
		/// <param name="strainU">Ultimate strain</param>
		/// <param name="stressStrainCurveType">The shape of the stress-strain curve</param>
		/// <param name="steelType">The kind of steel</param>
		/// <param name="density">The density</param>
		/// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
		protected SteelMaterialEN1993(string name, double elasticModulus, double poisson, double fyk, double fu, double strainU, StressStrainCurveType stressStrainCurveType,
			SteelTypes steelType, double density, double alfaThermalExpansion) 
			: base(name, elasticModulus, poisson, fyk, fu, strainU, stressStrainCurveType, steelType, density, alfaThermalExpansion)
		{
		}

		#endregion

		#region Public Methods Override

		/// <summary>
		/// Equality (see <see cref="SteelMaterial.Equals(object)"/>)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal material</returns>
		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		/// <summary>
		/// The hash code (see <see cref="SteelMaterial.GetHashCode"/>)
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		/// <summary>
		/// Serializes the data of <see cref="SteelMaterial"/>
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		/// <summary>
		/// The name of the type (see <see cref="object.ToString"/>)
		/// </summary>
		/// <returns>The text</returns>
		public override string ToString()
		{
			return base.ToString();
		}

		#endregion
	}
}
