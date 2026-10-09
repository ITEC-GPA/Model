using GPC.Model.Models;
using GPC.Model.Checking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Model.Elements;
using GPC.Model.Core;
using GPC.Model.Results.Locations;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Reports;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Results.Queries;
using GPC.Model.Structure.Members;
using GPC.Model.Structure.Topology;

namespace GPC.Model.Checking.Preparation
{
    public sealed class CheckWorkItem
    {
        public string Id { get; internal set; }
        public CheckScope Scope { get; internal set; }
        public CheckTargetReference Target { get; internal set; }
        public CheckMechanism Mechanism { get; internal set; }
        /// <summary>Discriminators of a task requested through SectionChecks; null for the legacy SectionMechanisms.</summary>
        public SectionCheckSpecification Check { get; internal set; }
        public string MethodId { get; internal set; }
        public ResultSelection Selection { get; internal set; }
        public MemberLocation MemberLocation { get; internal set; }
        public double? Station { get; internal set; }
        public string StationDomain { get; internal set; }
        public SectionSide Side { get; internal set; }
        public BeamPreparation Section { get; internal set; }
        public BeamActionPreparation Actions { get; internal set; }
        public PhysicalMemberCheckInput Member { get; internal set; }
        public DataStatus Data { get; internal set; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; internal set; } = new ModelDiagnostic[0];
        public CoverageAssessment Coverage { get; internal set; }
    }

    /// <summary>Explicit, inspectable tasks. Local sample checks and global span checks have different cardinalities.</summary>
    public sealed class BeamCheckPlan
    {
        private Models.Model _model;
        private string _preparedFingerprint;
        private bool _actionsOnly;
        public BeamCheckPlanRequest Request { get; private set; }
        public string ScopeFingerprint { get; private set; }
        public IReadOnlyList<CheckWorkItem> WorkItems { get; private set; }
        public bool IsCurrent => IsCurrentFor(Request);
        /// <summary>Checks current selectors/configuration and prepared inputs without rebuilding transformations or tasks.</summary>
        public bool IsCurrentFor(BeamCheckPlanRequest currentRequest)
        {
            using (ValidationReadScope.Enter(_model))
            {
                try { return ScopeFingerprint == ScopeDigest(_model, currentRequest, _actionsOnly)
                        && (ReferenceEquals(Request, currentRequest) || ScopeFingerprint == ScopeDigest(_model, Request, _actionsOnly)) && _preparedFingerprint == PreparedFingerprint()
                        && WorkItems.All(w => (w.Section?.Input == null || w.Section.Input.IsCurrent) && (w.Actions?.Input == null || w.Actions.Input.IsCurrent)
                            && (w.Member == null || w.Member.Sections.All(s => s.IsCurrent))); }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException || ex is KeyNotFoundException) { return false; }
            }
        }
        private string PreparedFingerprint() => ModelValues.Fingerprint(WorkItems.SelectMany(w => new object[] { w.Id, w.Target, w.Scope, w.Mechanism,
            w.MethodId, w.Selection, w.MemberLocation, w.Station, w.StationDomain, w.Side, w.Data, w.Coverage, w.Member?.Snapshot }
            .Concat(w.Check == null ? new object[0] : new object[] { w.Check })));

