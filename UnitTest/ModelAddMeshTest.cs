using GPC.Geometry;
using GPC.Geometry.Meshes;
using GPC.Model.LoadCases;
using GPC.Model.Loads;
using GPC.Model.Models;

namespace UnitTest;

/// <summary>
/// Model.AddMesh lumps a line load on the vertices of the line: L / (n - 1) to the inner nodes, half of it to the two ends. Until 11/10/2026
/// the ends were recognized comparing the node with a point, always false: the ends got the full share and the total was w L n / (n - 1)
/// (defect reported by the architecture review, correction authorized by the user).
/// </summary>
[TestClass]
public class ModelAddMeshTest
{
    [DataTestMethod, DataRow(2), DataRow(5)]
    public void TheLineLoadIsLumpedWithHalfSharesAtTheEnds(int nodes)
    {
        const double length = 4000, w = -2;
        var mesh = new Mesh();
        var vertices = Enumerable.Range(0, nodes)
            .Select(i => mesh.Vertices.AddUnique(new MeshVertex(new Point3d(length * i / (nodes - 1), 0, 0)))).ToArray();
        var load = new LineLoad(0, 0, w, 0, 0, 0, new Line3d(Point3d.Origin, new Point3d(length, 0, 0)), new LoadCaseBase("Q"));
        var model = new Model("line load");

        Assert.IsTrue(model.AddMesh(mesh, null, null, null, new Dictionary<ILineLoad, int[]> { { load, vertices } }, null, null, out var map, out _, out _));

        double share = w * length / (nodes - 1);
        var forces = vertices.Select(v => model.NodesElements[map[v]].Loads.Values.Cast<PointLoad>().Single().F3).ToArray();
        Assert.AreEqual(share / 2, forces.First(), 1e-9);
        Assert.AreEqual(share / 2, forces.Last(), 1e-9);
        foreach (double inner in forces.Skip(1).Take(nodes - 2)) Assert.AreEqual(share, inner, 1e-9);
        Assert.AreEqual(w * length, forces.Sum(), 1e-9, "equilibrium: the nodal forces add up to the load on the line");
    }
}
