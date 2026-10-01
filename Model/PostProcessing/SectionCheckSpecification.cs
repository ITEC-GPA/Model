using System;
using System.Linq;

namespace GPC.Model.PostProcessing
{
    /// <summary>Combination category declared for a result selection. Never inferred from case names.</summary>
    public enum CombinationCategory { Unspecified, Ultimate, UltimateSeismic, UltimateAccidental, Characteristic, Frequent, QuasiPermanent }

    /// <summary>Local section axis of a directional check (shear V1 or V2, in the section axes of the prepared input).</summary>
    public enum SectionCheckDirection { None, Axis1, Axis2 }

    /// <summary>Sub-check of a mechanism. Serviceability stresses and crack widths are distinct tasks.</summary>
    public enum SectionCheckCriterion { Default, StressLimits, CrackWidth }

    /// <summary>
    /// Typed discriminators of a local section task. Two directions or two serviceability sub-checks are never
    /// collapsed into the same task. The category selects the result selections declared with that category.
    /// </summary>
    [Serializable]
    public sealed class SectionCheckSpecification
    {
        public CheckMechanism Mechanism { get; set; }
        public SectionCheckDirection Direction { get; set; }
        public SectionCheckCriterion Criterion { get; set; }
        public CombinationCategory Category { get; set; }

        public SectionCheckSpecification() { }
        public SectionCheckSpecification(CheckMechanism mechanism, CombinationCategory category,
            SectionCheckDirection direction = SectionCheckDirection.None, SectionCheckCriterion criterion = SectionCheckCriterion.Default)
        { Mechanism = mechanism; Category = category; Direction = direction; Criterion = criterion; }

        /// <summary>Stable readable key, for example "Shear/Axis1/Default/Ultimate".</summary>
        public string Key => Mechanism + "/" + Direction + "/" + Criterion + "/" + Category;

        public static SectionCheckSpecification ShearAxis1() => new SectionCheckSpecification(CheckMechanism.Shear, CombinationCategory.Ultimate, SectionCheckDirection.Axis1);
        public static SectionCheckSpecification ShearAxis2() => new SectionCheckSpecification(CheckMechanism.Shear, CombinationCategory.Ultimate, SectionCheckDirection.Axis2);
        public static SectionCheckSpecification StressLimits(CombinationCategory category)
            => new SectionCheckSpecification(CheckMechanism.Serviceability, category, criterion: SectionCheckCriterion.StressLimits);

        public SectionCheckSpecification Copy() => (SectionCheckSpecification)MemberwiseClone();

        private static readonly CombinationCategory[] UltimateCategories = { CombinationCategory.Ultimate, CombinationCategory.UltimateSeismic, CombinationCategory.UltimateAccidental };
        private static readonly CombinationCategory[] ServiceCategories = { CombinationCategory.Characteristic, CombinationCategory.Frequent, CombinationCategory.QuasiPermanent };

        /// <summary>Rejects incoherent combinations of discriminators; it does not state that an engine supports the task.</summary>
        internal void Validate()
        {
            if (!Enum.IsDefined(typeof(CheckMechanism), Mechanism) || !Enum.IsDefined(typeof(SectionCheckDirection), Direction)
                || !Enum.IsDefined(typeof(SectionCheckCriterion), Criterion) || !Enum.IsDefined(typeof(CombinationCategory), Category))
                throw new ArgumentException("InvalidSectionCheckSpecification");
            bool directional = Mechanism == CheckMechanism.Shear;
            if (directional != (Direction != SectionCheckDirection.None)) throw new ArgumentException("SectionCheckDirection: " + Key);
            if ((Mechanism == CheckMechanism.Serviceability) != (Criterion != SectionCheckCriterion.Default)) throw new ArgumentException("SectionCheckCriterion: " + Key);
            switch (Mechanism)
            {
                case CheckMechanism.UlsBiaxialSection:
                case CheckMechanism.Shear:
                case CheckMechanism.Torsion:
                    if (!UltimateCategories.Contains(Category)) throw new ArgumentException("UltimateCategoryRequired: " + Key); break;
                case CheckMechanism.Serviceability:
                    if (!ServiceCategories.Contains(Category)) throw new ArgumentException("ServiceabilityCategoryRequired: " + Key); break;
            }
        }
    }
}
