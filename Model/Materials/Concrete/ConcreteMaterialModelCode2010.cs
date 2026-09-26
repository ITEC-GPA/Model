using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// A plain or fiber reinforced concrete of fib Model Code 2010 (for the FRC: residual strengths fFts, fFtu and the relations with fR1, fR3 of § 5.6.4)
    /// </summary>
    [Serializable]
    [UI(Description = "Concrete", Group = "Materials", Kind = "Material")]
    public class ConcreteMaterialModelCode2010 : ConcreteMaterialEuropeanCommon, ISerializable
    {
        #region Constructors

        /// <summary>
        /// Creates a concrete from generic stress-strain tables
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="strainYCompression">The strain at the peak compression (0: the strain of the minimum stress)</param>
        /// <param name="strainYTension">The strain at the tensile strength</param>
        /// <param name="stressStrainTableCompression">The table in compression</param>
        /// <param name="stressStrainTableTension">The table in tension</param>
        /// <param name="concreteType">The type of concrete</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        /// <param name="cementType">The class of cement</param>
        public ConcreteMaterialModelCode2010(string name, double strainYCompression, double strainYTension,
            StressStrainTable stressStrainTableCompression, StressStrainTable stressStrainTableTension, ConcreteTypes concreteType,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 10e-6,
            CementTypes cementType = CementTypes.ClassN)
            : base(name, strainYTension, strainYCompression, stressStrainTableCompression, stressStrainTableTension, concreteType, poisson, density, alfaThermalExpansion, cementType)
        {
        }

        /// <summary>
        /// Creates a plain concrete from fck (tension: linear up to fctk,0.05)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="fck">The characteristic compressive strength (the sign is ignored)</param>
        /// <param name="compressionStressStrainDiagrams">The diagram in compression</param>
        /// <param name="concreteType">The type of concrete</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion (see the default of <see cref="ConcreteMaterialEuropeanCommon"/>)</param>
        /// <param name="cementType">The class of cement</param>
        public ConcreteMaterialModelCode2010(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams, ConcreteTypes concreteType = ConcreteTypes.Concrete,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 10e-6, CementTypes cementType = CementTypes.ClassN)
            : base(name, fck, compressionStressStrainDiagrams, concreteType, poisson, density, alfaThermalExpansion, cementType)
        {

        }

        /// <summary>
        /// Creates a fiber reinforced concrete from fck and the residual strengths
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="fck">The characteristic compressive strength (the sign is ignored)</param>
        /// <param name="compressionStressStrainDiagrams">The diagram in compression</param>
        /// <param name="ffts">The serviceability residual strength fFts</param>
        /// <param name="fFtu">The ultimate residual strength fFtu</param>
        /// <param name="strainYTension">The strain at fFts (0: fFts / E)</param>
        /// <param name="strainUTension">The ultimate strain in tension</param>
        /// <param name="tensionStressStrainDiagrams">The diagram in tension</param>
        /// <param name="concreteType">The type of concrete</param>
        /// <param name="poisson">The Poisson's ratio</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        /// <param name="cementType">The class of cement</param>
        public ConcreteMaterialModelCode2010(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension, TensionStressStrainDiagrams tensionStressStrainDiagrams, ConcreteTypes concreteType = ConcreteTypes.FRC,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 10e-6, CementTypes cementType = CementTypes.ClassN)
            : base(name, fck, compressionStressStrainDiagrams, ffts, fFtu, strainYTension, strainUTension,
                  tensionStressStrainDiagrams, concreteType, poisson, density, alfaThermalExpansion, cementType)
        {
        }

        /// <summary>
        /// Creates a C25/30 with the parabola-rectangle diagram and a bilinear tension diagram (1.0, 1.25 up to 0.02) of type plain concrete
        /// </summary>
        /// <param name="name">The name</param>
        public ConcreteMaterialModelCode2010(string name)
            : this(name, 25.0, CompressionStressStrainDiagrams.ParabolaRectangle, 1.0, 1.25, 0.0001, 0.02, TensionStressStrainDiagrams.Bilinear, ConcreteTypes.Concrete)
        {
        }

        /// <summary>
        /// Deserialization constructor (see <see cref="ConcreteMaterialEuropeanCommon"/>)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected ConcreteMaterialModelCode2010(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Override Method

        /// <summary>
        /// fctk,0.05: fFts (the given fctk) for the fiber reinforced concrete, 0.7 fctm for the plain one
        /// </summary>
        /// <returns>The characteristic tensile strength; -1 for other types</returns>
        protected override double GetFctk05()
        {
            if (_concreteType == ConcreteTypes.FRC)
                return _fctk;
            else if (_concreteType == ConcreteTypes.Concrete)
                return base.GetFctk05();
            else
                return -1;
        }

        /// <summary>
        /// fctk,0.95 = 1.3 fctm
        /// </summary>
        /// <returns>The characteristic tensile strength (95%)</returns>
        protected override double GetFctk95()
        {
            return 1.3 * GetFctm();
        }

        /// <summary>
        /// fctm: fctk / 0.7 for the fiber reinforced concrete, the formula of the plain concrete otherwise
        /// </summary>
        /// <returns>The mean tensile strength; -1 for other types</returns>
        protected override double GetFctm()
        {
            if (_concreteType == ConcreteTypes.FRC)
                return _fctk / 0.7;
            else if (_concreteType == ConcreteTypes.Concrete)
                return base.GetFctm();
            else
                return -1;
        }

        /// <summary>
        /// The design tensile strength: αct fctk,0.05 / γc (γF for the fiber reinforced concrete)
        /// </summary>
        /// <param name="standard">The standard (Model Code 2010)</param>
        /// <returns>The design tensile strength; 0 for other types</returns>
        /// <exception cref="ArgumentException">For the other standards</exception>
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

        /// <summary>
        /// The ultimate residual strength fFtu from fR1 and fR3 (§ 5.6.4): fR3 / 3 for the rigid-plastic model, fFts - k (fFts - 0.5 fR3 + 0.2 fR1) ≥ 0
        /// for the linear ones (k = wu / CMOD3 = 1)
        /// </summary>
        /// <param name="fr1">The residual flexural strength fR1 (CMOD = 0.5 mm)</param>
        /// <param name="fr3">The residual flexural strength fR3 (CMOD = 2.5 mm)</param>
        /// <returns>fFtu</returns>
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

        /// <summary>
        /// The serviceability residual strength fFts from fR1 and fR3 (§ 5.6.4): 0.45 fR1 (fR3 / 3 for the rigid-plastic model)
        /// </summary>
        /// <param name="fr1">The residual flexural strength fR1 (CMOD = 0.5 mm)</param>
        /// <param name="fr3">The residual flexural strength fR3 (CMOD = 2.5 mm)</param>
        /// <returns>fFts</returns>
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

        /// <summary>
        /// The residual flexural strength fR1 from fFts: fFts / 0.45 (3 fFtu for the rigid-plastic model)
        /// </summary>
        /// <param name="ffts">The serviceability residual strength fFts</param>
        /// <param name="fftu">The ultimate residual strength fFtu</param>
        /// <returns>fR1</returns>
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

        /// <summary>
        /// The residual flexural strength fR3 from fFts and fFtu (inverse of <see cref="CalculateFFTu"/>); for the rigid-plastic model it returns fFtu / 3
        /// (the inverse of fFtu = fR3 / 3 is 3 fFtu: see the list of the defects found)
        /// </summary>
        /// <param name="ffts">The serviceability residual strength fFts</param>
        /// <param name="fftu">The ultimate residual strength fFtu</param>
        /// <returns>fR3</returns>
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

        /// <summary>
        /// The ratio k = wu / CMOD3 of the linear model (1)
        /// </summary>
        /// <returns>1</returns>
        protected double GetLinearCoefficient()
        {
            return 1.0;
        }

        /// <summary>
        /// Recalculates the mechanical properties and the tables: the FRC keeps its tension diagram and strengths, the plain concrete gets a linear
        /// tension up to fctk,0.05
        /// </summary>
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

        /// <summary>
        /// Sets the diagram in compression; the generic diagram is replaced by the parabola-rectangle
        /// </summary>
        /// <param name="compressionStressStrainDiagrams">The diagram</param>
        public override void SetCompressionStressStrainDiagram(CompressionStressStrainDiagrams compressionStressStrainDiagrams)
        {
            if (compressionStressStrainDiagrams != CompressionStressStrainDiagrams.Generic)
                _compressionStressStrainDiagrams = compressionStressStrainDiagrams;
            else
                _compressionStressStrainDiagrams = CompressionStressStrainDiagrams.ParabolaRectangle;
        }

        /// <summary>
        /// Sets the diagram in tension; the generic diagram is replaced by the bilinear
        /// </summary>
        /// <param name="tensionStressStrainDiagrams">The diagram</param>
        public void SetTensionStressStrainDiagram(TensionStressStrainDiagrams tensionStressStrainDiagrams)
        {
            if (tensionStressStrainDiagrams != TensionStressStrainDiagrams.Generic)
                _tensionStressStrainDiagrams = tensionStressStrainDiagrams;
            else
                _tensionStressStrainDiagrams = TensionStressStrainDiagrams.Bilinear;
        }

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Serializes the data of <see cref="ConcreteMaterialEuropeanCommon"/>
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Equality (see <see cref="ConcreteMaterialEuropeanCommon.Equals(object)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal concrete of Model Code 2010</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is ConcreteMaterialModelCode2010 objCasted) && base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code (see <see cref="ConcreteMaterialEuropeanCommon.GetHashCode"/>)
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode(); ;
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first concrete (not null, unless both are null)</param>
        /// <param name="obj2">The second concrete</param>
        /// <returns>True if the materials are equal</returns>
        public static bool operator ==(ConcreteMaterialModelCode2010 obj1, ConcreteMaterialModelCode2010 obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first concrete</param>
        /// <param name="obj2">The second concrete</param>
        /// <returns>True if the materials are different</returns>
        public static bool operator !=(ConcreteMaterialModelCode2010 obj1, ConcreteMaterialModelCode2010 obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
