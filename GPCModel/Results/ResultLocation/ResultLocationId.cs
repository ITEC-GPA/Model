using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
	[Serializable]
	public sealed class ResultLocationId : ResultLocation, ISerializable, IEquatable<ResultLocationId>
	{
		#region Public Constructors

		public ResultLocationId(IEnumerable<IPlateResult> results, int id)
			: base(results.Cast<ResultType>().ToArray(), id)
		{

		}

		public ResultLocationId(IEnumerable<INodeResult> results, int id)
			: base(results.Cast<ResultType>().ToArray(), id)
		{

		}

		public ResultLocationId(IEnumerable<IBrickResult> results, int id)
			: base(results.Cast<ResultType>().ToArray(), id)
		{

		}

		public ResultLocationId(ResultType[] results, int id, string name)
			: base(results, id, name)
		{

		}

		private ResultLocationId(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		#endregion

		#region Public Methods

		public override bool Equals(object obj)
		{
			return Equals((ResultLocationId)obj);
		}

		public bool Equals(ResultLocationId other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			return base.Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return base.GetHashCode();
			}
		}

		public static bool operator ==(ResultLocationId left, ResultLocationId right)
		{
			return EqualityComparer<ResultLocationId>.Default.Equals(left, right);
		}

		public static bool operator !=(ResultLocationId left, ResultLocationId right)
		{
			return !(left == right);
		}

		#endregion
	}
}
