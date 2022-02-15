using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
    /// <summary>
    /// This class collects all the coefficient of the Eurocode2 Standard
    /// </summary>
    /// <remarks>Reference: EN 1992-1-1:2004/AC:2010</remarks>
    [Serializable]
    public class StandardEN1992p11 : StandardModelCode2010, ISerializable
    {

        /// <summary>
        /// Default Constructor
        /// </summary>
        public StandardEN1992p11()
        {

        }

        protected StandardEN1992p11(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }
        
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return obj is StandardEN1992p11 code &&
                   _gammaC == code._gammaC &&
                   _gammaCAccidental == code._gammaCAccidental &&
                   _gammaCE == code._gammaCE &&
                   _gammaS == code._gammaS &&
                   _gammaSAccidental == code._gammaSAccidental &&
                   _gammaSPrestress == code._gammaSPrestress &&
                   _gammaSPrestressAccidental == code._gammaSPrestressAccidental &&
                   _alphaCC == code._alphaCC &&
                   _alphaCT == code._alphaCT &&
                   _steelCoefficientStrainTension == code._steelCoefficientStrainTension &&
                   _gammaF == code._gammaF;
        }

        public override int GetHashCode()
        {
            return 624022166 + base.GetHashCode();
        }
    }
}
