using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// UNI EN 1990: the Italian national annex of EN 1990 (the coefficients of <see cref="StandardEN1990"/>)
	/// </summary>
	[Serializable]
	public class StandardUNIEN1990 : StandardEN1990, ISerializable, IEquatable<StandardUNIEN1990>
	{
		/// <summary>
		/// Creates the standard with the default name and remarks
		/// </summary>
		public StandardUNIEN1990()
		{

		}

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected StandardUNIEN1990(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}

		/// <summary>
		/// Equality with an object of the same type
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is equal</returns>
		public override bool Equals(object obj)
		{
			return Equals(obj as StandardUNIEN1990);
		}

		/// <summary>
		/// Equality of the coefficients and of the base
		/// </summary>
		/// <param name="other">The object to compare</param>
		/// <returns>True if the objects are equal</returns>
		public bool Equals(StandardUNIEN1990 other)
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
			return base.GetHashCode();
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

		/// <summary>
		/// The name of the standard
		/// </summary>
		/// <returns>The name</returns>
		public override string ToString()
		{
			return base.ToString();
		}
	}
}
