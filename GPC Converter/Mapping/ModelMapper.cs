using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Combinations;
using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Restraints;
using GPC.Model.Models;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Analysis;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Loads.Assignments;
using GPC.Model.Structure.Assignments;
using GPC.Model.Structure.Topology;

namespace GPC.Converter
{
    /// <summary>Creates an isolated candidate; a failed mapping never modifies an active model.</summary>
    public static class ModelMapper
    {
        public static ImportReport Map(ImportBatch batch, CancellationToken cancellationToken = default(CancellationToken), IProgress<int> progress = null,
            MappingOptions options = null)
        {
            var report = new ImportReport { Status = ImportStatus.Rejected, SourceHash = batch.SourceHash };
            report.Preserved.AddRange(batch.Uninterpreted);
            report.Diagnostics.AddRange(batch.Diagnostics);
            var model = new GPC.Model.Models.Model(batch.ModelRevision); var nodes = new Dictionary<string, NodeElement>(StringComparer.Ordinal);
            var bySource = new Dictionary<SourceIdentity, Element>();
            var beams = new Dictionary<string, BeamElement>(StringComparer.Ordinal); var shells = new Dictionary<string, AreaElement>(StringComparer.Ordinal);
            var added = new HashSet<ElementProperty>(SameInstance.Comparer);
            var declaredShellAxes = new HashSet<string>(batch.Shells.Where(s => s.CoordinateSystem != null).Select(s => s.Id), StringComparer.Ordinal);
            model.AnalysisSource = new AnalysisSource { Program = batch.Program, SolverVersion = batch.SolverVersion,
                ModelRevision = batch.ModelRevision, AnalysisId = batch.AnalysisId, GeometryHash = batch.SourceHash };
            model.PreservedSourceData.AddRange(batch.Uninterpreted);
            string record = null;
            try
            {
                var properties = new PropertyMapper(batch, options, report.Diagnostics);
                foreach (var n in batch.Nodes)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = n.Record;
                    if (!Axes.IsFinite(n.GlobalPosition)) throw new ArgumentException("NonFiniteCoordinate");
                    var nodeAxes = ActionTransformations.AtPoint(n.CoordinateSystem ?? GPC.Geometry.CoordinateSystem.Global, n.GlobalPosition);
                    if (nodes.TryGetValue(n.Id, out var existing))
                    {
                        if (existing.Position.X != n.GlobalPosition.X || existing.Position.Y != n.GlobalPosition.Y || existing.Position.Z != n.GlobalPosition.Z
                            || !ActionTransformations.AtPoint(existing.CoordinateSystem, n.GlobalPosition).Equals(nodeAxes))
                            throw new ArgumentException("SourceCollision");
                        report.Diagnostics.Add(new ModelDiagnostic { Code = "IdenticalDuplicate", Severity = DiagnosticSeverity.Information, Record = record, Message = n.Id }); continue;
                    }
                    var node = new NodeElement(n.GlobalPosition, n.CoordinateSystem == null ? null : nodeAxes) { Source = new SourceIdentity(batch.Program, batch.ModelRevision, EntityFamily.Node, n.Id) };
                    model.NodesElements.Add(node); nodes.Add(n.Id, node); bySource.Add(node.Source, node); progress?.Report(model.NodesElements.Count);
                }
                foreach (var b in batch.Beams)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = b.Record;
                    ISectionShape shape = null;
                    var property = b.Property ?? properties.Beam(b, out shape);
                    var beam = new BeamElement(null, null, property, b.CoordinateSystem) { Source = new SourceIdentity(batch.Program, batch.ModelRevision, EntityFamily.Beam, b.Id) };
                    if (bySource.ContainsKey(beam.Source)) throw new ArgumentException("SourceCollision");
                    bySource.Add(beam.Source, beam);
                    beam.Assignments.Formulation = b.Formulation == BeamFormulation.Unknown ? BeamFormulation.StraightTwoNode : b.Formulation;
                    beam.Assignments.SectionAxes = b.CoordinateSystem;
                    beam.Assignments.OtherAssignments.AddRange(b.OtherAssignments);
                    model.BeamElements.Add(beam); model.ConnectBeam(beam.Id, nodes[b.I].Id, nodes[b.J].Id); beams.Add(b.Id, beam);
                    if (b.OffsetI != null || b.OffsetJ != null)
                    {
                        Axes.Validate(b.OffsetAxes);
                        beam.Assignments.OffsetI = b.OffsetI ?? new Vector3d(0, 0, 0); beam.Assignments.OffsetJ = b.OffsetJ ?? new Vector3d(0, 0, 0);
                        beam.Assignments.OffsetAxes = b.OffsetAxes;
                    }
                    beam.Assignments.RigidLengthI = b.RigidLengthI; beam.Assignments.RigidLengthJ = b.RigidLengthJ;
                    if (b.RigidLengthI != 0 || b.RigidLengthJ != 0 || b.OffsetI != null || b.OffsetJ != null) new BeamReferenceGeometry(beam);
                    if (b.Property is null && (b.SectionId != null || b.CentroidOffset != null)) beam.Assignments.SectionCentroidOffset = properties.CentroidOffset(b, shape);
                    if (property is ReinforcedConcreteSection concrete)
                        beam.Assignments.Sections.Add(new BeamSectionAssignment { Start = 0, End = 1, Section = concrete, Law = "Constant" });
                    AddProperty(model, property, added, b.Property is null);
                }
                foreach (var a in batch.Shells)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = a.Record;
                    ThicknessRecord thickness = null;
                    var property = a.Property ?? properties.Plate(a, out thickness);
                    var shell = new AreaElement(null, property, a.CoordinateSystem) { Source = new SourceIdentity(batch.Program, batch.ModelRevision, EntityFamily.Shell, a.Id) };
                    if (bySource.ContainsKey(shell.Source)) throw new ArgumentException("SourceCollision");
                    bySource.Add(shell.Source, shell);
                    model.AreaElements.Add(shell); model.ConnectShell(shell.Id, a.Nodes.Select(id => nodes[id].Id).ToArray()); shells.Add(a.Id, shell);
                    if (a.Property is null && thickness != null && thickness.OutOfPlane == null) shell.Assignments.PhysicalThickness = thickness.InPlane;
                    var offset = a.Offset ?? (a.Property is null ? thickness?.Offset : null);
                    if (offset.HasValue) shell.Assignments.Offset = Finite(offset.Value, "shell offset");
                    AddProperty(model, property, added, a.Property is null);
                }
                foreach (var g in batch.Groups)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = g.Record;
                    if (model.Groups.ContainsKey(g.Name)) throw new ArgumentException("DuplicateGroup: " + g.Name);
                    model.AddGroup(g.Name);
                    model.AssignGroup(g.Name, g.Members.Select(source => bySource.TryGetValue(source, out var element)
                        ? element : throw new ArgumentException("MissingGroupMember: " + source.OriginalId)));
                }
                foreach (var g in batch.Groups.Where(g => g.ParentName != null))
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = g.Record;
                    model.ReparentGroup(g.Name, g.ParentName);
                }
                var cases = new Dictionary<string, LoadCaseBase>(StringComparer.Ordinal);
                var selfWeight = new HashSet<string>(batch.Gravity.Select(g => g.Case), StringComparer.Ordinal);
                foreach (var c in batch.LoadCases)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = c.Record;
                    // ModelGravityLoad requires a self weight LoadCase; the other cases keep the untyped base.
                    var loadCase = selfWeight.Contains(c.Name) ? new LoadCase(c.Name, LoadCase.LoadCaseTypes.SelfWeight) : new LoadCaseBase(c.Name);
                    cases.Add(c.Name, loadCase); model.LoadCases.Add(loadCase);
                }
                if (batch.Combinations.Count != 0)
                {
                    record = "combinations";
                    var definitions = batch.Combinations.ToArray(); int linear = 0, envelopes = 0;
                    foreach (var definition in definitions)
                    {
                        cancellationToken.ThrowIfCancellationRequested(); record = definition.Record;
                        var name = CombinationExpansion.ModelName(definitions, definition, cases.ContainsKey);
                        if (model.Combinations.ContainsKey(name)) continue;
                        var combination = CombinationExpansion.ToCombination(definitions, definition, cases, name);
                        if (combination != null) { if (model.AddCombination(combination)) linear++; }
                        // An envelope is declared without coefficients: it names the concomitant states rebuilt from the static
                        // results (CombinationResults); declaring it now keeps the analysis fingerprint of the imported results.
                        else if (CombinationExpansion.IsExpandable(definitions, definition.Id) && model.AddCombination(new Combination(name))) envelopes++;
                    }
                    var preserved = CombinationExpansion.Preserve(definitions, batch.Program);
                    model.PreservedSourceData.Add(preserved); report.Preserved.Add(preserved);
                    report.Diagnostics.Add(new ModelDiagnostic { Code = "CombinationsImported", Severity = DiagnosticSeverity.Information, Record = "combinations",
                        Message = linear.ToString(System.Globalization.CultureInfo.InvariantCulture) + " of " + definitions.Length.ToString(System.Globalization.CultureInfo.InvariantCulture)
                            + " source combinations are linear in the static cases and are Model combinations with their factors; " + envelopes.ToString(System.Globalization.CultureInfo.InvariantCulture)
                            + " envelopes are Model combinations without factors, for their rebuilt concomitant states; all definitions are preserved (CombinationExpansion)." });
                }
                foreach (var g in batch.Gravity)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = g.Record;
                    var f = g.Factors ?? throw new ArgumentException("MissingGravityFactors");
                    double factor = Math.Sqrt(Finite(f.X, "gravity") * f.X + Finite(f.Y, "gravity") * f.Y + Finite(f.Z, "gravity") * f.Z);
                    if (!(factor > 0)) throw new ArgumentException("NullGravity");
                    model.ModelLoads.Add(new ModelGravityLoad(new Vector3d(f.X / factor, f.Y / factor, f.Z / factor), factor * ModelGravityLoad.GRAVITYACCELERATION,
                        cases[g.Case], CoordinateSystem.Global, g.Record));
                }
                foreach (var l in batch.NodeLoads)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = l.Record;
                    var v = l.Components;
                    if (v == null || v.Length != 6 || v.Any(x => double.IsNaN(x) || double.IsInfinity(x)))
                        throw new ArgumentException("Six finite load components required.");
                    Axes.Validate(l.CoordinateSystem);
                    var node = nodes[l.NodeId];
                    node.Loads.Add(new PointLoad(v[0], v[1], v[2], v[3], v[4], v[5], node.Position,
                        cases[l.Case], l.CoordinateSystem, l.Record));
                }
                int beamLoad = 0;
                foreach (var l in batch.BeamLoads)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = l.Record;
                    var beam = beams[l.BeamId]; var loadCase = cases[l.Case]; Axes.Validate(l.CoordinateSystem);
                    if (Finite(l.Start, "station") < 0 || Finite(l.End, "station") > 1 || l.End < l.Start) throw new ArgumentException("InvalidBeamLoadInterval");
                    var assignment = new BeamLoadAssignment { Start = l.Start, End = l.End, SourceRecord = l.Record, StationDomain = "NodeToNode",
                        OriginalAssignmentId = batch.Program + " beam load " + (++beamLoad).ToString(System.Globalization.CultureInfo.InvariantCulture),
                        LengthConvention = l.ProjectionPlaneNormal == null ? BeamLoadLengthConvention.ActualLength : BeamLoadLengthConvention.ProjectedLength,
                        ProjectionPlaneNormal = l.ProjectionPlaneNormal };
                    if (l.Eccentricity != null) { Axes.Validate(l.EccentricityAxes); assignment.Eccentricity = l.Eccentricity; assignment.EccentricityCoordinateSystem = l.EccentricityAxes; }
                    if (l.Values != null)
                    {
                        if (l.Start != l.End || l.StartValues != null || l.EndValues != null || l.ProjectionPlaneNormal != null) throw new ArgumentException("InconsistentConcentratedBeamLoad");
                        var v = Components(l.Values);
                        assignment.Concentrated = new PointLoad(v[0], v[1], v[2], v[3], v[4], v[5], beam.StartPoint + (beam.EndPoint - beam.StartPoint) * l.Start,
                            loadCase, l.CoordinateSystem, l.Record);
                    }
                    else
                    {
                        if (l.Start == l.End) throw new ArgumentException("InvalidBeamLoadInterval");
                        var p = Components(l.StartValues); var q = Components(l.EndValues);
                        assignment.StartIntensity = new LineLoad(p[0], p[1], p[2], p[3], p[4], p[5], beam.Line, loadCase, l.CoordinateSystem, l.Record);
                        assignment.EndIntensity = new LineLoad(q[0], q[1], q[2], q[3], q[4], q[5], beam.Line, loadCase, l.CoordinateSystem, l.Record);
                    }
                    beam.Assignments.Loads.Add(assignment);
                }
                foreach (var l in batch.ShellLoads)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = l.Record;
                    var shell = shells[l.ShellId]; var loadCase = cases[l.Case];
                    if (l.NodalPressures != null)
                    {
                        if (l.Edge.HasValue || l.Components != null || l.NodalPressures.Length != shell.Nodes.Count) throw new ArgumentException("One pressure per shell node required.");
                        CoordinateSystem direction;
                        if (l.Normal)
                        {
                            if (!declaredShellAxes.Contains(l.ShellId)) throw new ArgumentException("ShellAxesRequiredForNormalPressure");
                            direction = Frames.At(shell.CoordinateSystem, shell.CoordinateSystem.Origin);
                        }
                        else direction = Along(l.Direction ?? throw new ArgumentException("MissingPressureDirection"));
                        shell.Loads.Add(new NonUniformPlatePressure(shell.Nodes.Select(n => n.Position).ToArray(), l.NodalPressures, loadCase, direction, l.Record));
                    }
                    else if (l.Edge.HasValue)
                    {
                        Axes.Validate(l.CoordinateSystem);
                        var v = l.Components; int count = shell.Nodes.Count;
                        if (l.Edge.Value < 0 || l.Edge.Value >= count) throw new ArgumentException("InvalidShellEdge");
                        if (l.Normal || v == null || v.Length != 3 || v.Any(x => double.IsNaN(x) || double.IsInfinity(x))) throw new ArgumentException("Three finite edge load components required.");
                        var edge = new Line3d(shell.Nodes[l.Edge.Value].Position, shell.Nodes[(l.Edge.Value + 1) % count].Position);
                        shell.Loads.Add(new LineLoad(v[0], v[1], v[2], 0, 0, 0, edge, loadCase, l.CoordinateSystem, l.Record));
                    }
                    else if (l.Normal)
                    {
                        if (!declaredShellAxes.Contains(l.ShellId)) throw new ArgumentException("ShellAxesRequiredForNormalPressure");
                        Axes.Validate(shell.CoordinateSystem);
                        shell.Loads.Add(new NormalAreaLoad(Finite(l.Pressure, "pressure"), shell.Shape, loadCase, shell.CoordinateSystem, l.Record));
                    }
                    else
                    {
                        Axes.Validate(l.CoordinateSystem);
                        var v = l.Components;
                        if (v == null || v.Length != 3 || v.Any(x => double.IsNaN(x) || double.IsInfinity(x))) throw new ArgumentException("Three finite traction components required.");
                        shell.Loads.Add(new AreaLoad(v[0], v[1], v[2], shell.Shape, loadCase, l.CoordinateSystem, l.Record));
                    }
                }
                foreach (var r in batch.NodeRestrains)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = r.Record;
                    if (r.FixedDofs == null || r.FixedDofs.Length != 6) throw new ArgumentException("Six restraint flags required.");
                    Axes.Validate(r.CoordinateSystem);
                    var node = nodes[r.NodeId];
                    var dofs = Enumerable.Range(0, 6).Where(i => r.FixedDofs[i])
                        .Select(i => new DofRestrain((GeometryRestrain.DOF)i)).ToList();
                    if (dofs.Count != 0) node.Assignments.AssignRestrain(new RestrainAssignment
                    {
                        Restrain = new NodeRestrain(node, r.CoordinateSystem, dofs), SourceRecord = r.Record
                    }, AssignmentMode.Add);
                }
                foreach (var r in batch.BeamReleases)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = r.Record;
                    var release = r.Release ?? throw new ArgumentException("MissingBeamRelease");
                    if (release.I == null || release.J == null || release.I.Length != 6 || release.J.Length != 6 || release.I.Concat(release.J).Any(d => d == null))
                        throw new ArgumentException("InvalidBeamRelease");
                    Axes.Validate(release.CoordinateSystem);
                    if (beams[r.BeamId].Attributes.Values.OfType<GPC.Model.Attributes.BeamReleasesAttribute>().Any())
                        throw new ArgumentException("ConflictingBeamRelease");
                    beams[r.BeamId].Attributes.Add(release);
                }
                foreach (var r in batch.NodeLinks)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = r.Record;
                    var first = nodes[r.I]; var second = nodes[r.J];
                    if (ReferenceEquals(first, second) || (r.Spring == null) == (r.RigidDofs == null)) throw new ArgumentException("InvalidNodeLink");
                    var link = new NodalLink { OtherNodeId = second.Id, SourceRecord = r.Record, Spring = r.Spring };
                    if (r.RigidDofs != null)
                    {
                        if (r.RigidDofs.Length != 6 || !r.RigidDofs.Any(v => v)) throw new ArgumentException("InvalidRigidLinkDofs");
                        var equations = GPC.Model.Constraints.RigidLink.GetRigidLink(first, second).Where((e, index) => r.RigidDofs[index]).ToArray();
                        link.KinematicCoefficients = new double[equations.Length * 12]; link.RightHandSide = equations.Select(e => e.ConstValue).ToArray();
                        link.LeverArm = second.Position - first.Position;
                        for (int row = 0; row < equations.Length; row++)
                            foreach (var term in equations[row].Equations)
                                link.KinematicCoefficients[row * 12 + (ReferenceEquals(term.NodeSlave, first) ? 0 : 6) + (int)term.GdlNode] += term.Value;
                    }
                    first.Assignments.Links.Add(link);
                }
                report.Diagnostics.AddRange(model.ValidateTopology());
                if (report.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)) return report;
                report.Model = model; report.Status = ImportStatus.Partial;
                report.Diagnostics.Add(new ModelDiagnostic
                {
                    Code = "GeometryOnly",
                    Severity = DiagnosticSeverity.Warning,
                    Message = "Model input mapped; results require a separate matched import, and unresolved assignments are listed in the diagnostics.",
                    SuggestedAction = "Supply matching analysis exports and assignment records."
                });
            }
            catch (OperationCanceledException) { report.Status = ImportStatus.Cancelled; }
            catch (Exception ex) when (ex is ArgumentException || ex is KeyNotFoundException || ex is InvalidOperationException || ex is NotSupportedException)
            { report.Diagnostics.Add(new ModelDiagnostic { Code = "ImportRejected", Severity = DiagnosticSeverity.Error, Record = record, Message = ex.Message, SuggestedAction = "Correct this source record; the active model was not changed." }); }
            return report;
        }

        private static double[] Components(double[] values)
        {
            if (values == null || values.Length != 6 || values.Any(x => double.IsNaN(x) || double.IsInfinity(x)))
                throw new ArgumentException("Six finite load components required.");
            return values;
        }

        /// <summary>A global frame whose V3 is the given direction.</summary>
        private static CoordinateSystem Along(Vector3d direction)
        {
            double length = Math.Sqrt(direction.X * direction.X + direction.Y * direction.Y + direction.Z * direction.Z);
            if (!(length > 0) || double.IsInfinity(length)) throw new ArgumentException("InvalidPressureDirection");
            var z = new Vector3d(direction.X / length, direction.Y / length, direction.Z / length);
            var seed = Math.Abs(z.X) < 0.9 ? new Vector3d(1, 0, 0) : new Vector3d(0, 1, 0);
            var x = seed - z * Axes.Dot(seed, z); x = x / Axes.Length(x);
            var axes = new CoordinateSystem(new Point3d(0, 0, 0), x, z.CrossProduct(x), z); Axes.Validate(axes); return axes;
        }

        private static double Finite(double value, string what)
        {
            if (double.IsNaN(value) || double.IsInfinity(value)) throw new ArgumentException("NonFiniteValue: " + what);
            return value;
        }

        /// <summary>Registers a shared property once. A mapped property whose name is taken gets a distinct registry name;
        /// a caller-supplied property keeps its name.</summary>
        private static void AddProperty(GPC.Model.Models.Model model, ElementProperty property, HashSet<ElementProperty> added, bool rename)
        {
            if (property is null || !added.Add(property)) return;
            var name = property.Name; int suffix = 1;
            while (!model.AddProperty(property) && rename) property.Name = name + " (" + (++suffix).ToString(System.Globalization.CultureInfo.InvariantCulture) + ")";
        }

        private sealed class SameInstance : IEqualityComparer<ElementProperty>
        {
            public static readonly SameInstance Comparer = new SameInstance();
            public bool Equals(ElementProperty x, ElementProperty y) => ReferenceEquals(x, y);
            public int GetHashCode(ElementProperty value) => RuntimeHelpers.GetHashCode(value);
        }
    }
}
