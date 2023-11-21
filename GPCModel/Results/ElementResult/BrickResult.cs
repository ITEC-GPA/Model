using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
	[Serializable]
	public sealed class BrickResult : FiniteElementResult, ISerializable, IEquatable<BrickResult>
	{
		#region Public Constructors

		public BrickResult(ILoadCase Case, IEnumerable<ResultLocation> resultLocations,
			int stageId = ModelObjectId.IDUNASSIGNED)
			: base(Case, resultLocations, stageId)
		{
			if (resultLocations is null)
				throw new ArgumentNullException(nameof(resultLocations));

			// controllo che siano IBrickResult
			if (!(resultLocations.First().GetResults().First() is IBrickResult))
				throw new ArgumentException("Result type is not a IBrickResult");
		}

		private BrickResult(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}

		#endregion

		#region Public Methods

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Equals((BrickResult)obj);
		}

		public bool Equals(BrickResult other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			return base.Equals(other);
		}

		public override string ToString()
		{
			return base.ToString();
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		public static bool operator ==(BrickResult obj1, BrickResult obj2)
		{
			if (obj1 is null)
				return obj2 is null;

			if (ReferenceEquals(obj1, obj2))
				return true;

			return obj1.Equals(obj2);
		}

		public static bool operator !=(BrickResult obj1, BrickResult obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion
	}
}
