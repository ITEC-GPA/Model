
using System;
using GPC.Model.Materials;

namespace GPC.Model.FEM.Properties
{

    public class BrickProperty : ElementProperty, IBrickProperty
    {
        
        protected Material _material;

        public BrickProperty(Material material) : base(Guid.NewGuid())
        {

            _material = material ?? throw new ArgumentNullException("Brick property material cannot be null");
        }

        public double GetE()
        {
            return _material.E;
        }

        public double GetNi()
        {
            return _material.Ni;
        }

    }

}
