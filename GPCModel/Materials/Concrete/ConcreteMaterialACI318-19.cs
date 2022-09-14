using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// This class collects all the coefficient of the ACI 318-19
    /// </summary>
    [Serializable]
    public class ConcreteMaterialACI31819 : ConcreteMaterialACI318, ISerializable
    {
        #region Constructor

        public ConcreteMaterialACI31819(string name, double fc, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1E-06)
            : base(name, fc, compressionStressStrainDiagrams, poisson, density, alfaThermalExpansion)
        {
        }

        public ConcreteMaterialACI31819(string name, StressStrainTable stressStrainTableCompression,
            StressStrainTable stressStrainTableTension, double elasticModulusCompression,
            double elasticModulusTension, double poisson, double density, double alfaThermalExpansion)
            : base(name, stressStrainTableCompression, stressStrainTableTension, elasticModulusCompression,
                  elasticModulusTension, poisson, density, alfaThermalExpansion)
        {
        }

        public ConcreteMaterialACI31819(string name, double fck, CompressionStressStrainDiagrams compressionStressStrainDiagrams,
            double ffts, double fFtu, double strainYTension, double strainUTension,
            TensionStressStrainDiagrams tensionStressStrainDiagrams,
            double poisson = 0.2, double density = 0.0025, double alfaThermalExpansion = 1E-06)
            : base(name, fck, compressionStressStrainDiagrams, ffts, fFtu, strainYTension, strainUTension,
                  tensionStressStrainDiagrams, poisson, density, alfaThermalExpansion)
        {
        }

        protected ConcreteMaterialACI31819(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        #endregion

        #region Equals, hashcode, operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

        public override bool Equals(object obj)
        {
            return obj is ConcreteMaterialACI31819 &&
                   base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return 23 + base.GetHashCode();
        }

        public static bool operator ==(ConcreteMaterialACI31819 left, ConcreteMaterialACI31819 right)
        {
            return EqualityComparer<ConcreteMaterialACI31819>.Default.Equals(left, right);
        }

        public static bool operator !=(ConcreteMaterialACI31819 left, ConcreteMaterialACI31819 right)
        {
            return !(left == right);
        }

        #endregion
    }
}
