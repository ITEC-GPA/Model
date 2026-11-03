using System;
using System.Runtime.Serialization;
using GPC.Model.Materials;
using GPC.Model.Sections;

namespace GPC.Model.ElementProperties
{
    /// <summary>Elastic beam section without an inferred design grade or strength. N, mm, t.</summary>
    [Serializable]
    public sealed class ElasticBeamProperty : BeamProperty
    {
        public Material Material { get; set; }
        public ISectionShape Section { get; set; }
        /// <summary>Effective shear areas (mm²); zero means no shear deformation in the native beam formulation.</summary>
        public double ShearArea1 { get; set; }
        public double ShearArea2 { get; set; }
        public BeamCutoffLaw CutoffLaw { get; set; }

        public ElasticBeamProperty(string name, Material material, ISectionShape section, double shearArea1 = 0, double shearArea2 = 0) : base(name)
        { Material = material; Section = section; ShearArea1 = shearArea1; ShearArea2 = shearArea2; Validate(); }
        public void Validate()
        {
            if (Material == null || Section == null) throw new ArgumentException("MissingElasticBeamMaterialOrSection");
            ElasticPropertyValidation.Positive(Section.Area, "SectionArea");
            ElasticPropertyValidation.Nonnegative(Section.J11, "SectionI11"); ElasticPropertyValidation.Nonnegative(Section.J22, "SectionI22");
            ElasticPropertyValidation.Nonnegative(Section.Jt, "SectionTorsion");
            ElasticPropertyValidation.Nonnegative(ShearArea1, "ShearArea1"); ElasticPropertyValidation.Nonnegative(ShearArea2, "ShearArea2");
            ElasticPropertyValidation.Material(Material);
            // A beam has one axial modulus: the native writer and the checkers read Material.E (review B5).
            if (Material.IsOrthotropic) throw new ArgumentException("OrthotropicBeamMaterialNotSupported");
            CutoffLaw?.Validate();
        }
        private ElasticBeamProperty(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            int version=SerializationVersions.Read(info, "ElasticBeamPropertyVersion", 2);
            if(version>=2)CutoffLaw=(BeamCutoffLaw)info.GetValue("CutoffLaw",typeof(BeamCutoffLaw));
            Material = (Material)info.GetValue("Material", typeof(Material)); Section = (ISectionShape)info.GetValue("Section", typeof(ISectionShape));
            ShearArea1 = info.GetDouble("ShearArea1"); ShearArea2 = info.GetDouble("ShearArea2");
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context); info.AddValue("ElasticBeamPropertyVersion", CutoffLaw==null?1:2);
            if(CutoffLaw!=null)info.AddValue("CutoffLaw",CutoffLaw);
            info.AddValue("Material", Material); info.AddValue("Section", Section); info.AddValue("ShearArea1", ShearArea1); info.AddValue("ShearArea2", ShearArea2);
        }
        /// <summary>Same name and same fingerprinted content; never throws, also for an incomplete property.</summary>
        public override bool Equals(object obj) => obj is ElasticBeamProperty other && base.Equals(other) && GPC.Model.Core.ModelValues.ContentEquals(this, other);
        /// <summary>The name hash (equal properties have the same name), so distinct properties do not share one bucket.</summary>
        public override int GetHashCode() => Name == null ? 0 : StringComparer.Ordinal.GetHashCode(Name);
    }

    public enum ElasticPlateFormulation { Shell, Membrane, PlaneStress, PlaneStrain, Axisymmetric, LoadPatch, ShearPanel }

    /// <summary>Elastic plate/shell property; strength checks require a separate recognized checking material.
    /// Independent bending and membrane thicknesses use mm. Load patches carry load without stiffness.</summary>
    [Serializable]
    public sealed class ElasticPlateProperty : PlateProperty, IFemPlateProperty
    {
        public Material Material { get; set; }
        public double BendingThickness { get; set; }
        public double MembraneThickness { get; set; }
        public ElasticPlateFormulation Formulation { get; set; }
        public ElasticPlateProperty(string name, Material material, double bendingThickness, double membraneThickness,
            ElasticPlateFormulation formulation = ElasticPlateFormulation.Shell) : base(name)
        { Material = material; BendingThickness = bendingThickness; MembraneThickness = membraneThickness; Formulation = formulation; Validate(); }
        public void Validate()
        {
            if (!Enum.IsDefined(typeof(ElasticPlateFormulation), Formulation)) throw new ArgumentException("InvalidElasticPlateFormulation");
            if (Formulation == ElasticPlateFormulation.LoadPatch)
            {
                if(Material != null || BendingThickness != 0 || MembraneThickness != 0) throw new ArgumentException("LoadPatchHasNoMaterialOrThickness");
                return;
            }
            ElasticPropertyValidation.Material(Material);
            ElasticPropertyValidation.Nonnegative(BendingThickness, "BendingThickness");
            ElasticPropertyValidation.Nonnegative(MembraneThickness, "MembraneThickness");
            if (Formulation != ElasticPlateFormulation.LoadPatch && MembraneThickness == 0 && BendingThickness == 0)
                throw new ArgumentException("ZeroElasticPlateThickness");
        }
        private ElasticPlateProperty(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            int version=SerializationVersions.Read(info, "ElasticPlatePropertyVersion", 2);
            Material = (Material)info.GetValue("Material", typeof(Material)); BendingThickness = info.GetDouble("BendingThickness");
            MembraneThickness = info.GetDouble("MembraneThickness"); Formulation = (ElasticPlateFormulation)info.GetInt32("Formulation");
            if(Formulation==ElasticPlateFormulation.ShearPanel&&version<2)throw new SerializationException("ShearPanelSchemaMismatch");
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context); info.AddValue("ElasticPlatePropertyVersion", Formulation==ElasticPlateFormulation.ShearPanel?2:1);
            info.AddValue("Material", Material); info.AddValue("BendingThickness", BendingThickness); info.AddValue("MembraneThickness", MembraneThickness); info.AddValue("Formulation", (int)Formulation);
        }
        /// <summary>Same name and same fingerprinted content; never throws, also for an incomplete property.</summary>
        public override bool Equals(object obj) => obj is ElasticPlateProperty other && base.Equals(other) && GPC.Model.Core.ModelValues.ContentEquals(this, other);
        public override int GetHashCode() => Name == null ? 0 : StringComparer.Ordinal.GetHashCode(Name);
    }

    internal static class ElasticPropertyValidation
    {
        internal static void Positive(double value, string name)
        { Nonnegative(value, name); if (value == 0) throw new ArgumentException("Zero" + name); }
        internal static void Nonnegative(double value, string name)
        { if (double.IsNaN(value) || double.IsInfinity(value) || value < 0) throw new ArgumentException("Invalid" + name); }
        internal static void Material(Material material)
        {
            if (material == null) throw new ArgumentException("MissingElasticMaterial");
            material.AnalysisData?.Validate();
            Nonnegative(material.Density, "Density");
            if (double.IsNaN(material.AlfaThermalExpansion) || double.IsInfinity(material.AlfaThermalExpansion)) throw new ArgumentException("InvalidThermalExpansion");
            if (material.Orthotropy != null) material.Orthotropy.Validate();
            else { Positive(material.E, "ElasticModulus"); if (material.Ni <= -1 || material.Ni >= .5 || double.IsNaN(material.Ni)) throw new ArgumentException("InvalidPoissonRatio"); }
        }
    }
}
