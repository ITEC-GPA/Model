using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;

namespace GPC.Model.Models
{
    /// <summary>Value revisions, independent of counters on legacy mutable setters. Captured at edit boundaries, never per solver iteration.</summary>
    public sealed class ModelRevision
    {
        /// <summary>Exact document token for optimistic conflict detection on the same source instance.
        /// Geometry serializers may regenerate internal GUIDs in copies; compare Analysis/Verification for semantic changes.</summary>
        public string Document { get; }
        public string Analysis { get; }
        public string Verification { get; }
        private ModelRevision(string document, string analysis, string verification)
        { Document = document; Analysis = analysis; Verification = verification; }
        public static ModelRevision Capture(Model model)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            string analysis = model.AnalysisFingerprint();
            string verification = ModelArchive.Fingerprint(new object[] { "VerificationInputs-v1", model.VerificationFingerprint(null),
                model.BeamElements.Values.Select(b => b.BeamProperty).ToArray(), model.PhysicalMembers, model.Datasets,
                model.VerificationContext, model.VerificationScenarios,
                model.AllElements.Select(e => new object[] { e.Id, e.Guid, e.Groups.Keys.OrderBy(k => k, StringComparer.Ordinal).ToArray(), e.Results.ToArray() }).ToArray() });
            using (var stream = new MemoryStream())
            using (var sha = SHA256.Create())
            {
                ModelArchive.SaveDocument(model, stream);
                return new ModelRevision(BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-", ""), analysis, verification);
            }
        }
    }
    public sealed class ModelEditResult
    {
        public Model Model { get; }
        public ModelRevision Before { get; }
        public ModelRevision After { get; }
        /// <summary>Comparison of the structural input fingerprints, not permission to reuse FEM results.</summary>
        public bool AnalysisChanged => Before.Analysis != After.Analysis;
        public bool VerificationChanged => Before.Verification != After.Verification;
        /// <summary>Compatibility at publication, including the recorded influence of reinforcement on FEM analysis.</summary>
        public AnalysisCompatibilityResult AnalysisCompatibilityAtCommit { get; }
        internal ModelEditResult(Model model, ModelRevision before, ModelRevision after)
        {
            Model = model; Before = before; After = after;
            AnalysisCompatibilityAtCommit = AnalysisCompatibilityValidator.Validate(model);
        }
    }
    /// <summary>Optimistic copy-on-commit edit. Commit returns a replacement document; it never mutates the source.
    /// Use one owner for document replacement. Uncoordinated concurrent writes through legacy setters are not made thread-safe by this API.</summary>
    public sealed class ModelEditSession : IDisposable
    {
        private readonly Model _source;
        private readonly ModelRevision _baseline;
        private bool _closed;
        public Model Draft { get; }
        public ModelRevision Baseline => _baseline;
        public ModelEditSession(Model source)
        {
            _source = source ?? throw new ArgumentNullException(nameof(source));
            _baseline = ModelRevision.Capture(source); Draft = ModelArchive.Copy(source);
            RequireCurrentSource();
        }
        private void RequireCurrentSource()
        {
            if (ModelRevision.Capture(_source).Document != _baseline.Document)
                throw new InvalidOperationException("ModelEditConflict: source document changed.");
        }
        private void RequireOpen() { if (_closed) throw new InvalidOperationException("ModelEditSessionClosed"); }
        public IReadOnlyList<ModelDiagnostic> Validate()
        {
            RequireOpen();
            var diagnostics = Draft.ValidateTopology().Concat(Draft.ValidateAssignments()).ToList();
            foreach (var entry in Draft.PhysicalMembers)
            {
                if (entry.Value == null || entry.Key != entry.Value.Id) { diagnostics.Add(ModelDiagnostic.Error("InvalidPhysicalMemberIdentity")); continue; }
                try { new PhysicalMemberGeometry(Draft, entry.Value); }
                catch (Exception ex) when (ex is ArgumentException || ex is NotSupportedException || ex is InvalidOperationException)
                { diagnostics.Add(ModelDiagnostic.Error("InvalidPhysicalMember", message: entry.Key + ": " + ex.Message)); }
            }
            return diagnostics.AsReadOnly();
        }
        public ModelEditResult Commit()
        {
            RequireOpen(); RequireCurrentSource();
            var errors = Validate().Where(d => d.Severity == DiagnosticSeverity.Error).ToArray();
            if (errors.Length != 0) throw new InvalidOperationException("InvalidModelEdit: " + string.Join(", ", errors.Select(d => d.Code)));
            // A retained Draft reference cannot modify the published document after commit.
            var published = ModelArchive.Copy(Draft); var revision = ModelRevision.Capture(published);
            RequireCurrentSource(); _closed = true;
            return new ModelEditResult(published, _baseline, revision);
        }
        public void Dispose() { _closed = true; }
    }
}
