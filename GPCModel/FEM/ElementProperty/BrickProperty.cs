using System;
using System.Diagnostics;
using System.Runtime.Serialization;
using GPC.Model.FEM.Materials;

namespace GPC.Model.FEM.Properties
{
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    [Serializable]
    public class BrickProperty : ElementProperty, ISerializable
    {

        protected FemMaterial _material;

        public FemMaterial Material => _material;


        public BrickProperty(FemMaterial material, string name) : base(name)
        {
            _material = material ?? throw new ArgumentNullException("Material cannot be null");
        }

        protected BrickProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _material = (FemMaterial)info.GetValue("Material", typeof(FemMaterial));
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Material", _material);
        }

        private string GetDebuggerDisplay()
        {
            return $"BrickProperty: {_name}";
        }
    }
}