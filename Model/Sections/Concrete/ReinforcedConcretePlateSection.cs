using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.Materials;
using GPC.Model.ElementProperties;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;

namespace GPC.Model.Sections.Concrete
{
    /// <summary>Concrete plate section with its own reinforcement, in section-local coordinates (mm/rad).
    /// Assignment axes place this reusable section on each element. No resistant method is implied.</summary>
    [Serializable]
    public sealed class ReinforcedConcretePlateSection : ConcretePlateProperty
    {
        public List<ShellRebarLayer> RebarLayers { get; private set; }
        public ReinforcedConcretePlateSection(ConcreteMaterial material, double physicalThickness, double bendingThickness,
            double membraneThickness, IEnumerable<ShellRebarLayer> rebarLayers, string name = "")
            : base(material, bendingThickness, membraneThickness, name)
        {
            PhysicalThickness = physicalThickness;
            RebarLayers = rebarLayers?.ToList() ?? throw new ArgumentNullException(nameof(rebarLayers));
            Validate();
        }
        public void Validate()
        {
            PlateSectionValidation.ValidateProperty(this);
        }
        private ReinforcedConcretePlateSection(SerializationInfo info, StreamingContext context) : base(info, context)
        { RebarLayers = ((ShellRebarLayer[])info.GetValue("RebarLayers", typeof(ShellRebarLayer[]))).ToList(); Validate(); }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        { base.GetObjectData(info, context); info.AddValue("RebarLayers", RebarLayers.ToArray()); }
        public override bool Equals(object obj) => obj is ReinforcedConcretePlateSection other && base.Equals(other)
            && ModelArchive.Fingerprint(RebarLayers.Cast<object>()) == ModelArchive.Fingerprint(other.RebarLayers.Cast<object>());
        public override int GetHashCode() => base.GetHashCode();
    }
}
