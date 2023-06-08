using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
	[Serializable]
	[UI(Description = "Concrete", Group = "Materials", Kind = "Material")]
	public class ConcreteMaterialEN1992 : ConcreteMaterialEuropeanCommon, ISerializable
	{
		#region Constructor

		public ConcreteMaterialEN1992(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
			double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6, CementTypes cementType = CementTypes.ClassN)
			: base(name, fck, compressionStressStrainDiagrams, ConcreteTypes.Concrete, poisson, density, alfaThermalExpansion, cementType)
		{

		}

		public ConcreteMaterialEN1992(string name, double strainYCompression, double strainYTension, StressStrainTable stressStrainTableCompression,
			StressStrainTable stressStrainTableTension,
			double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6,
			CementTypes cementType = CementTypes.ClassN)
			: base(name, strainYTension, strainYCompression, stressStrainTableCompression, stressStrainTableTension, ConcreteTypes.Concrete, poisson, density, alfaThermalExpansion, cementType)
		{

		}

		protected ConcreteMaterialEN1992(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			int version;
			try
			{
				version = info.GetInt32("ConcreteMaterialEN1992Version");
			}
			catch (Exception)
			{
				version = 1;
			}
		}

		#endregion

		/// <summary>
		/// Allows only Normal concrete
		/// </summary>
		/// <param name="concreteType"></param>
		public override void SetConcreteType(ConcreteTypes concreteType)
		{
			_concreteType = ConcreteTypes.Concrete;
		}

		public override void RecalculateMechanicalProperties()
		{
			SetMechanicalProperties(_fck, 0, 0, 0, 0, _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

			SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
			SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);

			SetStressProperties();
        }

        public override void SetCompressionStressStrainDiagram(CompressionStressStrainDiagrams compressionStressStrainDiagrams)
        {
            if (compressionStressStrainDiagrams != CompressionStressStrainDiagrams.Generic)
                _compressionStressStrainDiagrams = compressionStressStrainDiagrams;
            else
                _compressionStressStrainDiagrams = CompressionStressStrainDiagrams.ParabolaRectangle;
        }

        #region Equals, hashcode, operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;

			return (obj is ConcreteMaterialEN1992 objCasted) && base.Equals(objCasted);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode(); ;
				return hashCode;
			}
		}

		public static bool operator ==(ConcreteMaterialEN1992 obj1, ConcreteMaterialEN1992 obj2)
		{
			if (ReferenceEquals(obj1, obj2))
				return true;

			return obj1.Equals(obj2);
		}

		public static bool operator !=(ConcreteMaterialEN1992 obj1, ConcreteMaterialEN1992 obj2)
		{
			return !(obj1 == obj2);
		}


		#endregion
	}
}
