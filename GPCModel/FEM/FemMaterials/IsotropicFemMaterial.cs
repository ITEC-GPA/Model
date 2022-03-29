using System;
using System.Runtime.Serialization;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.Fem.Materials
{
    [Serializable]
    public class IsotropicFemMaterial : FemMaterial
    {
        protected readonly double _e;
        protected readonly double _ni;
        protected readonly double _g;
        protected readonly double _alpha;

        public double E => _e;

        public double G => _g;

        public double Ni => _ni;

        public double Alpha => _alpha;

        /// <param name="E"></param>
        /// <param name="ni"></param>
        /// <param name="alpha"></param>
        /// <param name="density"></param>
        /// <remarks>If <paramref name="E"/> is zero, it will be setted to <see cref="FemOptions.ZeroElasticModulus"/>
        /// <para><see cref="G"/> is calculated from <paramref name="E"/> and <paramref name="ni"/></para></remarks>
        /// <exception cref="ArgumentException"></exception>
        public IsotropicFemMaterial(double E, double ni, double alpha, double density)
            : base(string.Empty, density)
        {
            _e = E < FemOptions.Instance.ZeroElasticModulus ? FemOptions.Instance.ZeroElasticModulus : E;

            _ni = ni < 0 || ni >= 0.5 ? throw new ArgumentException($"Poisson cannot be greater equal than 0.5 or lower than 0") : ni;

            _alpha = alpha < 0 ? throw new ArgumentException($"Linear thermal expansion coefficient cannot be lower than zero") : alpha;

            _g = E / (2.0 * (1.0 + ni));

            if (_g < 0)
                throw new ArgumentException($"Shear modulus cannot be lower than zero");
        }

        public IsotropicFemMaterial(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _e = info.GetDouble("E");
            _ni = info.GetDouble("Ni");
            _g = info.GetDouble("G");
            _alpha = info.GetDouble("Alfa");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("E", _e, typeof(double));
            info.AddValue("Ni", _ni, typeof(double));
            info.AddValue("G", _g, typeof(double));
            info.AddValue("Alfa", _alpha, typeof(double));
        }

        public override Matrix<double> GetPlaneStress()
        {
            return GetMatrixPlaneStress(_e, _ni);
        }

        public override Matrix<double> Get3DSolidStress()
        {
            return GetBrickD(_e, _ni);
        }

        #region Matematica

        /// <summary>
        /// Matrice stato piano di tensione da materiale elastico lineare isotropo
        /// </summary>
        /// <param name="E"></param>
        /// <param name="ni"></param>
        /// <returns></returns>
        internal static Matrix<double> GetMatrixPlaneStress(double E, double ni)
        {
            Matrix<double> D = Matrix<double>.Build.Dense(3, 3);
            D[0, 0] = 1.0;
            D[0, 1] = ni;
            D[1, 0] = ni;
            D[1, 1] = 1.0;
            D[2, 2] = (1.0 - ni) / 2.0;
            D = E / (1.0 - ni * ni) * D;
            return D;
        }

        /// <summary>
        /// reference eq. 11.10 - Finite element method by Rao
        /// </summary>
        /// <param name="E"></param>
        /// <param name="poisson"></param>
        /// <returns></returns>
        internal static Matrix<double> GetBrickD(double E, double poisson)
        {
            double factor = E / ((1.0 + poisson) * (1.0 - 2.0 * poisson));

            Matrix<double> d = Matrix<double>.Build.Dense(6, 6);

            d[0, 0] = 1.0 - poisson;
            d[0, 1] = poisson;
            d[0, 2] = poisson;

            d[1, 0] = poisson;
            d[1, 1] = 1.0 - poisson;
            d[1, 2] = poisson;

            d[2, 0] = poisson;
            d[2, 1] = poisson;
            d[2, 2] = (1.0 - poisson);

            d[3, 3] = (1.0 - 2.0 * poisson) / 2.0;

            d[4, 4] = (1.0 - 2.0 * poisson) / 2.0;

            d[5, 5] = (1.0 - 2.0 * poisson) / 2.0;

            return factor * d;
        }


        #endregion
    }
}
