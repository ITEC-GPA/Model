using MathNet.Numerics.LinearAlgebra;
using System;
using System.Runtime.Serialization;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.Materials
{

    public class IsotropicFemMaterial : FemMaterial
    {

        protected double _e;

        protected double _ni;

        protected double _g;

        protected double _alpha;


        public double E => _e;
        public double G => _g;
        public double Ni => _ni;
        public double Alpha => _alpha;


        internal IsotropicFemMaterial(double E, double ni, double alpha, double density) : base(string.Empty, density)
        {
            _e = E < FemOptions.Instance.ZeroElasticModulus ? FemOptions.Instance.ZeroElasticModulus : E;

            _ni = ni < 0 || ni >= 0.5 ? throw new ArgumentException($"Poisson cannot be greater equal than 0.5 or lower than 0") : ni;

            _alpha = alpha < 0 ? throw new ArgumentException($"Linear thermal expansion coefficient cannot be lower than zero") : alpha;

            _density = density < 0 ? throw new ArgumentException($"{nameof(density)} cannot be lower than zero") : density;
            
            _g = E * 2.0 * (1.0 + ni);

            if (_g < 0)
                throw new ArgumentException($"Shear modulus cannot be lower than zero");

        }


        internal IsotropicFemMaterial(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }


        public override Matrix<double> GetPlaneStress()
        {
            return GetMatrixPlaneStress(_e,_ni);
        }

        public override Matrix<double> Get3DSolidStress()
        {
            return GetBrickD(_e,_ni);
        }

        public double GetShearModule()
        {
            return GetShearModulus(E, Ni);
        }

        #region Matematica
        /// <summary>
        /// Matrice stato piano di tensione da materiale elastico lineare isotropo
        /// </summary>
        /// <param name="E"></param>
        /// <param name="ni"></param>
        /// <returns></returns>
        internal static mnl.Matrix<double> GetMatrixPlaneStress(double E, double ni)
        {
            mnl.Matrix<double> D = mnl.Matrix<double>.Build.Dense(3, 3);
            D[0, 0] = 1.0;
            D[0, 1] = ni;
            D[1, 0] = ni;
            D[1, 1] = 1.0;
            D[2, 2] = (1.0 - ni) / 2.0;
            D = E / (1.0 - ni * ni) * D;
            return D;
        }

        private static double GetShearModulus(double E, double ni)
        {
            return E / (2.0 * (1.0 + ni));
        }

        /// <summary>
        /// reference eq. 11.10 - Finite element method by Rao
        /// </summary>
        /// <param name="E"></param>
        /// <param name="poisson"></param>
        /// <returns></returns>
        internal static mnl.Matrix<double> GetBrickD(double E, double poisson)
        {
            double factor = E / ((1.0 + poisson) * (1.0 - 2.0 * poisson));

            mnl.Matrix<double> d = mnl.Matrix<double>.Build.Dense(6, 6);

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

            /*Console.WriteLine("D");
            Util.WriteMatrix(factor * d);*/
            return factor * d;
        }
        #endregion
    }

}