        private sealed class Scope
        {
            internal Dictionary<int, BeamElement> Beams = new Dictionary<int, BeamElement>();
            internal Dictionary<string, PhysicalMemberGeometry> Members = new Dictionary<string, PhysicalMemberGeometry>(StringComparer.Ordinal);
            internal Dictionary<int, PhysicalMemberGeometry> Owners = new Dictionary<int, PhysicalMemberGeometry>();
        }
        private static Scope Resolve(Models.Model model, BeamCheckPlanRequest r)
        {
            if (model == null || r == null || r.MemberIds == null || r.Results == null || r.SectionMechanisms == null || r.MemberChecks == null || r.Locations == null
                || r.Results.Length == 0 || r.Results.Any(s => s == null || string.IsNullOrWhiteSpace(s.Dataset) || string.IsNullOrWhiteSpace(s.Case))
                || r.MemberIds.Any(string.IsNullOrWhiteSpace) || r.MemberIds.Distinct(StringComparer.Ordinal).Count() != r.MemberIds.Length
                || r.SectionMechanisms.Any(m => !Enum.IsDefined(typeof(CheckMechanism), m)) || r.SectionMechanisms.Distinct().Count() != r.SectionMechanisms.Length
                || !Enum.IsDefined(typeof(BeamCoveragePolicy), r.CoveragePolicy) || r.Locations.Any(l => l == null) || r.MemberChecks.Any(s => s == null))
                throw new ArgumentException("InvalidBeamCheckPlanRequest");
            if (!r.HasSectionChecks && r.MemberChecks.Length == 0) throw new ArgumentException("NoRequestedChecks");
            if (r.SectionChecks != null)
            {
                if (r.SectionChecks.Any(c => c == null)) throw new ArgumentException("InvalidBeamCheckPlanRequest");
                foreach (var check in r.SectionChecks) check.Validate();
                if (r.SectionChecks.Select(c => c.Key).Distinct(StringComparer.Ordinal).Count() != r.SectionChecks.Length)
                    throw new ArgumentException("DuplicateSectionCheckSpecification");
                // A mechanism is requested either with the legacy semantics or with explicit discriminators, never both.
                if (r.SectionChecks.Any(c => r.SectionMechanisms.Contains(c.Mechanism))) throw new ArgumentException("MechanismRequestedTwice: remove it from SectionMechanisms");
                var missing = r.SectionChecks.Where(c => c.Category != CombinationCategory.Unspecified && !r.Results.Any(s => s.Category == c.Category)).ToArray();
                if (missing.Length != 0) throw new ArgumentException("NoResultSelectionForCategory: " + string.Join(", ", missing.Select(c => c.Key)));
            }
            if (r.Results.Any(s => s.Category.HasValue && !Enum.IsDefined(typeof(CombinationCategory), s.Category.Value))) throw new ArgumentException("InvalidResultSelectionCategory");
            var scope = new Scope();
            foreach (var id in r.MemberIds)
            {
                if (!model.PhysicalMembers.TryGetValue(id, out var definition) || definition == null || definition.Id != id) throw new ArgumentException("MissingOrMismatchedPhysicalMember");
                var geometry = new PhysicalMemberGeometry(model, definition); scope.Members.Add(id, geometry);
                foreach (var part in definition.Parts)
                {
                    if (scope.Owners.ContainsKey(part.BeamId)) throw new ArgumentException("OverlappingMemberScopes: use separate jobs for alternative physical definitions.");
                    scope.Owners.Add(part.BeamId, geometry); scope.Beams[part.BeamId] = model.BeamElements[part.BeamId];
                }
            }
            if (r.Elements != null)
            {
                if (r.Elements.Families == null || r.Elements.Families.Any(f => f != EntityFamily.Beam)) throw new NotSupportedException("BeamPlanRequiresBeamSelection");
                foreach (var e in r.Elements.Resolve(model).Cast<BeamElement>()) scope.Beams[e.Id] = e;
            }
            if (scope.Beams.Count == 0) throw new ArgumentException("EmptyBeamCheckScope");
            foreach (var spec in r.MemberChecks)
            {
                if (!scope.Members.TryGetValue(spec.MemberId ?? "", out var member) || string.IsNullOrWhiteSpace(spec.MethodId) || spec.Context == null
                    || !Enum.IsDefined(typeof(CheckMechanism), spec.Mechanism)) throw new ArgumentException("ExplicitCompleteMemberScopeRequired");
                CheckValue.Finite(spec.Start, "span start"); CheckValue.Finite(spec.End, "span end");
                if (spec.Start < 0 || (spec.End ?? member.Length) > member.Length || spec.Start >= (spec.End ?? member.Length)
                    || spec.Context.Restraints.Any(b => b.Distance > member.Length)) throw new ArgumentException("InvalidMemberSpanOrRestraintPosition");
            }
            if (r.MemberChecks.GroupBy(s => ModelValues.Fingerprint(new object[] { s.MemberId, s.MethodId, s.Mechanism, s.Start, s.End }))
                .Any(g => g.Select(s => ModelValues.Fingerprint(new object[] { s.Context })).Distinct().Count() > 1))
                throw new ArgumentException("ConflictingMemberCheckContexts: use distinct explicit jobs.");
            if (r.CoveragePolicy == BeamCoveragePolicy.ExportedSamples && r.Locations.Length != 0) throw new ArgumentException("LocationsRequireExplicitCoveragePolicy");
            return scope;
        }
        public static string FingerprintScope(Models.Model model, BeamCheckPlanRequest request)
        {
            var scope = Resolve(model, request);
            return ModelValues.Fingerprint(new object[] { request, model.VerificationFingerprint(request.Settings) }
                .Concat(request.Results.Select(s => s.Dataset).Distinct(StringComparer.Ordinal).OrderBy(s => s, StringComparer.Ordinal)
                    .SelectMany(id => new object[] { id, model.Datasets.TryGetValue(id, out var dataset) ? dataset : null }))
                .Concat(scope.Members.Values.OrderBy(m => m.Definition.Id, StringComparer.Ordinal).Select(m => (object)m.Definition))
                .Concat(scope.Beams.Values.OrderBy(b => b.Id).SelectMany(b => new object[] { b.Id, b.Groups.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray(),
                    request.Results.SelectMany(s => ResultQueries.Samples<StationResultBeamForces>(b, s)).ToArray() })));
        }
        private static string ScopeDigest(Models.Model model, BeamCheckPlanRequest request, bool actionsOnly)
            => !actionsOnly ? FingerprintScope(model, request) : ModelValues.Fingerprint(new object[] { "MaterialActions-v1", FingerprintScope(model, request) }
                .Concat(Resolve(model, request).Beams.Values.OrderBy(b => b.Id).SelectMany(b => new object[] { b.Id, b.BeamProperty })));

