using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

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
            throw new NotImplementedException();
        }

        public override Matrix<double> Get3DSolidStress()
        {
            throw new NotImplementedException();
        }
    }

}
