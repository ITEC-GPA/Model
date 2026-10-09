using System.Threading;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Execution;
using GPC.Model.Checking.Preparation;
namespace GPC.Model.Compatibility
{
    /// <summary>Compatibility facade over the member execution service.</summary>
    public static class MemberVerification
    {
        public static CheckResult Run(BeamCheckPlan plan, CheckWorkItem item, IPhysicalMemberVerifier verifier, CancellationToken token = default(CancellationToken))
            => global::GPC.Model.Checking.Execution.MemberCheckExecution.Run(plan, item, verifier, token);
    }
}