        /// <summary>Resolves only the selected elements, without preparing samples or invoking an engine.</summary>
        public static IReadOnlyList<int> ResolveBeamIds(Models.Model model, BeamCheckPlanRequest request)
            => Array.AsReadOnly(Resolve(model, request).Beams.Keys.OrderBy(id => id).ToArray());

        public static BeamCheckPlan Prepare(Models.Model model, BeamCheckPlanRequest request, CancellationToken token = default(CancellationToken))
            => PrepareCore(model, request, token, false);
        /// <summary>Same coverage and identities, without assuming a reinforced-concrete section.</summary>
        public static BeamCheckPlan PrepareActions(Models.Model model, BeamCheckPlanRequest request, CancellationToken token = default(CancellationToken))
            => PrepareCore(model, request, token, true);
        private static BeamCheckPlan PrepareCore(Models.Model model, BeamCheckPlanRequest request, CancellationToken token, bool actionsOnly)
        {
            using (ValidationReadScope.Enter(model)) return PrepareRead(model, request, token, actionsOnly);
        }
        private static BeamCheckPlan PrepareRead(Models.Model model, BeamCheckPlanRequest request, CancellationToken token, bool actionsOnly)
        {
            Resolve(model, request); var r = request.Copy(); var scope = Resolve(model, r);
            var plan = new BeamCheckPlan { _model = model, _actionsOnly = actionsOnly, Request = r, ScopeFingerprint = ScopeDigest(model, r, actionsOnly) };
            var rows = new List<CheckWorkItem>();
            var selections = r.Results.GroupBy(s => ModelValues.Fingerprint(new object[] { s })).Select(g => g.First()).ToArray();
            var requested = ResolveLocations(scope, r);
            foreach (var selection in selections)
            {
                if (r.HasSectionChecks)
                {
                    if (r.CoveragePolicy == BeamCoveragePolicy.ExportedSamples)
                        foreach (var beam in scope.Beams.Values.OrderBy(b => b.Id))
                        {
                            var samples = ResultQueries.Samples<StationResultBeamForces>(beam, selection);
                            if (samples.Count == 0) AddSection(rows, model, scope, r, selection, beam.Id, null, null, beam.Assignments.StationDomain, SectionSide.Unspecified, token, actionsOnly);
                            foreach (var group in samples.GroupBy(s => ModelValues.Fingerprint(new object[] { State(s), s.ParametricDistance, s.StationDomain, s.Side })))
                            {
                                var sample = group.First();
                                AddSection(rows, model, scope, r, group.Count() == 1 ? selection : State(sample, selection.Category), beam.Id,
                                    group.Count() == 1 ? sample : null, sample.ParametricDistance, sample.StationDomain, sample.Side, token, actionsOnly, "AmbiguousExportedLocationState");
                            }
                        }
                    else foreach (var location in requested)
                    {
                        var beam = scope.Beams[location.BeamId]; var geometry = new BeamReferenceGeometry(beam);
                        var matches = ResultQueries.Samples<StationResultBeamForces>(beam, selection).Where(s => Matches(geometry, s, location.Station, location.StationDomain, location.Side)).ToArray();
                        AddSection(rows, model, scope, r, selection, beam.Id, matches.Length == 1 ? matches[0] : null, location.Station, location.StationDomain, location.Side, token, actionsOnly,
                            matches.Length > 1 ? "AmbiguousRequiredLocationState" : "MissingRequiredLocation");
                    }
                }
                foreach (var spec in r.MemberChecks) AddMember(rows, model, scope.Members[spec.MemberId], r, spec, selection, token);
            }
            // Overlapping state selectors and repeated locations must not double-count the same task.
            plan.WorkItems = Array.AsReadOnly(rows.GroupBy(w => w.Id, StringComparer.Ordinal).Select(g => g.First()).ToArray());
            if (plan.WorkItems.Count == 0) throw new ArgumentException("EmptyBeamCheckPlan");
            plan._preparedFingerprint = plan.PreparedFingerprint(); return plan;
        }
        private static bool Matches(BeamReferenceGeometry geometry, StationResultBeamForces sample, double station, string domain, SectionSide side)
        {
            try { return Math.Abs(geometry.ConvertStation(sample.ParametricDistance, sample.StationDomain, domain) - station) <= 1e-10
                    && (sample.Side == side || (station == 0 || station == 1) && sample.Side == SectionSide.Unspecified); }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException) { return false; }
        }
        private static IReadOnlyList<MemberLocation> ResolveLocations(Scope scope, BeamCheckPlanRequest r)
        {
            var locations = new List<MemberLocation>();
            foreach (var p in r.Locations)
            {
                if (!Enum.IsDefined(typeof(SectionSide), p.Side)) throw new ArgumentException("InvalidRequiredSide");
                if (p.MemberId != null)
                {
                    if (!p.Distance.HasValue || p.BeamId.HasValue || p.Station.HasValue || p.StationDomain != null || !scope.Members.TryGetValue(p.MemberId, out var geometry))
                        throw new ArgumentException("InvalidRequiredMemberLocation");
                    locations.AddRange(geometry.Locate(p.Distance.Value, p.Side));
                }
                else
                {
                    if (!p.BeamId.HasValue || !p.Station.HasValue || p.Distance.HasValue || !scope.Beams.TryGetValue(p.BeamId.Value, out var beam)) throw new ArgumentException("InvalidRequiredFemLocation");
                    new BeamReferenceGeometry(beam).PointAt(p.Station.Value, p.StationDomain);
                    locations.Add(new MemberLocation(null, beam.Id, 0, p.Station.Value, p.StationDomain, p.Side));
                }
            }
            if (r.CoveragePolicy == BeamCoveragePolicy.RequiredLocations && r.HasSectionChecks
                && scope.Beams.Keys.Any(id => !locations.Any(l => l.BeamId == id))) throw new ArgumentException("EverySelectedBeamNeedsRequiredLocations");
            return locations;
        }
        private static void AddSection(List<CheckWorkItem> rows, Models.Model model, Scope scope, BeamCheckPlanRequest r, ResultSelection selection,
            int beamId, StationResultBeamForces sample, double? station, string domain, SectionSide side, CancellationToken token, bool actionsOnly, string missingCode = "MissingBeamSamples")
        {
            var prepared = token.IsCancellationRequested || actionsOnly ? null : BeamCheckPreparation.Prepare(model, beamId, sample, r.Settings);
            var actions = token.IsCancellationRequested || !actionsOnly ? null : BeamActionPreparation.Prepare(model, beamId, sample, r.Settings);
            var member = scope.Owners.TryGetValue(beamId, out var owner) ? owner : null;
            MemberLocation location = null;
            var diagnostics = new List<ModelDiagnostic>();
            if (sample == null) diagnostics.Add(ModelDiagnostic.Error(missingCode, model.BeamElements[beamId]));
            try { if (member != null && station.HasValue) location = member.FromElement(beamId, station.Value, domain, side); }
            catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException)
            {
                diagnostics.Add(ModelDiagnostic.Error("InvalidMemberSampleLocation", model.BeamElements[beamId], ex.Message));
                prepared = new BeamPreparation { BeamId = beamId, Sample = sample, Status = DataStatus.Insufficient,
                    Diagnostics = (prepared?.Diagnostics ?? new ModelDiagnostic[0]).Concat(diagnostics).ToArray() };
            }
            foreach (var mechanism in r.SectionMechanisms)
            {
                var actualState = sample == null ? selection.Copy() : State(sample);
                var item = new CheckWorkItem { Scope = CheckScope.SectionSample, Target = new CheckTargetReference(beamId, member?.Definition.Id),
                    Selection = actualState, Mechanism = mechanism, Section = prepared, Actions = actions, MemberLocation = location,
                    Station = sample?.ParametricDistance ?? station, StationDomain = sample?.StationDomain ?? domain, Side = sample?.Side ?? side,
                    Data = prepared?.Status ?? actions?.Status ?? DataStatus.Insufficient, Coverage = new CoverageAssessment(r.CoveragePolicy, 1, sample == null ? 0 : 1),
                    Diagnostics = diagnostics.AsReadOnly() };
                item.Id = ModelValues.Fingerprint(new object[] { item.Scope, item.Target, mechanism, actualState, item.Station, item.StationDomain, item.Side }); rows.Add(item);
            }
            // Explicit tasks: only for the selections declared with the requested category (Unspecified: every selection).
            foreach (var check in r.SectionChecks ?? new SectionCheckSpecification[0])
            {
                if (check.Category != CombinationCategory.Unspecified && selection.Category != check.Category) continue;
                var actualState = sample == null ? selection.Copy() : State(sample);
                actualState.Category = selection.Category;
                var item = new CheckWorkItem { Scope = CheckScope.SectionSample, Target = new CheckTargetReference(beamId, member?.Definition.Id),
                    Selection = actualState, Mechanism = check.Mechanism, Check = check.Copy(), Section = prepared, Actions = actions, MemberLocation = location,
                    Station = sample?.ParametricDistance ?? station, StationDomain = sample?.StationDomain ?? domain, Side = sample?.Side ?? side,
                    Data = prepared?.Status ?? actions?.Status ?? DataStatus.Insufficient, Coverage = new CoverageAssessment(r.CoveragePolicy, 1, sample == null ? 0 : 1),
                    Diagnostics = diagnostics.AsReadOnly() };
                item.Id = ModelValues.Fingerprint(new object[] { item.Scope, item.Target, "SectionCheck", check, actualState, item.Station, item.StationDomain, item.Side }); rows.Add(item);
            }
        }
        internal static ResultSelection State(StationResultBeamForces sample) => new ResultSelection { Dataset = sample.State?.DatasetId, Case = sample.Case?.Name,
            Phase = sample.State?.Phase, Step = sample.State?.Step, ConcomitantState = sample.State?.ConcomitantStateId, Mode = sample.State?.Mode, MovingLoadPosition = sample.State?.MovingLoadPosition };
        private static ResultSelection State(StationResultBeamForces sample, CombinationCategory? category) { var state = State(sample); state.Category = category; return state; }

        private static void AddMember(List<CheckWorkItem> rows, Models.Model model, PhysicalMemberGeometry geometry, BeamCheckPlanRequest r,
            MemberCheckSpecification spec, ResultSelection selection, CancellationToken token)
        {
            var errors = new List<ModelDiagnostic>(); var inputs = new List<BeamCheckInput>(); int required = 0, available = 0; bool stale = false;
            double end = spec.End ?? geometry.Length;
            foreach (var part in geometry.Definition.Parts)
            {
                var start = geometry.FromElement(part.BeamId, 0, geometry.Definition.StationDomain, SectionSide.Unspecified).Distance;
                var stop = geometry.FromElement(part.BeamId, 1, geometry.Definition.StationDomain, SectionSide.Unspecified).Distance;
                if (Math.Min(start, stop) >= end || Math.Max(start, stop) <= spec.Start) continue;
                var located = new List<StationResultBeamForces>();
                foreach (var candidate in ResultQueries.Samples<StationResultBeamForces>(model.BeamElements[part.BeamId], selection))
                    try { var distance = geometry.FromElement(part.BeamId, candidate.ParametricDistance, candidate.StationDomain, candidate.Side).Distance;
                        if (distance >= spec.Start && distance <= end) located.Add(candidate); }
                    catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException)
                    { errors.Add(ModelDiagnostic.Error("InvalidMemberSampleLocation", model.BeamElements[part.BeamId], ex.Message)); }
                var samples = located.ToArray();
                if (samples.GroupBy(s => ModelValues.Fingerprint(new object[] { State(s), s.ParametricDistance, s.StationDomain, s.Side })).Any(g => g.Count() > 1))
                    errors.Add(ModelDiagnostic.Error("AmbiguousMemberSamples", model.BeamElements[part.BeamId]));
                required += Math.Max(1, samples.Length); available += samples.Length;
                if (samples.Length == 0) errors.Add(ModelDiagnostic.Error("MissingMemberElementState", model.BeamElements[part.BeamId]));
                foreach (var sample in samples)
                {
                    if (token.IsCancellationRequested) continue;
                    var prepared = BeamCheckPreparation.Prepare(model, part.BeamId, sample, r.Settings);
                    stale |= prepared.Status == DataStatus.Stale;
                    if (prepared.Input == null) errors.AddRange(prepared.Diagnostics); else inputs.Add(prepared.Input);
                }
            }
            if (!spec.Context.GlobalStateConfirmed || string.IsNullOrWhiteSpace(selection.ConcomitantState)) errors.Add(ModelDiagnostic.Error("GlobalMemberStateNotConfirmed"));
            if (inputs.Select(i => ModelValues.Fingerprint(new object[] { State(i.Sample) })).Distinct().Count() > 1) errors.Add(ModelDiagnostic.Error("IncoherentMemberStates"));
            var snapshot = new MemberInputSnapshot(geometry, spec, inputs);
            foreach (var boundary in new[] { spec.Start, end })
                if (!snapshot.Samples.Any(s => Math.Abs(s.Location.Distance - boundary) <= 1e-7)) errors.Add(ModelDiagnostic.Error("MissingMemberSpanBoundary"));
            var item = new CheckWorkItem { Scope = CheckScope.PhysicalMember, Target = new CheckTargetReference(geometry.Definition.Id), Mechanism = spec.Mechanism,
                MethodId = spec.MethodId, Selection = selection.Copy(), Data = stale ? DataStatus.Stale : errors.Count == 0 ? DataStatus.Ready : DataStatus.Insufficient,
                Member = new PhysicalMemberCheckInput { Snapshot = snapshot, Sections = inputs.AsReadOnly(), MethodId = spec.MethodId, Mechanism = spec.Mechanism },
                Diagnostics = errors.AsReadOnly(), Coverage = new CoverageAssessment(BeamCoveragePolicy.ExportedSamples, required, available) };
            item.Id = ModelValues.Fingerprint(new object[] { item.Scope, item.Target, spec, selection }); rows.Add(item);
        }
    }
}
