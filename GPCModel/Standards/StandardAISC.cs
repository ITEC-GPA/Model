using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the AISC
	/// </summary>
	public abstract class StandardAISC : Standard
	{
		#region Variables

		#endregion

		#region Properties

		
		#endregion

		#region Constructors

		public StandardAISC(string name = "AISC", string remarks = "Specification for Structural Steel Buildings")
			: base(name, remarks)
		{

		}

		public StandardAISC(string name = "AISC")
			: this(name, "Specification for Structural Steel Buildings")
		{
		}

		public StandardAISC()
			: this("AISC", "Specification for Structural Steel Buildings")
		{
		}

		protected StandardAISC(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			int version;
			try
			{
				version = info.GetInt32("StandardAISCVersion");
			}
			catch (Exception)
			{
				version = 1;
			}
		}

		#endregion

		#region Equals - hashcode - operators

		public override bool Equals(object obj)
		{
			return Equals(obj as StandardAISC);
		}

		public bool Equals(StandardAISC other)
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

			double version = 0;
			info.AddValue("StandardAISCVersion", version);
		}

		#endregion
	}
}
