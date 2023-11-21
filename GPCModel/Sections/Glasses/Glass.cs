using System.Runtime.Serialization;

namespace GPC.Model.Sections.Glass
{
	public abstract class Glass : ModelObject
	{
		protected Glass(string name)
			: base(name)
		{

		}

		protected Glass(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;

			return obj is Glass glass && base.Equals(glass);
		}


		public override int GetHashCode()
		{
			unchecked
			{
				return -391 * base.GetHashCode();
			}
		}

		public static bool operator ==(Glass obj1, Glass obj2)
		{
			if (ReferenceEquals(obj1, obj2))
				return true;

			if (obj1 is null || obj2 is null)
				return false;

			return obj1.Equals(obj2);
		}

		public static bool operator !=(Glass obj1, Glass obj2)
		{
			return !(obj1 == obj2);
		}
	}
}
