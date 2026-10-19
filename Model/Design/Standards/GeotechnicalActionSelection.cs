using System;

namespace GPC.Model.Standards
{
    /// <summary>Characteristic action category; the normative factor is selected by the specific check and situation.</summary>
    public enum GeotechnicalActionCategory { PermanentStructural, PermanentNonStructural, Variable }

    /// <summary>Selection of one characteristic action in a combination. CombinationFactor is an explicitly sourced psi/group factor,
    /// before the partial action factor; it is not an already factored design action. Favourability is supplied for that combination.</summary>
    public sealed class GeotechnicalActionSelection
    {
        public string Id { get; }
        public GeotechnicalActionCategory Category { get; }
        public bool Unfavourable { get; }
        public double CombinationFactor { get; }
        public string Source { get; }
        public GeotechnicalActionSelection(string id, GeotechnicalActionCategory category, bool unfavourable, double combinationFactor, string source)
        {
            if (string.IsNullOrWhiteSpace(id) || !Enum.IsDefined(typeof(GeotechnicalActionCategory), category) || string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Action identifier, valid category and source required.");
            if (double.IsNaN(combinationFactor) || double.IsInfinity(combinationFactor) || combinationFactor < 0 || combinationFactor > 1)
                throw new ArgumentOutOfRangeException(nameof(combinationFactor), "Combination factor must lie between zero and one.");
            if (category != GeotechnicalActionCategory.Variable && combinationFactor != 1)
                throw new ArgumentException("Permanent actions use a combination factor of one; their favourable factor comes from the standard.");
            Id = id; Category = category; Unfavourable = unfavourable; CombinationFactor = combinationFactor; Source = source;
        }
    }
}
