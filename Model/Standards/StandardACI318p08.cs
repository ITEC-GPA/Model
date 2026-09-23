using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the ACI 318-08
	/// </summary>
	[Serializable]
	public class StandardACI318p08 : StandardACI318, IEquatable<StandardACI318>
	{
		#region Constructors

		public StandardACI318p08(string name = "ACI 318-08", string remarks = "Building Code Requirements for Structural Concrete: ACI Standard ACI 318-08")
			: base(name, remarks)
		{

		}

		public StandardACI318p08(string name = "ACI 318-08")
			: this(name, "Building Code Requirements for Structural Concrete: ACI Standard ACI 318-08")
		{
		}

		public StandardACI318p08()
			: this("ACI 318-08", "Building Code Requirements for Structural Concrete: ACI Standard ACI 318-08")
		{
		}

		protected StandardACI318p08(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#endregion

		#region Equals - hashcode - operators

		public override bool Equals(object obj)
		{
			return Equals(obj as StandardACI318p08);
		}

		public bool Equals(StandardACI318p08 other)
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
