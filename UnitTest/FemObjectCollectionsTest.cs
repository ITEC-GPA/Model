using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GPC.Model.FEM.Collections;
using GPC.Model.FEM;
using GPC.Model.FEM.FiniteElements;
using GPC.Geometry;
using GPC.TestUtilities;
using GPC.Model.LoadCases;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Properties;
using GPC.Model.Materials;
using System.Collections.Generic;

namespace FemTest
{
    [TestClass]
    public class FemObjectCollectionsTest : UnitTestBase
    {

        [TestMethod]
        public void FemObjectCollectionTest1()
        {
            FemObjectCollection<Node> cnode = new FemObjectCollection<Node>();
            
            Node n1 = new Node(Point3d.Origin, 1);
            Node n2 = new Node(Point3d.Origin, 2);
            Node n3 = new Node(new Point3d(0, 1, 2), 1);

            cnode.Add(n1);
            cnode.Add(n2);
            cnode.Add(n3);

            Assert.IsTrue(cnode.Count == 2, cnode.Count.ToString());

            Assert.IsTrue(cnode.Contains(n3));

            Assert.IsTrue(cnode.GetElementById(1).Id == 1);
            Assert.IsTrue(cnode.GetElementById(2).Id == 2);

            Assert.AreEqual(cnode.GetElementById(1).Position, Point3d.Origin);
            Assert.AreEqual(cnode.GetElementById(2).Position, new Point3d(0, 1, 2));

        }


        [TestMethod]
        public void FemObjectCollectionTest2()
        {
            FemObjectCollection<FiniteElement> cfe = new FemObjectCollection<FiniteElement>();

            Plate p1 = new Plate(new Node[] { new Node(new Point3d(0, 1, 2), 1), new Node(new Point3d(1, 1, 2), 2), new Node(new Point3d(2, 1, 2), 3), new Node(new Point3d(3, 1, 2), 4) } );
            p1.SetId(1);

            Plate p2 = new Plate(new Node[] { new Node(new Point3d(0, 2, 2), 1), new Node(new Point3d(1, 2, 2), 2), new Node(new Point3d(2, 3, 2), 3), new Node(new Point3d(3, 4, 2), 4) });
            p2.SetId(1);

            Plate p3 = new Plate(new Node[] { new Node(new Point3d(0, 1, 2), 1), new Node(new Point3d(1, 1, 2), 2), new Node(new Point3d(2, 1, 2), 3), new Node(new Point3d(3, 1, 2), 4) });
            p3.SetId(1);

            cfe.Add(p1);
            cfe.Add(p2);
            cfe.Add(p3);

            Assert.IsTrue(cfe.Count == 2, cfe.Count.ToString());

            Assert.IsTrue(cfe.GetElementById(1).Id == 1);
            Assert.IsTrue(cfe.GetElementById(2).Id == 2);

            Assert.AreEqual(cfe.GetElementById(1).Nodes[0].Position, new Point3d(0, 1, 2));
            Assert.AreEqual(cfe.GetElementById(2).Nodes[0].Position, new Point3d(0, 2, 2));

        }


        [TestMethod]
        public void FemObjectStageCollectionTest1()
        {
            NodeStageCollection<Node, Stage.StageProperty> nodes = new NodeStageCollection<Node, Stage.StageProperty>();

            Node n1 = new Node(Point3d.Origin, 1);          // A 1
            Node n2 = new Node(Point3d.Origin, 2);

            Node n3 = new Node(new Point3d(0, 1, 2), 1);    // C 2
            Node n5 = new Node(new Point3d(0, 1, 2), 1);
            Node n6 = new Node(new Point3d(0, 1, 2), 5);

            Node n4 = new Node(new Point3d(0, 1, 3), 1);    // D 3

            Stage.StageProperty sp1 = new Stage.StageProperty();
            Stage.StageProperty sp2 = new Stage.StageProperty();
            sp1.AddLoadCaseAttribute(new NodeForceAttribute(new LoadCase("lc1"), null, 0, 1, 2, 3, 4, 5));
            sp2.AddLoadCaseAttribute(new NodeForceAttribute(new LoadCase("lc2"), null, 0, 1, 2, 3, 4, 5));


            nodes.Add(n1, sp1);
            nodes.Add(n2, sp2);
            nodes.Add(n3, sp1);
            nodes.Add(n4, sp1);
            nodes.Add(n5, sp2);
            nodes.Add(n6, sp2);

            Assert.IsTrue(nodes.Count == 3);

            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes[0].LoadCase.Name == "lc1");
            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes[1].LoadCase.Name == "lc2");

