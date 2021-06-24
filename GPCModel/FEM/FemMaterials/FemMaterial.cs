using System;
using System.Runtime.Serialization;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM.Materials
{

    public abstract class FemMaterial : ModelObject
    {
        protected readonly double _density;

        public double Density => _density;

        protected FemMaterial(string name, double density) 
            : base(name)
        {
            _density = density < 0 ? throw new ArgumentException($"{nameof(density)} cannot be lower than zero") : density;
        }

        protected FemMaterial(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _density = info.GetDouble("Density");
        }

        public abstract Matrix<double> GetPlaneStress();

        public abstract Matrix<double> Get3DSolidStress();

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Density", _density);
        }
    }
}