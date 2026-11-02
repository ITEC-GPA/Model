using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.Elements;

namespace GPC.Model.Core.Identity
{
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class AssemblyIdentityEntry
    {
        [DataMember(Order = 0)] public Guid InputGuid { get; private set; }
        [DataMember(Order = 1)] public Guid CanonicalGuid { get; private set; }
        [DataMember(Order = 2)] public EntityFamily Family { get; private set; }
        [DataMember(Order = 3)] public int ModelId { get; private set; }
        [DataMember(Order = 4)] public int OriginalModelId { get; private set; }
        [DataMember(Order = 5)] public SourceIdentity Source { get; private set; }
        internal AssemblyIdentityEntry(Element input, Element canonical, EntityFamily family)
        { InputGuid = input.Guid; CanonicalGuid = canonical.Guid; Family = family; ModelId = canonical.Id; OriginalModelId = input.Id; Source = input.Source; }
    }

    /// <summary>Persistent assembly provenance, including every coincident node merged into
    /// a canonical entity. The model ID is a registry key, never a solver ID.</summary>
    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class AssemblyIdentityMap
    {
        [DataMember(Order = 0)] private AssemblyIdentityEntry[] entries;
        [DataMember(Order = 1)] public Guid ModelGuid { get; private set; }
        [DataMember(Order = 2)] public double? MergeToleranceMm { get; private set; }
        public IReadOnlyList<AssemblyIdentityEntry> Entries => Array.AsReadOnly(entries);
        internal AssemblyIdentityMap(Guid modelGuid, double? tolerance, IEnumerable<AssemblyIdentityEntry> entries)
        { ModelGuid = modelGuid; MergeToleranceMm = tolerance; this.entries = entries.OrderBy(e => e.InputGuid).ToArray(); Validate(); }
        [OnDeserialized] private void OnRead(StreamingContext context) => Validate();
        public void Validate()
        {
            if (ModelGuid == Guid.Empty || MergeToleranceMm.HasValue && (double.IsNaN(MergeToleranceMm.Value) || double.IsInfinity(MergeToleranceMm.Value) || MergeToleranceMm < 0) || entries == null)
                throw new SerializationException("InvalidAssemblyIdentityMap");
            var identities = new Dictionary<Guid, AssemblyIdentityEntry>();
            foreach (var entry in entries)
            {
                if (entry == null || entry.InputGuid == Guid.Empty || entry.CanonicalGuid == Guid.Empty || entry.ModelId <= 0 || !Enum.IsDefined(typeof(EntityFamily), entry.Family) || identities.ContainsKey(entry.InputGuid))
                    throw new SerializationException("InvalidAssemblyIdentityEntry");
                identities.Add(entry.InputGuid, entry);
            }
            var keys = new HashSet<(EntityFamily, int)>();
            foreach (var entry in entries)
            {
                if (!identities.TryGetValue(entry.CanonicalGuid, out var canonical) || canonical.CanonicalGuid != canonical.InputGuid || canonical.Family != entry.Family || canonical.ModelId != entry.ModelId)
                    throw new SerializationException("MissingCanonicalAssemblyIdentity");
                if (entry.InputGuid == entry.CanonicalGuid && !keys.Add((entry.Family, entry.ModelId))) throw new SerializationException("DuplicateAssemblyModelId");
            }
        }
        /// <summary>Requires exactly one positive solver ID per canonical entity. Families
        /// have separate numbering spaces; the structural input fingerprint binds the map to a run.</summary>
        public SolverIdentityMap ToSolver(Models.Model model, string solver, IEnumerable<KeyValuePair<Guid, int>> ids)
        {
            if (model == null) throw new ArgumentNullException(nameof(model));
            Validate();
            var current = model.AllElements.ToDictionary(e => e.Guid);
            var canonical = entries.Where(e => e.InputGuid == e.CanonicalGuid).ToArray();
            if (model.Guid != ModelGuid || current.Count != canonical.Length || canonical.Any(e => !current.TryGetValue(e.CanonicalGuid, out var entity) || entity.Id != e.ModelId ||
                (entity is NodeElement ? EntityFamily.Node : entity is BeamElement ? EntityFamily.Beam : entity is AreaElement ? EntityFamily.Shell : EntityFamily.Solid) != e.Family))
                throw new InvalidOperationException("StaleAssemblyIdentityMap: reassemble before export.");
            return new SolverIdentityMap(this, solver, Models.ModelRevisions.AnalysisFingerprint(model), ids);
        }
    }

    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class SolverIdentityEntry
    {
        [DataMember(Order = 0)] public Guid EntityGuid { get; private set; }
        [DataMember(Order = 1)] public EntityFamily Family { get; private set; }
        [DataMember(Order = 2)] public int ModelId { get; private set; }
        [DataMember(Order = 3)] public int SolverId { get; private set; }
        internal SolverIdentityEntry(AssemblyIdentityEntry entry, int solverId)
        { EntityGuid = entry.CanonicalGuid; Family = entry.Family; ModelId = entry.ModelId; SolverId = solverId; }
    }

    [Serializable, DataContract(Namespace = "http://schemas.datacontract.org/2004/07/GPC.Model.PostProcessing")]
    public sealed class SolverIdentityMap
    {
        [DataMember(Order = 0)] public Guid ModelGuid { get; private set; }
        [DataMember(Order = 1)] public string Solver { get; private set; }
        [DataMember(Order = 2)] public string AnalysisFingerprint { get; private set; }
        [DataMember(Order = 3)] private SolverIdentityEntry[] entries;
        public IReadOnlyList<SolverIdentityEntry> Entries => Array.AsReadOnly(entries);
        internal SolverIdentityMap(AssemblyIdentityMap map, string solver, string fingerprint, IEnumerable<KeyValuePair<Guid, int>> ids)
        {
            map.Validate(); var supplied = (ids ?? throw new ArgumentNullException(nameof(ids))).ToDictionary(p => p.Key, p => p.Value);
            ModelGuid = map.ModelGuid; Solver = solver; AnalysisFingerprint = fingerprint;
            var canonical = map.Entries.Where(e => e.InputGuid == e.CanonicalGuid).ToArray();
            if (supplied.Count != canonical.Length || canonical.Any(e => !supplied.ContainsKey(e.CanonicalGuid))) throw new ArgumentException("IncompleteSolverIdentityMap");
            entries = canonical.Select(e => new SolverIdentityEntry(e, supplied[e.CanonicalGuid])).ToArray(); Validate();
        }
        [OnDeserialized] private void OnRead(StreamingContext context) => Validate();
        public void Validate()
        {
            if (ModelGuid == Guid.Empty || string.IsNullOrWhiteSpace(Solver) || string.IsNullOrWhiteSpace(AnalysisFingerprint) || entries == null)
                throw new SerializationException("InvalidSolverIdentityMap");
            var guids = new HashSet<Guid>(); var modelIds = new HashSet<(EntityFamily, int)>(); var solverIds = new HashSet<(EntityFamily, int)>();
            foreach (var entry in entries)
                if (entry == null || entry.EntityGuid == Guid.Empty || entry.ModelId <= 0 || entry.SolverId <= 0 || !Enum.IsDefined(typeof(EntityFamily), entry.Family) ||
                    !guids.Add(entry.EntityGuid) || !modelIds.Add((entry.Family, entry.ModelId)) || !solverIds.Add((entry.Family, entry.SolverId)))
                    throw new SerializationException("InvalidSolverIdentityEntry");
        }
    }
}