            Assert.IsTrue(nodes.GetStageProperty(2).LoadCaseAttributes[0].LoadCase.Name == "lc1");
            Assert.IsTrue(nodes.GetStageProperty(2).LoadCaseAttributes[1].LoadCase.Name == "lc2");
            Assert.IsTrue(nodes.GetStageProperty(2).LoadCaseAttributes.Count == 3, nodes.GetStageProperty(2).LoadCaseAttributes.Count.ToString());

            Assert.IsTrue(nodes.GetStageProperty(3).LoadCaseAttributes.Count == 1, nodes.GetStageProperty(3).LoadCaseAttributes.Count.ToString());
            Assert.IsTrue(nodes.GetStageProperty(3).LoadCaseAttributes[0].LoadCase.Name == "lc1");

        }


        [TestMethod]
        public void FemObjectStageCollectionTest2()
        {
            NodeStageCollection<Node, Stage.StageProperty> nodes = new NodeStageCollection<Node, Stage.StageProperty>();

            Node n1 = new Node(Point3d.Origin, 1);          // A 1
            Node n2 = new Node(Point3d.Origin, 2);

            Stage.StageProperty sp1 = new Stage.StageProperty();
            Stage.StageProperty sp2 = new Stage.StageProperty();
            sp1.AddLoadCaseAttribute(new NodeForceAttribute(new LoadCase("lc1"), null, 0, 1, 2, 3, 4, 5));
            sp2.AddLoadCaseAttribute(new NodeForceAttribute(new LoadCase("lc2"), null, 0, 1, 2, 3, 4, 5));


            nodes.Add(n1, sp1);
            nodes.Add(n2, sp2);

            Assert.IsTrue(sp1.LoadCaseAttributes.Count == 1, sp1.LoadCaseAttributes.Count.ToString());
            Assert.IsTrue(sp2.LoadCaseAttributes.Count == 1, sp2.LoadCaseAttributes.Count.ToString());


            Assert.IsTrue(nodes.Count == 1);

            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes.Count == 2, nodes.GetStageProperty(1).LoadCaseAttributes.Count.ToString());
            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes[0].LoadCase.Name == "lc1");
            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes[1].LoadCase.Name == "lc2");


        }

        [TestMethod]
        public void FemObjectStageCollectionTest3()
        {
            FiniteElementStageCollection<FiniteElement, Stage.StageProperty> nodes = new FiniteElementStageCollection<FiniteElement, Stage.StageProperty>();

            Plate p1 = new Plate(new Node[] { new Node(new Point3d(0, 1, 2), 1), new Node(new Point3d(1, 1, 2), 2), new Node(new Point3d(2, 1, 2), 3), new Node(new Point3d(3, 1, 2), 4) });
            p1.SetId(1);

            Plate p2 = new Plate(new Node[] { new Node(new Point3d(0, 2, 2), 1), new Node(new Point3d(1, 2, 2), 2), new Node(new Point3d(2, 3, 2), 3), new Node(new Point3d(3, 4, 2), 4) });
            p2.SetId(1);

            Plate p3 = new Plate(new Node[] { new Node(new Point3d(0, 1, 2), 1), new Node(new Point3d(1, 1, 2), 2), new Node(new Point3d(2, 1, 2), 3), new Node(new Point3d(3, 1, 2), 4) });
            p3.SetId(1);


            Stage.StageFiniteElementProperty sp1 = new Stage.StageFiniteElementProperty(new PlateProperty(new SteelMaterial("m1", 1, 0.1, 1, 2, 0.1, 0), 1, 2, "p"));
            Stage.StageFiniteElementProperty sp2 = new Stage.StageFiniteElementProperty(new PlateProperty(new SteelMaterial("m2", 1, 0.1, 1, 2, 0.1, 0), 1, 2, "p"));
            sp1.AddLoadCaseAttribute(new NodeForceAttribute(new LoadCase("lc1"), null, 0, 1, 2, 3, 4, 5));
            sp2.AddLoadCaseAttribute(new NodeForceAttribute(new LoadCase("lc2"), null, 0, 1, 2, 3, 4, 5));

            nodes.Add(p1, sp1);
            nodes.Add(p2, sp2);
            nodes.Add(p3, sp2);

            Assert.IsTrue(nodes.Count == 2);

            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes.Count == 2, nodes.GetStageProperty(1).LoadCaseAttributes.Count.ToString());
            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes[0].LoadCase.Name == "lc1");
            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes[1].LoadCase.Name == "lc2");


        }
    }
}
