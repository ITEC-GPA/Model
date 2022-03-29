using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.Glasses;
using GPC.Model.Materials;

namespace GPC.Model.Fem.Properties
{
    public class LaminatedGlassProperty : ElementProperty, IPlateProperty, IGlassProperty, IEquatable<LaminatedGlassProperty>
    {

        private readonly List<double> _glassBendingThickness;
        private readonly List<double> _glassMembraneThickness;

        private readonly List<double> _interlayerThickness;

        private readonly List<GlassMaterial> _glassMaterials;
        private readonly List<InterlayerMaterial> _interlayerMaterials;


        public List<double> GlassBendingThickness => _glassBendingThickness;

        public List<double> GlassMembraneThickness => _glassMembraneThickness;

        public List<double> InterlayerThickness => _interlayerThickness;


        public LaminatedGlassProperty(LaminatedGlass laminatedGlass, string name)
            : base(name)
        {
            if (laminatedGlass == null)
                throw new ArgumentNullException("Laminated glass can not be null");

            if (laminatedGlass.MonolithicGlasses.Count() > 2)
                throw new NotImplementedException();

            if (laminatedGlass.Interlayers.Count() > 1)
                throw new NotImplementedException();

            _glassBendingThickness = laminatedGlass.MonolithicGlasses.Select(i => i.Thickness).ToList();
            _glassMembraneThickness = laminatedGlass.MonolithicGlasses.Select(i => i.Thickness).ToList();

            _interlayerThickness = laminatedGlass.Interlayers.Select(i => i.Thickness).ToList();

            _glassMaterials = laminatedGlass.MonolithicGlasses.Select(i => i.Material).ToList();
            _interlayerMaterials = laminatedGlass.Interlayers.Select(i => i.Material).ToList();
        }


        protected LaminatedGlassProperty(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            throw new NotImplementedException();
        }


        public virtual List<double> GetInterlayerShearModule(double loadDuration, double temperature)
        {
            return _interlayerMaterials.Select(i => i.GetShearModule(loadDuration, temperature)).ToList();
        }



        public bool Equals(LaminatedGlassProperty other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._glassBendingThickness.Equals(_glassBendingThickness) &&
                                       other._glassMembraneThickness.Equals(_glassMembraneThickness) &&
                                       other._interlayerThickness.Equals(_interlayerThickness) &&
                                       other._glassMaterials.Equals(_glassMaterials) &&
                                       other._interlayerMaterials.Equals(_interlayerMaterials) &&
                                       base.Equals(other);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as LaminatedGlassProperty);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<List<double>>.Default.GetHashCode(_glassBendingThickness);
                hashCode = hashCode * -17 + EqualityComparer<List<double>>.Default.GetHashCode(_glassMembraneThickness);
                hashCode = hashCode * -17 + EqualityComparer<List<double>>.Default.GetHashCode(_interlayerThickness);
                hashCode = hashCode * -17 + EqualityComparer<List<GlassMaterial>>.Default.GetHashCode(_glassMaterials);
                hashCode = hashCode * -17 + EqualityComparer<List<InterlayerMaterial>>.Default.GetHashCode(_interlayerMaterials);
                return hashCode; 
            }
        }

        public static bool operator ==(LaminatedGlassProperty obj1, LaminatedGlassProperty obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(LaminatedGlassProperty obj1, LaminatedGlassProperty obj2)
        {
            return !(obj1 == obj2);
        }
    }
}
