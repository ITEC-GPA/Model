using System;
using System.Runtime.Serialization;
using GPC.Model.Checking.Contracts;

namespace GPC.Model.Checking.Reports
{
    /// <summary>Readable, immutable specification of a shell task, independent of runtime checker types.</summary>
    [Serializable]
    public sealed class ShellCheckSnapshot : ISerializable
    {
        public string Id { get; }
        public string MethodId { get; }
        public CheckMechanism Mechanism { get; }
        public CombinationCategory RequestedCategory { get; }
        public CombinationCategory? ResultCategory { get; }
        public SectionCheckDirection Direction { get; }
        public string PhysicalFace { get; }
        /// <summary>+1 or -1 relative to the normal of the prepared coordinate system; null when no face is requested.</summary>
        public int? FaceNormalSign { get; }
        public bool ReinforcementRequired { get; }
        public ShellCheckSnapshot(string id, string methodId, CheckMechanism mechanism, CombinationCategory requestedCategory,
            CombinationCategory? resultCategory, SectionCheckDirection direction, string physicalFace, int? faceNormalSign, bool reinforcementRequired)
        {
            CheckValue.Text(id, nameof(id)); CheckValue.Text(methodId, nameof(methodId));
            if (!Enum.IsDefined(typeof(CheckMechanism), mechanism) || !Enum.IsDefined(typeof(CombinationCategory), requestedCategory)
                || resultCategory.HasValue && !Enum.IsDefined(typeof(CombinationCategory), resultCategory.Value)
                || !Enum.IsDefined(typeof(SectionCheckDirection), direction)
                || (physicalFace != null) != faceNormalSign.HasValue || physicalFace != null && string.IsNullOrWhiteSpace(physicalFace)
                || faceNormalSign.HasValue && faceNormalSign != 1 && faceNormalSign != -1)
                throw new ArgumentException("InvalidShellCheckSpecification");
            Id = id; MethodId = methodId; Mechanism = mechanism; RequestedCategory = requestedCategory; ResultCategory = resultCategory;
            Direction = direction; PhysicalFace = physicalFace; FaceNormalSign = faceNormalSign; ReinforcementRequired = reinforcementRequired;
        }
        private ShellCheckSnapshot(SerializationInfo info, StreamingContext context)
            : this(info.GetString("Id"), info.GetString("Method"), (CheckMechanism)info.GetValue("Mechanism", typeof(CheckMechanism)),
                (CombinationCategory)info.GetValue("RequestedCategory", typeof(CombinationCategory)),
                (CombinationCategory?)info.GetValue("ResultCategory", typeof(CombinationCategory?)),
                (SectionCheckDirection)info.GetValue("Direction", typeof(SectionCheckDirection)), info.GetString("Face"),
                (int?)info.GetValue("NormalSign", typeof(int?)), info.GetBoolean("ReinforcementRequired")) { }
        public void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Id", Id); info.AddValue("Method", MethodId); info.AddValue("Mechanism", Mechanism);
            info.AddValue("RequestedCategory", RequestedCategory); info.AddValue("ResultCategory", ResultCategory);
            info.AddValue("Direction", Direction); info.AddValue("Face", PhysicalFace); info.AddValue("NormalSign", FaceNormalSign);
            info.AddValue("ReinforcementRequired", ReinforcementRequired);
        }
    }
}
