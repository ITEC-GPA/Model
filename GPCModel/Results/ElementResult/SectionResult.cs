using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class SectionResult : ElementResult, ISerializable, IEquatable<SectionResult>
    {
        #region Public Constructors

        public SectionResult(ILoadCase Case, IEnumerable<ResultLocationId> resultLocations, string name = "")
            : base(Case, resultLocations.ToArray(), name)
        {

        }

        internal SectionResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public bool Equals(SectionResult other)
        {
            if (other == null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return Equals(obj is SectionResult);
        }

        public static bool operator ==(SectionResult left, SectionResult right)
        {
            return EqualityComparer<SectionResult>.Default.Equals(left, right);
        }

        public static bool operator !=(SectionResult left, SectionResult right)
        {
            return !(left == right);
        }

		#endregion
	}
}
