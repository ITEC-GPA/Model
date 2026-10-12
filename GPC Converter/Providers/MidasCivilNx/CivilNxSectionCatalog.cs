using System;
using System.Linq;
using System.Threading;
using GPC.Model.Analysis;
using GPC.Model.Core.Diagnostics;

namespace GPC.Converter.CivilNx
{
    public sealed partial class CivilNxModelProfile
    {
        /// <summary>Reads UNIT/SECT and optional SECTIONALL without requiring nodes, elements, materials or combinations.
        /// Uses the very same parser as full model import; preserved source responses remain the source of unsupported attributes.</summary>
        public ImportBatch ReadSectionCatalog(CivilNxSnapshot snapshot, AnalysisSource identity, CancellationToken cancellationToken = default)
        {
            if (snapshot == null || identity == null || identity.Program != Program || string.IsNullOrWhiteSpace(identity.ModelRevision))
                throw new ArgumentException("ExplicitCivilNxModelIdentityRequired");
            return new Reader(snapshot, identity, cancellationToken, stageResultCases).ReadSectionCatalog();
        }
        private sealed partial class Reader
        {
            // Physical geometry only. This does not turn a steel/concrete assembly into a homogeneous beam property.
            private SectionComponentRecord[] CompositeGeometry(SectionData data, string record)
            {
                var before = data.Before; var size = before?.First?.Size; var slab = data.After?.Slab;
                var stiffener = before?.First?.Stiffener;
                if (before?.Shape != "I" || size == null || size.Length < 6 || slab == null || slab.Length < 3 ||
                    slab[2] != 0 || stiffener != null && (stiffener.Counts == null || stiffener.Counts.Any(n => n != 0)))
                {
                    Warn("CivilNxCompositeOutlinePreserved", record,
                        "Composite outline retained only: supported inspection is symmetric steel-I with slab, no haunch and no stiffeners.");
                    return null;
                }
                if (size.Take(6).Concat(slab.Take(2)).Any(v => !(v > 0) || double.IsInfinity(v)))
                    throw new System.IO.InvalidDataException("InvalidCivilNxCompositeDimensions: " + record);
                // Civil NX I: Hw, tw, B1, tf1, B2, tf2. Model I uses overall height.
                var d = size.Select(L).ToArray(); var b = L(slab[0]); var t = L(slab[1]); var h = d[0] + d[3] + d[5];
                return new[]
                {
                    new SectionComponentRecord { Name = "Steel girder", Material = MaterialKind.Steel, Shape = SectionShapeKind.I,
                        Dimensions = new[] { h, d[2], d[1], d[3], d[4], d[5], 0d }, X = (b - Math.Max(d[2], d[4])) / 2 },
                    new SectionComponentRecord { Name = "Concrete slab", Material = MaterialKind.Concrete, Shape = SectionShapeKind.SolidRectangle,
                        Dimensions = new[] { t, b }, Y = h }
                };
            }

            public ImportBatch ReadSectionCatalog()
            {
                token.ThrowIfCancellationRequested(); Units(); Sections();
                foreach (var w in warnings) batch.Diagnostics.Add(new ModelDiagnostic { Code = w.Key.Split('\u001f')[0],
                    Severity = DiagnosticSeverity.Warning, Record = w.Key.Split('\u001f')[1], Message = w.Key.Split('\u001f')[2] });
                return batch;
            }
        }
    }
}
