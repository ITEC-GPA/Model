using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// Stainless Steel Grade ISO 3506.
	/// Codes: ISO 3506-1:1997, UNI EN ISO 3506-1:2020.
    /// </summary>
    [Serializable]
	[UI(Description = "Steel", Group = "Materials", Kind = "Material")]
	public class BoltMaterialEN1993Inox : SteelMaterial
	{
		#region Private variables

		private string _steelGrade;
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

        public BoltMaterialEN1993Inox(string steelGrade, string propertyClass, double elasticModulus, double fyb, double fub, double strainU = 0.1, StressStrainCurveType stressStrainCurveType = StressStrainCurveType.ElasticPerfectPlastic,
			SteelTypes steelType = SteelTypes.Bolt, double poisson = 0.3, double density = 0.00785, double alfaThermalExpansion = 1.2E-05) 
			: base("", elasticModulus, fyb, fub, strainU, stressStrainCurveType, steelType, poisson, density, alfaThermalExpansion)
		{
			_steelGrade = steelGrade;
			_propertyClass = propertyClass;
			SetName();
        }

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

		protected BoltMaterialEN1993Inox(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}

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

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			var nameSplit = _name.Split('-');
			if (nameSplit.Length > 1 )
                _steelGrade = nameSplit[0];
            if (nameSplit.Length > 2)
                _propertyClass = nameSplit[1];
        }

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
