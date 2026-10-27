using GPC.Model.Core;
using GPC.Model.Materials;
using GPC.Model.Sections;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Model.Core.Tests;

/// <summary>The value codec is built once per type and shared: concurrent copies of different
/// values of the same type must stay detached, complete and independent of each other.</summary>
[TestClass]
public class ValueCodecConcurrencyTest
{
    [TestMethod]
    public void SharedCodecCopiesDifferentValuesConcurrently()
    {
        var sources = Enumerable.Range(0, 64).Select(i => new SteelSection(new SectionH(300 + i, 10, 150, 15 + i % 5, 160, 18, "H" + i),
            new SteelMaterial("Steel " + i, 210000, 235 + i, 360 + i), Section.SectionTypes.Welded, Section.FormedTypes.HotFinished)).ToArray();
        var expected = sources.Select(s => ModelValues.Fingerprint(new object[] { s })).ToArray();
        var copies = new SteelSection[sources.Length];
        Parallel.For(0, 8 * sources.Length, new ParallelOptions { MaxDegreeOfParallelism = 8 }, k =>
        {
            int i = k % sources.Length; var copy = ModelValues.CopyValue(sources[i]);
            Assert.AreNotSame(sources[i], copy);
            Assert.AreEqual(expected[i], ModelValues.Fingerprint(new object[] { copy }), "value " + i);
            copies[i] = copy;
        });
        for (int i = 0; i < sources.Length; i++)
        {
            Assert.AreEqual(300 + i, copies[i].Height);
            Assert.AreEqual(235 + i, copies[i].SteelMaterial.Fyk);
        }
    }
}
