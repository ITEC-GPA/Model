using System;
using System.Linq;
using System.Threading;

namespace GPC.Model.PostProcessing
{
    public static class MemberVerification
    {
        public static CheckResult Run(BeamCheckPlan plan, CheckWorkItem item, IPhysicalMemberVerifier verifier, CancellationToken token = default(CancellationToken))
        {
            if (plan == null || item?.Scope != CheckScope.PhysicalMember || !plan.WorkItems.Contains(item)) throw new ArgumentException("ForeignMemberWorkItem");
            var result = new CheckResult { Data = item.Data };
            result.Diagnostics.AddRange(item.Diagnostics);
            if (token.IsCancellationRequested) { result.Execution = ExecutionStatus.Cancelled; return result; }
            if (!plan.IsCurrent) { result.Data = DataStatus.Stale; result.Diagnostics.Add(ModelDiagnostic.Error("StaleMemberPlan")); return result; }
            try
            {
                if (verifier == null || !verifier.Supports(item.MethodId, item.Mechanism))
                { result.Data = DataStatus.NotSupported; result.Diagnostics.Add(ModelDiagnostic.Error("MemberCheckerUnavailable", message: item.MethodId)); return result; }
                result.EngineVersion = verifier.Version; result.EngineConfiguration = verifier.Configuration; result.Standard = verifier.Standard;
                if (item.Data != DataStatus.Ready) return result;
                var evaluated = verifier.Verify(item.Member, token);
                token.ThrowIfCancellationRequested();
                if (!plan.IsCurrent || result.EngineVersion != verifier.Version || result.EngineConfiguration != verifier.Configuration
                    || result.Standard?.Identity != verifier.Standard?.Identity)
                { result.Data = DataStatus.Stale; result.Diagnostics.Add(ModelDiagnostic.Error("MemberInputsChangedDuringVerification")); return result; }
                if (!CheckResultRules.ValidDecision(evaluated)) throw new InvalidOperationException("Invalid member verifier outcome.");
                evaluated.EngineVersion = result.EngineVersion; evaluated.EngineConfiguration = result.EngineConfiguration; evaluated.Standard = result.Standard;
                return evaluated;
            }
            catch (OperationCanceledException) { result.Execution = ExecutionStatus.Cancelled; }
            catch (Exception ex) { result.Execution = ExecutionStatus.Error; result.Diagnostics.Add(ModelDiagnostic.Error("MemberVerifierError", message: ex.Message)); }
            return result;
        }
    }
}
