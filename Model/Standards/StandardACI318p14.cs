using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the ACI 318-14
	/// </summary>
	[Serializable]
	public class StandardACI318p14 : StandardACI318, IEquatable<StandardACI318>
	{
		#region Constructors

		/// <summary>
		/// Creates the standard
		/// </summary>
		/// <param name="name">The name</param>
		/// <param name="remarks">The remarks</param>
		public StandardACI318p14(string name = "ACI 318-14", string remarks = "Building Code Requirements for Structural Concrete: ACI Standard ACI 318-14")
			: base(name, remarks)
		{

		}

		/// <summary>
		/// Creates the standard with the default remarks
		/// </summary>
		/// <param name="name">The name</param>
		public StandardACI318p14(string name = "ACI 318-14")
			: this(name, "Building Code Requirements for Structural Concrete: ACI Standard ACI 318-14")
		{
		}

		/// <summary>
		/// Creates the standard with the default name and remarks
		/// </summary>
		public StandardACI318p14()
			: this("ACI 318-14", "Building Code Requirements for Structural Concrete: ACI Standard ACI 318-14")
		{
		}

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected StandardACI318p14(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#endregion

		#region Equals - hashcode - operators

		/// <summary>
		/// Equality with an object of the same type
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is equal</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as StandardACI318p14);
		}

		/// <summary>
		/// Equality of the coefficients and of the base
		/// </summary>
		/// <param name="other">The object to compare</param>
		/// <returns>True if the objects are equal</returns>
		public bool Equals(StandardACI318p14 other)
		{
			return other != null &&
				   base.Equals(other);
		}

		/// <summary>
		/// The hash code of the coefficients and of the base
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Serializes the object
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		#endregion
	}
}
