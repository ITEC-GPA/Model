using GPC.Model.Materials;
using System.Runtime.Serialization;
using GPC.Model.Elements.Glasses;


namespace GPC.Model.Elements
{
    public class MonolithicGlassProperty : PlateProperty, IGlassProperty
    {



        public MonolithicGlassProperty(MonolithicGlass monolithicGlass)
            : this(monolithicGlass.Thickness, monolithicGlass.Thickness, monolithicGlass.Material)
        {

        }

        public MonolithicGlassProperty(double thickness, GlassMaterial material)
            : this(thickness, thickness, material)
        {

        }

        public MonolithicGlassProperty(double tb, double tm, GlassMaterial material) 
            : base(material, tb, tm)
        {

        }

        public MonolithicGlassProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }

    }
}
