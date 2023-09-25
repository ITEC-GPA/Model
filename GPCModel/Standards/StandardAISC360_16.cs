using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the ANSI/AISC 360-16
	/// </summary>
	public abstract class StandardAISC360_16 : StandardAISC
	{
		#region Variables

		#endregion

		#region Properties

		
		#endregion

		#region Constructors

		public StandardAISC360_16(string name = "ANSI/AISC 360-16", string remarks = "Specification for Structural Steel Buildings")
			: base(name, remarks)
		{

		}

		public StandardAISC360_16(string name = "ANSI/AISC 360-16")
			: this(name, "Specification for Structural Steel Buildings")
		{
		}

		public StandardAISC360_16()
			: this("ANSI/AISC 360-16", "Specification for Structural Steel Buildings")
		{
		}

		protected StandardAISC360_16(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			int version;
			try
			{
				version = info.GetInt32("StandardAISC360_16Version");
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
			return Equals(obj as StandardAISC360_16);
		}

		public bool Equals(StandardAISC360_16 other)
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
			info.AddValue("StandardAISC360_16Version", version);
		}

		#endregion
	}
}
