using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    [Serializable]
    public class StandardUNIEN1993p11 : StandardEN1993p11, ISerializable
    {        
        public StandardUNIEN1993p11()
        {
            _gammaM0 = 1.05;
            _gammaM1 = 1.10;
            _gammaM2 = 1.25;
        }

        protected StandardUNIEN1993p11(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override bool Equals(object obj)
        {
            return obj is StandardUNIEN1993p11 p &&
                   base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 624022166 + base.GetHashCode();
        }

    }
}
