using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the AASHTO-LRFD
	/// </summary>
	public class StandardAASHTO : StandardACI318, IEquatable<StandardACI318>
	{
		#region Constructors

		public StandardAASHTO(string name = "AASHTO-LRFD", string remarks = "AASHTO LRFD Bridge Design Specifications")
			: base(name, remarks)
		{

		}

		public StandardAASHTO(string name = "AASHTO-LRFD")
			: this(name, "AASHTO LRFD Bridge Design Specifications8")
		{
		}

		public StandardAASHTO()
			: this("AASHTO-LRFD", "AASHTO LRFD Bridge Design Specifications")
		{
		}

		protected StandardAASHTO(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}

		#endregion

		#region Equals - hashcode - operators

		public override bool Equals(object obj)
		{
			return Equals(obj as StandardAASHTO);
		}

		public bool Equals(StandardAASHTO other)
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
