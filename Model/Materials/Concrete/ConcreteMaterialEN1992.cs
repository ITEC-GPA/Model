using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
	/// <summary>
	/// A plain concrete of EN 1992-1-1 (fiber reinforced concrete is not allowed)
	/// </summary>
	[Serializable]
	[UI(Description = "Concrete", Group = "Materials", Kind = "Material")]
	public class ConcreteMaterialEN1992 : ConcreteMaterialEuropeanCommon, ISerializable
	{
		#region Constructor

		/// <summary>
		/// Creates a concrete from fck
		/// </summary>
		/// <param name="name">The name</param>
		/// <param name="fck">The characteristic compressive strength (the sign is ignored)</param>
		/// <param name="compressionStressStrainDiagrams">The diagram in compression</param>
		/// <param name="poisson">The Poisson's ratio</param>
		/// <param name="density">The density</param>
		/// <param name="alfaThermalExpansion">The coefficient of thermal expansion (see the default of <see cref="ConcreteMaterialEuropeanCommon"/>)</param>
		/// <param name="cementType">The class of cement</param>
		public ConcreteMaterialEN1992(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
			double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 10e-6, CementTypes cementType = CementTypes.ClassN)
			: base(name, fck, compressionStressStrainDiagrams, ConcreteTypes.Concrete, poisson, density, alfaThermalExpansion, cementType)
		{

		}

		/// <summary>
		/// Creates a concrete from generic stress-strain tables
		/// </summary>
		/// <param name="name">The name</param>
		/// <param name="strainYCompression">The strain at the peak compression (0: the strain of the minimum stress)</param>
		/// <param name="strainYTension">The strain at the tensile strength</param>
		/// <param name="stressStrainTableCompression">The table in compression</param>
		/// <param name="stressStrainTableTension">The table in tension</param>
		/// <param name="poisson">The Poisson's ratio</param>
		/// <param name="density">The density</param>
		/// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
		/// <param name="cementType">The class of cement</param>
		public ConcreteMaterialEN1992(string name, double strainYCompression, double strainYTension, StressStrainTable stressStrainTableCompression,
			StressStrainTable stressStrainTableTension,
			double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 10e-6,
			CementTypes cementType = CementTypes.ClassN)
			: base(name, strainYTension, strainYCompression, stressStrainTableCompression, stressStrainTableTension, ConcreteTypes.Concrete, poisson, density, alfaThermalExpansion, cementType)
		{

        }

        /// <summary>
        /// Creates a C25/30 with the parabola-rectangle diagram
        /// </summary>
        /// <param name="name">The name</param>
        public ConcreteMaterialEN1992(string name)
            : this(name, 25, CompressionStressStrainDiagrams.ParabolaRectangle)
        {
        }

        /// <summary>
        /// Deserialization constructor (see <see cref="ConcreteMaterialEuropeanCommon"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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
		/// Allows only plain concrete: the type is always <see cref="ConcreteMaterial.ConcreteTypes.Concrete"/>
		/// </summary>
		/// <param name="concreteType">Ignored</param>
		public override void SetConcreteType(ConcreteTypes concreteType)
		{
			_concreteType = ConcreteTypes.Concrete;
		}

		/// <summary>
		/// Recalculates the mechanical properties and the tables from fck and the diagrams (tension: linear up to fctk,0.05)
		/// </summary>
		public override void RecalculateMechanicalProperties()
		{
			SetMechanicalProperties(_fck, 0, 0, 0, 0, _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

			SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
			SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);

			SetStressProperties();
        }

        /// <summary>
        /// Sets the diagram in compression; the generic diagram is replaced by the parabola-rectangle
        /// </summary>
        /// <param name="compressionStressStrainDiagrams">The diagram</param>
        public override void SetCompressionStressStrainDiagram(CompressionStressStrainDiagrams compressionStressStrainDiagrams)
        {
            if (compressionStressStrainDiagrams != CompressionStressStrainDiagrams.Generic)
                _compressionStressStrainDiagrams = compressionStressStrainDiagrams;
            else
                _compressionStressStrainDiagrams = CompressionStressStrainDiagrams.ParabolaRectangle;
        }

        #region Equals, hashcode, operators

        /// <summary>
        /// Serializes the data of <see cref="ConcreteMaterialEuropeanCommon"/>
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		/// <summary>
		/// Equality (see <see cref="ConcreteMaterialEuropeanCommon.Equals(object)"/>)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal concrete of EN 1992</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;

			return (obj is ConcreteMaterialEN1992 objCasted) && base.Equals(objCasted);
		}

		/// <summary>
		/// The hash code (see <see cref="ConcreteMaterialEuropeanCommon.GetHashCode"/>)
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode(); ;
				return hashCode;
			}
		}

		/// <summary>
		/// Equality operator (see <see cref="Equals(object)"/>)
		/// </summary>
		/// <param name="obj1">The first concrete (not null, unless both are null)</param>
		/// <param name="obj2">The second concrete</param>
		/// <returns>True if the materials are equal</returns>
		public static bool operator ==(ConcreteMaterialEN1992 obj1, ConcreteMaterialEN1992 obj2)
		{
			if (ReferenceEquals(obj1, obj2))
				return true;

			return obj1.Equals(obj2);
		}

		/// <summary>
		/// Inequality operator (see <see cref="Equals(object)"/>)
		/// </summary>
		/// <param name="obj1">The first concrete</param>
		/// <param name="obj2">The second concrete</param>
		/// <returns>True if the materials are different</returns>
		public static bool operator !=(ConcreteMaterialEN1992 obj1, ConcreteMaterialEN1992 obj2)
		{
			return !(obj1 == obj2);
		}


		#endregion
	}
}
