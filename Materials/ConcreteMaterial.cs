using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    public class ConcreteMaterial : Material
    {
        #region VARIABLES

        protected double _elasticModulus;
        protected double _poisson;
        protected double _fck;

        #endregion VARIABLES

        #region PROPERTIES

        // TODO aggiungere le altre proprietà del calcestruzzo derivate fa fck
        protected double ElasticModulus => _elasticModulus;

        protected double Poisson => _poisson;
        protected double Fck => _fck;

        #endregion PROPERTIES

        #region CONSTRUCTORS

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Elastic secant modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fck">Concrete compression resistance reference value (28 days)</param>
        /// <param name="guid">Guid of the material</param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        public ConcreteMaterial(double elasticModulus, double poisson, double fck, double density, double alfaThermalExpansion, Guid guid)
            : base(density, alfaThermalExpansion, guid)
        {
            _elasticModulus = elasticModulus;
            _poisson = poisson;
            _fck = fck;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Elastic secant modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fck">Concrete compression resistance reference value (28 days)</param>
        public ConcreteMaterial(double elasticModulus, double poisson, double fck, double density)
            : this(elasticModulus, poisson, fck, density, 0, Guid.Empty)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Elastic secant modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fck">Concrete compression resistance reference value (28 days)</param>
        /// <param name="alfaThermalExpansion"></param>
        public ConcreteMaterial(double elasticModulus, double poisson, double fck, double density, double alfaThermalExpansion)
            : this(elasticModulus, poisson, fck, density, alfaThermalExpansion, Guid.Empty)
        {
        }

        public ConcreteMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _elasticModulus = info.GetDouble("ElasticModulus");
            _poisson = info.GetDouble("Poisson");
            _fck = info.GetDouble("Fck");
        }

        #endregion CONSTRUCTORS

        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ElasticModulus", _elasticModulus);
            info.AddValue("Poisson", _poisson);
            info.AddValue("Fck", _fck);
        }

        #endregion PUBLIC METHODS

        //public ConcreteMaterial(double poisson, double fck) : this(0, poisson, fck, Guid.Empty, 0)
        //{
        //    // TODO calcolare modulo elastico da fck, dipende da normativa però
        //}
    }
}