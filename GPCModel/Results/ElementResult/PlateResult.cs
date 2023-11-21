using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
	[Serializable]
	public sealed class PlateResult : FiniteElementResult, ISerializable, IEquatable<PlateResult>, IElementResult
	{
		#region Constructors

		/// <param name="Case"></param>
		/// <param name="resultLocations"></param>
		/// <param name="stageId"></param>
		/// <param name="name"></param>
		public PlateResult(ILoadCase Case, IEnumerable<ResultLocationId> resultLocations,
			int stageId = ModelObjectId.IDUNASSIGNED, string name = "")
			: base(Case, resultLocations, stageId, name)
		{
			if (resultLocations is null)
				throw new ArgumentNullException(nameof(resultLocations));

			// controllo che siano iplate result
			if (!(resultLocations.First().GetResults().First() is IPlateResult))
				throw new ArgumentException("Result type is not a IplateResult");
		}

		/// <param name="Case"></param>
		/// <param name="resultLocations"></param>
		/// <param name="stageId"></param>
		/// <param name="name"></param>
		public PlateResult(ILoadCase Case, IEnumerable<ResultLocationPoint> resultLocations, int stageId = ModelObjectId.IDUNASSIGNED, string name = "")
			: base(Case, resultLocations, stageId, name)
		{
			if (resultLocations is null)
				throw new ArgumentNullException(nameof(resultLocations));

			// controllo che siano iplate result
			if (!(resultLocations.First().GetResults().First() is IPlateResult))
				throw new ArgumentException("Result type is not a IplateResult");
		}

		private PlateResult(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}

		#endregion

		#region Public Methods

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return Equals((PlateResult)obj);
		}

		public bool Equals(PlateResult other)
		{
			if (other == null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			return base.Equals(other);
		}

		public static bool operator ==(PlateResult obj1, PlateResult obj2)
		{
			if (obj1 is null)
			{
				return obj2 is null;
			}

			if (ReferenceEquals(obj1, obj2))
				return true;

			return obj1.Equals(obj2);
		}

		public static bool operator !=(PlateResult obj1, PlateResult obj2)
		{
			return !(obj1 == obj2);
		}

		#endregion
	}
}
