using GPC.Model.Materials;
using GPC.Model.FEM.Materials;
using System;
using System.Diagnostics;

namespace GPC.Model.FEM.Properties
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class BrickProperty : ElementProperty
    {

        protected FemMaterial _material;

        public FemMaterial Material => _material;


        public BrickProperty(FemMaterial material, string name) : base(name)
        {
            _material = material ?? throw new ArgumentNullException("Material cannot be null");
        }



        private string GetDebuggerDisplay()
        {
            return $"BrickProperty: {_name}";
        }
    }
}