using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Restrains;
using GPC.Model.Models;
using GPC.Model.PostProcessing;

namespace GPC.Converter
{
    /// <summary>Creates an isolated candidate; a failed mapping never modifies an active model.</summary>
    public static class ModelMapper
    {
        public static ImportReport Map(ImportBatch batch, CancellationToken cancellationToken = default(CancellationToken), IProgress<int> progress = null)
        {
            var report = new ImportReport { Status = ImportStatus.Rejected, SourceHash = batch.SourceHash };
            report.Preserved.AddRange(batch.Uninterpreted);
            report.Diagnostics.AddRange(batch.Diagnostics);
            var model = new GPC.Model.Models.Model(batch.ModelRevision); var nodes = new Dictionary<string, NodeElement>(StringComparer.Ordinal);
            var bySource = new Dictionary<SourceIdentity, Element>();
            model.AnalysisSource = new AnalysisSource { Program = batch.Program, SolverVersion = batch.SolverVersion,
                ModelRevision = batch.ModelRevision, AnalysisId = batch.AnalysisId, GeometryHash = batch.SourceHash };
            model.PreservedSourceData.AddRange(batch.Uninterpreted);
            string record = null;
            try
            {
                foreach (var n in batch.Nodes)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = n.Record;
                    if (!Axes.IsFinite(n.GlobalPosition)) throw new ArgumentException("NonFiniteCoordinate");
                    var nodeAxes = ResultTransformations.AtPoint(n.CoordinateSystem ?? GPC.Geometry.CoordinateSystem.Global, n.GlobalPosition);
                    if (nodes.TryGetValue(n.Id, out var existing))
                    {
                        if (existing.Position.X != n.GlobalPosition.X || existing.Position.Y != n.GlobalPosition.Y || existing.Position.Z != n.GlobalPosition.Z
                            || !ResultTransformations.AtPoint(existing.CoordinateSystem, n.GlobalPosition).Equals(nodeAxes))
                            throw new ArgumentException("SourceCollision");
                        report.Diagnostics.Add(new ModelDiagnostic { Code = "IdenticalDuplicate", Severity = DiagnosticSeverity.Information, Record = record, Message = n.Id }); continue;
                    }
                    var node = new NodeElement(n.GlobalPosition, n.CoordinateSystem == null ? null : nodeAxes) { Source = new SourceIdentity(batch.Program, batch.ModelRevision, EntityFamily.Node, n.Id) };
                    model.NodesElements.Add(node); nodes.Add(n.Id, node); bySource.Add(node.Source, node); progress?.Report(model.NodesElements.Count);
                }
                foreach (var b in batch.Beams)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = b.Record;
                    var beam = new BeamElement(null, null, b.Property, b.CoordinateSystem) { Source = new SourceIdentity(batch.Program, batch.ModelRevision, EntityFamily.Beam, b.Id) };
                    if (bySource.ContainsKey(beam.Source)) throw new ArgumentException("SourceCollision");
                    bySource.Add(beam.Source, beam);
                    beam.Assignments.Formulation = BeamFormulation.StraightTwoNode; beam.Assignments.SectionAxes = b.CoordinateSystem;
                    beam.Assignments.OtherAssignments.AddRange(b.OtherAssignments);
                    model.BeamElements.Add(beam); model.ConnectBeam(beam.Id, nodes[b.I].Id, nodes[b.J].Id);
                    if (!(b.Property is null)) model.AddProperty(b.Property);
                }
                foreach (var a in batch.Shells)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = a.Record;
                    var shell = new AreaElement(null, a.Property, a.CoordinateSystem) { Source = new SourceIdentity(batch.Program, batch.ModelRevision, EntityFamily.Shell, a.Id) };
                    if (bySource.ContainsKey(shell.Source)) throw new ArgumentException("SourceCollision");
                    bySource.Add(shell.Source, shell);
                    model.AreaElements.Add(shell); model.ConnectShell(shell.Id, a.Nodes.Select(id => nodes[id].Id).ToArray());
                    if (!(a.Property is null)) model.AddProperty(a.Property);
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
                foreach (var c in batch.LoadCases)
                {
                    cancellationToken.ThrowIfCancellationRequested(); record = c.Record;
                    var loadCase = new LoadCaseBase(c.Name); cases.Add(c.Name, loadCase); model.LoadCases.Add(loadCase);
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
                report.Diagnostics.AddRange(model.ValidateTopology());
                if (report.Diagnostics.Any(d => d.Severity == DiagnosticSeverity.Error)) return report;
                report.Model = model; report.Status = ImportStatus.Partial;
                report.Diagnostics.Add(new ModelDiagnostic
                {
                    Code = "GeometryOnly",
                    Severity = DiagnosticSeverity.Warning,
                    Message = "Geometry mapped; result coverage and missing assignments require separate import and validation.",
                    SuggestedAction = "Supply matching analysis exports and assignment records."
                });
            }
            catch (OperationCanceledException) { report.Status = ImportStatus.Cancelled; }
            catch (Exception ex) when (ex is ArgumentException || ex is KeyNotFoundException || ex is InvalidOperationException)
            { report.Diagnostics.Add(new ModelDiagnostic { Code = "ImportRejected", Severity = DiagnosticSeverity.Error, Record = record, Message = ex.Message, SuggestedAction = "Correct this source record; the active model was not changed." }); }
            return report;
        }
    }
}
