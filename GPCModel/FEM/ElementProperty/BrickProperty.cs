using GPC.Model.Materials;
using System;

namespace GPC.Model.FEM.Properties
{
    public class BrickProperty : ElementProperty
    {
        protected Material _material;

        public BrickProperty(Material material, string name) : base(name)
        {
            _material = material ?? throw new ArgumentNullException("Material cannot be null");
        }

        public override double GetAlphaThermalExpansion()
        {
            return _material.AlfaThermalExpansion;
        }

        public override double GetDensity()
        {
            return _material.Density;
        }

        public override double GetE()
        {
            return _material.E;
        }

        public override double GetNi()
        {
            return _material.Ni;
        }

        public override double GetShearModule()
        {
            return _material.GetShearModule();
        }
    }
}