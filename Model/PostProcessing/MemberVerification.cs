using System.Threading;
namespace GPC.Model.PostProcessing
{
    /// <summary>Compatibility facade over the member execution service.</summary>
    public static class MemberVerification
    {
        public static CheckResult Run(BeamCheckPlan plan, CheckWorkItem item, IPhysicalMemberVerifier verifier, CancellationToken token = default(CancellationToken))
            => Checking.MemberCheckExecution.Run(plan, item, verifier, token);
    }
}