using System;
using System.Runtime.Serialization;
using GPC.Model.Materials;
using GPC.Model.Glasses;
using System.Collections.Generic;

namespace GPC.Model.FEM.Properties
{
    public sealed class MonolithicGlassProperty : PlateProperty, IGlassProperty, IEquatable<MonolithicGlassProperty>
    {

        public MonolithicGlassProperty(MonolithicGlass monolithicGlass, string name)
            : this(monolithicGlass.Thickness, monolithicGlass.Thickness, monolithicGlass.Material, name)
        {

        }

        public MonolithicGlassProperty(double thickness, GlassMaterial material, string name)
            : this(thickness, thickness, material, name)
        {
            
        }

        public MonolithicGlassProperty(double tb, double tm, GlassMaterial material, string name) 
            : base(material, tb, tm, name)
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

        public bool Equals(MonolithicGlassProperty other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as MonolithicGlassProperty);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(MonolithicGlassProperty obj1, MonolithicGlassProperty obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(MonolithicGlassProperty obj1, MonolithicGlassProperty obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
