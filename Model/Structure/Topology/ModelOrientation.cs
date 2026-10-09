using GPC.Model.Results.Processing;
using System;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Loads;
using GPC.Model.Core;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.PostProcessing
{
    /// <summary>Atomic editing operations returning a separate Model. Source datasets retain their original input fingerprints:
    /// an edited model requires source revalidation/reimport before any result becomes current again.</summary>
    public static class ModelOrientation
    {
        private static Models.Model Copy(Models.Model source)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            return ModelValues.Copy(source);
        }
        private static CoordinateSystem Flip(CoordinateSystem axes, Point3d origin)
        { Axes.Validate(axes); return new CoordinateSystem(new Point3d(origin.X, origin.Y, origin.Z), axes.V1, axes.V2 * -1, axes.V3 * -1); }
        private static SectionSide Flip(SectionSide side) => side == SectionSide.Left ? SectionSide.Right : side == SectionSide.Right ? SectionSide.Left : side;

        public static Models.Model ReverseBeam(Models.Model source, int beamId)
        {
            var model = Copy(source); var beam = model.BeamElements[beamId]; var a = beam.Assignments;
            if (beam.NodeI == null || beam.NodeJ == null || a.Formulation != BeamFormulation.StraightTwoNode || a.OtherAssignments.Count != 0
                || beam.Attributes.Values.Any(v => !(v is BeamReleasesAttribute))) throw new NotSupportedException("UnresolvedBeamAssignmentsCannotBeReversed");
            if (beam.Loads.Values.Any(l => !(l is PointLoad) && !(l is LineLoad))) throw new NotSupportedException("UnsupportedBeamLoadReversal");
            var geometry = new BeamReferenceGeometry(beam); var oldAxes = a.SectionAxes; Axes.Validate(oldAxes);
            var geometryAxes = a.SectionGeometryAxes ?? ActionTransformations.AtPoint(oldAxes, oldAxes.Origin);
            var target = Flip(oldAxes, geometry.End);
            // Re-express positive cut actions and reverse station/side. The source record/Original data are retained.
            foreach (var set in beam.Results)
                for (int i = 0; i < set.Results.Count; i++)
                {
                    if (!(set.Results[i] is StationResultBeamForces sample)) throw new NotSupportedException("UnsupportedBeamResultReversal");
                    geometry.ValidateSample(sample);
                    var canonical = ResultOrientation.Beam(sample, ActionTransformations.AtPoint(oldAxes, sample.ResultBeamForces.CoordinateSystem.Origin));
                    set.Results[i] = ResultOrientation.Beam(canonical, ActionTransformations.AtPoint(target, sample.ResultBeamForces.CoordinateSystem.Origin), true, geometry.DomainLength(sample.StationDomain));
                }
            var oldI = beam.NodeI.Id; model.ConnectBeam(beamId, beam.NodeJ.Id, oldI);
            beam.CoordinateSystem = target; a.SectionAxes = target;
            a.SectionGeometryAxes = Axes.Dot(geometryAxes.V1, target.V1) > 1 - 1e-10 && Axes.Dot(geometryAxes.V2, target.V2) > 1 - 1e-10 ? null : geometryAxes;
            beam.RotationAroundFirstAxis = -beam.RotationAroundFirstAxis;
            var offset = a.OffsetI; a.OffsetI = a.OffsetJ; a.OffsetJ = offset;
            double rigid = a.RigidLengthI; a.RigidLengthI = a.RigidLengthJ; a.RigidLengthJ = rigid;
            if (a.SectionCentroidOffset != null) a.SectionCentroidOffset = new Vector2d(a.SectionCentroidOffset.X, -a.SectionCentroidOffset.Y);
            var sections = a.Sections.ToArray(); a.Sections.Clear();
            foreach (var section in sections.Reverse())
            {
                if (section.Law != "Constant" && section.Law != "LinearRectangular" && section.Law != "Tabulated") throw new NotSupportedException("UnsupportedSectionLawReversal");
                var reversed = new BeamSectionAssignment { Start = 1 - section.End, End = 1 - section.Start, Law = section.Law,
                    Property = section.Law == "LinearRectangular" ? section.EndProperty : section.Property,
                    EndProperty = section.Law == "LinearRectangular" ? section.Property : section.EndProperty };
                foreach (var s in section.Stations.OrderByDescending(s => s.Station)) reversed.Stations.Add(new BeamSectionStation { Station = 1 - s.Station, Side = Flip(s.Side), Property = s.Property });
                a.Sections.Add(reversed);
            }
            foreach (var load in a.Loads)
            {
                if (model.BeamElements.Values.Where(b => b.Id != beamId).Any(b => b.Assignments.Loads.Any(l => ReferenceEquals(l, load)))) throw new NotSupportedException("SharedBeamLoadAssignment");
                double start = load.Start; load.Start = 1 - load.End; load.End = 1 - start;
                var intensity = load.StartIntensity; load.StartIntensity = load.EndIntensity; load.EndIntensity = intensity;
                // Load axes, physical application points and eccentricity vectors remain in their original explicit frames.
            }
            foreach (var release in beam.Attributes.Values.OfType<BeamReleasesAttribute>())
                for (int i = 0; i < 6; i++) { var connection = release.I[i]; release.I[i] = release.J[i]; release.J[i] = connection; }
            if (a.AnalysisProfile != null)
            {
                var p = a.AnalysisProfile; p.Validate();
                var reversed = new BeamAnalysisProfile { Interpolation = p.Interpolation, StationDomain = p.StationDomain, Modifiers = p.Modifiers, SourceRecord = p.SourceRecord };
                for (int i = p.Stations.Count - 1; i >= 0; i--)
                {
                    int values = p.Interpolation == ProfileInterpolation.Constant ? Math.Max(0, i - 1) : i;
                    reversed.Stations.Add(BeamAnalysisStation.From(1 - p.Stations[i].Station, p.Stations[values].Values));
                }
                a.AnalysisProfile = reversed;
            }
            foreach (var sample in model.NodesElements.Values.SelectMany(n => n.Results).SelectMany(r => r.Results).OfType<NodeResultForces>()
                .Where(r => r.OwnerElementFamily == EntityFamily.Beam && r.OwnerElementId == beamId))
            { if (sample.ElementEnd != "I" && sample.ElementEnd != "J") throw new ArgumentException("UnresolvedBeamEnd"); sample.ElementEnd = sample.ElementEnd == "I" ? "J" : "I"; }
            Validate(model); return model;
        }

        public static Models.Model ReverseShell(Models.Model source, int shellId)
        {
            var model = Copy(source); var shell = model.AreaElements[shellId]; var nodes = shell.Nodes.ToArray();
            if (nodes.Length != 3 && nodes.Length != 4 || shell.Attributes.Count != 0) throw new NotSupportedException("UnsupportedShellReversal");
            if (shell.Loads.Values.Any(l => !(l is PointLoad) && !(l is AreaLoad) && !(l is NormalAreaLoad) && !(l is LineLoad) && !(l is NonUniformPlatePressure)))
                throw new NotSupportedException("UnsupportedShellLoadReversal");
            var target = Flip(shell.CoordinateSystem, shell.CoordinateSystem.Origin);
            foreach (var set in shell.Results)
                for (int i = 0; i < set.Results.Count; i++)
                {
                    if (!(set.Results[i] is PointResultPlateForces p)) throw new NotSupportedException("UnsupportedShellResultReversal");
                    // First align the source to the authoritative old normal, then reverse the connectivity coordinates.
                    var aligned = ResultOrientation.Shell(p, ActionTransformations.AtPoint(shell.CoordinateSystem, p.Forces.CoordinateSystem.Origin));
                    set.Results[i] = ResultOrientation.Shell(aligned, ActionTransformations.AtPoint(target, p.Forces.CoordinateSystem.Origin), true, nodes.Length);
                }
            model.ConnectShell(shellId, new[] { nodes[0].Id }.Concat(nodes.Skip(1).Reverse().Select(n => n.Id)).ToArray());
            shell.CoordinateSystem = target;
            var a = shell.Assignments;
            if (a.LayerAxes != null)
            {
                var changed = ResultOrientation.ShellLayers(a, Flip(a.LayerAxes, a.LayerAxes.Origin));
                a.LayerAxes = changed.LayerAxes; a.Layers.Clear(); a.Layers.AddRange(changed.Layers);
            }
            else if (a.Layers.Count != 0) throw new ArgumentException("MissingLayerAxes");
            a.Offset = -a.Offset;
            // Area/normal/line loads and non-uniform pressures carry their own global geometry and frame (the vertex values stay on their points);
            // retaining these preserves their physical direction.
            Validate(model); return model;
        }
        private static void Validate(Models.Model model)
        {
            var errors = model.ValidateTopology().Concat(model.ValidateAssignments()).Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            if (errors.Length != 0) throw new ArgumentException("InvalidReorientedModel: " + string.Join(";", errors.Select(d => d.Code)));
        }
    }
}
