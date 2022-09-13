using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the ACI 318
	/// </summary>
	public class StandardACI318p19 : StandardACI318, IEquatable<StandardACI318>
	{
		#region Constructors

		public StandardACI318p19(string name = "ACI 318-19", string remarks = "Building Code Requirements for Structural Concrete: ACI Standard ACI 318")
			: base(name, remarks)
		{

		}

		public StandardACI318p19(string name = "ACI 318-19")
			: this(name, "Building Code Requirements for Structural Concrete: ACI Standard ACI 318")
		{
		}

		public StandardACI318p19()
			: this("ACI 318-19", "Building Code Requirements for Structural Concrete: ACI Standard ACI 318")
		{
		}

		protected StandardACI318p19(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}

		#endregion

		#region Equals - hashcode - operators

		public override bool Equals(object obj)
		{
			return Equals(obj as StandardACI318p19);
		}

		public bool Equals(StandardACI318p19 other)
		{
			return other != null &&
				   base.Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17+ base.GetHashCode();
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
