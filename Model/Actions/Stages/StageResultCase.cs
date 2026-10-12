using System;
using System.Runtime.Serialization;
using GPC.Model.LoadCases;

namespace GPC.Model.Stages
{
    /// <summary>A solver output case in the construction-stage scope. It is not a static load definition or a case per step.</summary>
    [Serializable]
    public sealed class StageResultCase : LoadCaseBase
    {
        public string SourceCaseName { get; }
        public StageResultCase(string name, string sourceCaseName) : base(name)
        {
            if (string.IsNullOrWhiteSpace(sourceCaseName)) throw new ArgumentException("Explicit source result case required.", nameof(sourceCaseName));
            SourceCaseName = sourceCaseName;
        }
        private StageResultCase(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            SerializationVersions.Read(info, "StageResultCaseVersion", 1);
            SourceCaseName = info.GetString("SourceCaseName");
            if (string.IsNullOrWhiteSpace(SourceCaseName)) throw new SerializationException("Missing stage source case.");
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        { base.GetObjectData(info, context); info.AddValue("StageResultCaseVersion", 1); info.AddValue("SourceCaseName", SourceCaseName); }
    }
}
