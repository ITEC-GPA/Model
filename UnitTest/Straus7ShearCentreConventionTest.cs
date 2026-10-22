using GPC.Converter;
using GPC.Converter.Straus7;
using GPC.Model.Results.Locations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

public partial class Straus7ApiWorkflowTest
{
    [TestMethod]
    public void ShearCentreMetadataDoesNotRemoveSolverTorsionAndIsPreservedInEvidence()
    {
        var converter = new Straus7ApiConverter(() => new SyntheticApi());
        var model = converter.Import(Request()).Model;
        var native = converter.ReadResults(ResultsRequest());
        // metres in global Y and Z: both transverse directions are nonzero.
        native.Tables.Single(t => t.Quantity == "BeamForceGlobal").BeamShearCentreOffsetGlobal = new[] { 0d, .07, -.02 };
        var result = Straus7LinearStaticResults.Import(model, native, "shear-centre", CaseMap, isSynthetic: true);
        Assert.AreEqual(ImportStatus.Completed, result.Status);
        var sample = model.BeamElements.Values.Single().Results.SelectMany(r => r.Results).OfType<StationResultBeamForces>().First();
        Assert.AreEqual(20000000, sample.ResultBeamForces.T, "Keep the actual torsional demand in Nmm.");
        Assert.AreEqual(40000000, sample.ResultBeamForces.M1);
        Assert.AreEqual(60000000, sample.ResultBeamForces.M2);
        StringAssert.Contains(sample.State.Original.Convention, "torque is retained about the shear centre");
        Assert.IsTrue(model.PreservedSourceData.Any(p => p.RawData?.Contains("BeamShearCentreOffsetGlobal") == true));
    }

    [DataTestMethod]
    [DataRow(false)] [DataRow(true)]
    public void InvalidShearCentreMetadataRejectsNormalizationAtomically(bool nonFinite)
    {
        var converter = new Straus7ApiConverter(() => new SyntheticApi());
        var model = converter.Import(Request()).Model;
        var native = converter.ReadResults(ResultsRequest());
        native.Tables.Single(t => t.Quantity == "BeamForceGlobal").BeamShearCentreOffsetGlobal = nonFinite
            ? new[] { 0d, double.NaN, 0 } : new double[2];
        var result = Straus7LinearStaticResults.Import(model, native, "invalid-shear-centre", CaseMap, isSynthetic: true);
        Assert.AreEqual(ImportStatus.Rejected, result.Status);
        Assert.AreEqual(0, model.Datasets.Count);
    }
}
