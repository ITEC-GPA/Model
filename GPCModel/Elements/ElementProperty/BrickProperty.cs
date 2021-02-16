
using System;
using GPC.Model.Materials;



namespace GPC.Model.Elements
{

    public class BrickProperty : ElementProperty, IBrickProperty
    {
        
        protected Material _material;


        public BrickProperty(Material material) : base(Guid.NewGuid())
        {

            _material = material ?? throw new ArgumentNullException("Brick property material cannot be null");
        }

    }

}
