using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    public class ConcreteMaterialModelCode2010FRC : ConcreteMaterialModelCode2010
    {

        public ConcreteMaterialModelCode2010FRC(string name, double strainYTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension,
            double poisson, double density, double alfaThermalExpansion, CementType cementType = CementType.ClassN)
            : base(name, strainYTension, stressStrainTableCompression, stressStrainTableTension, poisson, density, alfaThermalExpansion, cementType)
        {

        }


        public ConcreteMaterialModelCode2010FRC(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension,
            TensionStressStrainDiagrams tensionStressStrainDiagrams, double poisson, double density,
            double alfaThermalExpansion, CementType cementType = CementType.ClassN)
            : base(name, fck, compressionStressStrainDiagrams, ffts, fFtu, strainYTension, strainUTension, tensionStressStrainDiagrams, poisson, density, alfaThermalExpansion, cementType)
        {

        }

        protected ConcreteMaterialModelCode2010FRC(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }


        #region Override Method

        protected override double GetFctk05()
        {
            return _fctk;
        }

        protected override double GetFctk95()
        {
            return 1.3 * GetFctm();
        }

        protected override double GetFctm()
        {
            return _fctk / 0.7;
        }


        #endregion

        #region Equals, hashcode, operators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ConcreteMaterialModelCode2010FRC objCasted) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode(); ;
                return hashCode;
            }
        }


        public static bool operator ==(ConcreteMaterialModelCode2010FRC obj1, ConcreteMaterialModelCode2010FRC obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ConcreteMaterialModelCode2010FRC obj1, ConcreteMaterialModelCode2010FRC obj2)
        {
            return !(obj1 == obj2);
        }


        #endregion
    }
}
