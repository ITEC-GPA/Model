using System;
using GPC.Model.Collections;
using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using GPC.Model.Standards;

namespace GPC.Model.Design.Combinations
{
    /// <summary>Generates design combinations using an explicitly selected normative policy.
    /// Result algebra only consumes the resulting coefficients; it never chooses a standard.</summary>
    public static class NormativeCombinations
    {
        public static UniqueNameCollection<Combination> Generate(Standard.ICombinationsGenerator policy,
            LoadCaseBase[] loadCases, Standard.CombinationsOptions options, string prefix = "cmb")
        {
            if (policy == null) throw new ArgumentNullException(nameof(policy));
            if (loadCases == null) throw new ArgumentNullException(nameof(loadCases));
            if (options == null) throw new ArgumentNullException(nameof(options));
            return policy.CreateCombinations(loadCases, options, prefix);
        }
    }
}
