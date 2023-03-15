using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Concrete", Group = "Materials", Kind = "Material")]
    public class ConcreteMaterialModelCode2010 : ConcreteMaterialEuropeanCommon, ISerializable
    {
        #region Constructors

        public ConcreteMaterialModelCode2010(string name, double strainYCompression, double strainYTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension, ConcreteTypes concreteType,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6,
            CementTypes cementType = CementTypes.ClassN)
            : base(name, strainYTension, strainYCompression, stressStrainTableCompression, stressStrainTableTension, concreteType, poisson, density, alfaThermalExpansion, cementType)
        {
        }

        public ConcreteMaterialModelCode2010(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams, ConcreteTypes concreteType = ConcreteTypes.Concrete,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6, CementTypes cementType = CementTypes.ClassN)
            : base(name, fck, compressionStressStrainDiagrams, concreteType, poisson, density, alfaThermalExpansion, cementType)
        {

        }

        public ConcreteMaterialModelCode2010(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension, TensionStressStrainDiagrams tensionStressStrainDiagrams, ConcreteTypes concreteType = ConcreteTypes.FRC,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1e-6, CementTypes cementType = CementTypes.ClassN)
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
            else if (_concreteType == ConcreteTypes.Concrete)
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
            else if (_concreteType == ConcreteTypes.Concrete)
                return base.GetFctm();
            else
                return -1;
        }

        public override double CalculateDesignTensileStrength(Standards.Standard standard)
        {
            if (standard is Standards.StandardModelCode2010 standardModelCode2010)
            {
                if (_concreteType == ConcreteTypes.Concrete)
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
            if (_tensionStressStrainDiagrams == TensionStressStrainDiagrams.RigidPlastic)
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
			switch (_concreteType)
			{
				case ConcreteTypes.FRC:
					SetMechanicalProperties(_fck, _fctk, _fctu, _strainYTension, _strainUTension,
                        _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

					SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
					SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);

                    SetStressProperties();
                    break;
				case ConcreteTypes.Concrete:
					SetMechanicalProperties(_fck, 0, 0, 0, 0, _compressionStressStrainDiagrams, _tensionStressStrainDiagrams);

					SetStressStrainTableCompression(_fck, _strainYCompression, _strainUCompression, _compressionStressStrainDiagrams);
					SetStressStrainTableTension(_fctk, _fctu, _strainYTension, _strainUTension, _tensionStressStrainDiagrams);

                    SetStressProperties();
                    break;
				default:
					break;
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
