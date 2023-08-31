using System;
using System.Diagnostics;
using System.Runtime.Serialization;

namespace GPC.Model.Fem.Properties
{
	[DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
	[Serializable]
	public abstract class BeamProperty : ElementProperty, IBeamProperty, ISerializable
	{
		#region Variables


		#endregion

		#region Properties


		#endregion

		public BeamProperty(string name, int id = IDUNASSIGNED)
			: base(name, id)
		{

		}

		public BeamProperty(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		private string GetDebuggerDisplay()
		{
			return $"BeamProperty: {_name}";
		}

		public override bool Equals(object obj)
		{
			return (obj is BeamProperty objCasted) && base.Equals(objCasted);
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
	}
}
