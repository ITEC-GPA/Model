using GPC.Model.Materials;
using System;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.Elements.Glasses;
using System.Collections.Generic;

namespace GPC.Model.Elements
{
    public class LaminatedGlassProperty : ElementProperty, IPlateProperty, IGlassProperty, IEquatable<LaminatedGlassProperty>
    {

        private List<double> _glassBendingThickness;
        private List<double> _glassMembraneThickness;

        private List<double> _interlayerThickness;

        private List<GlassMaterial> _glassMaterials;
        private List<InterlayerMaterial> _interlayerMaterials;


        public List<double> GlassBendingThickness => _glassBendingThickness;

        public List<double> GlassMembraneThickness => _glassMembraneThickness;

        public List<double> InterlayerThickness => _interlayerThickness;


        public LaminatedGlassProperty(LaminatedGlass laminatedGlass)
            : base(Guid.NewGuid())
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


        public LaminatedGlassProperty(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        public virtual List<double> GetGlassE()
        {
            return _glassMaterials.Select(i => i.E).ToList();
        }

        public virtual List<double> GetGlassNi()
        {
            return _glassMaterials.Select(i => i.Ni).ToList();
        }

        public virtual List<double> GetGlassG()
        {
            return _glassMaterials.Select(i => i.E / (2.0 * (1.0 + i.Ni))).ToList();
        }

        public virtual List<double> GetInterlayerG(double loadDuration, double temperature)
        {
            return _interlayerMaterials.Select(i => i.GetShearModule(loadDuration, temperature)).ToList();
        }

        public bool Equals(LaminatedGlassProperty other)
        {
            return !(other is null) && base.Equals(other) &&
                                       other._glassBendingThickness == _glassBendingThickness &&
                                       other._glassMembraneThickness == _glassMembraneThickness &&
                                       other._interlayerThickness == _interlayerThickness &&
                                       other._glassMaterials == _glassMaterials &&
                                       other._interlayerMaterials == _interlayerMaterials;
        }
    }
}
