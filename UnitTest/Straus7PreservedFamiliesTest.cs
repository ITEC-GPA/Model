using GPC.Converter;
using GPC.Converter.Straus7;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest;

public partial class Straus7ApiWorkflowTest
{
    /// <summary>Since Converter 2.7 bricks and links no longer reject the model (requested): the beams and plates are imported, the bricks are
    /// preserved, and links read by a binding without the extended API are preserved with a warning.</summary>
    [DataTestMethod]
    [DataRow("bricks", "Straus7BricksPreserved")][DataRow("links", "Straus7LinksPreserved")]
    public void BricksAndUnreadLinks_ArePreservedWithAWarning(string fault, string code)
    {
        var api = new SyntheticApi { Fault = fault }; var report = new Straus7ApiConverter(() => api).Import(Request());
        Partial(report); Assert.IsTrue(api.Disposed);
        Assert.AreEqual(1, report.Model.BeamElements.Count); Assert.AreEqual(1, report.Model.AreaElements.Count);
        Assert.IsTrue(report.Diagnostics.Any(d => d.Code == code && d.Severity == GPC.Model.Core.Diagnostics.DiagnosticSeverity.Warning), code);
    }
}
