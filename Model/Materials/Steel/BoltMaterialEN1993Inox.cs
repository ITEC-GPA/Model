using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Stainless steel bolts, grade ISO 3506 (codes: ISO 3506-1:1997, UNI EN ISO 3506-1:2020). The name is "grade-class" (e.g. "A2-70")
    /// </summary>
    [Serializable]
	[UI(Description = "Steel", Group = "Materials", Kind = "Material")]
	public class BoltMaterialEN1993Inox : SteelMaterial
	{
		#region Private variables

		/// <summary>
		/// The steel grade (e.g. "A1", "C3", "F1")
		/// </summary>
		private string _steelGrade;
		/// <summary>
		/// The property class (e.g. "50", "70", "80")
		/// </summary>
		private string _propertyClass;

        #endregion

        #region Public properties

		/// <summary>
		/// Quality and type of stainless bolt material.
		/// For example: "A1", "C3", "F1".
		/// </summary>
		public string SteelGrade => _steelGrade;

		/// <summary>
		/// Material strength class.
		/// For example: "50", "70", "80".
		/// </summary>
		public string PropertyClass => _propertyClass;

        #endregion

        #region Constructor

        /// <summary>
        /// Creates the material with the tables of the given curve; the name is "steelGrade-propertyClass"
        /// </summary>
        /// <param name="steelGrade">The steel grade</param>
        /// <param name="propertyClass">The property class</param>
        /// <param name="elasticModulus">Elastic modulus</param>
        /// <param name="fyb">Characteristic yield strength</param>
        /// <param name="fub">Ultimate strength</param>
        /// <param name="strainU">Ultimate strain</param>
        /// <param name="stressStrainCurveType">The shape of the stress-strain curve</param>
        /// <param name="steelType">The kind of steel</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        public BoltMaterialEN1993Inox(string steelGrade, string propertyClass, double elasticModulus, double fyb, double fub, double strainU = 0.1, StressStrainCurveType stressStrainCurveType = StressStrainCurveType.ElasticPerfectPlastic,
			SteelTypes steelType = SteelTypes.Bolt, double poisson = 0.3, double density = 0.00785, double alfaThermalExpansion = 1.2E-05) 
			: base("", elasticModulus, fyb, fub, strainU, stressStrainCurveType, steelType, poisson, density, alfaThermalExpansion)
		{
			_steelGrade = steelGrade;
			_propertyClass = propertyClass;
			SetName();
        }

		/// <summary>
		/// Creates the material from all the properties (see <see cref="SteelMaterial"/>); the name is "steelGrade-propertyClass"
		/// </summary>
		/// <param name="steelGrade">The steel grade</param>
		/// <param name="propertyClass">The property class</param>
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
		/// <param name="density">The density</param>
		/// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
		public BoltMaterialEN1993Inox(string steelGrade, string propertyClass, double elasticModulusCompression, double elasticModulusTension, double strainYCompression, double strainUCompression, 
			double strainYTension, double strainUTension, double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
			StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTensio, StressStrainCurveType stressStrainCurveType = StressStrainCurveType.ElasticPerfectPlastic, SteelTypes steelType = SteelTypes.Bolt, 
			double poisson = 0.3, double density = 0.00785, double alfaThermalExpansion = 1.2E-05)
			: base("", elasticModulusCompression, elasticModulusTension, strainYCompression, strainUCompression, 
				  strainYTension, strainUTension, stressYCompression, stressUCompression, stressYTension, stressUTension, 
				  stressStrainTableCompression, stressStrainTableTensio, stressStrainCurveType, steelType, poisson, density, alfaThermalExpansion)
        {
            _steelGrade = steelGrade;
            _propertyClass = propertyClass;
            SetName();
        }

		/// <summary>
		/// Deserialization constructor (see <see cref="SteelMaterial"/>): grade and class are not serialized (null)
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected BoltMaterialEN1993Inox(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}

		/// <summary>
		/// Protected constructor (see <see cref="SteelMaterial"/>); the name is "steelGrade-propertyClass"
		/// </summary>
		/// <param name="steelGrade">The steel grade</param>
		/// <param name="propertyClass">The property class</param>
		/// <param name="elasticModulus">Elastic modulus</param>
		/// <param name="poisson">Poisson's ratio</param>
		/// <param name="fyb">Characteristic yield strength</param>
		/// <param name="fub">Ultimate strength</param>
		/// <param name="strainU">Ultimate strain</param>
		/// <param name="stressStrainCurveType">The shape of the stress-strain curve</param>
		/// <param name="steelType">The kind of steel</param>
		/// <param name="density">The density</param>
		/// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
		protected BoltMaterialEN1993Inox(string steelGrade, string propertyClass, double elasticModulus, double poisson, double fyb, double fub, double strainU, StressStrainCurveType stressStrainCurveType,
			SteelTypes steelType, double density, double alfaThermalExpansion) 
			: base("", elasticModulus, poisson, fyb, fub, strainU, stressStrainCurveType, steelType, density, alfaThermalExpansion)
        {
            _steelGrade = steelGrade;
            _propertyClass = propertyClass;
            SetName();
        }

		#endregion

		#region Public Methods Override

		/// <summary>
		/// Equality (see <see cref="SteelMaterial.Equals(object)"/>: grade and class are not compared)
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
		/// Serializes the data of <see cref="SteelMaterial"/> (grade and class are not written: the method sets them again from the name, the
		/// class only if the name has more than two parts)
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			var nameSplit = _name.Split('-');
			if (nameSplit.Length > 1 )
                _steelGrade = nameSplit[0];
            if (nameSplit.Length > 2)
                _propertyClass = nameSplit[1];
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

		#region Protected methoods

		/// <summary>
		/// Name is always a composition of steel grade and property class.
		/// </summary>
		private void SetName() => _name = $"{_steelGrade}-{_propertyClass}";

        #endregion
    }
}
