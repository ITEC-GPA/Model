using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    public class ConcreteMaterialModelCode2010FRC : ConcreteMaterialModelCode2010
    {

        #region Static Properties

        public static ConcreteMaterialModelCode2010FRC C25_30_5 => new ConcreteMaterialModelCode2010FRC("C25/30 5 kg/m³", 25, 
            CompressionStressStrainDiagrams.ParabolaRectangle, 1.09, 1.04, 1.09 / ConcreteMaterialEN1992.C25_30.E, 0.02, TensionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialModelCode2010FRC C25_30_10 => new ConcreteMaterialModelCode2010FRC("C25/30 10 kg/m³", 25, 
            CompressionStressStrainDiagrams.ParabolaRectangle, 1.55, 1.63, 1.55 / ConcreteMaterialEN1992.C25_30.E, 0.02, TensionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialModelCode2010FRC C25_30_17 => new ConcreteMaterialModelCode2010FRC("C25/30 17 kg/m³", 25, 
            CompressionStressStrainDiagrams.ParabolaRectangle, 2.41, 2.65, 2.41 / ConcreteMaterialEN1992.C25_30.E, 0.02, TensionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialModelCode2010FRC C30_37_5 => new ConcreteMaterialModelCode2010FRC("C30/37 5 kg/m³", 30,
            CompressionStressStrainDiagrams.ParabolaRectangle, 0.92, 0.88, 0.92 / ConcreteMaterialEN1992.C30_37.E, 0.02, TensionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialModelCode2010FRC C30_37_10 => new ConcreteMaterialModelCode2010FRC("C30/37 10 kg/m³", 30,
            CompressionStressStrainDiagrams.ParabolaRectangle, 1.55, 1.80, 1.55 / ConcreteMaterialEN1992.C30_37.E, 0.02, TensionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialModelCode2010FRC C30_37_15 => new ConcreteMaterialModelCode2010FRC("C30/37 15 kg/m³", 30,
            CompressionStressStrainDiagrams.ParabolaRectangle, 2.14, 2.56, 2.14 / ConcreteMaterialEN1992.C30_37.E, 0.02, TensionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialModelCode2010FRC C30_37_25 => new ConcreteMaterialModelCode2010FRC("C30/37 25 kg/m³", 30,
            CompressionStressStrainDiagrams.ParabolaRectangle, 3.20, 3.74, 3.20 / ConcreteMaterialEN1992.C30_37.E, 0.02, TensionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialModelCode2010FRC C45_55_5 => new ConcreteMaterialModelCode2010FRC("C45/55 5 kg/m³", 45,
            CompressionStressStrainDiagrams.ParabolaRectangle, 1.11, 0.97, 1.11 / ConcreteMaterialEN1992.C45_55.E, 0.02, TensionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialModelCode2010FRC C45_55_10 => new ConcreteMaterialModelCode2010FRC("C45/55 10 kg/m³", 45,
            CompressionStressStrainDiagrams.ParabolaRectangle, 1.84, 1.98, 1.84 / ConcreteMaterialEN1992.C45_55.E, 0.02, TensionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialModelCode2010FRC C45_55_15 => new ConcreteMaterialModelCode2010FRC("C45/55 15 kg/m³", 45,
            CompressionStressStrainDiagrams.ParabolaRectangle, 2.16, 2.66, 2.16 / ConcreteMaterialEN1992.C45_55.E, 0.02, TensionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialModelCode2010FRC C70_85_5 => new ConcreteMaterialModelCode2010FRC("C70/85 5 kg/m³", 70,
            CompressionStressStrainDiagrams.ParabolaRectangle, 0.95, 0.79, 0.95 / ConcreteMaterialEN1992.C70_85.E, 0.02, TensionStressStrainDiagrams.Bilinear);

        public static ConcreteMaterialModelCode2010FRC C70_85_15 => new ConcreteMaterialModelCode2010FRC("C70/85 15 kg/m³", 70,
            CompressionStressStrainDiagrams.ParabolaRectangle, 2.64, 2.86, 2.64 / ConcreteMaterialEN1992.C70_85.E, 0.02, TensionStressStrainDiagrams.Bilinear);

        #endregion


        public ConcreteMaterialModelCode2010FRC(string name, double strainYTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6,
            CementType cementType = CementType.ClassN)
            : base(name, strainYTension, stressStrainTableCompression, stressStrainTableTension, poisson, density, alfaThermalExpansion, cementType)
        {

        }

        public ConcreteMaterialModelCode2010FRC(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension, TensionStressStrainDiagrams tensionStressStrainDiagrams, 
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6, CementType cementType = CementType.ClassN)
            : base(name, fck, compressionStressStrainDiagrams, ffts, fFtu, strainYTension, strainUTension, 
                  tensionStressStrainDiagrams, poisson, density, alfaThermalExpansion, cementType)
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

		protected override void RecalculateMechanicalProperties()
		{
            SetMechanicalProperties(_fck, _fctk, _fctu, _strainYTension, _strainUTension,
                _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

            SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
            SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);
        }

		protected override bool IsFiberReinforced()
		{
            return true;
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
