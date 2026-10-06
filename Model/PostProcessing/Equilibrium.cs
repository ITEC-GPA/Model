using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Loads;
using GPC.Model.Persistence;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.PostProcessing
{
    /// <summary>A contribution to an explicit free body, in global N and Nmm. One physical action may have several parts
    /// (e.g. equivalent nodal loads), but only one representation may appear in a balance.</summary>
    public sealed class EquilibriumContribution
    {
        public string PhysicalActionId { get; set; }
        public string Representation { get; set; }
        public string Part { get; set; } = "whole";
        public int ExpectedParts { get; set; } = 1;
        public string Case { get; internal set; }
        public ResultState State { get; internal set; }
        public Point3d Point { get; internal set; }
        public Vector3d Force { get; internal set; }
        public Vector3d Moment { get; internal set; }
        public string SourceRecord { get; internal set; }
        internal Func<bool> Current { get; set; }
        internal string ValueFingerprint { get; set; }
        internal string Fingerprint() => ModelArchive.Fingerprint(new object[] { Case, State, Point, Force, Moment, SourceRecord });
        public bool IsCurrent => ValueFingerprint == Fingerprint() && (Current == null || Current());
    }
    public sealed class EquilibriumScope
    {
        public Point3d ReferencePoint { get; set; } = Point3d.Origin;
        public ResultSelection Selection { get; set; }
        public List<string> ExpectedPhysicalActions { get; private set; } = new List<string>();
        /// <summary>Explicit source statement that this manifest covers the entire selected free body and state.</summary>
        public string CoverageEvidence { get; set; }
        public bool CompleteCoverageConfirmed { get; set; }
        public double ForceTolerance { get; set; } = 1e-6;
        public double MomentTolerance { get; set; } = 1e-3;
        public double RelativeTolerance { get; set; } = 1e-8;
    }
    public sealed class EquilibriumReport
    {
        public DataStatus Status { get; internal set; } = DataStatus.Insufficient;
        public bool? IsBalanced { get; internal set; }
        public Vector3d ForceResidual { get; internal set; }
        public Vector3d MomentResidual { get; internal set; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; internal set; }
        public string Coverage { get; internal set; }
    }

    public static class Equilibrium
    {
        private static Vector3d Global(CoordinateSystem axes, double x, double y, double z)
        {
            Axes.Validate(axes); NumericGuard.Finite(x, "component"); NumericGuard.Finite(y, "component"); NumericGuard.Finite(z, "component");
            return axes.V1 * x + axes.V2 * y + axes.V3 * z;
        }
        /// <summary>Explicit imported resultant, for body forces/links or unsupported load laws resolved by the source software.</summary>
        public static EquilibriumContribution Explicit(string physicalAction, string representation, string caseName, Point3d point,
            Vector3d globalForce, Vector3d globalMoment, string sourceRecord)
        {
            if (!Axes.IsFinite(point) || globalForce == null || globalMoment == null || string.IsNullOrWhiteSpace(sourceRecord)) throw new ArgumentException("IncompleteEquilibriumContribution");
            var result = new EquilibriumContribution { PhysicalActionId = physicalAction, Representation = representation, Case = caseName,
                Point = new Point3d(point.X, point.Y, point.Z), Force = Global(CoordinateSystem.Global, globalForce.X, globalForce.Y, globalForce.Z),
                Moment = Global(CoordinateSystem.Global, globalMoment.X, globalMoment.Y, globalMoment.Z), SourceRecord = sourceRecord };
            result.ValueFingerprint = result.Fingerprint(); return result;
        }
        public static EquilibriumContribution PointLoad(PointLoad load, string physicalAction, string representation = "applied")
        {
            if (load == null) throw new ArgumentNullException(nameof(load));
            var c = Explicit(physicalAction, representation, load.LoadCase?.Name, load.Point,
                Global(load.CoordinateSystem, load.F1, load.F2, load.F3), Global(load.CoordinateSystem, load.M1, load.M2, load.M3), "PointLoad:" + load.Guid);
            string hash = ModelArchive.Fingerprint(new object[] { load }); c.Current = () => hash == ModelArchive.Fingerprint(new object[] { load }); return c;
        }
        /// <summary>Uses reported nodal forces; never computes spring/link reactions from incomplete displacements.</summary>
        public static EquilibriumContribution NodalAction(Models.Model model, int nodeId, NodeResultForces sample, ActionBody bodyOnFreeBody,
            string physicalAction, string representation)
        {
            ResultAlgebra.ValidateRegistered(model, new ElementKey { Family = EntityFamily.Node, Id = nodeId }, sample);
            if (sample.Kind == NodalForceKind.SupportReaction && bodyOnFreeBody == ActionBody.OnElement
                || (sample.Kind == NodalForceKind.ElementEndForce || sample.Kind == NodalForceKind.ElementNodeForce) && bodyOnFreeBody == ActionBody.OnSupport)
                throw new ArgumentException("IncompatibleActionBody");
            double sign;
            if (bodyOnFreeBody == sample.Body) sign = 1;
            else if (bodyOnFreeBody == ActionBody.OnNode && (sample.Body == ActionBody.OnSupport || sample.Body == ActionBody.OnElement)
                || sample.Body == ActionBody.OnNode && (bodyOnFreeBody == ActionBody.OnSupport || bodyOnFreeBody == ActionBody.OnElement)) sign = -1;
            else throw new ArgumentException("UnresolvedActionReactionPair");
            var axes = sample.ResultBeamForces.CoordinateSystem;
            var c = Explicit(physicalAction, representation, sample.Case.Name, axes.Origin,
                Global(axes, sample.Fx, sample.Fy, sample.Fz) * sign, Global(axes, sample.Mx, sample.My, sample.Mz) * sign, sample.State.SourceRecord ?? sample.State.ConcomitantStateId);
            c.State = sample.State.Copy(); string input = model.AnalysisFingerprint(); string hash = ModelArchive.Fingerprint(new object[] { sample, model.Datasets[sample.State.DatasetId] });
            c.Current = () => input == model.AnalysisFingerprint() && model.NodesElements.ContainsKey(nodeId)
                && model.NodesElements[nodeId].Results.SelectMany(r => r.Results).Any(r => ReferenceEquals(r, sample))
                && sample.State?.DatasetId != null && model.Datasets.TryGetValue(sample.State.DatasetId, out var ds) && hash == ModelArchive.Fingerprint(new object[] { sample, ds });
            c.ValueFingerprint = c.Fingerprint(); return c;
        }
        /// <summary>Exact resultant of a point or linearly varying beam load, including eccentricity and distributed couples.</summary>
        public static EquilibriumContribution BeamLoad(BeamElement beam, BeamLoadAssignment load)
        {
            if (beam == null || load == null || string.IsNullOrWhiteSpace(load.OriginalAssignmentId)) throw new ArgumentException("BeamLoadIdentityRequired");
            var geometry = new BeamReferenceGeometry(beam); string domain = load.StationDomain ?? "NodeToNode";
            NumericGuard.Station(load.Start); NumericGuard.Station(load.End);
            if (load.End < load.Start) throw new ArgumentException("InvalidBeamLoadInterval");
            Point3d start = geometry.PointAt(load.Start, domain), end = geometry.PointAt(load.End, domain);
            var eccentricity = load.Eccentricity == null ? new Vector3d(0, 0, 0) : Global(load.EccentricityCoordinateSystem, load.Eccentricity.X, load.Eccentricity.Y, load.Eccentricity.Z);
            string representation = load.IsEquivalentNodalRepresentation ? "equivalent" : "distributed";
            EquilibriumContribution result;
            if (load.Concentrated != null)
            {
                if (load.Start != load.End || load.StartIntensity != null || load.EndIntensity != null || Axes.Length(load.Concentrated.Point - start) > 1e-7)
                    throw new ArgumentException("InconsistentConcentratedBeamLoad");
                result = PointLoad(load.Concentrated, load.OriginalAssignmentId, representation); result.Point = start + eccentricity;
            }
            else
            {
                var p = load.StartIntensity; var q = load.EndIntensity;
                if (p == null || q == null || p.LoadCase == null || !Equals(p.LoadCase, q.LoadCase) || load.Start == load.End) throw new ArgumentException("IncompleteBeamLoad");
                Vector3d delta = end - start; double length = Axes.Length(delta);
                if (load.LengthConvention == BeamLoadLengthConvention.ProjectedLength)
                {
                    var normal = load.ProjectionPlaneNormal;
                    if (normal == null || !Axes.IsFinite(new Point3d(normal.X, normal.Y, normal.Z)) || Math.Abs(Axes.Length(normal) - 1) > 1e-8) throw new ArgumentException("ProjectionPlaneRequired");
                    double cosine = Axes.Dot(delta / length, normal); length *= Math.Sqrt(Math.Max(0, 1 - cosine * cosine));
                }
                else if (load.LengthConvention != BeamLoadLengthConvention.ActualLength) throw new ArgumentException("MissingLoadLengthConvention");
                var f0 = Global(p.CoordinateSystem, p.F1, p.F2, p.F3); var f1 = Global(q.CoordinateSystem, q.F1, q.F2, q.F3);
                var m0 = Global(p.CoordinateSystem, p.M1, p.M2, p.M3); var m1 = Global(q.CoordinateSystem, q.M1, q.M2, q.M3);
                var force = (f0 + f1) * (length / 2);
                var moment = (m0 + m1) * (length / 2) + eccentricity.CrossProduct(force) + delta.CrossProduct((f0 / 6 + f1 / 3) * length);
                result = Explicit(load.OriginalAssignmentId, representation, p.LoadCase.Name, start, force, moment, load.SourceRecord ?? "BeamLoad:" + beam.Id);
            }
            string hash = ModelArchive.Fingerprint(new object[] { beam.StartPoint, beam.EndPoint, beam.Assignments.OffsetI, beam.Assignments.OffsetJ, beam.Assignments.OffsetAxes,
                beam.Assignments.RigidLengthI, beam.Assignments.RigidLengthJ, load });
            result.Current = () => hash == ModelArchive.Fingerprint(new object[] { beam.StartPoint, beam.EndPoint, beam.Assignments.OffsetI, beam.Assignments.OffsetJ, beam.Assignments.OffsetAxes,
                beam.Assignments.RigidLengthI, beam.Assignments.RigidLengthJ, load });
            result.ValueFingerprint = result.Fingerprint(); return result;
        }
        /// <summary>Uniform global traction on the full planar triangular/quadrilateral connectivity, in N/mm².</summary>
        public static EquilibriumContribution ShellPressure(AreaElement shell, Vector3d globalTraction, string caseName, string physicalAction, string sourceRecord)
        {
            var points = shell?.Points;
            if (points == null || points.Length < 3 || points.Length > 4 || points.Any(p => !Axes.IsFinite(p)) || globalTraction == null) throw new ArgumentException("UnsupportedPressureGeometry");
            Vector3d ab = points[1] - points[0], ac = points[2] - points[0]; var normal = ab.CrossProduct(ac); double n = Axes.Length(normal);
            if (n <= 1e-9) throw new ArgumentException("DegeneratePressureGeometry");
            normal /= n; var force = new Vector3d(0, 0, 0); var moment = new Vector3d(0, 0, 0);
            for (int i = 0; i < points.Length; i++)
            {
                Vector3d edge = points[(i + 1) % points.Length] - points[i], next = points[(i + 2) % points.Length] - points[(i + 1) % points.Length];
                if (Axes.Dot(edge.CrossProduct(next), normal) <= 0) throw new ArgumentException("NonConvexPressureGeometry");
            }
            for (int i = 1; i < points.Length - 1; i++)
            {
                Vector3d v = points[i] - points[0], w = points[i + 1] - points[0]; double area = Axes.Dot(v.CrossProduct(w), normal) / 2;
                if (area <= 0 || Math.Abs(Axes.Dot(w, normal)) > 1e-7) throw new ArgumentException("InvalidPressureWindingOrPlane");
                var f = globalTraction * area; force += f; moment += ((v + w) / 3).CrossProduct(f);
            }
            var c = Explicit(physicalAction, "area", caseName, points[0], force, moment, sourceRecord);
            string hash = ModelArchive.Fingerprint(new object[] { shell.Points, globalTraction }); c.Current = () => hash == ModelArchive.Fingerprint(new object[] { shell.Points, globalTraction }); return c;
        }
        /// <summary>Exact resultant of a pressure varying linearly/bilinearly over its own vertices, in global N and Nmm.</summary>
        public static EquilibriumContribution PlatePressure(NonUniformPlatePressure load, string physicalAction, string representation = "applied")
        {
            if (load == null) throw new ArgumentNullException(nameof(load));
            var origin = new Point3d(0, 0, 0); var (force, moment) = load.GetGlobalResultant(origin);
            var c = Explicit(physicalAction, representation, load.LoadCase?.Name, origin, force, moment, "NonUniformPlatePressure:" + load.Guid);
            string hash = ModelArchive.Fingerprint(new object[] { load }); c.Current = () => hash == ModelArchive.Fingerprint(new object[] { load });
            c.ValueFingerprint = c.Fingerprint(); return c;
        }

        /// <summary>Weight of the beams and plates under a model gravity load, in global N and Nmm about the origin: beams A ρ g along the
        /// node-to-node line (steel section area; gross concrete area, without rebars), plates t ρ g over the planar element area with the
        /// physical thickness or else the membrane thickness, on the mid-surface moved by the shell offset along V3. Elements without a steel/concrete beam property or a FEM plate property are
        /// added to <paramref name="withoutMass"/> when supplied, otherwise they reject the evaluation.</summary>
        public static EquilibriumContribution Gravity(Models.Model model, ModelGravityLoad load, string physicalAction, ICollection<Element> withoutMass = null)
        {
            if (model == null || load == null) throw new ArgumentNullException();
            var g = load.GravityVector ?? throw new ArgumentException("MissingGravityVector");
            Global(CoordinateSystem.Global, g.X, g.Y, g.Z);
            var force = new Vector3d(0, 0, 0); var moment = new Vector3d(0, 0, 0);
            void Add(double mass, Point3d centroid)
            {
                var f = g * NumericGuard.Finite(mass, "mass"); force += f; moment += ((Vector3d)(centroid - Point3d.Origin)).CrossProduct(f);
            }
            void Missing(Element element)
            {
                if (withoutMass == null) throw new ArgumentException("MissingMassProperty: element " + element.Id);
                withoutMass.Add(element);
            }
            foreach (var beam in model.BeamElements.Values)
            {
                double area, density;
                if (beam.BeamProperty is Sections.Steel.SteelSection steel && steel.SteelMaterial != null) { area = steel.Area; density = steel.SteelMaterial.Density; }
                else if (beam.BeamProperty is Sections.Concrete.ReinforcedConcreteSection rc && rc.ConcreteMaterial != null) { area = rc.SectionShape.Area; density = rc.ConcreteMaterial.Density; }
                else { Missing(beam); continue; }
                Add(area * beam.Length * density, beam.StartPoint + (beam.EndPoint - beam.StartPoint) * 0.5);
            }
            foreach (var shell in model.AreaElements.Values)
            {
                var points = shell.Points;
                if (!(shell.PlateProperty is ElementProperties.IFemPlateProperty plate) || plate.Material == null || points.Length < 3 || points.Length > 4) { Missing(shell); continue; }
                double thickness = shell.Assignments.PhysicalThickness ?? plate.MembraneThickness, area = 0; var first = new Vector3d(0, 0, 0);
                for (int i = 1; i < points.Length - 1; i++)
                {
                    Vector3d v = points[i] - points[0], w = points[i + 1] - points[0]; double a = Axes.Length(v.CrossProduct(w)) / 2;
                    area += a; first += ((Vector3d)(points[0] - Point3d.Origin) + (v + w) / 3) * a;
                }
                if (!(area > 0)) throw new ArgumentException("DegenerateShell: " + shell.Id);
                var c = first / area;
                // The mass lies on the mid-surface, offset from the node plane along V3.
                if (shell.Assignments.Offset.HasValue) { Axes.Validate(shell.CoordinateSystem); c += shell.CoordinateSystem.V3 * NumericGuard.Finite(shell.Assignments.Offset.Value, "offset"); }
                Add(area * thickness * plate.Material.Density, new Point3d(c.X, c.Y, c.Z));
            }
            if (model.VolumeElements.Count != 0) foreach (var volume in model.VolumeElements.Values) Missing(volume);
            var contribution = Explicit(physicalAction, "gravity", load.LoadCase?.Name, new Point3d(0, 0, 0), force, moment, "ModelGravityLoad:" + load.Guid);
            string input = model.AnalysisFingerprint(); contribution.Current = () => input == model.AnalysisFingerprint();
            contribution.ValueFingerprint = contribution.Fingerprint(); return contribution;
        }

        public static EquilibriumReport Check(EquilibriumScope scope, IEnumerable<EquilibriumContribution> contributions)
        {
            var report = new EquilibriumReport(); var issues = new List<ModelDiagnostic>(); report.Diagnostics = issues;
            try
            {
                if (scope?.Selection == null || !Axes.IsFinite(scope.ReferencePoint) || contributions == null) throw new ArgumentException("IncompleteEquilibriumScope");
                if (string.IsNullOrWhiteSpace(scope.Selection.Case) || string.IsNullOrWhiteSpace(scope.Selection.ConcomitantState)) throw new ArgumentException("ExplicitEquilibriumStateRequired");
                foreach (var v in new[] { scope.ForceTolerance, scope.MomentTolerance, scope.RelativeTolerance })
                    if (NumericGuard.Finite(v, "tolerance") < 0) throw new ArgumentException("NegativeTolerance");
                var terms = contributions.ToArray();
                if (terms.Any(c => c == null || string.IsNullOrWhiteSpace(c.PhysicalActionId) || string.IsNullOrWhiteSpace(c.Representation) || string.IsNullOrWhiteSpace(c.Part)))
                    throw new ArgumentException("MissingPhysicalActionIdentity");
                var expected = scope.ExpectedPhysicalActions;
                if (!scope.CompleteCoverageConfirmed || string.IsNullOrWhiteSpace(scope.CoverageEvidence) || expected.Count == 0
                    || expected.Any(string.IsNullOrWhiteSpace) || expected.Distinct().Count() != expected.Count
                    || !new HashSet<string>(expected).SetEquals(terms.Select(c => c.PhysicalActionId))) throw new ArgumentException("IncompleteEquilibriumCoverage");
                foreach (var group in terms.GroupBy(c => c.PhysicalActionId))
                    if (group.Select(c => c.Representation).Distinct().Count() != 1 || group.Select(c => c.Part).Distinct().Count() != group.Count()
                        || group.Any(c => c.ExpectedParts != group.Count())) throw new ArgumentException("DuplicateOrIncompletePhysicalAction");
                var force = new Vector3d(0, 0, 0); var moment = new Vector3d(0, 0, 0); double forceScale = 0, momentScale = 0;
                foreach (var c in terms)
                {
                    if (!c.IsCurrent) { report.Status = DataStatus.Stale; throw new ArgumentException("StaleEquilibriumContribution"); }
                    if (c.Case != scope.Selection.Case || c.State != null && (c.State.DatasetId != scope.Selection.Dataset || c.State.Phase != scope.Selection.Phase
                        || c.State.Step != scope.Selection.Step || c.State.Mode != scope.Selection.Mode || c.State.MovingLoadPosition != scope.Selection.MovingLoadPosition
                        || c.State.ConcomitantStateId != scope.Selection.ConcomitantState)) throw new ArgumentException("IncompatibleEquilibriumState");
                    if (!Axes.IsFinite(c.Point)) throw new ArgumentException("InvalidContributionPoint");
                    var f = Global(CoordinateSystem.Global, c.Force.X, c.Force.Y, c.Force.Z);
                    Vector3d r = c.Point - scope.ReferencePoint;
                    var m = Global(CoordinateSystem.Global, c.Moment.X, c.Moment.Y, c.Moment.Z) + r.CrossProduct(f);
                    force += f; moment += m; forceScale += Axes.Length(f); momentScale += Axes.Length(m);
                }
                NumericGuard.Finite(Axes.Length(force), "force residual"); NumericGuard.Finite(Axes.Length(moment), "moment residual");
                NumericGuard.Finite(forceScale, "force scale"); NumericGuard.Finite(momentScale, "moment scale");
                report.ForceResidual = force; report.MomentResidual = moment; report.Status = DataStatus.Ready;
                report.IsBalanced = Axes.Length(force) <= scope.ForceTolerance + scope.RelativeTolerance * forceScale
                    && Axes.Length(moment) <= scope.MomentTolerance + scope.RelativeTolerance * momentScale;
                report.Coverage = scope.CoverageEvidence;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            { issues.Add(ModelDiagnostic.Error("EquilibriumNotEvaluated", message: ex.Message)); }
            return report;
        }
    }
}
