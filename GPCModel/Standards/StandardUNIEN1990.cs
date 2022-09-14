using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	[Serializable]
	public class StandardUNIEN1990 : StandardEN1990, ISerializable, IEquatable<StandardUNIEN1990>
	{
		public StandardUNIEN1990()
		{

		}

		protected StandardUNIEN1990(SerializationInfo info, StreamingContext context) 
			: base(info, context)
		{
		}

		public override bool Equals(object obj)
		{
			return Equals(obj as StandardUNIEN1990);
		}

		public bool Equals(StandardUNIEN1990 other)
		{
			return other != null &&
				   base.Equals(other);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		public override string ToString()
		{
			return base.ToString();
		}
	}
}
