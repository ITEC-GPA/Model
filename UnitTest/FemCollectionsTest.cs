using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GPC.Model.Fem.Collections;

using GPC.Model.Fem.FiniteElements;
using GPC.Geometry;
using GPC.TestUtilities;
using GPC.Model.LoadCases;
using GPC.Model.Fem.Attributes;
using GPC.Model.Fem.Properties;
using GPC.Model.Materials;
using System.Collections.Generic;
using System.Diagnostics;
using GPC.Model.Stages;

namespace FemTest
{
    [TestClass]
    public class FemCollectionsTest : UnitTestBase
    {

        [TestMethod]
        public void FemObjectCollectionTest1()
        {
            FemObjectCollection<Node> cnode = new FemObjectCollection<Node>();
            
            Node n1 = new Node(Point3d.Origin, 1);
            Node n2 = new Node(Point3d.Origin, 2);
            Node n3 = new Node(new Point3d(0, 1, 2), 1);

            cnode.AddUnique(n1);
            cnode.AddUnique(n2);
            cnode.AddUnique(n3);

            Assert.IsTrue(cnode.Count == 2, cnode.Count.ToString());

            Assert.IsTrue(cnode.Contains(n3) != 0);

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
            p1.Id = 1;

            Plate p2 = new Plate(new Node[] { new Node(new Point3d(0, 2, 2), 1), new Node(new Point3d(1, 2, 2), 2), new Node(new Point3d(2, 3, 2), 3), new Node(new Point3d(3, 4, 2), 4) });
            p2.Id = 1;

            Plate p3 = new Plate(new Node[] { new Node(new Point3d(0, 1, 2), 1), new Node(new Point3d(1, 1, 2), 2), new Node(new Point3d(2, 1, 2), 3), new Node(new Point3d(3, 1, 2), 4) });
            p3.Id = 1;

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();

            cfe.AddUnique(p1);
            cfe.AddUnique(p2);
            cfe.AddUnique(p3);

            stopWatch.Stop();
            Debug.WriteLine(stopWatch.ElapsedMilliseconds, "Elapsed time");
            // Con compare della collection 59ms
            // Con comparer con for parallelo 35ms



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
            Node n2 = new Node(Point3d.Origin, 1);

            Node n3 = new Node(new Point3d(0, 1, 2), 2);    // C 2
            Node n5 = new Node(new Point3d(0, 1, 2), 2);
            Node n6 = new Node(new Point3d(0, 1, 2), 2);

            Node n4 = new Node(new Point3d(0, 1, 3), 3);    // D 3

            Stage.StageProperty sp1 = new Stage.StageProperty();
            Stage.StageProperty sp2 = new Stage.StageProperty();
            sp1.AddLoadCaseAttribute(new NodeForceAttribute("lc1", null, 0, 1, 2, 3, 4, 5));
            sp2.AddLoadCaseAttribute(new NodeForceAttribute("lc2", null, 0, 1, 2, 3, 4, 5));


            nodes.AddUnique(n1, sp1);
            nodes.AddUnique(n2, sp2);
            nodes.AddUnique(n3, sp1);
            nodes.AddUnique(n4, sp1);
            nodes.AddUnique(n5, sp2);
            nodes.AddUnique(n6, sp2);

            Assert.IsTrue(nodes.Count == 3);

            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes[0].LoadCaseName == "lc1");
            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes[1].LoadCaseName == "lc2");

            Assert.IsTrue(nodes.GetStageProperty(2).LoadCaseAttributes[0].LoadCaseName == "lc1");
            Assert.IsTrue(nodes.GetStageProperty(2).LoadCaseAttributes[1].LoadCaseName == "lc2");
            Assert.IsTrue(nodes.GetStageProperty(2).LoadCaseAttributes.Count == 3, nodes.GetStageProperty(2).LoadCaseAttributes.Count.ToString());

