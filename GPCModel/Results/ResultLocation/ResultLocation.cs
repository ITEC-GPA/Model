using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public abstract class ResultLocation : ModelObjectId, ISerializable
    {

        protected readonly ResultType[] _results;

        
        /// <summary>
        /// Return a clone of the results
        /// </summary>
        public ResultType[] Results => (ResultType[])_results.Clone();
        

        public ResultLocation(ResultType[] results, int id) 
            : this(results, id, "")
        {

        }

        public ResultLocation(ResultType[] results, int id, string name) 
            : base(id, name)
        {
            _results = results ?? throw new ArgumentNullException(nameof(results));
        }

        public ResultLocation(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _results = (ResultType[])info.GetValue("ResultType", typeof(ResultType[]));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ResultType", _results, typeof(ResultType[]));
        }


        public IEnumerator GetResultsEnumerator()
        {
            return _results.GetEnumerator();
        }



        #region  Equals - hashcode - Operators

        public override bool Equals(object obj)
        {
            return obj is ResultLocation resultLocation && resultLocation._results.Equals(_results) && base.Equals(resultLocation);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -19 + base.GetHashCode();

                for (int i = 0; i < _results.Length; i++)
                {
                    hashCode = hashCode * -17 * _results[i].GetHashCode();
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