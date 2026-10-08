using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Cryptography;
using System.Xml;
using GPC.Model.ElementProperties;
using GPC.Model.Persistence;
using GPC.Model.Sections.Concrete;

namespace GPC.Model.PostProcessing
{
    public enum AnalysisCompatibility { Unknown, Compatible, RequiresReanalysis }
    public enum ReinforcementAnalysisRole { Unknown, ExcludedFromAnalysis, IncludedInAnalysis }

    internal static class AnalysisStorage
    {
        internal static byte[] Write<T>(T value)
        {
            using (var stream = new MemoryStream())
            {
                ModelArchive.Serializer(typeof(T)).WriteObject(stream, value);
                return stream.ToArray();
            }
        }
        internal static T Read<T>(byte[] bytes)
        {
            using (var stream = new MemoryStream(bytes, false))
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null, MaxCharactersInDocument = 256L * 1024 * 1024 }))
                return (T)ModelArchive.Serializer(typeof(T)).ReadObject(reader);
        }
        internal static string Digest(byte[] bytes)
        {
            using (var sha = SHA256.Create()) return Convert.ToBase64String(sha.ComputeHash(bytes));
        }
        internal static string Design(Models.Model model) => ModelArchive.Fingerprint(model.BeamElements.Values
            .Select(b => (object)new object[] { b.Guid, b.BeamProperty, b.Assignments.Sections.ToArray() })
            .Concat(model.AreaElements.Values.Select(a => (object)new object[] { a.Guid, a.Assignments })));
        internal static string Reinforcement(Models.Model model) => ModelArchive.Fingerprint(ConcreteSections(model)
            .Select(s => (object)new object[] { s.Rebars.ToArray(), s.ShearData, s.TorsionData }).Concat(model.AreaElements.Values.Select(a => (object)new object[] {
                a.Guid, a.Assignments.LayerAxes, a.Assignments.Layers.ToArray() })));
        internal static string Prestress(Models.Model model) => ModelArchive.Fingerprint(ConcreteSections(model)
            .Select(s => (object)s.Rebars.Where(r => r.EpsilonP != 0).ToArray()));
        private static IEnumerable<ReinforcedConcreteSection> ConcreteSections(Models.Model model) => model.BeamElements.Values
            .SelectMany(b => new[] { b.BeamProperty }.Concat(b.Assignments.Sections.SelectMany(s =>
                new[] { s.Property, s.EndProperty }.Concat(s.Stations.Select(p => p.Property)))))
            .OfType<ReinforcedConcreteSection>();
        internal static string Results(Models.Model model) => ModelArchive.Fingerprint(model.AllElements
            .Select(e => (object)new object[] { e.Guid, e.Results.ToArray() }));
    }

    /// <summary>An independent, immutable copy of the inputs recorded BEFORE importing/calculating results.
    /// OpenModel always returns a new graph. Capturing a current design is not evidence of a historical analysis.</summary>
    [Serializable]
    public sealed class AnalysisSnapshot
    {
        private byte[] _model;
        private string _contentHash;
        public string Id { get; private set; }
        public Guid ModelGuid { get; private set; }
        public string InputFingerprint { get; private set; }
        public string SourceFingerprint { get; private set; }
        public string ReinforcementFingerprint { get; private set; }
        public string PrestressFingerprint { get; private set; }
        public ReinforcementAnalysisRole ReinforcementRole { get; private set; }
        public string AssumptionReference { get; private set; }
        private AnalysisSnapshot() { }
        public static AnalysisSnapshot Capture(Models.Model model, ReinforcementAnalysisRole reinforcementRole = ReinforcementAnalysisRole.Unknown,
            string assumptionReference = null)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            if (!Enum.IsDefined(typeof(ReinforcementAnalysisRole), reinforcementRole)) throw new ArgumentOutOfRangeException(nameof(reinforcementRole));
            if (reinforcementRole != ReinforcementAnalysisRole.Unknown && string.IsNullOrWhiteSpace(assumptionReference))
                throw new ArgumentException("An explicit analysis assumption and its source are required.", nameof(assumptionReference));
            if (model.Datasets.Count != 0 || model.AllElements.Any(e => e.Results.Any()))
                throw new InvalidOperationException("Capture analysis inputs before results. Historical provenance cannot be inferred from the current design.");
            var bytes = AnalysisStorage.Write(model.AnalysisArchiveView());
            var copy = AnalysisStorage.Read<Models.Model>(bytes);
            return new AnalysisSnapshot { Id = Guid.NewGuid().ToString("D"), ModelGuid = copy.Guid, _model = bytes,
                _contentHash = AnalysisStorage.Digest(bytes), InputFingerprint = copy.AnalysisFingerprint(),
                SourceFingerprint = ModelArchive.Fingerprint(new object[] { copy.AnalysisSource }),
                ReinforcementFingerprint = AnalysisStorage.Reinforcement(copy), PrestressFingerprint = AnalysisStorage.Prestress(copy),
                ReinforcementRole = reinforcementRole, AssumptionReference = assumptionReference };
        }
        public Models.Model OpenModel()
        {
            if (_model == null || _contentHash != AnalysisStorage.Digest(_model) || string.IsNullOrWhiteSpace(Id)
                || !Enum.IsDefined(typeof(ReinforcementAnalysisRole), ReinforcementRole)
                || ReinforcementRole != ReinforcementAnalysisRole.Unknown && string.IsNullOrWhiteSpace(AssumptionReference))
                throw new SerializationException("Invalid analysis snapshot.");
            var model = AnalysisStorage.Read<Models.Model>(_model);
            if (model.Analysis != null || model.VerificationContext != null || model.Datasets.Count != 0 || model.AllElements.Any(e => e.Results.Any())
                || model.Guid != ModelGuid || model.AnalysisFingerprint() != InputFingerprint
                || ModelArchive.Fingerprint(new object[] { model.AnalysisSource }) != SourceFingerprint
                || AnalysisStorage.Reinforcement(model) != ReinforcementFingerprint || AnalysisStorage.Prestress(model) != PrestressFingerprint)
                throw new SerializationException("Analysis snapshot inputs disagree with their provenance.");
            return model;
        }
    }

    /// <summary>Design alternatives keyed by persistent element GUID. Setters capture values; later caller edits do not change the scenario.</summary>
    [Serializable]
    public sealed class VerificationScenario
    {
        private readonly Dictionary<Guid, byte[]> _beams = new Dictionary<Guid, byte[]>();
        private readonly Dictionary<Guid, byte[]> _sections = new Dictionary<Guid, byte[]>();
        private readonly Dictionary<Guid, byte[]> _shells = new Dictionary<Guid, byte[]>();
        public string Id { get; private set; }
        public string AnalysisId { get; private set; }
        public string Name { get; private set; }
        private VerificationScenario() { }
        public static VerificationScenario Create(string analysisId, string name)
        {
            if (string.IsNullOrWhiteSpace(analysisId) || string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Analysis identity and scenario name required.");
            return new VerificationScenario { Id = Guid.NewGuid().ToString("D"), AnalysisId = analysisId, Name = name };
        }
        public string Fingerprint => ModelArchive.Fingerprint(new object[] { Id, AnalysisId, Name,
            _beams.OrderBy(p => p.Key).ToArray(), _sections.OrderBy(p => p.Key).ToArray(), _shells.OrderBy(p => p.Key).ToArray() });
        public void SetBeamDesign(Guid element, BeamProperty property)
        {
            if (element == Guid.Empty || property is null) throw new ArgumentException("Element GUID and beam property required.");
            _beams[element] = AnalysisStorage.Write(property);
        }
        public void SetBeamSections(Guid element, IEnumerable<BeamSectionAssignment> sections)
        {
            if (element == Guid.Empty || sections == null) throw new ArgumentException("Element GUID and section assignments required.");
            var items = sections.ToArray();
            if (items.Any(s => s == null)) throw new ArgumentException("Null section assignment.");
            _sections[element] = AnalysisStorage.Write(items);
        }
        public void SetShellDesign(Guid element, ShellAssignments assignments)
        {
            if (element == Guid.Empty || assignments == null) throw new ArgumentException("Element GUID and shell assignments required.");
            _shells[element] = AnalysisStorage.Write(assignments);
        }
        internal void Apply(Models.Model model)
        {
            foreach (var item in _beams)
            {
                var beam = model.BeamElements.Values.SingleOrDefault(b => b.Guid == item.Key)
                    ?? throw new ArgumentException("ScenarioBeamNotFound: " + item.Key);
                if (!_sections.ContainsKey(item.Key) && beam.Assignments.Sections.Count != 0
                    && (beam.Assignments.Sections.Count != 1 || beam.Assignments.Sections[0].Law != "Constant"
                        || beam.Assignments.Sections[0].Start != 0 || beam.Assignments.Sections[0].End != 1))
                    throw new ArgumentException("Variable sections require explicit SetBeamSections.");
                beam.BeamProperty = AnalysisStorage.Read<BeamProperty>(item.Value);
                if (!_sections.ContainsKey(item.Key) && beam.Assignments.Sections.Count == 1)
                {
                    // Replace both legacy and general slots without leaving conflicting definitions.
                    beam.Assignments.Sections[0].Section = null;
                    beam.Assignments.Sections[0].Property = beam.BeamProperty;
                }
            }
            foreach (var item in _sections)
            {
                var beam = model.BeamElements.Values.SingleOrDefault(b => b.Guid == item.Key)
                    ?? throw new ArgumentException("ScenarioBeamNotFound: " + item.Key);
                beam.Assignments.Sections.Clear(); beam.Assignments.Sections.AddRange(AnalysisStorage.Read<BeamSectionAssignment[]>(item.Value));
            }
            foreach (var item in _shells)
            {
                var shell = model.AreaElements.Values.SingleOrDefault(a => a.Guid == item.Key)
                    ?? throw new ArgumentException("ScenarioShellNotFound: " + item.Key);
                var value = AnalysisStorage.Read<ShellAssignments>(item.Value);
                shell.Assignments.PhysicalThickness = value.PhysicalThickness; shell.Assignments.Offset = value.Offset;
                shell.Assignments.LayerAxes = value.LayerAxes; shell.Assignments.ReinforcementZone = value.ReinforcementZone;
                shell.Assignments.Layers.Clear(); shell.Assignments.Layers.AddRange(value.Layers);
            }
        }
    }

    public sealed class AnalysisCompatibilityResult
    {
        public AnalysisCompatibility Status { get; }
        public string Code { get; }
        public string Message { get; }
        internal AnalysisCompatibilityResult(AnalysisCompatibility status, string code, string message)
        { Status = status; Code = code; Message = message; }
    }
    public static class AnalysisCompatibilityValidator
    {
        // Low-level legacy section preparation remains usable without a Model analysis record.
        // ModelChecker always requires the strict Validate gate, including for legacy models.
        internal static bool KnownAnalysisIsCompatible(Models.Model model) => model.Analysis == null
            || Validate(model).Status == AnalysisCompatibility.Compatible && (model.VerificationContext == null || model.VerificationContext.IsCurrent(model));
        internal static ModelDiagnostic KnownAnalysisDiagnostic(Models.Model model, out DataStatus status)
        {
            status = DataStatus.Insufficient;
            if (KnownAnalysisIsCompatible(model)) return null;
            var result = Validate(model);
            if (result.Status == AnalysisCompatibility.RequiresReanalysis || result.Status == AnalysisCompatibility.Compatible) status = DataStatus.Stale;
            return ModelDiagnostic.Error(result.Status == AnalysisCompatibility.Compatible ? "StalePreparedScenario" : result.Code, message: result.Message);
        }
        public static AnalysisCompatibilityResult Validate(Models.Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            try { return ValidateInputs(model); }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            { return Unknown("InvalidAnalysisInputs", ex.Message); }
        }
        private static AnalysisCompatibilityResult ValidateInputs(Models.Model model)
        {
            var analysis = model.Analysis;
            if (analysis == null) return Unknown("UnknownAnalysisProvenance", "Record the original analysis inputs before results; legacy provenance is insufficient for automatic verification.");
            if (model.Guid != analysis.ModelGuid) return Unknown("ForeignAnalysisSnapshot", "The snapshot belongs to another model.");
            if (ModelArchive.Fingerprint(new object[] { model.AnalysisSource }) != analysis.SourceFingerprint)
                return Unknown("AnalysisSourceChanged", "The solver provenance changed since the recorded analysis.");
            if (model.AnalysisFingerprint() != analysis.InputFingerprint)
                return new AnalysisCompatibilityResult(AnalysisCompatibility.RequiresReanalysis, "RequiresReanalysis", "Physical analysis inputs changed. Reanalyse the scenario before verification.");
            foreach (var pair in model.Datasets)
                if (pair.Value == null || pair.Key != pair.Value.Id || pair.Value.InputFingerprint != analysis.InputFingerprint
                    || pair.Value.AnalysisSnapshotId != null && pair.Value.AnalysisSnapshotId != analysis.Id)
                    return Unknown("DatasetAnalysisMismatch", "A dataset is not bound to the recorded analysis.");
            if (AnalysisStorage.Prestress(model) != analysis.PrestressFingerprint)
                return new AnalysisCompatibilityResult(AnalysisCompatibility.RequiresReanalysis, "PrestressRequiresReanalysis", "Prestressing changed and can change the applied actions.");
            if (AnalysisStorage.Reinforcement(model) != analysis.ReinforcementFingerprint)
            {
                if (analysis.ReinforcementRole == ReinforcementAnalysisRole.Unknown)
                    return Unknown("UnknownReinforcementInfluence", "The analysis does not declare whether reinforcement affects stiffness or response.");
                if (analysis.ReinforcementRole == ReinforcementAnalysisRole.IncludedInAnalysis)
                    return new AnalysisCompatibilityResult(AnalysisCompatibility.RequiresReanalysis, "ReinforcementRequiresReanalysis", "The analysis includes the modified reinforcement.");
            }
            return new AnalysisCompatibilityResult(AnalysisCompatibility.Compatible, "CompatibleAnalysis", "The scenario preserves the recorded analysis assumptions.");
        }
        private static AnalysisCompatibilityResult Unknown(string code, string message) => new AnalysisCompatibilityResult(AnalysisCompatibility.Unknown, code, message);
    }

    /// <summary>Immutable identities and value digests retained with each verification outcome.</summary>
    [Serializable]
    public sealed class VerificationProvenance
    {
        public string AnalysisSnapshotId { get; private set; }
        public string AnalysisFingerprint { get; private set; }
        public string ScenarioId { get; private set; }
        public string ScenarioFingerprint { get; private set; }
        public string DesignFingerprint { get; private set; }
        private Dictionary<string, string> _datasets;
        public IReadOnlyDictionary<string, string> DatasetFingerprints => new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(_datasets);
        internal static VerificationProvenance Capture(Models.Model model, VerificationScenario scenario = null) => new VerificationProvenance {
            AnalysisSnapshotId = model.Analysis?.Id, AnalysisFingerprint = model.Analysis?.InputFingerprint,
            ScenarioId = scenario?.Id ?? "current-model", ScenarioFingerprint = scenario?.Fingerprint ?? AnalysisStorage.Design(model),
            DesignFingerprint = AnalysisStorage.Design(model), _datasets = model.Datasets.ToDictionary(p => p.Key, p => ModelArchive.Fingerprint(new object[] { p.Value })) };
        public bool IsCurrent(Models.Model model)
        {
            try
            {
                return model != null && AnalysisSnapshotId != null && model.Analysis?.Id == AnalysisSnapshotId
                    && (!model.VerificationScenarios.TryGetValue(ScenarioId, out var scenario) || scenario.Fingerprint == ScenarioFingerprint)
                    && model.AnalysisFingerprint() == AnalysisFingerprint && AnalysisStorage.Design(model) == DesignFingerprint
                    && model.Datasets.Count == _datasets.Count && _datasets.All(p => model.Datasets.TryGetValue(p.Key, out var value)
                        && ModelArchive.Fingerprint(new object[] { value }) == p.Value)
                    && AnalysisCompatibilityValidator.Validate(model).Status == AnalysisCompatibility.Compatible;
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException || ex is NotSupportedException)
            { return false; }
        }
    }

    public sealed class PreparedVerification
    {
        /// <summary>Independent working graph. Mutations invalidate IsCurrent; the analysed graph is never exposed.</summary>
        public Models.Model Model { get; }
        public VerificationProvenance Provenance { get; }
        public AnalysisCompatibilityResult Compatibility { get; }
        private readonly string _results;
        public bool IsCurrent => Provenance.IsCurrent(Model) && _results == AnalysisStorage.Results(Model);
        internal PreparedVerification(Models.Model model, VerificationScenario scenario)
        {
            Model = model; Provenance = VerificationProvenance.Capture(model, scenario); model.VerificationContext = Provenance;
            Compatibility = AnalysisCompatibilityValidator.Validate(model); _results = AnalysisStorage.Results(model);
        }
    }
    public static class VerificationPreparation
    {
        /// <summary>The source supplies the samples, not just AnalysisDataset metadata. No result is rebound to the edited design.</summary>
        public static PreparedVerification Prepare(Models.Model source, VerificationScenario scenario)
        {
            if (source == null || scenario == null) throw new ArgumentNullException();
            if (source.Analysis == null || source.Analysis.Id != scenario.AnalysisId) throw new ArgumentException("ScenarioAnalysisMismatch");
            source.Analysis.OpenModel(); // Validate persisted snapshot before using its declaration.
            var model = AnalysisStorage.Read<Models.Model>(AnalysisStorage.Write(source.AnalysisArchiveView()));
            model.AttachAnalysis(source.Analysis);
            var frozenScenario = AnalysisStorage.Read<VerificationScenario>(AnalysisStorage.Write(scenario));
            model.VerificationScenarios.Add(frozenScenario.Id, frozenScenario);
            frozenScenario.Apply(model);
            return new PreparedVerification(model, frozenScenario);
        }
        public static VerificationProvenance CurrentProvenance(Models.Model model) => model.VerificationContext ?? VerificationProvenance.Capture(model);
    }
}
