using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results
{
    /// <summary>
    /// This class collects the result of a finite element for a particular loadcase o combination. 
    /// In particular it collects multiple location and multiple result for each location
    /// </summary>
    [Serializable]
    public abstract class FiniteElementResult : ElementResult, ISerializable, IFemResult
    {
        #region Variables

        protected int _stageId;

        #endregion

        #region Properties

        public int StageId => _stageId;

        #endregion

        #region Public Constructors

        public FiniteElementResult(ILoadCase Case, IEnumerable<ResultLocation> resultLocation,
            int stageId = ModelObjectId.IDUNASSIGNED, string name = "")
            : base(Case, resultLocation.ToArray(), name)
        {
            _stageId = stageId;
        }

        protected FiniteElementResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _stageId = (int)info.GetValue("StageId", typeof(int));
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("StageId", _stageId, typeof(ResultLocation[]));
        }

		#endregion

		#region Equals - hashcode

		public override bool Equals(object obj)
        {
            return obj is FiniteElementResult result &&
                   base.Equals(obj) &&
                   _stageId == result._stageId;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -19 + _stageId.GetHashCode();
                hashCode = hashCode * -19 + base.GetHashCode();
                return hashCode;
            }
        }
        #endregion
    }
}
