using System.Reflection;
using System.Reflection.Emit;
using GPC.Model.Checking;
using GPC.Model.Results.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Preparation;
using GPC.Model.Compatibility;

namespace UnitTest;

[TestClass]
public class ArchitectureBoundaryTest
{
    [TestMethod]
    public void DomainDoesNotReferenceEnginesOrOrchestrators()
    {
        var references = typeof(GPC.Model.Models.Model).Assembly.GetReferencedAssemblies();
        Assert.IsFalse(references.Any(a => a.Name!.Contains("Checker") || a.Name.Contains("Converter") || a.Name.Contains("Anthea")));
    }

    [TestMethod]
    public void PhysicalTransformationsDoNotCallDesignPoliciesOrVerification()
    {
        foreach (var method in typeof(ActionTransformations).GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
            foreach (var called in Calls(method))
                Assert.IsFalse(called.DeclaringType?.Namespace?.StartsWith("GPC.Model.Design") == true
                    || called.DeclaringType?.Namespace?.StartsWith("GPC.Model.Checking") == true,
                    method.Name + " calls " + called.DeclaringType);
    }

    [TestMethod]
    public void CompatibilityEntrypointsDelegateToTheNewServices()
    {
        Assert.IsTrue(Calls(typeof(Verification).GetMethod("PrepareBeam")!).Any(m => m.DeclaringType == typeof(BeamCheckPreparation)));
        Assert.IsTrue(Calls(typeof(ResultTransformations).GetMethod("RotateBeam")!).Any(m => m.DeclaringType == typeof(ActionTransformations)));
        Assert.AreEqual("GPC.Model.Checking.Contracts.CheckResult", typeof(CheckResult).FullName);
        Assert.AreEqual(typeof(GPC.Model.Models.Model).Assembly, typeof(CheckResult).Assembly);
    }

    private static IEnumerable<MethodBase> Calls(MethodInfo method)
    {
        var codes = typeof(OpCodes).GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(f => f.FieldType == typeof(OpCode)).Select(f => (OpCode)f.GetValue(null)!)
            .ToDictionary(c => unchecked((ushort)c.Value));
        var bytes = method.GetMethodBody()?.GetILAsByteArray() ?? Array.Empty<byte>();
        for (int i = 0; i < bytes.Length;)
        {
            ushort value = bytes[i++];
            if (value == 0xfe) value = (ushort)(0xfe00 | bytes[i++]);
            var code = codes[value];
            if (code.OperandType == OperandType.InlineMethod)
                yield return method.Module.ResolveMethod(BitConverter.ToInt32(bytes, i))!;
            i += code.OperandType switch
            {
                OperandType.InlineNone => 0,
                OperandType.ShortInlineBrTarget or OperandType.ShortInlineI or OperandType.ShortInlineVar => 1,
                OperandType.InlineVar => 2,
                OperandType.InlineI8 or OperandType.InlineR => 8,
                OperandType.InlineSwitch => 4 + 4 * BitConverter.ToInt32(bytes, i),
                _ => 4
            };
        }
    }
}
