using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    public class SteelMaterial : Material
    {
        #region VARIABLES

        protected double _elasticModulus;
        protected double _poisson;
        protected double _fy;
        protected double _fu;
        protected double _epsilon0;

        #endregion VARIABLES

        #region PROPERTIES

        public double ElasticModulus => _elasticModulus;
        public double Poisson => _poisson;
        public double Fy => _fy;
        public double Fu => _fu;
        public double Epsilon0 => _epsilon0;

        #endregion PROPERTIES

        #region CONSTRUCTORS

        /// <summary>
        ///
        /// </summary>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="epsilon0">Yielding strain</param>
        /// <param name="alfaThermalExpansion">Linear thermal expasion coefficient</param>
        /// <param name="guid">Guid of the material</param>
        public SteelMaterial(double elasticModulus, double poisson, double fy, double fu, double epsilon0, double density, double alfaThermalExpansion, Guid guid)
            : base(density, alfaThermalExpansion, guid)
        {
            if (fu == 0)
            {
                throw new ArgumentException($"{nameof(fu)} cannot be zero");
            }
            if (fy == 0)
            {
                throw new ArgumentException($"{nameof(fy)} cannot be zero");
            }
            if (elasticModulus == 0)
            {
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be zero");
            }
            if (poisson == 0)
            {
                throw new ArgumentException($"{nameof(poisson)} cannot be zero");
            }
            if (epsilon0 == 0)
            {
                throw new ArgumentException($"{nameof(epsilon0)} cannot be zero");
            }

            this._fu = fu;
            this._fy = fy;
            this._epsilon0 = epsilon0;
            this._elasticModulus = elasticModulus;
            this._poisson = poisson;
        }

        /// <summary>
        /// Guid setted to empty, alfaThermalExpansion setted to 0
        /// </summary>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        /// <param name="epsilon0">Yielding strain</param>
        public SteelMaterial(double elasticModulus, double poisson, double fy, double fu, double epsilon0, double density)
            : this(elasticModulus, poisson, fy, fu, epsilon0, density, 0, Guid.Empty)
        {
        }

        /// <summary>
        /// Guid setted to empty, alfaThermalExpansion setted to 0. Epsilon0 equal to fy / E
        /// </summary>
        /// <param name="elasticModulus">Steel elastic modulus</param>
        /// <param name="poisson">Poissoins's Ratio</param>
        /// <param name="fy">Yielding stress</param>
        /// <param name="fu">Ultimate stress</param>
        public SteelMaterial(double elasticModulus, double poisson, double fy, double fu, double density)
            : this(elasticModulus, poisson, fy, fu, fy / elasticModulus, density, 0, Guid.Empty)
        {
            if (fy == 0)
            {
                throw new ArgumentException($"{nameof(fy)} cannot be zero");
            }
            if (elasticModulus == 0)
            {
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be zero");
            }
        }

        public SteelMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _fu = info.GetDouble("Fu");
            _fy = info.GetDouble("Fy");
            _epsilon0 = info.GetDouble("Epsilon0");
            _elasticModulus = info.GetDouble("ElasticModulus");
            _poisson = info.GetDouble("Poisson");
        }

        #endregion CONSTRUCTORS

        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ElasticModulus", _elasticModulus);
            info.AddValue("Poisson", _poisson);
            info.AddValue("Epsilon0", _epsilon0);
            info.AddValue("Fy", _fy);
            info.AddValue("Fu", _fu);
        }

        #endregion PUBLIC METHODS
    }
}