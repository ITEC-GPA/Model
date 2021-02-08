using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Elements.Glasses
{
    /// <summary>
    /// Abstract class that represent the interlayer between two monolithic glasses to compose a laminated glass
    /// </summary>
    [Serializable]
    public class Interlayer : ModelObject, IEquatable<Interlayer>
    {
        #region VARIABLES

        protected double _thickness;

        protected InterlayerMaterial _interlayerMaterial;

        #endregion

        #region PROPERTIES

        public double Thickness => _thickness;

        public InterlayerMaterial Material => _interlayerMaterial;

        #endregion

        public Interlayer(string name, double thickness, InterlayerMaterial interlayerMaterial)
            : base(Guid.NewGuid(), name)
        {
            this._thickness = thickness;
            this._interlayerMaterial = interlayerMaterial;
        }

        public Interlayer(string name, double thickness, InterlayerMaterial interlayerMaterial, Guid guid)
            : base(guid, name)
        {
            this._thickness = thickness;
            this._interlayerMaterial = interlayerMaterial;
        }

        public Interlayer(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _interlayerMaterial = (InterlayerMaterial)info.GetValue("InterlayerMaterial", typeof(InterlayerMaterial));
            _thickness = info.GetDouble("Thickness");
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("InterlayerMaterial", _interlayerMaterial);
            info.AddValue("Thickness", _thickness);
        }

        public bool Equals(Interlayer other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._thickness.Equals(_thickness)
                                    && other._interlayerMaterial.Equals(_interlayerMaterial)
                                    && base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj as Interlayer);
        }

        public override int GetHashCode()
        {
            int hashCode = 23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + _thickness.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<InterlayerMaterial>.Default.GetHashCode(_interlayerMaterial);
            return hashCode;
        }

        public static bool operator ==(Interlayer obj1, Interlayer obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(Interlayer obj1, Interlayer obj2)
        {
            return !(obj1 == obj2);
        }
    }
}