using System.Runtime.Serialization;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.Materials
{

    public abstract class FemMaterial : ModelObject
    {
        protected double _density;

        public double Density => _density;


        protected FemMaterial(string name, double density) 
            : base(name)
        {
            _density = density;
        }


        protected FemMaterial(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }

        public abstract Matrix<double> GetPlaneStress();

        public abstract Matrix<double> Get3DSolidStress();


    }
}