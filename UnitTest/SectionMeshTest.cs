using System;
using System.Linq;
using System.Threading.Tasks;
using GPC.Geometry.Meshes;
using GPC.Model.Sections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// Mesh of the sections, generated at the first access (September 2026)
    /// </summary>
    [TestClass]
    public class SectionMeshTest
    {
        private static double MeshArea(Mesh mesh) => mesh.GetFaces().Sum(f => mesh.GetFaceArea(f));

        [TestMethod]
        public void ThinWallMeshIsTheOneOfTheThinWalls()
        {
            var sections = new ThinWallSection[]
            {
                new SectionH(300, 7.1, 150, 10.7, 150, 10.7, "IPE300"),
                new SectionRHS(200, 100, 8, 12, 5, 6, "RHS"),
                new SectionRectangular(400, 250, "R"),
                new SectionL(80, 8, 120, 10, "L"),
            };

            foreach (ThinWallSection section in sections)
            {
                Mesh expected = section.GetMesh();
                Mesh mesh = section.Mesh;

                Assert.AreEqual(expected.FacesCount, mesh.FacesCount, section.Name);
                Assert.AreEqual(expected.VerticesCount, mesh.VerticesCount, section.Name);
                Assert.AreEqual(section.ThinWalls.Sum(w => w.Area), MeshArea(mesh), 1e-9 * section.Area, section.Name);
                Assert.AreSame(mesh, section.Mesh, "the mesh is generated once");
            }
        }

        [TestMethod]
        public void CircularMeshes()
        {
            var circular = new SectionCircular(100);
            Assert.AreEqual(64, circular.Mesh.FacesCount);
            Assert.AreEqual(32, circular.Mesh.Faces.Count(f => f.IsTriangle));
            double radius = circular.Mesh.GetVertices().Max(v => v.Point.X) - circular.Centroid.X;
            Assert.AreEqual(50, radius, 1);

            var tube = new SectionCHS(100, 5);
            Assert.AreEqual(32, tube.Mesh.FacesCount);
            Assert.IsTrue(tube.Mesh.Faces.All(f => !f.IsTriangle));
        }

        [TestMethod]
        public void MeshFollowsTheChangesOfTheSection()
        {
            var circular = new SectionCircular(100);
            Mesh before = circular.Mesh;

            circular.Diameter = 200;
            Assert.AreNotSame(before, circular.Mesh);
            Assert.AreEqual(4 * MeshArea(before), MeshArea(circular.Mesh), 1e-9 * MeshArea(circular.Mesh));

            // before, a non-positive diameter was accepted
            circular.Diameter = -10;
            Assert.AreEqual(200, circular.Diameter);

            var rhs = new SectionRHS(200, 100, 8, 12, 5, 6, "RHS");
            double area = MeshArea(rhs.Mesh);
            rhs.Height = 300;
            Assert.AreEqual(area + (5 + 6) * 100, MeshArea(rhs.Mesh), 1e-9 * area, "the webs are 100 mm longer");
        }

        [TestMethod]
        public void MeshIsGeneratedOnceInParallel()
        {
            var section = new SectionH(300, 7.1, 150, 10.7, 150, 10.7, "IPE300");
            Mesh[] meshes = new Mesh[16];
            Parallel.For(0, meshes.Length, i => meshes[i] = section.Mesh);
            Assert.IsTrue(meshes.All(m => ReferenceEquals(m, meshes[0])));
        }

        [TestMethod]
        public void RhsFlangesAfterAChangeAreTheOnesOfTheConstructor()
        {
            // the top and the bottom flanges have different thickness
            var changed = new SectionRHS(250, 100, 8, 12, 5, 6, "RHS") { Height = 200 };
            var built = new SectionRHS(200, 100, 8, 12, 5, 6, "RHS");

            Assert.AreEqual(built.ThinWalls.Length, changed.ThinWalls.Length);
            for (int i = 0; i < built.ThinWalls.Length; i++)
            {
                Assert.AreEqual(built.ThinWalls[i].T, changed.ThinWalls[i].T, 1e-12);
                Assert.AreEqual(built.ThinWalls[i].L, changed.ThinWalls[i].L, 1e-12);
                Assert.AreEqual(built.ThinWalls[i].Point.X, changed.ThinWalls[i].Point.X, 1e-12);
                Assert.AreEqual(built.ThinWalls[i].Point.Y, changed.ThinWalls[i].Point.Y, 1e-12);
            }
            Assert.AreEqual(built.Centroid.Y, changed.Centroid.Y, 1e-9);
            Assert.AreEqual(built.Jxx, changed.Jxx, 1e-9 * built.Jxx);

            // the top flange (8 mm) is at the top
            Assert.AreEqual(200 - 4, changed.ThinWalls[3].Point.Y, 1e-12);
            Assert.AreEqual(6, changed.ThinWalls[2].Point.Y, 1e-12);
        }

        [TestMethod]
        public void RhsDistanceFromTop()
        {
            var rhs = new SectionRHS(200, 100, 8, 12, 5, 6, "RHS");
            Assert.AreEqual(200, rhs.DistanceYCentroidFromTop() + rhs.DistanceYCentroidFromBottom(), 1e-9);
        }
    }
}
