using System;
using System.Threading;
using GPC.Model.Elements;
using GPC.Model.PostProcessing;
using GPC.Model.Sections.Concrete;

namespace GPC.Model.Checker
{
    /// <summary>Routes ordinary RC to the existing section adapter; step 4 capabilities are not implied.</summary>
    public sealed class ConcreteMaterialChecker : IMaterialChecker
    {
        public ConcreteVerificationOptions Options { get; }
        public string Id => "Concrete.Section";
        public string Version => NativeResults.Version(typeof(GPC.Checkers.Concrete.Checkers.SectionChecker));
        public string Configuration => Options.CreateVerifier().Configuration;
        public CheckStandardContext Standard => Options.CreateVerifier().StandardContext;
        public ConcreteMaterialChecker(ConcreteVerificationOptions options) { Options = options ?? throw new ArgumentNullException(nameof(options)); }
        public bool Accepts(BeamElement element) => element.BeamProperty is ReinforcedConcreteSection rc && rc.SteelSections.Count == 0;
        public IMaterialCheckSession CreateSession() => new Session(Options.CreateVerifier());
        private sealed class Session : ISectionCheckSession
        {
            private readonly ConcreteSectionVerifier _verifier;
            internal Session(ConcreteSectionVerifier verifier) { _verifier = verifier; }
            public int CreatedCheckers => _verifier.CreatedCheckers;
            public CheckResult Verify(BeamActionInput input, CheckMechanism mechanism, CancellationToken token)
                => Verification.Run(Verification.PrepareBeam(input.Model, input.Element.Id, input.Sample, input.Settings), mechanism, _verifier, token);
            public bool Supports(SectionCheckSpecification check) => _verifier.Supports(check);
            public CheckResult Verify(BeamActionInput input, SectionCheckSpecification check, CancellationToken token)
                => Verification.Run(Verification.PrepareBeam(input.Model, input.Element.Id, input.Sample, input.Settings), check, _verifier, token);
        }
    }
}
