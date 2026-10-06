using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Sections;
using GPC.Model.Data.Steel;
using GPC.Model.ElementProperties;
using GPC.Model.Materials;
using GPC.Model.PostProcessing;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;

namespace GPC.Converter
{
    /// <summary>Caller-supplied material for a source record; null leaves the record to the default grade resolution.</summary>
    public delegate Material MaterialResolver(MaterialRecord source);

    public sealed class MappingOptions
    {
        public MaterialResolver ResolveMaterial { get; set; }
    }

    /// <summary>Builds Model materials, section shapes and element properties from normalized source records.
    /// Strengths come from a recognised grade (EN 1993 steels, EN 1992 concrete classes) or from the caller's resolver;
    /// elastic source values never generate a strength. An unresolved record leaves the element without property.</summary>
    internal sealed class PropertyMapper
    {
        private static readonly Regex SteelGrade = new Regex(@"^S\s*(\d{3})", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
        private static readonly Regex ConcreteClass = new Regex(@"^C\s*(\d{2,3}(?:[.,]\d+)?)\s*[/_\-]\s*(\d{2,3})", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        private readonly ImportBatch batch;
        private readonly MappingOptions options;
        private readonly List<ModelDiagnostic> diagnostics;
        private readonly Dictionary<string, MaterialRecord> materialRecords;
        private readonly Dictionary<string, SectionRecord> sectionRecords;
        private readonly Dictionary<string, ThicknessRecord> thicknessRecords;
        private readonly Dictionary<string, Material> materials = new Dictionary<string, Material>(StringComparer.Ordinal);
        private readonly Dictionary<string, ISectionShape> shapes = new Dictionary<string, ISectionShape>(StringComparer.Ordinal);
        private readonly Dictionary<string, BeamProperty> beamProperties = new Dictionary<string, BeamProperty>(StringComparer.Ordinal);
        private readonly Dictionary<string, PlateProperty> plateProperties = new Dictionary<string, PlateProperty>(StringComparer.Ordinal);
        private readonly HashSet<string> reported = new HashSet<string>(StringComparer.Ordinal);

        public PropertyMapper(ImportBatch batch, MappingOptions options, List<ModelDiagnostic> diagnostics)
        {
            this.batch = batch; this.options = options ?? new MappingOptions(); this.diagnostics = diagnostics;
            materialRecords = Index(batch.Materials, m => m.Id, "Material");
            sectionRecords = Index(batch.Sections, s => s.Id, "Section");
            thicknessRecords = Index(batch.Thicknesses, t => t.Id, "Thickness");
        }

        private static Dictionary<string, T> Index<T>(IEnumerable<T> records, Func<T, string> id, string family)
        {
            var result = new Dictionary<string, T>(StringComparer.Ordinal);
            foreach (var record in records)
            {
                var key = id(record);
                if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Missing" + family + "Id");
                if (result.ContainsKey(key)) throw new ArgumentException("Duplicate" + family + ": " + key);
                result.Add(key, record);
            }
            return result;
        }

        /// <summary>The property of a beam: one shared instance for each section/material pair. Null when unresolved (diagnosed once).</summary>
        public BeamProperty Beam(BeamRecord beam, out ISectionShape shape)
        {
            shape = null;
            if (beam.SectionId == null && beam.MaterialId == null) return null;
            if (beam.SectionId == null || !sectionRecords.TryGetValue(beam.SectionId, out var section)) throw new ArgumentException("MissingSection: " + beam.SectionId);
            if (beam.MaterialId == null || !materialRecords.TryGetValue(beam.MaterialId, out var materialRecord)) throw new ArgumentException("MissingMaterial: " + beam.MaterialId);
            shape = Shape(section);
            var key = section.Id + "\u001f" + materialRecord.Id;
            if (beamProperties.TryGetValue(key, out var existing)) return existing;
            var material = Material(materialRecord);
            BeamProperty property = null;
            if (shape != null && material is SteelMaterial steel)
            {
                var type = section.CatalogDesignation != null || section.Shape == SectionShapeKind.Pipe || section.Shape == SectionShapeKind.SolidRectangle
                    || section.Shape == SectionShapeKind.SolidCircle || section.Shape == SectionShapeKind.Unknown ? Section.SectionTypes.Rolled : Section.SectionTypes.Welded;
                if (section.CatalogDesignation == null && section.Values == null)
                    Report("SteelSectionTypeAssumed", section.Record, "Section " + section.Name + ": user dimensions are taken as " + type + "; change it if the member is " +
                        (type == Section.SectionTypes.Rolled ? "welded." : "rolled."), DiagnosticSeverity.Information);
                property = new SteelSection(shape, steel, type, Section.FormedTypes.HotFinished);
            }
            else if (shape != null && material is ConcreteMaterial concrete)
            {
                property = new ReinforcedConcreteSection(shape, concrete);
                Report("ConcreteSectionWithoutReinforcement", section.Record, "Section " + section.Name + ": concrete shape imported without reinforcement; assign the rebars before checking.",
                    DiagnosticSeverity.Information);
            }
            else if (shape != null && material != null)
                Report("UnsupportedSectionMaterial", materialRecord.Record, "Material " + materialRecord.Name + " is neither steel nor concrete: beams keep no property.");
            if (!(property is null))
            {
                var name = section.Name ?? section.Id;
                if (batch.Beams.Where(b => b.SectionId == section.Id).Select(b => b.MaterialId).Distinct().Count() > 1) name += " / " + (materialRecord.Name ?? materialRecord.Id);
                property.Name = name;
            }
            beamProperties.Add(key, property);
            return property;
        }

        /// <summary>The property of a shell: one shared instance for each thickness/material pair. Null when unresolved (diagnosed once).</summary>
        public PlateProperty Plate(ShellRecord shell, out ThicknessRecord thickness)
        {
            thickness = null;
            if (shell.ThicknessId == null && shell.MaterialId == null) return null;
            if (shell.ThicknessId == null || !thicknessRecords.TryGetValue(shell.ThicknessId, out thickness)) throw new ArgumentException("MissingThickness: " + shell.ThicknessId);
            if (shell.MaterialId == null || !materialRecords.TryGetValue(shell.MaterialId, out var materialRecord)) throw new ArgumentException("MissingMaterial: " + shell.MaterialId);
            Positive(thickness.InPlane, "Thickness " + thickness.Id);
            if (thickness.OutOfPlane.HasValue) Positive(thickness.OutOfPlane.Value, "Thickness " + thickness.Id);
            var key = thickness.Id + "\u001f" + materialRecord.Id;
            if (plateProperties.TryGetValue(key, out var existing)) return existing;
            var material = Material(materialRecord);
            double membrane = thickness.InPlane, bending = thickness.OutOfPlane ?? thickness.InPlane;
            PlateProperty property = null;
            if (material is SteelMaterial steel) property = new SteelPlateProperty(steel, bending, membrane);
            else if (material is ConcreteMaterial concrete) property = new ConcretePlateProperty(concrete, bending, membrane);
            else if (material != null) Report("UnsupportedPlateMaterial", materialRecord.Record, "Material " + materialRecord.Name + " is neither steel nor concrete: plates keep no property.");
            if (!(property is null))
            {
                var thicknessId = thickness.Id; var name = thickness.Name ?? thicknessId;
                if (batch.Shells.Where(s => s.ThicknessId == thicknessId).Select(s => s.MaterialId).Distinct().Count() > 1) name += " / " + (materialRecord.Name ?? materialRecord.Id);
                property.Name = name;
            }
            plateProperties.Add(key, property);
            return property;
        }

        /// <summary>Centroid from the beam reference line in V1,V2 (mm); null when the geometry needed is unavailable.</summary>
        public Vector2d CentroidOffset(BeamRecord beam, ISectionShape shape)
        {
            if (beam.CentroidOffset != null) return beam.CentroidOffset;
            if (beam.SectionId == null) return null;
            var section = sectionRecords[beam.SectionId]; var reference = section.Reference ?? SectionReference.Centroid;
            Finite(reference.HorizontalShift, "section shift"); Finite(reference.VerticalShift, "section shift");
            if (reference.Horizontal == SectionHorizontalReference.Centroid && reference.Vertical == SectionVerticalReference.Centroid)
                return new Vector2d(-reference.HorizontalShift, -reference.VerticalShift);
            var points = shape?.GetSectionPoints();
            if (points == null || points.Length == 0)
            {
                Report("SectionReferenceUnresolved", section.Record, "Section " + section.Name + ": the reference point needs the section outline, which numeric properties do not give.");
                return null;
            }
            var c = shape.Centroid;
            double minX = points.Min(p => p.X), maxX = points.Max(p => p.X), minY = points.Min(p => p.Y), maxY = points.Max(p => p.Y);
            double x = reference.Horizontal == SectionHorizontalReference.Centroid ? c.X : reference.Horizontal == SectionHorizontalReference.Center ? (minX + maxX) / 2
                : reference.Horizontal == SectionHorizontalReference.MinimumV1 ? minX : maxX;
            double y = reference.Vertical == SectionVerticalReference.Centroid ? c.Y : reference.Vertical == SectionVerticalReference.Center ? (minY + maxY) / 2
                : reference.Vertical == SectionVerticalReference.MinimumV2 ? minY : maxY;
            return new Vector2d(c.X - (x + reference.HorizontalShift), c.Y - (y + reference.VerticalShift));
        }

        private Material Material(MaterialRecord record)
        {
            if (materials.TryGetValue(record.Id, out var cached)) return cached;
            var material = options.ResolveMaterial?.Invoke(record);
            if (material == null && record.Kind != MaterialKind.Other)
            {
                bool fromName = string.IsNullOrWhiteSpace(record.Grade);
                var designation = (fromName ? record.Name : record.Grade)?.Trim() ?? "";
                material = Grade(designation, record.Kind);
                if (material == null && !fromName) material = Grade(record.Name?.Trim() ?? "", record.Kind);
                if (material != null && fromName)
                    Report("MaterialGradeFromName", record.Record, "Material " + record.Name + ": strength class read from the name, the source declares no database grade.",
                        DiagnosticSeverity.Information);
            }
            if (material == null)
                Report("MaterialGradeUnresolved", record.Record, "Material " + (record.Name ?? record.Id) + (record.Grade == null ? "" : " (" + record.Grade + ")")
                    + ": no recognised strength class. Elements using it keep no property; supply a MaterialResolver or assign it in the model.");
            else
            {
                if (!string.IsNullOrWhiteSpace(record.Name)) material.SetName(record.Name);
                // The mass density of the analysis replaces the default of the strength class: it is not a strength, and the self weight must match.
                if (record.Density.HasValue && record.Density.Value > 0)
                {
                    if (Math.Abs(record.Density.Value - material.Density) > 0.005 * material.Density)
                        Report("MaterialDensityFromAnalysis", record.Record, "Material " + record.Name + ": density " + (record.Density.Value * 1e12).ToString("G6", CultureInfo.InvariantCulture)
                            + " kg/m³ of the analysis replaces " + (material.Density * 1e12).ToString("G6", CultureInfo.InvariantCulture) + " kg/m³ of the strength class.", DiagnosticSeverity.Information);
                    material.Density = Finite(record.Density.Value, "density");
                }
                double modelModulus = material is ConcreteMaterial ? material.ElasticModulusCompression : material.ElasticModulusTension;
                if (record.ElasticModulus.HasValue && Math.Abs(record.ElasticModulus.Value - modelModulus) > 0.02 * Math.Abs(modelModulus))
                    Report("MaterialElasticModulusDiffers", record.Record, "Material " + record.Name + ": the analysis used E = "
                        + record.ElasticModulus.Value.ToString("G6", CultureInfo.InvariantCulture) + " N/mm², the strength class gives "
                        + modelModulus.ToString("G6", CultureInfo.InvariantCulture) + " N/mm².", DiagnosticSeverity.Information);
            }
            materials.Add(record.Id, material);
            return material;
        }

        private static Material Grade(string designation, MaterialKind kind)
        {
            if (kind != MaterialKind.Concrete)
            {
                var steel = SteelGrade.Match(designation);
                if (steel.Success)
                    switch (steel.Groups[1].Value)
                    {
                        case "235": return SteelMaterialEN1993Data.S235;
                        case "275": return SteelMaterialEN1993Data.S275;
                        case "355": return SteelMaterialEN1993Data.S355;
                        case "420": return SteelMaterialEN1993Data.S420;
                        case "450": return SteelMaterialEN1993Data.S450;
                    }
            }
            if (kind != MaterialKind.Steel)
            {
                var concrete = ConcreteClass.Match(designation);
                if (concrete.Success)
                {
                    double fck = double.Parse(concrete.Groups[1].Value.Replace(',', '.'), CultureInfo.InvariantCulture);
                    var name = "C" + concrete.Groups[1].Value.Replace(',', '.') + "/" + concrete.Groups[2].Value;
                    if (fck >= 12 && fck <= 90) return new ConcreteMaterialEN1992(name, fck, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
                }
            }
            return null;
        }

        private ISectionShape Shape(SectionRecord s)
        {
            if (shapes.TryGetValue(s.Id, out var cached)) return cached;
            ISectionShape shape = null; var name = s.Name ?? s.Id;
            if (!string.IsNullOrWhiteSpace(s.CatalogDesignation))
            {
                var profile = SectionCatalogs.Find(s.CatalogDesignation);
                if (profile != null) shape = SectionMappings.CreateSection(profile);
                else if (s.Shape == SectionShapeKind.Unknown && s.Values == null)
                    Report("SectionNotInCatalog", s.Record, "Section " + name + ": " + s.CatalogDesignation + " is not in the catalogs of GPCModelData.");
            }
            if (shape == null && s.Shape != SectionShapeKind.Unknown) shape = Parametric(s, name);
            if (shape == null && s.Values != null)
            {
                var v = s.Values;
                Positive(v.Area, "Section " + s.Id + " area"); Positive(v.I11, "Section " + s.Id + " I11"); Positive(v.I22, "Section " + s.Id + " I22");
                Finite(v.Torsion, "Section " + s.Id + " torsion"); Finite(v.Warping, "Section " + s.Id + " warping");
                shape = new Section(v.Area, v.I11, v.I22, v.Torsion, v.Warping, new Point2d(0, 0), new Point3d(0, 0, 0), 0, name);
                Report("SectionByValuesOnly", s.Record, "Section " + name + ": numeric properties only, no outline; checks needing the geometry are not available.",
                    DiagnosticSeverity.Information);
            }
            if (shape == null && s.Shape == SectionShapeKind.Unknown && string.IsNullOrWhiteSpace(s.CatalogDesignation))
                Report("SectionShapeUnsupported", s.Record, "Section " + name + ": shape not supported by the converter; beams keep no property.");
            shapes.Add(s.Id, shape);
            return shape;
        }

        /// <summary>Dimension order (mm): I and Channel H, Btop, tw, tftop, Bbottom, tfbottom[, r]; Angle H, B, tw (vertical leg), tf (horizontal leg)[, r];
        /// Tee H, B, tw, tf[, r]; Box H, B, tw, tftop, tfbottom; Pipe D, t; SolidRectangle H, B; SolidCircle D.</summary>
        private static ISectionShape Parametric(SectionRecord s, string name)
        {
            var d = s.Dimensions ?? new double[0];
            int required;
            switch (s.Shape)
            {
                case SectionShapeKind.I: case SectionShapeKind.Channel: required = 6; break;
                case SectionShapeKind.Angle: case SectionShapeKind.Tee: required = 4; break;
                case SectionShapeKind.Box: required = 5; break;
                case SectionShapeKind.Pipe: case SectionShapeKind.SolidRectangle: required = 2; break;
                case SectionShapeKind.SolidCircle: required = 1; break;
                default: throw new NotSupportedException("UnsupportedSectionShape: " + s.Shape);
            }
            if (d.Length < required) throw new ArgumentException("Section " + s.Id + ": " + s.Shape + " requires " + required + " dimensions.");
            for (int i = 0; i < required; i++) Positive(d[i], "Section " + s.Id + " dimension " + (i + 1));
            double r = d.Length > required ? Math.Max(0, Finite(d[required], "Section " + s.Id + " radius")) : 0;
            switch (s.Shape)
            {
                case SectionShapeKind.I: return new SectionH(d[0], d[2], d[1], d[3], d[4], d[5], name, r);
                case SectionShapeKind.Channel: return new SectionC(d[0], d[2], d[1], d[3], d[4], d[5], name, r);
                case SectionShapeKind.Angle: return new SectionL(d[1], d[3], d[0], d[2], name, r);
                case SectionShapeKind.Tee: return new SectionT(d[0], d[1], d[2], d[3], name, r);
                case SectionShapeKind.Box: return new SectionRHS(d[0], d[1], d[3], d[4], d[2], d[2], name);
                case SectionShapeKind.Pipe: return new SectionCHS(d[0], d[1], name);
                case SectionShapeKind.SolidRectangle: return new SectionRectangular(d[0], d[1], name);
                default: return new SectionCircular(d[0], name);
            }
        }

        private void Report(string code, string record, string message, DiagnosticSeverity severity = DiagnosticSeverity.Warning)
        {
            if (reported.Add(code + "\u001f" + record + "\u001f" + message))
                diagnostics.Add(new ModelDiagnostic { Code = code, Severity = severity, Record = record, Message = message });
        }
        private static double Finite(double value, string what)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentException("NonFiniteValue: " + what);
            return value;
        }
        private static void Positive(double value, string what)
        {
            if (!(Finite(value, what) > 0)) throw new ArgumentException("NonPositiveValue: " + what);
        }
    }
}
