using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
    public class ConcreteMaterialEN1992 : ConcreteMaterialModelCode2010
    {

        #region Static Constructor

        public static ConcreteMaterialEN1992 C25_30 =>  new ConcreteMaterialEN1992(25, CompressionStressStrainDiagrams.ParabolaRectangle,"C25/30");
        public static ConcreteMaterialEN1992 C30_37 =>  new ConcreteMaterialEN1992(30, CompressionStressStrainDiagrams.ParabolaRectangle,"C30/37");
        public static ConcreteMaterialEN1992 C35_45 =>  new ConcreteMaterialEN1992(35, CompressionStressStrainDiagrams.ParabolaRectangle,"C35/45");
        public static ConcreteMaterialEN1992 C40_50 =>  new ConcreteMaterialEN1992(40, CompressionStressStrainDiagrams.ParabolaRectangle,"C40/50");
        public static ConcreteMaterialEN1992 C45_55 =>  new ConcreteMaterialEN1992(45, CompressionStressStrainDiagrams.ParabolaRectangle,"C45/55");
        public static ConcreteMaterialEN1992 C50_60 =>  new ConcreteMaterialEN1992(50, CompressionStressStrainDiagrams.ParabolaRectangle,"C50/60");
        public static ConcreteMaterialEN1992 C55_67 =>  new ConcreteMaterialEN1992(55, CompressionStressStrainDiagrams.ParabolaRectangle,"C55/67");
        public static ConcreteMaterialEN1992 C60_75 =>  new ConcreteMaterialEN1992(60, CompressionStressStrainDiagrams.ParabolaRectangle,"C60/75");
        public static ConcreteMaterialEN1992 C70_85 =>  new ConcreteMaterialEN1992(70, CompressionStressStrainDiagrams.ParabolaRectangle,"C70/85");
        public static ConcreteMaterialEN1992 C80_95 =>  new ConcreteMaterialEN1992(80, CompressionStressStrainDiagrams.ParabolaRectangle,"C80/90");
        public static ConcreteMaterialEN1992 C90_105 => new ConcreteMaterialEN1992(90, CompressionStressStrainDiagrams.ParabolaRectangle, "C90/105");

        #endregion



        public ConcreteMaterialEN1992(double fck,
                                        CompressionStressStrainDiagrams compressionStressStrainDiagrams, string name = "",
                                        CementType cementType = CementType.ClassN)
            : base(name, fck, compressionStressStrainDiagrams, 0.2, 0.0025, 1e-6, cementType)
        {

        }


        public ConcreteMaterialEN1992(string name, double fck, 
                                        CompressionStressStrainDiagrams compressionStressStrainDiagrams, 
                                        double poisson, double density, double alfaThermalExpansion,
                                        CementType cementType = CementType.ClassN) 
            : base(name, fck, compressionStressStrainDiagrams, poisson, density, alfaThermalExpansion, cementType)
        {

        }


        public ConcreteMaterialEN1992(string name, double strainYTension, 
                                        StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension, 
                                        double poisson, double density, double alfaThermalExpansion, CementType cementType = CementType.ClassN) 
            : base(name, strainYTension, stressStrainTableCompression, stressStrainTableTension, poisson, density, alfaThermalExpansion, cementType)
        {

        }


        public ConcreteMaterialEN1992(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        #region Equals, hashcode, operators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ConcreteMaterialEN1992 objCasted) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();;
                return hashCode;
            }
        }


        public static bool operator ==(ConcreteMaterialEN1992 obj1, ConcreteMaterialEN1992 obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ConcreteMaterialEN1992 obj1, ConcreteMaterialEN1992 obj2)
        {
            return !(obj1 == obj2);
        }


        #endregion
    }
}
