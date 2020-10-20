using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    public class GlassMaterialPrEn : GlassMaterial
    {
        private double _fgk;

        public double Fgk => _fgk;

        #region PUBLIC CONSTRUCTORS

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Elastic modulus of the glass</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="fgk">Characeristic value of bending strength of annealed glass</param>
        /// <param name="density">Density of the material</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        public GlassMaterialPrEn(double elasticModulus, double poisson, double fgk, double density, double alfaThermalExpansion)
            : this(elasticModulus, poisson, fgk, density, alfaThermalExpansion, Guid.Empty)
        {

        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Elastic modulus of the glass</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="fgk">Characeristic value of bending strength of annealed glass</param>
        /// <param name="density">Density of the material</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        public GlassMaterialPrEn(double elasticModulus, double poisson, double fgk, double density, double alfaThermalExpansion, Guid guid)
            : base(elasticModulus, poisson, density, alfaThermalExpansion, guid)
        {
            if (fgk <= 0)
            {
                throw new ArgumentException($"{nameof(fgk)} cannot be zero or lower");
            }
            this._fgk = fgk;
        }

        public GlassMaterialPrEn(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _fgk = info.GetDouble("Fgk");
        }

        #endregion PUBLIC CONSTRUCTORS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Fgk", _fgk);
        }
    }
}