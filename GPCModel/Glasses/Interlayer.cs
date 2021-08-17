using GPC.Model.Materials;
using GPC.Utilities.Attributes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Glasses
{
    /// <summary>
    /// Abstract class that represent the interlayer between two monolithic glasses to compose a laminated glass
    /// </summary>
    [Serializable]
    [UI(Description = "Interlayer", Group = "Glasses", Kind = "Interlayer")]
    public sealed class Interlayer : ModelObject, IEquatable<Interlayer>, IGlassPackage
    {
        #region VARIABLES

        private double _thickness;

        private InterlayerMaterial _interlayerMaterial;

        #endregion

        #region PROPERTIES

        public double Thickness => _thickness;

        public InterlayerMaterial Material => _interlayerMaterial;

        #endregion

        #region Constructors

        public Interlayer(string name, double thickness, InterlayerMaterial interlayerMaterial)
            : base(Guid.NewGuid(), name)
        {
            _thickness = thickness;
            _interlayerMaterial = interlayerMaterial;
        }

        public Interlayer(string name, double thickness, InterlayerMaterial interlayerMaterial, Guid guid)
            : base(guid, name)
        {
            _thickness = thickness;
            _interlayerMaterial = interlayerMaterial;
        }

        public Interlayer(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _interlayerMaterial = (InterlayerMaterial)info.GetValue("InterlayerMaterial", typeof(InterlayerMaterial));
            _thickness = info.GetDouble("Thickness");
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("InterlayerMaterial", _interlayerMaterial, typeof(InterlayerMaterial));
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
            return Equals(obj as Interlayer);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _thickness.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<InterlayerMaterial>.Default.GetHashCode(_interlayerMaterial);
                return hashCode; 
            }
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

        #endregion
    }
}