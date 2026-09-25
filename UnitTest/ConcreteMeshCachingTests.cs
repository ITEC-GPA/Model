using GPC.Model.Data.Concrete;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading.Tasks;

namespace ModelObjectTest
{
    [TestClass]
    public class ConcreteMeshCachingTests
    {
        [TestMethod]
        public void MeshIsGeneratedOnceForTheSameOptions()
        {
            var section = new ReinforcedConcreteSection(
                new SectionRectangular(500, 300),
                ConcreteMaterialEN1992Data.C25_30);

            var first = section.GetMesh(50, initialMeshOnly: true, recombine: false, refine: false);
            var second = section.GetMesh(50, initialMeshOnly: true, recombine: false, refine: false);
            var changed = section.GetMesh(25, initialMeshOnly: true, recombine: false, refine: false);

            Assert.AreSame(first, second);
            Assert.AreNotSame(first, changed);
        }

        [TestMethod]
        public void MeshCacheIsThreadSafeAndInvalidatedByMeshSize()
        {
            var section = new ReinforcedConcreteSection(
                new SectionRectangular(500, 300),
                ConcreteMaterialEN1992Data.C25_30);
            var meshes = new GPC.Geometry.Meshes.Mesh[16];

            Parallel.For(0, meshes.Length, i => meshes[i] = section.Mesh);

            for (int i = 1; i < meshes.Length; i++)
                Assert.AreSame(meshes[0], meshes[i]);

            section.SetMeshSize(25);
            var resized = section.Mesh;

            Assert.AreNotSame(meshes[0], resized);
            Assert.AreSame(resized, section.Mesh);
        }
    }
}
