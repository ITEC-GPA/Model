using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    [Serializable]
    public abstract class GlassMaterial : Material
    {
        #region VARIABLES

        protected double _elasticModulus;
        protected double _poisson;

        #endregion VARIABLES

        #region PROPERTIES

        protected double ElasticModulus => _elasticModulus;
        protected double Poisson => _poisson;

        #endregion PROPERTIES

        #region CONSTRUCTOR

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elasticModulus">Elastic modulus of the glass</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="density">Density of the material</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        protected GlassMaterial(double elasticModulus, double poisson, double density, double alfaThermalExpansion, Guid guid)
            : base(density, alfaThermalExpansion, guid)
        {
            if (elasticModulus <= 0)
            {
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be zero or lower");
            }
            if (poisson <= 0)
            {
                throw new ArgumentException($"{nameof(poisson)} cannot be zero or lower");
            }
            else if (poisson >= 1)
            {
                throw new ArgumentException($"{nameof(poisson)} cannot be greater than 1");
            }

            this._elasticModulus = elasticModulus;
            this._poisson = poisson;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elasticModulus">Elastic modulus of the glass</param>
        /// <param name="poisson">poisson ratio's of the glass</param>
        /// <param name="fgk">Characeristic value of bending strength of annealed glass</param>
        /// <param name="density">Density of the material</param>
        /// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
        protected GlassMaterial(double elasticModulus, double poisson, double density, double alfaThermalExpansion)
            : this(elasticModulus, poisson, density, alfaThermalExpansion, Guid.Empty)
        {

        }

        protected GlassMaterial(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _elasticModulus = info.GetDouble("ElasticModulus");
            _poisson = info.GetDouble("Poisson");
        }

        #endregion CONSTRUCTOR

        #region PUBLIC METHODS
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ElasticModulus", _elasticModulus);
            info.AddValue("Poisson", _poisson);
        }

        #endregion PUBLIC METHODS
    }
}