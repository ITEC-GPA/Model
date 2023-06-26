using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
	[Serializable]
	public class Material : ModelObject, ISerializable
	{
		#region Variables

		protected double _elasticModulusCompression;
		protected double _elasticModulusTension;

		protected double _strainYCompression;
		protected double _strainUCompression;

		protected double _strainYTension;
		protected double _strainUTension;

		protected double _stressYCompression;
		protected double _stressUCompression;

		protected double _stressYTension;
		protected double _stressUTension;

		protected double _ni;
		protected double _alfaThermalExpansion;
		protected double _density;

		protected StressStrainTable _stressStrainTableCompression;
		protected StressStrainTable _stressStrainTableTension;

		#endregion

		#region Properties

		/// <summary>
		/// Elastic modulus of material in compression
		/// </summary>
		public virtual double ElasticModulusCompression { get => _elasticModulusCompression; set => _elasticModulusCompression = value; }

		/// <summary>
		/// Elastic modulus of material in tension
		/// </summary>
		public double ElasticModulusTension { get => _elasticModulusTension; set => _elasticModulusTension = value; }

		public double E
		{
			get
			{
				if (_elasticModulusCompression == _elasticModulusTension)
					return _elasticModulusCompression;
				else
					return 0;
			}
		}

		/// <summary>
		/// Strain in the material at the yelding stress 
		/// </summary>
		public double StrainYCompression { get => _strainYCompression; set => _strainYCompression = value; }

		/// <summary>
		/// Ultimate strain in compression
		/// </summary>
		public double StrainUCompression { get => _strainUCompression; set => _strainUCompression = value; }

		/// <summary>
		/// Strain in the material at the yelding stress
		/// </summary>
		public double StrainYTension { get => _strainYTension; set => _strainYTension = value; }

		/// <summary>
		/// Ultimate strain in tension
		/// </summary>
		public double StrainUTension { get => _strainUTension; set => _strainUTension = value; }

		/// <summary>
		/// Yelding stress in compression
		/// </summary>
		public double StressYCompression { get => _stressYCompression; set => _stressYCompression = value; }

		/// <summary>
		/// Ultimate stress in compression
		/// </summary>
		public double StressUCompression { get => _stressUCompression; set => _stressUCompression = value; }

		/// <summary>
		/// Yelding stress in tension
		/// </summary>
		public double StressYTension { get => _stressYTension; set => _stressYTension = value; }

		/// <summary>
		/// Ultimate stress in tension
		/// </summary>
		public double StressUTension { get => _stressUTension; set => _stressUTension = value; }

		/// <summary>
		/// Poisson's ratio of material
		/// </summary>
		public double Ni { get => _ni; set => _ni = value; }

		/// <summary>
		/// Alfa thermal expansion coefficient of material
		/// </summary>
		public double AlfaThermalExpansion { get => _alfaThermalExpansion; set => _alfaThermalExpansion = value; }

		/// <summary>
		/// Density of material
		/// </summary>
		public double Density { get => _density; set => _density = value; }

		/// <summary>
		/// Characteristic Stress strain table in comrpession
		/// </summary>
		public StressStrainTable StressStrainTableCompression { get => _stressStrainTableCompression; set => _stressStrainTableCompression = value; }

		/// <summary>
		/// Characteristic Stress strain table in tension
		/// </summary>
		public StressStrainTable StressStrainTableTension { get => _stressStrainTableTension; set => _stressStrainTableTension = value; }

		#endregion

		#region Public Constructor

		public Material(string name, StressStrainTable stressStrainTableCompression,
			StressStrainTable stressStrainTableTension, double elasticModulusCompression, double elasticModulusTension,
			double poisson, double density, double alfaThermalExpansion)
			: base(name)
		{
			if (poisson > 0.5)
				throw new ArgumentException($"{nameof(poisson)} cannot be greater than 0.5");

			_ni = poisson < 0 ? throw new ArgumentException($"Poisson cannot be lower than zero") : poisson;
			_alfaThermalExpansion = alfaThermalExpansion < 0 ? throw new ArgumentException($"{nameof(alfaThermalExpansion)} cannot be lower than zero") : alfaThermalExpansion;
			_density = density < 0 ? throw new ArgumentException($"{nameof(density)} cannot be lower than zero") : density;

			_stressStrainTableCompression = stressStrainTableCompression;
			_stressStrainTableTension = stressStrainTableTension;

			_elasticModulusTension = elasticModulusTension < 0 ? throw new ArgumentException($"{nameof(elasticModulusTension)} cannot be lower than zero") : elasticModulusTension;
			_elasticModulusCompression = elasticModulusCompression < 0 ? throw new ArgumentException($"{nameof(elasticModulusCompression)} cannot be lower than zero") : elasticModulusCompression;
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="name"></param>
		/// <param name="elasticModulus"> Elastic Modulus [MPa]</param>
		/// <param name="poisson"> Poisson modulus </param>
		/// <param name="alfaThermalExpansion"> Thermal expansion constant</param>
		/// <param name="density"> Density [T/mm^3]</param>
		public Material(string name, double elasticModulus, double poisson, double density, double alfaThermalExpansion)
			: this(name, new StressStrainTable(null, null), new StressStrainTable(null, null), elasticModulus, elasticModulus,
				  poisson, density, alfaThermalExpansion)
		{
		}

		protected Material(string name)
			: base(Guid.NewGuid(), name)
		{

		}

		protected Material(string name, double elasticModulusCompression, double elasticModulusTension,
			double strainYCompression, double strainUCompression, double strainYTension, double strainUTension,
			double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
			StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension,
			double poisson, double alfaThermalExpansion, double density)
			: base(name)
		{
			if (poisson > 0.5)
				throw new ArgumentException($"{nameof(poisson)} cannot be greater than 0.5");

			_elasticModulusTension = elasticModulusTension < 0 ? throw new ArgumentException($"{nameof(elasticModulusTension)} cannot be lower than zero") : elasticModulusTension;
			_elasticModulusCompression = elasticModulusCompression < 0 ? throw new ArgumentException($"{nameof(elasticModulusCompression)} cannot be lower than zero") : elasticModulusCompression;

			_strainYCompression = strainYCompression;
			_strainUCompression = strainUCompression;
			_strainYTension = strainYTension;
			_strainUTension = strainUTension;
			_stressYCompression = stressYCompression;
			_stressUCompression = stressUCompression;
			_stressYTension = stressYTension;
			_stressUTension = stressUTension;

			_ni = poisson < 0 ? throw new ArgumentException($"Poisson cannot be lower than zero") : poisson;
			_alfaThermalExpansion = alfaThermalExpansion < 0 ? throw new ArgumentException($"{nameof(alfaThermalExpansion)} cannot be lower than zero") : alfaThermalExpansion;
			_density = density < 0 ? throw new ArgumentException($"{nameof(density)} cannot be lower than zero") : density;

			_stressStrainTableCompression = stressStrainTableCompression;
			_stressStrainTableTension = stressStrainTableTension;
		}

		protected Material(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			double version;
			try
			{
				version = info.GetInt64("MaterialVersion");
			}
			catch (Exception)
			{
				version = 1;
			}

			if (version >= 2)
			{
				_elasticModulusCompression = info.GetDouble("ElasticModulusCompression");
				_elasticModulusTension = info.GetDouble("ElasticModulusTension");

				_strainYCompression = info.GetDouble("StrainYCompression");
				_strainUCompression = info.GetDouble("StrainUCompression");
				_strainYTension = info.GetDouble("StrainYTension");
				_strainUTension = info.GetDouble("StrainUTension");

				_stressYCompression = info.GetDouble("StressYCompression");
				_stressUCompression = info.GetDouble("StressUCompression");
				_stressYTension = info.GetDouble("StressYTension");
				_stressUTension = info.GetDouble("StressUTension");

				_stressStrainTableCompression = (StressStrainTable)info.GetValue("TableCompression", typeof(StressStrainTable));
				_stressStrainTableTension = (StressStrainTable)info.GetValue("TableTension", typeof(StressStrainTable));
			}
			else
			{
				_elasticModulusCompression = info.GetDouble("ElasticModulus");
			}

			_alfaThermalExpansion = info.GetDouble("AlfaThermalExpansion");
			_density = info.GetDouble("Density");
			_ni = info.GetDouble("Ni");
		}

		#endregion

		#region Public Methods

		public virtual double GetShearModule()
		{
			return ElasticModulusCompression / (2.0 * (1.0 + Ni));
		}

		public virtual Fem.Materials.IsotropicFemMaterial GetIsotropicFemMaterial()
		{
			throw new NotImplementedException("");
		}

		public virtual Fem.Materials.OrthotropicFemMaterial GetOrthotropicFemMaterial()
		{
			throw new NotImplementedException("");
		}

		public void SetName(string name)
		{
			if (name != null)
				_name = name;
		}

		/// <returns>The characteristic stress related to <paramref name="strain"/></returns>
		public double GetStress(double strain)
		{
			if (strain > 0)
			{
				return StressStrainTableTension.GetStress(strain);
			}
			else
			{
				return StressStrainTableCompression.GetStress(strain);
			}
		}

		protected virtual void SetStressProperties()
		{
			_stressYCompression = _stressStrainTableCompression.GetStress(_strainYCompression);
			_stressUCompression = _stressStrainTableCompression.GetStress(_strainUCompression);
			_stressYTension = _stressStrainTableTension.GetStress(_strainYTension);
			_stressUTension = _stressStrainTableTension.GetStress(_strainUTension);
		}

		#endregion

		#region Equals - HashCode - Operators

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			double version = 2;

			info.AddValue("MaterialVersion", version);

			info.AddValue("AlfaThermalExpansion", _alfaThermalExpansion);
			info.AddValue("Density", _density);

			info.AddValue("ElasticModulusCompression", _elasticModulusCompression);
			info.AddValue("ElasticModulusTension", _elasticModulusTension);

			info.AddValue("StrainYCompression", _strainYCompression);
			info.AddValue("StrainUCompression", _strainUCompression);
			info.AddValue("StrainYTension", _strainYTension);
			info.AddValue("StrainUTension", _strainUTension);

			info.AddValue("StressYCompression", _stressYCompression);
			info.AddValue("StressUCompression", _stressUCompression);
			info.AddValue("StressYTension", _stressYTension);
			info.AddValue("StressUTension", _stressUTension);

			info.AddValue("Ni", _ni);
			info.AddValue("TableCompression", _stressStrainTableCompression);
			info.AddValue("TableTension", _stressStrainTableTension);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				hashCode = hashCode * -17 + _elasticModulusCompression.GetHashCode();
				hashCode = hashCode * -17 + _elasticModulusTension.GetHashCode();
				hashCode = hashCode * -17 + _ni.GetHashCode();
				hashCode = hashCode * -17 + _alfaThermalExpansion.GetHashCode();
				hashCode = hashCode * -17 + _density.GetHashCode();
				hashCode = hashCode * -17 + _stressStrainTableCompression.GetHashCode();
				hashCode = hashCode * -17 + _stressStrainTableTension.GetHashCode();
				return hashCode;
			}
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;

			return obj is Material material &&
				   base.Equals(obj) &&
				   _elasticModulusCompression == material._elasticModulusCompression &&
				   _elasticModulusTension == material._elasticModulusTension &&
				   _strainYCompression == material._strainYCompression &&
				   _strainUCompression == material._strainUCompression &&
				   _strainYTension == material._strainYTension &&
				   _strainUTension == material._strainUTension &&
				   _stressYCompression == material._stressYCompression &&
				   _stressUCompression == material._stressUCompression &&
				   _stressYTension == material._stressYTension &&
				   _stressUTension == material._stressUTension &&
				   _ni == material._ni &&
				   _alfaThermalExpansion == material._alfaThermalExpansion &&
				   _density == material._density &&
				   EqualityComparer<StressStrainTable>.Default.Equals(_stressStrainTableCompression, material._stressStrainTableCompression) &&
				   EqualityComparer<StressStrainTable>.Default.Equals(_stressStrainTableTension, material._stressStrainTableTension);
		}

		public static bool operator ==(Material obj1, Material obj2)
		{
			if (ReferenceEquals(obj1, obj2))
				return true;

			if (obj1 is null || obj2 is null)
				return false;

			return obj1.Equals(obj2);
		}

		public static bool operator !=(Material obj1, Material obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion
	}
}
