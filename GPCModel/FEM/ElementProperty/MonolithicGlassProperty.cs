using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Model.Fem.Materials;
using GPC.Model.Glasses;
using GPC.Model.Materials;

namespace GPC.Model.Fem.Properties
{
    [Serializable]
    public sealed class MonolithicGlassProperty : PlateProperty, IGlassProperty, IPlateProperty, IEquatable<MonolithicGlassProperty>, ISerializable
    {

        public MonolithicGlassProperty(MonolithicGlass monolithicGlass, string name)
            : this(monolithicGlass.Thickness, monolithicGlass.Thickness, monolithicGlass.Material.GetIsotropicFemMaterial(), name)
        {

        }

        public MonolithicGlassProperty(double thickness, FemMaterial material, string name)
            : this(thickness, thickness, material, name)
        {

        }

        public MonolithicGlassProperty(double tb, double tm, FemMaterial material, string name)
            : base(material, tb, tm, name)
        {

        }

        public MonolithicGlassProperty(SerializationInfo info, StreamingContext context) : base(info, context)
        {

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
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                return hashCode; 
            }
        }

        public static bool operator ==(MonolithicGlassProperty obj1, MonolithicGlassProperty obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(MonolithicGlassProperty obj1, MonolithicGlassProperty obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
