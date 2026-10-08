using GPC.Model.Checking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using GPC.Model.Elements;
using GPC.Model.Persistence;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.PostProcessing
{
    [Serializable]
    public sealed class ElementKey
    {
        public EntityFamily Family { get; set; }
        public int Id { get; set; }
    }
    [Serializable]
    public sealed class ElementSelection
    {
        public string[] Groups { get; set; } = new string[0];
        public ElementKey[] Elements { get; set; } = new ElementKey[0];
        public bool IncludeDescendants { get; set; } = true;
        public EntityFamily[] Families { get; set; } = new[] { EntityFamily.Beam, EntityFamily.Shell };

        /// <summary>Union of explicit elements and group members. Empty selectors mean all elements in the requested families.</summary>
        public IReadOnlyList<Element> Resolve(Models.Model model)
        {
            if (model == null || Groups == null || Elements == null || Families == null || Families.Length == 0) throw new ArgumentException("InvalidElementSelection");
            if (Families.Any(f => f != EntityFamily.Node && f != EntityFamily.Beam && f != EntityFamily.Shell)) throw new NotSupportedException("OnlyNodeBeamAndShellSelectionSupported");
            var selected = new HashSet<Element>(ReferenceComparer<Element>.Instance);
            foreach (var name in Groups) foreach (var element in model.GetGroupElements(name, IncludeDescendants))
                if (Families.Contains(Models.Model.FamilyOf(element))) selected.Add(element);
            foreach (var key in Elements)
            {
                if (key == null || !Families.Contains(key.Family)) throw new ArgumentException("ElementFamilyOutsideSelection");
                var element = model.AllElements.SingleOrDefault(e => Models.Model.FamilyOf(e) == key.Family && e.Id == key.Id);
                if (element == null) throw new ArgumentException("MissingSelectedElement");
                selected.Add(element);
            }
            if (Groups.Length == 0 && Elements.Length == 0)
                foreach (var element in model.AllElements.Where(e => Families.Contains(Models.Model.FamilyOf(e)))) selected.Add(element);
            return selected.OrderBy(Models.Model.FamilyOf).ThenBy(e => e.Id).ToArray();
        }
        public ElementSelection Copy() => new ElementSelection { Groups = (string[])Groups.Clone(), IncludeDescendants = IncludeDescendants,
            Families = (EntityFamily[])Families.Clone(), Elements = Elements.Select(e => new ElementKey { Family = e.Family, Id = e.Id }).ToArray() };
    }
    [Serializable]
    public sealed class PreparationRequest
    {
        public ElementSelection Selection { get; set; } = new ElementSelection();
        public ResultSelection[] Results { get; set; } = new ResultSelection[0];
        public CheckMechanism[] Mechanisms { get; set; } = new[] { CheckMechanism.UlsBiaxialSection };
        public string Settings { get; set; }
        public PreparationRequest Copy() => new PreparationRequest { Selection = Selection.Copy(), Results = Results.Select(r => r.Copy()).ToArray(),
            Mechanisms = (CheckMechanism[])Mechanisms.Clone(), Settings = Settings };
    }
    public sealed class PreparedElementSample
    {
        public ElementKey Element { get; internal set; }
        public ResultSelection Selection { get; internal set; }
        public ResultLocation Sample { get; internal set; }
        public BeamPreparation Beam { get; internal set; }
        public ShellInputPreparation Shell { get; internal set; }
        public bool Cancelled { get; internal set; }
    }
    public sealed class ModelPreparation
    {
        public PreparationRequest Request { get; private set; }
        public string ScopeFingerprint { get; private set; }
        public IReadOnlyList<PreparedElementSample> Samples { get; private set; }

        private static void Validate(PreparationRequest request)
        {
            if (request?.Selection == null || request.Results == null || request.Results.Length == 0 || request.Results.Any(r => r == null
                || string.IsNullOrWhiteSpace(r.Dataset) || string.IsNullOrWhiteSpace(r.Case))) throw new ArgumentException("ExplicitResultSelectionsRequired");
            if (request.Selection.Families == null || request.Selection.Families.Any(f => f != EntityFamily.Beam && f != EntityFamily.Shell))
                throw new NotSupportedException("OnlyBeamAndShellVerificationSupported: nodal actions have a separate preparation contract.");
            if (request.Mechanisms == null || request.Mechanisms.Length == 0 || request.Mechanisms.Distinct().Count() != request.Mechanisms.Length
                || request.Mechanisms.Any(m => !Enum.IsDefined(typeof(CheckMechanism), m))) throw new ArgumentException("InvalidCheckMechanisms");
        }
        private static PreparedElementSample[] Enumerate(Models.Model model, PreparationRequest request, bool preserveCategories = false)
        {
            Validate(request);
            var rows = new List<PreparedElementSample>(); var selected = request.Selection.Resolve(model);
            if (selected.Count == 0) throw new InvalidOperationException("EmptyVerificationSelection");
            foreach (var element in selected)
            {
                var seen = new HashSet<ResultLocation>(ReferenceComparer<ResultLocation>.Instance);
                var categorySamples = new Dictionary<CombinationCategory, HashSet<ResultLocation>>();
                var missing = new HashSet<string>();
                foreach (var selection in request.Results)
                {
                    if (preserveCategories)
                    {
                        var category = selection.Category ?? CombinationCategory.Unspecified;
                        if (!categorySamples.TryGetValue(category, out seen))
                            categorySamples.Add(category, seen = new HashSet<ResultLocation>(ReferenceComparer<ResultLocation>.Instance));
                    }
                    var values = element is BeamElement beam ? ResultQueries.Samples<StationResultBeamForces>(beam, selection).Cast<ResultLocation>().ToArray()
                        : ResultQueries.Samples<PointResultPlateForces>(element, selection).Cast<ResultLocation>().ToArray();
                    if (values.Length == 0 && missing.Add(ModelArchive.Fingerprint(new object[] { selection }))) values = new ResultLocation[] { null };
                    foreach (var sample in values)
                        if (sample == null || seen.Add(sample)) rows.Add(new PreparedElementSample { Element = new ElementKey { Family = Models.Model.FamilyOf(element), Id = element.Id }, Selection = selection, Sample = sample });
                }
            }
            return rows.ToArray();
        }
        public static string FingerprintScope(Models.Model model, PreparationRequest request)
            => FingerprintScopeCore(model, request, false);
        public static string FingerprintShellScope(Models.Model model, PreparationRequest request)
            => FingerprintScopeCore(model, request, true);
        private static string FingerprintScopeCore(Models.Model model, PreparationRequest request, bool preserveCategories)
        {
            var entries = Enumerate(model, request, preserveCategories);
            return ModelArchive.Fingerprint(new object[] { request }.Concat(entries.SelectMany(e => new object[] { e.Element, e.Selection, e.Sample })));
        }
        public static ModelPreparation Prepare(Models.Model model, PreparationRequest request, CancellationToken cancellationToken = default)
        {
            using (GPC.Model.Checking.ValidationReadScope.Enter(model)) return PrepareRead(model, request, cancellationToken);
        }
        public static ModelPreparation PrepareShellActions(Models.Model model, PreparationRequest request, ShellInputAxes axesKind,
            GPC.Geometry.CoordinateSystem axes = null, CancellationToken cancellationToken = default)
        {
            if (!Enum.IsDefined(typeof(ShellInputAxes), axesKind) || request?.Selection?.Families == null
                || request.Selection.Families.Any(f => f != EntityFamily.Shell) || (axesKind == ShellInputAxes.Explicit) != (axes != null))
                throw new ArgumentException("ExplicitShellScopeAndAxesRequired");
            using (ValidationReadScope.Enter(model))
            {
                Validate(request); var snapshot = request.Copy(); var rows = Enumerate(model, snapshot, true);
                var scope = FingerprintShellScope(model, snapshot);
                foreach (var row in rows)
                {
                    if (cancellationToken.IsCancellationRequested) { row.Cancelled = true; continue; }
                    var element = model.AreaElements[row.Element.Id];
                    var target = axesKind == ShellInputAxes.Explicit ? axes : axesKind == ShellInputAxes.Reinforcement ? element.Assignments.LayerAxes : element.CoordinateSystem;
                    row.Shell = ShellInputPreparation.PrepareActions(model, element.Id, (PointResultPlateForces)row.Sample, target, snapshot.Settings);
                }
                return new ModelPreparation { Request = snapshot, ScopeFingerprint = scope, Samples = rows };
            }
        }
        private static ModelPreparation PrepareRead(Models.Model model, PreparationRequest request, CancellationToken cancellationToken)
        {
            Validate(request); var snapshot = request.Copy(); var rows = Enumerate(model, snapshot);
            var scope = FingerprintScope(model, snapshot);
            foreach (var row in rows)
            {
                if (cancellationToken.IsCancellationRequested) { row.Cancelled = true; continue; }
                if (row.Element.Family == EntityFamily.Beam) row.Beam = BeamCheckPreparation.Prepare(model, row.Element.Id, (StationResultBeamForces)row.Sample, snapshot.Settings);
                else row.Shell = ShellInputPreparation.Prepare(model, row.Element.Id, (PointResultPlateForces)row.Sample, snapshot.Settings);
            }
            return new ModelPreparation { Request = snapshot, ScopeFingerprint = scope, Samples = rows };
        }
    }
}
