using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public abstract class ResultType : ModelObject, ISerializable
    {

        protected readonly CoordinateSystem _coordinateSystem;


        protected ResultType(CoordinateSystem coordinateSystem) : base()
        {
            _coordinateSystem = coordinateSystem ?? throw new ArgumentNullException(nameof(coordinateSystem));
        }


        protected ResultType(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }




        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return base.GetHashCode();
            }
        }

        public static bool operator ==(ResultType obj1, ResultType obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }


        public static bool operator !=(ResultType obj1, ResultType obj2)
        {
            return !(obj1 == obj2);
        }




        ///// <summary>
        ///// Compare two <see cref="Result"/> using only <see cref="Result.Case"/> as equality parameter
        ///// </summary>
        //public class LoadCaseResultComparer<T> : IEqualityComparer<T> where T : Result
        //{
            
        //    /// <returns> 
        //    /// <para> true if both <paramref name="x"/> and <paramref name="y"/> are null </para>
        //    /// </returns>
        //    /// <remarks> Only <see cref="Result.Case"/> is used as equality parameter </remarks>
        //    public bool Equals(T x, T y)
        //    {
        //        if (ReferenceEquals(x, y))
        //            return true;

        //        if (x == null && y == null)
        //            return true;

        //        if (x == null || y == null)
        //            return false;

        //        return x.Case.Equals(y.Case);
        //    }


        //    /// <remarks> Only <see cref="Result.Case"/> is used as equality parameter </remarks>
        //    public int GetHashCode(T obj)
        //    {
        //        return 17 * obj.Case.GetHashCode();
        //    }

        //}

    }
}
