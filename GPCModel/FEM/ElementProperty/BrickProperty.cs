
using System;
using GPC.Model.Materials;



namespace GPC.Model.FEM.Properties
{

    public class BrickProperty : ElementProperty, IBrickProperty
    {
        
        protected Material _material;


        public BrickProperty(Material material, string name) : base(name, Guid.NewGuid())
        {

            _material = material ?? throw new ArgumentNullException("Brick property material cannot be null");
        }

    }

}
