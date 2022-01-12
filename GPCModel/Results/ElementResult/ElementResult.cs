using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.LoadCases;
using GPC.Utilities.Extensions;

namespace GPC.Model.Results
{
    /// <summary>
    /// This class collects the results related to one loadcase o combination,  
    /// </summary>
    [Serializable]
    public abstract class ElementResult : ModelObject, ISerializable
    {
        #region Variables

        protected readonly ILoadCase _case;

        protected readonly ResultLocation[] _resultLocations;

        #endregion

        #region Properties

        public ILoadCase Case => _case;

        /// <summary>
        /// Return a clone of the results
        /// </summary>
        public ResultLocation[] ResultLocations => (ResultLocation[])_resultLocations.Clone();

        #endregion

        #region Constructors

        /// <param name="Case">The case where these results are reffered </param>
        /// <param name="resultLocations"></param>
        /// <param name="name"></param>
        public ElementResult(ILoadCase Case, ResultLocation[] resultLocations, string name = "")
            : base(name)
        {
            _case = Case ?? throw new ArgumentNullException(nameof(Case));
            _resultLocations = resultLocations ?? throw new ArgumentNullException(nameof(resultLocations));

            if (resultLocations.Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple location type");

            if (resultLocations.SelectMany(i => i.GetResults().Select(j => j.GetType())).Distinct().Count() > 1)
                throw new ArgumentException("Multiple result type");

            if (resultLocations.Where(i => i != null).Select(i => i.GetType()).Distinct().Count() > 1)
                throw new ArgumentException("Multiple location point type");
        }

        protected ElementResult(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _case = (ILoadCase)info.GetValue("Case", typeof(ILoadCase));
            _resultLocations = (ResultLocation[])info.GetValue("ResultLocations", typeof(ResultLocation[]));
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Case", _case);
            info.AddValue("ResultLocations", _resultLocations);
        }

        internal ResultLocation[] GetResultLocations()
        {
            return _resultLocations;
        }

        public IEnumerator GetResultLocationsEnumerator()
        {
            return _resultLocations.GetEnumerator();
        }

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ElementResult other) && 
                _case.Equals(other._case) && 
                _resultLocations.ScrambledEquals(other.ResultLocations) &&
                base.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -391 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<ILoadCase>.Default.GetHashCode(_case);

                for (int i = 0; i < _resultLocations.Length; i++)
                {
                    hashCode = hashCode * -17 + _resultLocations[i].GetHashCode();
                }

                return hashCode;
            }
        }

        public static bool operator ==(ElementResult obj1, ElementResult obj2)
        {
            if (obj1 is null)            
                return obj2 is null;            

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ElementResult obj1, ElementResult obj2)
        {
            return !(obj1 == obj2);
        }

		#endregion
	}
}
