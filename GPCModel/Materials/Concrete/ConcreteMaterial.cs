using GPC.Utilities.Converters;
using System;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
	[Serializable]
	public abstract class ConcreteMaterial : Material, ISerializable
	{
		#region Public Enum        

		[TypeConverter(typeof(EnumDescriptionTypeConverter))]
		public enum ConcreteTypes
		{
			[Description("Concrete")]
			Concrete,

			[Description("Fiber-Reinforced")]
			FRC,
		}

		[TypeConverter(typeof(EnumDescriptionTypeConverter))]
		public enum CompressionStressStrainDiagrams
		{
			[Description("Parabola-Rectangle")]
			ParabolaRectangle,

			[Description("Bilinear")]
			Bilinear,

			[Description("Stress Block")]
			StressBlock,

			[Description("Non Linear")]
			NonLinear,

			[Description("Generic")]
			Generic,
		}

		[TypeConverter(typeof(EnumDescriptionTypeConverter))]
		public enum TensionStressStrainDiagrams
		{
			[Description("Linear")]
			Linear,

			[Description("Bilinear")]
			Bilinear,

			[Description("Rigid-Plastic")]
			RigidPlastic,

			[Description("Generic")]
			Generic,
		}

		public enum CementType
		{
			ClassR,
			ClassN,
			ClassS,
		}

		#endregion

		#region Variables

		protected ConcreteTypes _concreteType;
		protected CompressionStressStrainDiagrams _compressionStressStrainDiagrams;
		protected TensionStressStrainDiagrams _tensionStressStrainDiagrams;

		#endregion

		#region Properties

		/// <summary>
		/// Type of concrete
		/// </summary>
		public ConcreteTypes ConcreteType
		{
			get => _concreteType;
			set
			{
				SetConcreteType(value);
				RecalculateMechanicalProperties();
			}
		}

		/// <summary>
		/// The compression stress-strain relationship 
		/// </summary>
		public CompressionStressStrainDiagrams CompressionStressStrainDiagram
		{
			get => _compressionStressStrainDiagrams;
			set
			{
				SetCompressionStressStrainDiagram(value);
				RecalculateMechanicalProperties();
			}
		}

		/// <summary>
		/// The tension stress-strain relationship 
		/// </summary>
		public TensionStressStrainDiagrams TensionStressStrainDiagram
		{
			get => _tensionStressStrainDiagrams;
			set
			{
				SetTensionStressStrainDiagrams(value);
				RecalculateMechanicalProperties();
			}
		}

		#endregion

		#region Public Constructor

		public ConcreteMaterial(string name, StressStrainTable stressStrainTableCompression,
			StressStrainTable stressStrainTableTension, double elasticModulusCompression, double elasticModulusTension,
			double poisson, double density, double alfaThermalExpansion)
			: base(name, stressStrainTableCompression, stressStrainTableTension, elasticModulusCompression, elasticModulusTension,
				  poisson, density, alfaThermalExpansion)
		{
		}

		protected ConcreteMaterial(string name, double elasticModulus, double poisson, double density, double alfaThermalExpansion)
			: base(name, elasticModulus, poisson, density, alfaThermalExpansion)
		{
		}

		protected ConcreteMaterial(string name, double poisson, double density, double alfaThermalExpansion)
			: base(name, 0, poisson, density, alfaThermalExpansion)
		{
		}

		protected ConcreteMaterial(string name, double elasticModulusCompression, double elasticModulusTension,
			double strainYCompression, double strainUCompression, double strainYTension, double strainUTension,
			double stressYCompression, double stressUCompression, double stressYTension, double stressUTension,
			StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension, ConcreteTypes concreteType,
			double poisson, double alfaThermalExpansion, double density)
			: base(name, elasticModulusCompression, elasticModulusTension, strainYCompression,
				  strainUCompression, strainYTension, strainUTension, stressYCompression,
				  stressUCompression, stressYTension, stressUTension, stressStrainTableCompression,
				  stressStrainTableTension, poisson, alfaThermalExpansion, density)
		{
			_concreteType = concreteType;
		}

		protected ConcreteMaterial(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			int version;
			try
			{
				version = info.GetInt32("ConcreteMaterialVersion");
			}
			catch (Exception)
			{
				version = 1;
			}

			if (version == 1)
			{
				_stressStrainTableCompression = (StressStrainTable)info.GetValue("TableCompression", typeof(StressStrainTable));
				_stressStrainTableTension = (StressStrainTable)info.GetValue("TableTension", typeof(StressStrainTable));
				_elasticModulusTension = info.GetDouble("ElasticModulusTension");

				_strainYCompression = info.GetDouble("StrainYCompression");
				_strainUCompression = info.GetDouble("StrainUCompression");
				_strainYTension = info.GetDouble("StrainYTension");
				_strainUTension = info.GetDouble("StrainUTension");

				if (_strainYTension < _strainUTension)
					_concreteType = ConcreteTypes.FRC;
				else
					_concreteType = ConcreteTypes.Concrete;

				SetStressProperties();
			}
			if (version >= 2)
			{
				_concreteType = (ConcreteTypes)info.GetValue("ConcreteType", typeof(ConcreteTypes));
			}
			if (version >= 3)
			{
				_compressionStressStrainDiagrams = (CompressionStressStrainDiagrams)info.GetInt32("CompressionStressStrainDiagrams");
				_tensionStressStrainDiagrams = (TensionStressStrainDiagrams)info.GetInt32("TensionStressStrainDiagrams");
			}
		}

		#endregion

		#region Public abstract Methods

		public abstract double CalculateDesignStressConcrete(Standards.Standard standard, double strain);

		public abstract double CalculateDesignCompressiveStrength(Standards.Standard standard);

		public abstract double CalculateDesignTensileStrength(Standards.Standard standard);

		#endregion

		#region Public Methods

		/// <summary>
		/// Override if you want to validate the value before assign it
		/// </summary>
		/// <param name="concreteType">The value to assign</param>
		public virtual void SetConcreteType(ConcreteTypes concreteType)
		{
			_concreteType = concreteType;
		}

		public abstract void RecalculateMechanicalProperties();

		public virtual void SetCompressionStressStrainDiagram(CompressionStressStrainDiagrams compressionStressStrainDiagrams)
		{
			_compressionStressStrainDiagrams = compressionStressStrainDiagrams;
		}

		public virtual void SetTensionStressStrainDiagrams(TensionStressStrainDiagrams tensionStressStrainDiagrams)
		{
			_tensionStressStrainDiagrams = tensionStressStrainDiagrams;
		}

		#endregion

		#region Equals - hashcode - operators

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);

			double version = 3;
			info.AddValue("ConcreteMaterialVersion", version);

			info.AddValue("ConcreteType", _concreteType);
			info.AddValue("CompressionStressStrainDiagrams", _compressionStressStrainDiagrams);
			info.AddValue("TensionStressStrainDiagrams", _tensionStressStrainDiagrams);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;

			return (obj is ConcreteMaterial objCasted) &&
				objCasted._concreteType.Equals(_concreteType) &&
				objCasted._compressionStressStrainDiagrams.Equals(_compressionStressStrainDiagrams) &&
				objCasted._tensionStressStrainDiagrams.Equals(_tensionStressStrainDiagrams) &&
				base.Equals(objCasted);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				hashCode = hashCode * -17 + _concreteType.GetHashCode();
				hashCode = hashCode * -17 + _compressionStressStrainDiagrams.GetHashCode();
				hashCode = hashCode * -17 + _tensionStressStrainDiagrams.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(ConcreteMaterial obj1, ConcreteMaterial obj2)
		{
			if (ReferenceEquals(obj1, obj2))
				return true;

			return obj1.Equals(obj2);
		}

		public static bool operator !=(ConcreteMaterial obj1, ConcreteMaterial obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion
	}
}
