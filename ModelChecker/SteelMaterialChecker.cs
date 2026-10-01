using System;
using System.Collections.Generic;
using System.Threading;
using GPC.Checkers.Steel.Checkers;
using GPC.Model.Elements;
using GPC.Model.Persistence;
using GPC.Model.PostProcessing;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using GPC.Model.Standards;

namespace GPC.Model.Checker
{
    /// <summary>Qualified native plastic shear primitives on stocky symmetric H sections, T=0.
    /// Global buckling and cross-section interactions remain gated by the unresolved legacy regressions.</summary>
    public sealed class SteelMaterialChecker : IMaterialChecker
    {
        public StandardEN1993p11 Code { get; }
        public string Edition { get; }
        public string NationalAnnex { get; }
        public string Id => "Steel.EN1993.PlasticShear";
        public string Version => NativeResults.Version(typeof(EN1993p11Checker));
        public string Configuration => ModelArchive.Fingerprint(new object[] { Id, Code, Edition, NationalAnnex });
        public CheckStandardContext Standard => new CheckStandardContext(Code.Name, Edition, NationalAnnex, Id, Configuration,
            "Native CalculateVcRd1/CalculateVcRd2; section shear only, no torsion or global stability.");
        public SteelMaterialChecker(StandardEN1993p11 code, string edition, string nationalAnnex = null)
        {
            Code = code ?? throw new ArgumentNullException(nameof(code));
            if (string.IsNullOrWhiteSpace(edition)) throw new ArgumentException("ExplicitSteelEditionRequired");
            Edition = edition; NationalAnnex = nationalAnnex;
        }
        public bool Accepts(BeamElement element) => element.BeamProperty is SteelSection;
        public IMaterialCheckSession CreateSession() => new Session(this);
        private sealed class NativeShear : EN1993p11Checker
        {
            // These native constructor fields are not consumed by the section-only primitives below.
            internal NativeShear(BeamElement beam, StandardEN1993p11 code) : base(beam, new EN1993p11Options(
                EN1993p11Options.LoadConditions.Constant, EN1993p11Options.SupportConditions.HingesAtEnds,
                EN1993p11Options.LateralSupportConditions.HingesAtEnds, EN1993p11Options.LateralWarpingConditions.HingesAtEnds, null, null), code) { }
            internal double Capacity1(ISteelSection section) => CalculateVcRd1(section);
            internal double Capacity2(ISteelSection section) => CalculateVcRd2(section);
            internal double FullCapacity2(GPC.Model.Results.ResultBeamForces forces, ISteelSection section) => CalculateShear2Capacity(forces, section);
            internal SectionClass CompressionClass(ISteelSection section) => CalculateSectionClassDueToCompression(new GPC.Model.Results.ResultBeamForces(-1, 0, 0, 0, 0, 0, GPC.Geometry.CoordinateSystem.Global), section);
        }
        private sealed class Session : IMaterialCheckSession
        {
            private readonly SteelMaterialChecker _owner;
            private readonly Dictionary<string, NativeShear> _native = new Dictionary<string, NativeShear>();
            public int CreatedCheckers => _native.Count;
            internal Session(SteelMaterialChecker owner) { _owner = owner; }
            public CheckResult Verify(BeamActionInput input, CheckMechanism mechanism, CancellationToken token)
            {
                token.ThrowIfCancellationRequested();
                if (_owner.Edition != "2005") return NativeResults.Unavailable("SteelEditionNotQualified");
                if (mechanism != CheckMechanism.Shear) return NativeResults.Unavailable("SteelMethodNotQualified", "EN1993 legacy stability/classification/interaction regressions remain open; this adapter enables only the qualified shear primitives.");
                var section = input.Element.BeamProperty as SteelSection;
                if (!(section?.SectionShape is SectionH h) || h.GetType() != typeof(SectionH) || input.Element.Assignments.Sections.Count != 0)
                    return NativeResults.Unavailable("SteelRequiresConstantHSection");
                if (h.LenghtTopFlange != h.LenghtBottomFlange || h.ThicknessTopFlange != h.ThicknessBottomFlange)
                    return NativeResults.Unavailable("SteelAsymmetricSectionNotQualified");
                if (_owner.Code.GetType() != typeof(StandardEN1993p11)) return NativeResults.Unavailable("SteelStandardVariantNotQualified");
                if (!(_owner.Code.GammaM0 > 0) || double.IsInfinity(_owner.Code.GammaM0)) return NativeResults.Missing("InvalidGammaM0");
                if (input.Forces.T != 0) return NativeResults.Unavailable("SteelTorsionInteractionNotQualified");
                // The native compressed-section classification provides a conservative stockiness gate for both flange directions.
                string key = input.Snapshot.SectionFingerprint;
                if (!_native.TryGetValue(key, out var checker))
                {
                    checker = new NativeShear(new BeamElement(input.Element.StartPoint, input.Element.EndPoint, section), _owner.Code);
                    _native.Add(key, checker);
                }
                if (checker.CompressionClass(section) > EN1993p11Checker.SectionClass.Class2) return NativeResults.Unavailable("SteelSlenderSectionNotQualified");
                double v1 = checker.Capacity1(section), v2 = checker.Capacity2(section);
                if (Math.Abs(checker.FullCapacity2(input.Forces, section) - v2) > 1e-9 * Math.Max(1, v2)) return NativeResults.Unavailable("SteelShearBucklingNotQualified");
                var metrics = new[] { new CheckMetric("V1", input.Forces.V1, v1, "N", Math.Abs(input.Forces.V1) / v1),
                    new CheckMetric("V2", input.Forces.V2, v2, "N", Math.Abs(input.Forces.V2) / v2) };
                return NativeResults.Decision(new NativeMethodDetails(_owner.Id, "Two individual section shear checks. Does not certify N-M-V interaction or member stability.", key, metrics,
                    trace: new[] { new CheckCalculationValue("GammaM0", _owner.Code.GammaM0, "1", "Explicit code parameter"),
                        new CheckCalculationValue("Fy", section.SteelMaterial.Fyk, "MPa", "Model steel material") },
                    warnings: new[] { "R02: legacy EN1993 suite has unresolved failures. R03 bulk-result indexing path is not called.",
                        "The native CalculateVcRd1 primitive includes GammaM0; the different legacy CalculateShear1Capacity path is not used." }));
            }
        }
    }
}
