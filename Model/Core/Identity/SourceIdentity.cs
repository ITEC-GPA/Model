using GPC.Model.Results.Processing;
using System;
using System.Collections.Generic;
using GPC.Model.Elements;

namespace GPC.Model.PostProcessing
{

    [Serializable]
    public sealed class SourceIdentity : IEquatable<SourceIdentity>
    {
        public string Program { get; private set; }
        public string ModelRevision { get; private set; }
        public EntityFamily Family { get; private set; }
        public string OriginalId { get; private set; }
        public SourceIdentity(string program, string modelRevision, EntityFamily family, string originalId)
        {
            if (string.IsNullOrWhiteSpace(program) || string.IsNullOrWhiteSpace(modelRevision) || string.IsNullOrWhiteSpace(originalId))
                throw new ArgumentException("Source program, model/revision and original ID are required.");
            Program = program; ModelRevision = modelRevision; Family = family; OriginalId = originalId;
        }
        public bool Equals(SourceIdentity other) => other != null && Program == other.Program && ModelRevision == other.ModelRevision && Family == other.Family && OriginalId == other.OriginalId;
        public override bool Equals(object obj) => Equals(obj as SourceIdentity);
        public override int GetHashCode() => Program.GetHashCode() ^ ModelRevision.GetHashCode() ^ (int)Family ^ OriginalId.GetHashCode();
    }
}
