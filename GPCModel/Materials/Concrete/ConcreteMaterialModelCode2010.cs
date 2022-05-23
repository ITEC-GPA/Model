using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    public class ConcreteMaterialModelCode2010 : ConcreteMaterialEuropeanCommon, ISerializable
    {
        #region Static Properties

        public static ConcreteMaterialModelCode2010 C20_25 => new ConcreteMaterialModelCode2010("C20/25", 20,
            CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C25_30 => new ConcreteMaterialModelCode2010("C25/30", 25, 
            CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C28_35 => new ConcreteMaterialModelCode2010("C28/35", 28, 
            CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C30_37 => new ConcreteMaterialModelCode2010("C30/37", 30,
    CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C32_40 => new ConcreteMaterialModelCode2010("C32/40", 32,
    CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C35_45 => new ConcreteMaterialModelCode2010("C35/45", 35, 
            CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C40_50 => new ConcreteMaterialModelCode2010("C40/50", 40, 
            CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C45_55 => new ConcreteMaterialModelCode2010("C45/55", 45, 
            CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C50_60 => new ConcreteMaterialModelCode2010("C50/60", 50, 
            CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C55_67 => new ConcreteMaterialModelCode2010("C55/67", 55, 
            CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C60_75 => new ConcreteMaterialModelCode2010("C60/75", 60, 
            CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C70_85 => new ConcreteMaterialModelCode2010("C70/85", 70, 
            CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C80_95 => new ConcreteMaterialModelCode2010("C80/90", 80, 
            CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C90_105 => new ConcreteMaterialModelCode2010("C90/105", 90, 
            CompressionStressStrainDiagrams.ParabolaRectangle, ConcreteTypes.Normal);

        public static ConcreteMaterialModelCode2010 C25_30_5 => new ConcreteMaterialModelCode2010("C25/30 5 kg/m³", 25, 
            CompressionStressStrainDiagrams.ParabolaRectangle, 0.4905, 0.302, 0.4905 / ConcreteMaterialEN1992.C25_30.E * 5, 0.02, 
            TensionStressStrainDiagrams.Bilinear, ConcreteTypes.FRC);

        public static ConcreteMaterialModelCode2010 C25_30_10 => new ConcreteMaterialModelCode2010("C25/30 10 kg/m³", 25, 
            CompressionStressStrainDiagrams.ParabolaRectangle, 0.6975, 0.505, 0.6975 / ConcreteMaterialEN1992.C25_30.E * 5, 0.02,
            TensionStressStrainDiagrams.Bilinear, ConcreteTypes.FRC);

        public static ConcreteMaterialModelCode2010 C25_30_17 => new ConcreteMaterialModelCode2010("C25/30 17 kg/m³", 25, 
            CompressionStressStrainDiagrams.ParabolaRectangle, 1.0845, 0.843, 1.0845 / ConcreteMaterialEN1992.C25_30.E * 5, 0.02,
            TensionStressStrainDiagrams.Bilinear, ConcreteTypes.FRC);

        public static ConcreteMaterialModelCode2010 C30_37_5 => new ConcreteMaterialModelCode2010("C30/37 5 kg/m³", 30,
            CompressionStressStrainDiagrams.ParabolaRectangle, 0.414, 0.256, 0.414 / ConcreteMaterialEN1992.C30_37.E * 5, 0.02,
            TensionStressStrainDiagrams.Bilinear, ConcreteTypes.FRC);

        public static ConcreteMaterialModelCode2010 C30_37_10 => new ConcreteMaterialModelCode2010("C30/37 10 kg/m³", 30,
            CompressionStressStrainDiagrams.ParabolaRectangle, 0.6975, 0.59, 0.6975 / ConcreteMaterialEN1992.C30_37.E * 5, 0.02,
            TensionStressStrainDiagrams.Bilinear, ConcreteTypes.FRC);

        public static ConcreteMaterialModelCode2010 C30_37_15 => new ConcreteMaterialModelCode2010("C30/37 15 kg/m³", 30,
            CompressionStressStrainDiagrams.ParabolaRectangle, 0.963, 0.852, 0.963 / ConcreteMaterialEN1992.C30_37.E * 5, 0.02,
            TensionStressStrainDiagrams.Bilinear, ConcreteTypes.FRC);

        public static ConcreteMaterialModelCode2010 C30_37_25 => new ConcreteMaterialModelCode2010("C30/37 25 kg/m³", 30,
            CompressionStressStrainDiagrams.ParabolaRectangle, 1.44, 1.23, 1.44 / ConcreteMaterialEN1992.C30_37.E * 5, 0.02,
            TensionStressStrainDiagrams.Bilinear, ConcreteTypes.FRC);

        public static ConcreteMaterialModelCode2010 C45_55_5 => new ConcreteMaterialModelCode2010("C45/55 5 kg/m³", 45,
            CompressionStressStrainDiagrams.ParabolaRectangle, 0.4995, 0.263, 0.4995 / ConcreteMaterialEN1992.C45_55.E * 5, 0.02,
            TensionStressStrainDiagrams.Bilinear, ConcreteTypes.FRC);

        public static ConcreteMaterialModelCode2010 C45_55_10 => new ConcreteMaterialModelCode2010("C45/55 10 kg/m³", 45,
            CompressionStressStrainDiagrams.ParabolaRectangle, 0.828, 0.622, 0.828 / ConcreteMaterialEN1992.C45_55.E * 5, 0.02,
            TensionStressStrainDiagrams.Bilinear, ConcreteTypes.FRC);

        public static ConcreteMaterialModelCode2010 C45_55_15 => new ConcreteMaterialModelCode2010("C45/55 15 kg/m³", 45,
            CompressionStressStrainDiagrams.ParabolaRectangle, 0.972, 0.898, 0.972 / ConcreteMaterialEN1992.C45_55.E * 5, 0.02,
            TensionStressStrainDiagrams.Bilinear, ConcreteTypes.FRC);

        public static ConcreteMaterialModelCode2010 C70_85_5 => new ConcreteMaterialModelCode2010("C70/85 5 kg/m³", 70,
            CompressionStressStrainDiagrams.ParabolaRectangle, 0.4275, 0.205, 0.4275 / ConcreteMaterialEN1992.C70_85.E * 5, 0.02,
            TensionStressStrainDiagrams.Bilinear, ConcreteTypes.FRC);

        public static ConcreteMaterialModelCode2010 C70_85_15 => new ConcreteMaterialModelCode2010("C70/85 15 kg/m³", 70,
            CompressionStressStrainDiagrams.ParabolaRectangle, 1.188, 0.902, 1.188 / ConcreteMaterialEN1992.C70_85.E * 5, 0.02,
            TensionStressStrainDiagrams.Bilinear, ConcreteTypes.FRC);

        #endregion

        #region Constructors

        public ConcreteMaterialModelCode2010(string name, double strainYCompression, double strainYTension, 
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension, ConcreteTypes concreteType,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6,
            CementType cementType = CementType.ClassN)
            : base(name, strainYTension, strainYCompression, stressStrainTableCompression, stressStrainTableTension, concreteType, poisson, density, alfaThermalExpansion, cementType)
        {
        }

        public ConcreteMaterialModelCode2010(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams, ConcreteTypes concreteType = ConcreteTypes.Normal,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6, CementType cementType = CementType.ClassN)
            : base(name, fck, compressionStressStrainDiagrams, concreteType, poisson, density, alfaThermalExpansion, cementType)
        {

        }

        public ConcreteMaterialModelCode2010(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension, TensionStressStrainDiagrams tensionStressStrainDiagrams, ConcreteTypes concreteType = ConcreteTypes.FRC,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6, CementType cementType = CementType.ClassN)
            : base(name, fck, compressionStressStrainDiagrams, ffts, fFtu, strainYTension, strainUTension,
                  tensionStressStrainDiagrams, concreteType, poisson, density, alfaThermalExpansion, cementType)
        {
        }

        protected ConcreteMaterialModelCode2010(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Override Method

        protected override double GetFctk05()
        {
            if (_concreteType == ConcreteTypes.FRC)
                return _fctk;
            else if (_concreteType == ConcreteTypes.Normal)
                return base.GetFctk05();
            else
                return -1;
        }

        protected override double GetFctk95()
        {
            return 1.3 * GetFctm();
        }

        protected override double GetFctm()
        {
            if (_concreteType == ConcreteTypes.FRC)
                return _fctk / 0.7;
            else if (_concreteType == ConcreteTypes.Normal)
                return base.GetFctm();
            else
                return -1;
        }

        public override double CalculateFctd(Standards.Standard standard)
        {
            if (standard is Standards.StandardModelCode2010 standardModelCode2010)
            {
                if (_concreteType == ConcreteTypes.Normal)
                    return standardModelCode2010.AlphaCT * Fctk05 / standardModelCode2010.GammaC;
                else if (_concreteType == ConcreteTypes.FRC)
                    return standardModelCode2010.AlphaCT * Fctk05 / standardModelCode2010.GammaF;
                else
                    return 0;
            }
            else
                throw new ArgumentException();
        }

        #endregion

        #region Public Method

        public double CalculateFFTu(double fr1, double fr3)
		{
            if(_tensionStressStrainDiagrams == TensionStressStrainDiagrams.RigidPlastic)
			{
                return fr3 / 3.0;
			}
            else if (_tensionStressStrainDiagrams == TensionStressStrainDiagrams.Bilinear)
			{
                double ffts = CalculateFFTs(fr1, fr3);
                return Math.Max(ffts - (GetLinearCoefficient()) * (ffts - 0.5 * fr3 + 0.2 * fr1), 0.0);
            }
            else if (_tensionStressStrainDiagrams == TensionStressStrainDiagrams.Linear)
            {
                double ffts = CalculateFFTs(fr1, fr3);
                return Math.Max(ffts - (GetLinearCoefficient()) * (ffts - 0.5 * fr3 + 0.2 * fr1), 0.0);
            }
            else
            {
                double ffts = CalculateFFTs(fr1, fr3);
                return Math.Max(ffts - (GetLinearCoefficient()) * (ffts - 0.5 * fr3 + 0.2 * fr1), 0.0);
            }
        }

        public double CalculateFFTs(double fr1, double fr3)
        {
            if (_tensionStressStrainDiagrams == TensionStressStrainDiagrams.RigidPlastic)
            {
                return fr3 / 3.0;
            }
            else if (_tensionStressStrainDiagrams == TensionStressStrainDiagrams.Bilinear)
            {
                return 0.45 * fr1;
            }
            else if (_tensionStressStrainDiagrams == TensionStressStrainDiagrams.Linear)
			{
                return 0.45 * fr1;
            }
            else
                return 0.45 * fr1;
        }

        public double CalculateFR1(double ffts, double fftu)
        {
            if (_tensionStressStrainDiagrams == TensionStressStrainDiagrams.RigidPlastic)
            {
                return fftu * 3.0;
            }
            else if (_tensionStressStrainDiagrams == TensionStressStrainDiagrams.Bilinear)
            {
                return ffts / 0.45;
            }
            else if (_tensionStressStrainDiagrams == TensionStressStrainDiagrams.Linear)
            {
                return ffts / 0.45;
            }
            else
                return ffts / 0.45;
        }

        public double CalculateFR3(double ffts, double fftu)
        {
            if (_tensionStressStrainDiagrams == TensionStressStrainDiagrams.RigidPlastic)
            {
                return fftu / 3.0;
            }
            else if (_tensionStressStrainDiagrams == TensionStressStrainDiagrams.Bilinear)
            {
                double k = GetLinearCoefficient();
                return (fftu - ffts + k * ffts + 0.2 * k * CalculateFR1(ffts, fftu)) / (0.5 * k);
            }
            else if (_tensionStressStrainDiagrams == TensionStressStrainDiagrams.Linear)
            {
                double k = GetLinearCoefficient();
                return (fftu - ffts + k * ffts + 0.2 * k * CalculateFR1(ffts, fftu)) / (0.5 * k);
            }
            else
            {
                double k = GetLinearCoefficient();
                return (fftu - ffts + k * ffts + 0.2 * k * CalculateFR1(ffts, fftu)) / (0.5 * k);
            }
        }

        protected double GetLinearCoefficient()
		{
            return 1.0;
		}

        public override void RecalculateMechanicalProperties()
		{
            if (_concreteType == ConcreteTypes.FRC)
            {
                SetMechanicalProperties(_fck, _fctk, _fctu, _strainYTension, _strainUTension,
                    _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

                SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
                SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);
            }
            else if(_concreteType == ConcreteTypes.Normal)
			{
                SetMechanicalProperties(_fck, 0, 0, 0, 0, _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

                SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
                SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);
            }
            else
			{

			}
        }

        #endregion

        #region Equals, hashcode, operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ConcreteMaterialModelCode2010 objCasted) && base.Equals(objCasted);
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

		public static bool operator ==(ConcreteMaterialModelCode2010 obj1, ConcreteMaterialModelCode2010 obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ConcreteMaterialModelCode2010 obj1, ConcreteMaterialModelCode2010 obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