            Assert.IsTrue(nodes.GetStageProperty(3).LoadCaseAttributes.Count == 1, nodes.GetStageProperty(3).LoadCaseAttributes.Count.ToString());
            Assert.IsTrue(nodes.GetStageProperty(3).LoadCaseAttributes[0].LoadCaseName == "lc1");

        }


        [TestMethod]
        public void FemObjectStageCollectionTest2()
        {
            NodeStageCollection<Node, Stage.StageProperty> nodes = new NodeStageCollection<Node, Stage.StageProperty>();

            Node n1 = new Node(Point3d.Origin, 1);          // A 1
            Node n2 = new Node(Point3d.Origin, 2);

            Stage.StageProperty sp1 = new Stage.StageProperty();
            Stage.StageProperty sp2 = new Stage.StageProperty();
            sp1.AddLoadCaseAttribute(new NodeForceAttribute("lc1", null, 0, 1, 2, 3, 4, 5));
            sp2.AddLoadCaseAttribute(new NodeForceAttribute("lc2", null, 0, 1, 2, 3, 4, 5));


            nodes.AddUnique(n1, sp1);
            nodes.AddUnique(n2, sp2);

            Assert.IsTrue(sp1.LoadCaseAttributes.Count == 1, sp1.LoadCaseAttributes.Count.ToString());
            Assert.IsTrue(sp2.LoadCaseAttributes.Count == 1, sp2.LoadCaseAttributes.Count.ToString());


            Assert.IsTrue(nodes.Count == 1);

            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes.Count == 2, nodes.GetStageProperty(1).LoadCaseAttributes.Count.ToString());
            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes[0].LoadCaseName == "lc1");
            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes[1].LoadCaseName == "lc2");


        }

        [TestMethod]
        public void FemObjectStageCollectionTest3()
        {
            FiniteElementStageCollection<FiniteElement, Stage.StageProperty> nodes = new FiniteElementStageCollection<FiniteElement, Stage.StageProperty>();

            Plate p1 = new Plate(new Node[] { new Node(new Point3d(0, 1, 2), 1), new Node(new Point3d(1, 1, 2), 2), new Node(new Point3d(2, 1, 2), 3), new Node(new Point3d(3, 1, 2), 4) });
            p1.Id = 1;

            Plate p2 = new Plate(new Node[] { new Node(new Point3d(0, 2, 2), 1), new Node(new Point3d(1, 2, 2), 2), new Node(new Point3d(2, 3, 2), 3), new Node(new Point3d(3, 4, 2), 4) });
            p2.Id = 2;

            Plate p3 = new Plate(new Node[] { new Node(new Point3d(0, 1, 2), 1), new Node(new Point3d(1, 1, 2), 2), new Node(new Point3d(2, 1, 2), 3), new Node(new Point3d(3, 1, 2), 4) });
            p3.Id = 1;


            Stage.StageFiniteElementProperty sp1 = new Stage.StageFiniteElementProperty("m1");
            Stage.StageFiniteElementProperty sp2 = new Stage.StageFiniteElementProperty("m2");
            sp1.AddLoadCaseAttribute(new NodeForceAttribute("lc1", null, 0, 1, 2, 3, 4, 5));
            sp2.AddLoadCaseAttribute(new NodeForceAttribute("lc2", null, 0, 1, 2, 3, 4, 5));

            nodes.AddUnique(p1, sp1);
            nodes.AddUnique(p2, sp2);
            nodes.AddUnique(p3, sp2);

            Assert.IsTrue(nodes.Count == 2);

            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes.Count == 2, nodes.GetStageProperty(1).LoadCaseAttributes.Count.ToString());
            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes[0].LoadCaseName == "lc1");
            Assert.IsTrue(nodes.GetStageProperty(1).LoadCaseAttributes[1].LoadCaseName == "lc2");


        }


        [TestMethod]
        public void FiniteElementCollectionTest1()
        {
            FiniteElementCollection fec = new FiniteElementCollection();

            Plate p1 = new Plate(new Node[] {   new Node(192, 192, 0, string.Empty, 902), 
                                                new Node(192, 208, 0, string.Empty, 941), 
                                                new Node(208, 192, 0, string.Empty, 845), 
                                                new Node(208, 208, 0, string.Empty, 890) 
                                            });


            Plate p2 = new Plate(new Node[] {   new Node(208.0202415096, 352.0079137900, 0, string.Empty, 912), 
                                                new Node(208.0811887820, 367.9810163216, 0, string.Empty, 874), 
                                                new Node(224.0043348824, 351.9855939276, 0, string.Empty, 826), 
                                                new Node(224.0245050942, 367.9364075938, 0, string.Empty, 876)
                                            });
                        

            var ip1 = fec.Add(p1);
            var ip2 = fec.Add(p2);

            Console.WriteLine(ip1);
            Console.WriteLine(ip2);

            Assert.IsTrue(ip1 != ip2);


        }


        [TestMethod]
        public void FiniteElementCollectionTest2()
        {
            FiniteElementCollection fec = new FiniteElementCollection();

            Plate p1 = new Plate(new Node[] {   new Node(80, 192, 0, string.Empty, 653),
                                                new Node(96, 192, 0, string.Empty, 712),
                                                new Node(96, 208, 0, string.Empty, 679),
                                                new Node(80, 208, 0, string.Empty, 546)
                                            });


            Plate p3 = new Plate(new Node[] {   new Node(32, 688, 0, string.Empty, 277),
                                                new Node(48, 688, 0, string.Empty, 436),
                                                new Node(48, 704, 0, string.Empty, 321),
                                                new Node(32, 704, 0, string.Empty, 246)
                                            });


            Plate p2 = new Plate(new Node[] {   new Node(288, 256, 0, string.Empty, 335),
                                                new Node(304, 256, 0, string.Empty, 195),
                                                new Node(304, 272, 0, string.Empty, 270),
                                                new Node(288, 272, 0, string.Empty, 360)
                                            });

            var ip1 = fec.Add(p1);
            var ip2 = fec.Add(p2);

            Console.WriteLine(ip1);
            Console.WriteLine(ip2);

            Assert.IsTrue(ip1 != ip2);


        }
    }
}
