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

        #region Static Properties

        public static ConcreteMaterialEN1992 C25_30 =>  new ConcreteMaterialEN1992("C25/30", 25, CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialEN1992 C30_37 =>  new ConcreteMaterialEN1992("C30/37", 30, CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialEN1992 C35_45 =>  new ConcreteMaterialEN1992("C35/45", 35, CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialEN1992 C40_50 =>  new ConcreteMaterialEN1992("C40/50", 40, CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialEN1992 C45_55 =>  new ConcreteMaterialEN1992("C45/55", 45, CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialEN1992 C50_60 =>  new ConcreteMaterialEN1992("C50/60", 50, CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialEN1992 C55_67 =>  new ConcreteMaterialEN1992("C55/67", 55, CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialEN1992 C60_75 =>  new ConcreteMaterialEN1992("C60/75", 60, CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialEN1992 C70_85 =>  new ConcreteMaterialEN1992("C70/85", 70, CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialEN1992 C80_95 =>  new ConcreteMaterialEN1992("C80/90", 80, CompressionStressStrainDiagrams.ParabolaRectangle);

        public static ConcreteMaterialEN1992 C90_105 => new ConcreteMaterialEN1992("C90/105", 90, CompressionStressStrainDiagrams.ParabolaRectangle);

        #endregion



        public ConcreteMaterialEN1992(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams, 
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6, CementType cementType = CementType.ClassN) 
            : base(name, fck, compressionStressStrainDiagrams, poisson, density, alfaThermalExpansion, cementType)
        {

        }


        public ConcreteMaterialEN1992(string name, double strainYTension, StressStrainTable stressStrainTableCompression, 
            StressStrainTable stressStrainTableTension, double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6, 
            CementType cementType = CementType.ClassN) 
            : base(name, strainYTension, stressStrainTableCompression, stressStrainTableTension, poisson, density, alfaThermalExpansion, cementType)
        {

        }

        protected ConcreteMaterialEN1992(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

		protected override void RecalculateMechanicalProperties()
		{
            SetMechanicalProperties(_fck, 0, 0, 0, 0, _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);
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
