using GPC.Checkers.Concrete.Analysis;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Examples;
using GPC.Model.Checker;
using GPC.Model.PostProcessing;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ModelChecker.Tests;

[TestClass]
public class CalculationFactoryTest
{
    private sealed class Alternative : IConcreteCalculationFactory, ISectionResponseSolver, ISectionResistanceSolver
    {
        public string Id => "TEST-NUMERICAL-PORT";
        public string Version => "test1";
        public string Configuration => "contract-test-only";
        public int Calls, Sessions;
        public bool Converged = true;
        public ConcreteCalculationSession Create(ReinforcedConcreteSection section, StandardModelCode2010 standard, ConcreteCalculationOptions options)
        { Sessions++; return new(this, this); }
        public SectionResponse Solve(SectionAnalysisInput input, CancellationToken token) => throw new NotSupportedException();
        public SectionResistanceResponse SolveResistance(SectionAnalysisInput input, CancellationToken token)
        {
            Calls++;
            return Converged
                ? new(new(CalculationStatus.Completed, Id, Version), "ConstantN", new(0, 0, -.001, 0, 0), -500000, 1e8, 0, .9, "test")
                : new(new(CalculationStatus.NotConverged, Id, Version), "ConstantN");
        }
    }
    [TestMethod]
    public void AlternativeCalculationUsesExistingMethodAndKeepsFailureUnevaluated()
    {
        var model = MixedModelFactory.Create(); var factory = new Alternative();
        var verifier = new ConcreteSectionVerifier(factory, new StandardNTC2018Concrete(), SectionSolver.FailureAnalysisTypes.ConstantN,
            false, 32, 0, 0, "2018", null, SectionSolver.StressAnalysisTypes.Linear, 1);
        var prepared = Verification.PrepareBeam(model, 250,
            Verification.BeamSample(model.BeamElements[250], "synthetic-static", "P+", .5, SectionSide.Unspecified), "alternative solver");
        var result = Verification.Run(prepared, CheckMechanism.UlsBiaxialSection, verifier);
        Assert.AreEqual(EngineeringOutcome.Satisfied, result.Outcome); Assert.AreEqual(.9, result.Utilization);
        Assert.AreEqual("test1", result.EngineVersion); StringAssert.Contains(result.EngineConfiguration, factory.Id);
        factory.Converged = false;
        result = Verification.Run(prepared, CheckMechanism.UlsBiaxialSection, verifier);
        Assert.AreEqual(EngineeringOutcome.NotEvaluated, result.Outcome); Assert.IsNull(result.Utilization);
        Assert.AreEqual(1, factory.Sessions); Assert.AreEqual(2, factory.Calls);
    }
}
