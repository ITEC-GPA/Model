using System;
using System.Linq;
using System.Collections;
using GPC.Utilities.Extensions;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public abstract class ResultLocation : ModelObjectId, ISerializable
    {
        #region Variables

        protected readonly ResultType[] _resultTypes;

        #endregion

        #region Properties

        /// <summary>
        /// Return a clone of the results
        /// </summary>
        public ResultType[] ResultTypes => (ResultType[])_resultTypes.Clone();

        #endregion

        #region Public Constructors

        public ResultLocation(ResultType[] results, int id)
            : this(results, id, "")
        {

        }

        public ResultLocation(ResultType[] results, int id, string name)
            : base(id, name)
        {
            _resultTypes = results ?? throw new ArgumentNullException(nameof(results));
        }

        protected ResultLocation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _resultTypes = (ResultType[])info.GetValue("ResultType", typeof(ResultType[]));
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ResultType", _resultTypes, typeof(ResultType[]));
        }

        // non modificare la visibilità interna, la lista non deve essere modificabile
        internal ResultType[] GetResults()
        {
            return _resultTypes;
        }

        public IEnumerator GetResultsEnumerator()
        {
            return _resultTypes.GetEnumerator();
        }

		#endregion

		#region  Equals - hashcode - Operators

		public override bool Equals(object obj)
        {

            return (obj is ResultLocation resultLocation) && _resultTypes.ScrambledEquals(resultLocation._resultTypes) 
                                                          && base.Equals(resultLocation);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -19 + base.GetHashCode();

                for (int i = 0; i < _resultTypes.Length; i++)
                {
                    hashCode = hashCode * -17 * _resultTypes[i].GetHashCode();
                }

                return hashCode;
            }
        }

        public static bool operator ==(ResultLocation obj1, ResultLocation obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultLocation obj1, ResultLocation obj2)
        {
            return !(obj1 == obj2);
        }
        #endregion
    }
}