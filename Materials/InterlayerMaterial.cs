using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    public class InterlayerMaterial : Material
    {
        protected double _elasticModulus;
        protected double _shearModulus;
        protected double _poisson;

        #region PROPERTIES

        public double ElasticModulus => _elasticModulus;
        public double ShearModulus => _shearModulus;
        public double Poisson => _poisson;

        #endregion PROPERTIES

        #region CONSTRUCTOR

        public InterlayerMaterial(double elasticModulus, double shearModulus, double poisson, double density, double alfaThermalExpansion, Guid guid)
            : base(density, alfaThermalExpansion, guid)
        {
            this._elasticModulus = elasticModulus;
            this._shearModulus = shearModulus;
            this._poisson = poisson;
        }

        public InterlayerMaterial(SerializationInfo info, StreamingContext context) :
            base(info, context)
        {
            _shearModulus = info.GetDouble("ShearModulus");
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
            info.AddValue("ShearModulus", _shearModulus);
        }

        #endregion PUBLIC METHODS
    }
}