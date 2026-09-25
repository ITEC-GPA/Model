using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the AASHTO-LRFD
	/// </summary>
	[Serializable]
	public class StandardAASHTO : StandardACI318, IEquatable<StandardACI318>
	{
		#region Constructors

		/// <summary>
		/// Creates the standard
		/// </summary>
		/// <param name="name">The name</param>
		/// <param name="remarks">The remarks</param>
		public StandardAASHTO(string name = "AASHTO-LRFD", string remarks = "AASHTO LRFD Bridge Design Specifications")
			: base(name, remarks)
		{

		}

		/// <summary>
		/// Creates the standard with the default remarks
		/// </summary>
		/// <param name="name">The name</param>
		public StandardAASHTO(string name = "AASHTO-LRFD")
			: this(name, "AASHTO LRFD Bridge Design Specifications8")
		{
		}

		/// <summary>
		/// Creates the standard with the default name and remarks
		/// </summary>
		public StandardAASHTO()
			: this("AASHTO-LRFD", "AASHTO LRFD Bridge Design Specifications")
		{
		}

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected StandardAASHTO(SerializationInfo info, StreamingContext context) 
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
			return Equals(obj as StandardAASHTO);
		}

		/// <summary>
		/// Equality of the coefficients and of the base
		/// </summary>
		/// <param name="other">The object to compare</param>
		/// <returns>True if the objects are equal</returns>
		public bool Equals(StandardAASHTO other)
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
				hashCode = hashCode * -17+ base.GetHashCode();
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
