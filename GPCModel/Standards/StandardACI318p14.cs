using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the ACI 318-14
	/// </summary>
	public class StandardACI318p14 : StandardACI318, IEquatable<StandardACI318>
	{
		#region Constructors

		public StandardACI318p14(string name = "ACI 318-14", string remarks = "Building Code Requirements for Structural Concrete: ACI Standard ACI 318-14")
			: base(name, remarks)
		{

		}

		public StandardACI318p14(string name = "ACI 318-14")
			: this(name, "Building Code Requirements for Structural Concrete: ACI Standard ACI 318-14")
		{
		}

		public StandardACI318p14()
			: this("ACI 318-14", "Building Code Requirements for Structural Concrete: ACI Standard ACI 318-14")
		{
		}

		protected StandardACI318p14(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#endregion

		#region Equals - hashcode - operators

		public override bool Equals(object obj)
		{
			return Equals(obj as StandardACI318p14);
		}

		public bool Equals(StandardACI318p14 other)
		{
			return other != null &&
				   base.Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				return hashCode;
			}
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		#endregion
	}
}
