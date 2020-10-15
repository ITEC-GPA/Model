using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    public class GlassMaterial : Material
    {
        #region VARIABLES

        protected double _elasticModulus;
        protected double _poisson;
        protected double _fgk;

        #endregion VARIABLES

        #region PROPERTIES

        public double ElasticModulus => _elasticModulus;
        public double Poisson => _poisson;
        public double Fgk => _fgk;

        #endregion PROPERTIES

        #region CONSTRUCTOR

        public GlassMaterial(double elasticModulus, double poisson, double fgk, double density, double alfaThermalExpansion, Guid guid)
            : base(density, alfaThermalExpansion, guid)
        {
            if (elasticModulus == 0)
            {
                throw new ArgumentException($"{nameof(elasticModulus)} cannot be zero");
            }
            if (poisson == 0)
            {
                throw new ArgumentException($"{nameof(poisson)} cannot be zero");
            }
            if (fgk == 0)
            {
                throw new ArgumentException($"{nameof(fgk)} cannot be zero");
            }

            this._elasticModulus = elasticModulus;
            this._poisson = poisson;
            this._fgk = fgk;
        }

        public GlassMaterial(double elasticModulus, double poisson, double fgk, double density)
            : this(elasticModulus, poisson, fgk, density, 0, Guid.Empty)
        {
        }

        public GlassMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _fgk = info.GetDouble("Fgk");
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
            info.AddValue("Fgk", _fgk);
        }

        #endregion PUBLIC METHODS
    }
}