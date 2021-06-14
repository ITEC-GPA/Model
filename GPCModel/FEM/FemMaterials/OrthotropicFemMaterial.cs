using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Materials
{

    public class OrthotropicFemMaterial : FemMaterial
    {

        protected readonly double _e1;
        protected readonly double _e2;
        protected readonly double _e3;

        protected readonly double _ni12;
        protected readonly double _ni23;
        protected readonly double _ni31;

        protected readonly double _g12;
        protected readonly double _g23;
        protected readonly double _g31;

        protected readonly double _alpha1;
        protected readonly double _alpha2;
        protected readonly double _alpha3;


        public double E1 => _e1;
        public double E2 => _e2;
        public double E3 => _e3;

        public double Ni12 => _ni12;
        public double Ni23 => _ni23;
        public double Ni31 => _ni31;

        public double G12 => _g12;
        public double G23 => _g23;
        public double G31 => _g31;

        public double Alpha1 => _alpha1;
        public double Alpha2 => _alpha2;
        public double Alpha3 => _alpha3;



        /// <remarks>
        /// If <paramref name="e1"/> is zero, it will be setted to <see cref="FemOptions.ZeroElasticModulus"/>
        /// <para>If <paramref name="e2"/> is zero, it will be setted to <see cref="FemOptions.ZeroElasticModulus"/></para>
        /// <para>If <paramref name="e3"/> is zero, it will be setted to <see cref="FemOptions.ZeroElasticModulus"/></para>  
        /// If <paramref name="g12"/> is zero, it will be setted to <see cref="FemOptions.ZeroShearModulus"/>
        /// <para>If <paramref name="g23"/> is zero, it will be setted to <see cref="FemOptions.ZeroShearModulus"/></para>
        /// <para>If <paramref name="g31"/> is zero, it will be setted to <see cref="FemOptions.ZeroShearModulus"/></para>  
        /// </remarks>
        /// <exception cref="ArgumentException"></exception>
        public OrthotropicFemMaterial(double e1, double e2, double e3, double ni12, double ni23, double ni31, double g12, double g23, double g31, double alpha1, double alpha2, double alpha3, double density)
            : base(string.Empty, density)
        {
            _e1 = e1 < FemOptions.Instance.ZeroElasticModulus ? FemOptions.Instance.ZeroElasticModulus : e1;
            _e2 = e2 < FemOptions.Instance.ZeroElasticModulus ? FemOptions.Instance.ZeroElasticModulus : e2;
            _e3 = e3 < FemOptions.Instance.ZeroElasticModulus ? FemOptions.Instance.ZeroElasticModulus : e3;

            _ni12 = ni12 < 0 || ni12 >= 0.5 ? throw new ArgumentException($"Poisson cannot be greater equal than 0.5 or lower than 0") : ni12;
            _ni23 = ni23 < 0 || ni23 >= 0.5 ? throw new ArgumentException($"Poisson cannot be greater equal than 0.5 or lower than 0") : ni23;
            _ni31 = ni31 < 0 || ni31 >= 0.5 ? throw new ArgumentException($"Poisson cannot be greater equal than 0.5 or lower than 0") : ni31;

            _g12 = g12 < FemOptions.Instance.ZeroShearModulus ? FemOptions.Instance.ZeroShearModulus : g12;
            _g23 = g23 < FemOptions.Instance.ZeroShearModulus ? FemOptions.Instance.ZeroShearModulus : g23;
            _g31 = g31 < FemOptions.Instance.ZeroShearModulus ? FemOptions.Instance.ZeroShearModulus : g31;

            _alpha1 = alpha1 < 0 ? throw new ArgumentException($"Linear thermal expansion coefficient cannot be lower than zero") : alpha1;
            _alpha2 = alpha2 < 0 ? throw new ArgumentException($"Linear thermal expansion coefficient cannot be lower than zero") : alpha2;
            _alpha3 = alpha3 < 0 ? throw new ArgumentException($"Linear thermal expansion coefficient cannot be lower than zero") : alpha3;

        }
            

        public OrthotropicFemMaterial(SerializationInfo info, StreamingContext context) 
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
