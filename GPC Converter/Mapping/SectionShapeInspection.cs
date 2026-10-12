using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Sections;

namespace GPC.Converter
{
    /// <summary>Section geometry for inspection, independent of material strength resolution and of a model import.
    /// Uses the section and reference mapping used by ModelMapper. All lengths are in mm.</summary>
    public sealed class SectionShapeInspection
    {
        private SectionShapeInspection(SectionRecord source, ISectionShape shape, Vector2d offset,
            SectionInspectionPart[] parts, List<ModelDiagnostic> diagnostics)
        {
            Source = source; Shape = shape; CentroidOffset = offset; Parts = Array.AsReadOnly(parts); Diagnostics = diagnostics.AsReadOnly();
        }
        public SectionRecord Source { get; }
        /// <summary>Geometric shape. For multiple materials it is a physical union, NOT a homogenized analysis section.</summary>
        public ISectionShape Shape { get; }
        public bool HasMultipleMaterials => Source.GeometryComponents != null && Source.GeometryComponents.Length > 0;
        /// <summary>Geometric centroid minus beam reference point. Null when the source reference cannot be located on this geometry.</summary>
        public Vector2d CentroidOffset { get; }
        public IReadOnlyList<SectionInspectionPart> Parts { get; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; }

        /// <summary>Resolves a fresh section using the common mapper. Numeric-only sections keep Values with no invented outline.</summary>
        public static SectionShapeInspection Read(SectionRecord source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            var batch = new ImportBatch(); batch.Sections.Add(source);
            var diagnostics = new List<ModelDiagnostic>();
            var mapper = new PropertyMapper(batch, null, diagnostics);
            if (source.GeometryComponents == null || source.GeometryComponents.Length == 0)
            {
                var shape = mapper.Shape(source);
                var offset = mapper.CentroidOffset(new BeamRecord { SectionId = source.Id }, shape);
                return new SectionShapeInspection(source, shape, offset, shape == null ? Array.Empty<SectionInspectionPart>() :
                    new[] { new SectionInspectionPart(source.Name ?? source.Id, MaterialKind.Other, (Section)shape, 0, 0) }, diagnostics);
            }
            if (source.GeometryComponents.Length > 100) throw new ArgumentException("TooManySectionComponents");
            var parts = source.GeometryComponents.Select(c =>
            {
                if (c == null || double.IsNaN(c.X) || double.IsInfinity(c.X) || double.IsNaN(c.Y) || double.IsInfinity(c.Y))
                    throw new ArgumentException("InvalidSectionComponent");
                var shape = PropertyMapper.Parametric(new SectionRecord { Id = source.Id, Shape = c.Shape, Dimensions = c.Dimensions }, c.Name);
                return new SectionInspectionPart(c.Name, c.Material, (Section)shape, c.X, c.Y);
            }).ToArray();
            var union = new SectionBuiltUp(parts.Select(p => new SectionBuiltUp.Part(p.Shape, p.X, p.Y)), source.Name);
            var reference = source.GeometryReference ?? source.Reference ?? SectionReference.Centroid;
            // A physical centroid is NOT the elastic centroid of a composite. Only an unambiguous bounding reference is resolved.
            bool canResolve = !reference.IsUnresolved && reference.Vertical != SectionVerticalReference.Centroid &&
                reference.Horizontal != SectionHorizontalReference.Centroid;
            Vector2d unionOffset = null;
            if (canResolve)
            {
                var geometryBatch = new ImportBatch();
                geometryBatch.Sections.Add(new SectionRecord { Id = source.Id, Name = source.Name, Record = source.Record, Reference = reference });
                unionOffset = new PropertyMapper(geometryBatch, null, diagnostics).CentroidOffset(new BeamRecord { SectionId = source.Id }, union);
            }
            else diagnostics.Add(new ModelDiagnostic { Code = "CompositeReferenceUnresolved", Severity = DiagnosticSeverity.Warning,
                Record = source.Record, Message = "Composite reference needs an elastic centroid or an unsupported offset; physical geometry is not an equivalent analysis section." });
            return new SectionShapeInspection(source, union, unionOffset, parts, diagnostics);
        }
    }

    public sealed class SectionInspectionPart
    {
        internal SectionInspectionPart(string name, MaterialKind material, Section shape, double x, double y)
        { Name = name; Material = material; Shape = shape; X = x; Y = y; }
        public string Name { get; }
        public MaterialKind Material { get; }
        public Section Shape { get; }
        public double X { get; }
        public double Y { get; }
    }
}
