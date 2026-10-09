using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;

namespace GPC.Model.PostProcessing
{
    public static class AssignmentValidation
    {
        private static bool Finite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
        public static IReadOnlyList<ModelDiagnostic> ValidateAssignments(this Models.Model model)
        {
            var result = new List<ModelDiagnostic>();
            foreach (var node in model.NodesElements.Values)
            {
                var data = node.Assignments;
                if (data.Dofs != null && data.Dofs.Length != 6) result.Add(ModelDiagnostic.Error("UnsupportedDofCount", node));
                foreach (var restraint in data.Restrains)
                {
                    if (restraint?.Restrain == null) { result.Add(ModelDiagnostic.Error("MissingRestrain", node)); continue; }
                    if (!ReferenceEquals(restraint.Restrain.Point, node)) result.Add(ModelDiagnostic.Error("RestrainNodeMismatch", node));
                    var dofs = restraint.Restrain.Restrains;
                    if (dofs.Any(d => (int)d.Dof < 0 || (int)d.Dof > 5)) result.Add(ModelDiagnostic.Error("UnsupportedDof", node));
                    if (dofs.GroupBy(d => d.Dof).Any(g => g.Count() > 1)) result.Add(ModelDiagnostic.Error("ConflictingRestrain", node));
                    if (dofs.Any(d => !Finite(d.Stiffness) || !Finite(d.ImposedDisplacement))) result.Add(ModelDiagnostic.Error("NonFiniteRestrain", node));
                    if (dofs.Any(d => d.IsRestrained && d.HasImposedDisplacement && d.ImposedDisplacement != 0)) result.Add(ModelDiagnostic.Error("ConflictingRestrain", node));
                    if (dofs.Any(d => d.Stiffness < 0)) result.Add(ModelDiagnostic.Error("NegativeLegacySpring", node));
                    if (dofs.Any(d => d.HasImposedDisplacement) && restraint.Case == null) result.Add(ModelDiagnostic.Error("MissingImposedDisplacementCase", node));
                    try { Axes.Validate(restraint.Restrain.CoordinateSystem); } catch (ArgumentException) { result.Add(ModelDiagnostic.Error("InvalidRestrainAxes", node)); }
                }
                foreach (var scope in data.Restrains.Where(r => r?.Restrain != null).GroupBy(r => new { r.Case, r.Phase }))
                    if (scope.SelectMany(r => r.Restrain.Restrains).GroupBy(d => d.Dof).Any(g => g.Count() > 1)) result.Add(ModelDiagnostic.Error("ConflictingRestrainScope", node));
                foreach (var spring in data.GroundSprings) result.AddRange(spring.Diagnose());
                if (data.Mass != null && (data.Mass.Matrix6x6RowMajor == null || data.Mass.Matrix6x6RowMajor.Length != 36 || data.Mass.Matrix6x6RowMajor.Any(v => !Finite(v))))
                    result.Add(ModelDiagnostic.Error("InvalidMassMatrix", node));
                if (data.Mass != null) try { Axes.Validate(data.Mass.Axes); } catch (ArgumentException) { result.Add(ModelDiagnostic.Error("InvalidMassAxes", node)); }
                foreach (var link in data.Links)
                {
                    if (!model.NodesElements.ContainsKey(link.OtherNodeId)) result.Add(ModelDiagnostic.Error("DanglingLinkReference", node));
                    if (link.Spring != null) result.AddRange(link.Spring.Diagnose());
                    if (link.KinematicCoefficients != null && (link.RightHandSide == null || link.KinematicCoefficients.Length != 12 * link.RightHandSide.Length
                        || link.KinematicCoefficients.Any(v => !Finite(v)) || link.RightHandSide.Any(v => !Finite(v)))) result.Add(ModelDiagnostic.Error("InvalidMpcMatrix", node));
                    if (link.UnsupportedLaw != null) result.Add(ModelDiagnostic.Error("UnsupportedLinkLaw", node));
                }
            }
            foreach (var beam in model.BeamElements.Values)
            {
                try
                {
                    new BeamReferenceGeometry(beam);
                    beam.Assignments.AnalysisProfile?.Validate();
                    if (beam.Assignments.SectionGeometryAxes != null)
                    {
                        Axes.Validate(beam.Assignments.SectionGeometryAxes); Axes.Validate(beam.Assignments.SectionAxes);
                        if (Math.Abs(Axes.Dot(beam.Assignments.SectionGeometryAxes.V3, beam.Assignments.SectionAxes.V3)) < 1 - 1e-8) throw new ArgumentException("SectionGeometryPlaneMismatch");
                    }
                }
                catch (ArgumentException ex) { result.Add(ModelDiagnostic.Error("InvalidBeamAssignment", beam, ex.Message)); }
                foreach (var release in beam.Attributes.Values.OfType<Attributes.BeamReleasesAttribute>())
                {
                    if (release.I == null || release.J == null || release.I.Length != 6 || release.J.Length != 6 || release.I.Concat(release.J).Any(c => c == null))
                        result.Add(ModelDiagnostic.Error("InvalidBeamRelease", beam));
                    try { Axes.Validate(release.CoordinateSystem); } catch (ArgumentException) { result.Add(ModelDiagnostic.Error("MissingReleaseAxes", beam)); }
                }
                var sorted = beam.Assignments.Sections.OrderBy(a => a.Start).ToArray();
                for (int i = 0; i < sorted.Length; i++)
                {
                    var a = sorted[i];
                    if (!Finite(a.Start) || !Finite(a.End) || a.Start < 0 || a.End > 1 || a.Start >= a.End) result.Add(ModelDiagnostic.Error("InvalidSectionInterval", beam));
                    if (i > 0 && a.Start < sorted[i - 1].End) result.Add(ModelDiagnostic.Error("OverlappingSectionIntervals", beam));
                    try
                    {
                        if (a.Property is ReinforcedConcreteSection concrete) result.AddRange(ValidateRebars(concrete, beam));
                        if (a.EndProperty is ReinforcedConcreteSection endConcrete) result.AddRange(ValidateRebars(endConcrete, beam));
                        if (a.Law == "LinearRectangular") SectionLaws.EvaluateProperty(a, (a.Start + a.End) / 2, SectionSide.Unspecified);
                        else if (a.Law == "Tabulated")
                        {
                            if (a.Stations == null || a.Stations.Count == 0 || a.Stations.Any(s => s == null || !Finite(s.Station) || s.Station < a.Start || s.Station > a.End
                                || !Enum.IsDefined(typeof(SectionSide), s.Side) || s.Property is null)
                                || a.Stations.GroupBy(s => new { s.Station, s.Side }).Any(g => g.Count() > 1)) result.Add(ModelDiagnostic.Error("InvalidTabulatedSections", beam));
                            else foreach (var station in a.Stations)
                                if (station.Property is ReinforcedConcreteSection stationConcrete) result.AddRange(ValidateRebars(stationConcrete, beam));
                        }
                        else if (a.Law != "Constant") result.Add(ModelDiagnostic.Error("UnsupportedSectionLaw", beam));
                    }
                    catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is InvalidOperationException)
                    { result.Add(ModelDiagnostic.Error("InvalidSectionLaw", beam, ex.Message)); }
                }
                foreach (var load in beam.Assignments.Loads)
                {
                    if (load == null) { result.Add(ModelDiagnostic.Error("MissingBeamLoad", beam)); continue; }
                    if (!Finite(load.Start) || !Finite(load.End) || load.Start < 0 || load.End > 1 || load.End < load.Start) result.Add(ModelDiagnostic.Error("InvalidBeamLoadInterval", beam));
                    if (load.Concentrated == null && (load.StartIntensity == null || load.EndIntensity == null)) result.Add(ModelDiagnostic.Error("IncompleteBeamLoad", beam));
                    if (load.Concentrated != null && (load.Start != load.End || load.StartIntensity != null || load.EndIntensity != null)) result.Add(ModelDiagnostic.Error("ConflictingBeamLoad", beam));
                    if (load.StartIntensity != null && load.EndIntensity != null && !Equals(load.StartIntensity.LoadCase, load.EndIntensity.LoadCase)) result.Add(ModelDiagnostic.Error("BeamLoadCaseMismatch", beam));
                    if (load.LengthConvention == BeamLoadLengthConvention.Unknown && load.Concentrated == null) result.Add(ModelDiagnostic.Error("MissingLoadLengthConvention", beam));
                    try { new BeamReferenceGeometry(beam).DomainLength(load.StationDomain ?? "NodeToNode"); }
                    catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) { result.Add(ModelDiagnostic.Error("InvalidBeamLoadDomain", beam, ex.Message)); }
                    if (load.LengthConvention == BeamLoadLengthConvention.ProjectedLength && (load.ProjectionPlaneNormal == null
                        || !Finite(Axes.Length(load.ProjectionPlaneNormal)) || Math.Abs(Axes.Length(load.ProjectionPlaneNormal) - 1) > 1e-8)) result.Add(ModelDiagnostic.Error("ProjectionPlaneRequired", beam));
                }
            }
            foreach (var shell in model.AreaElements.Values)
            {
                var a = shell.Assignments;
                if (a.PhysicalThickness.HasValue && (!Finite(a.PhysicalThickness.Value) || a.PhysicalThickness.Value <= 0)) result.Add(ModelDiagnostic.Error("InvalidPhysicalThickness", shell));
                foreach (var layer in a.Layers)
                    if (!Finite(layer.Diameter) || layer.Diameter <= 0 || !Finite(layer.Pitch) || layer.Pitch <= 0 || !Finite(layer.DirectionRadians) || !Finite(layer.AxisPositionThroughThickness)
                        || (a.PhysicalThickness.HasValue && Math.Abs(layer.AxisPositionThroughThickness) + layer.Diameter / 2 > a.PhysicalThickness.Value / 2))
                        result.Add(ModelDiagnostic.Error("InvalidShellRebarLayer", shell));
            }
            return result;
        }
        public static IReadOnlyList<ModelDiagnostic> ValidateRebars(ReinforcedConcreteSection section, Element owner = null)
        {
            var issues = new List<ModelDiagnostic>();
            foreach (var bar in section.Rebars)
            {
                if (!(bar.RebarSection is RebarSectionCircular circular)) { issues.Add(ModelDiagnostic.Error("UnsupportedRebarGeometry", owner)); continue; }
                double radius = circular.Diameter / 2;
                bool invalid = !section.ConcreteShape.IsPointInside(bar.Position);
                foreach (var polygon in new[] { section.ConcreteShape.Fill }.Concat(section.ConcreteShape.Holes ?? new Polygon3d[0]))
                {
                    var points = polygon.Points;
                    for (int i = 0; i < points.Length; i++) if (new Line2d(points[i], points[(i + 1) % points.Length]).SquareDistanceTo(bar.Position) < radius * radius - 1e-10) invalid = true;
                }
                if (invalid) issues.Add(ModelDiagnostic.Error("RebarOutsideSection", owner));
            }
            return issues;
        }
    }
}
