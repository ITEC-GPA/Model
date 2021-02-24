using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GPC.Model.FEM.Collections;
using GPC.Model.FEM;
using GPC.Utilities.Time;
using System.Collections.Generic;

namespace UnitTest
{
    [TestClass]
    public class PerformanceTest
    {
        public TestContext TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInitialize(TestContext context)
        {
            // Nothing
        }

        [TestInitialize]
        public void TestInitialize()
        {
            // Nothing
        }

        [TestCleanup]
        public void CleanUp()
        {
            if (Directory.Exists(TestContext.TestDir))
                Directory.Delete(TestContext.TestDir, true);
        }

        private int[] indexArray;

        private void FunctionToTest0()
        {
            int amountOfNodes = 10000;
            List<Node> nodesCollection = new List<Node>();
            for (int i = 0; i < amountOfNodes; i++)
            {
                nodesCollection.Add(new Node(10, 20, 30, indexArray[i]));
            }
        }

        private void FunctionToTest1()
        {
            int amountOfNodes = 10000;
            FemObjectCollection<Node> nodesCollection = new FemObjectCollection<Node>();

            for (int i = 0; i < amountOfNodes; i++)
            {
                nodesCollection.Add(new Node(10, 20, 30, indexArray[i]));
            }
        }

        private void FunctionToTest2()
        {
            int amountOfNodes = 10000;
            Dictionary<int, Node> nodesCollection = new Dictionary<int, Node>();

            for (int i = 0; i < amountOfNodes; i++)
            {
                nodesCollection.Add(i, new Node(10, 20, 30, indexArray[i]));
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            int amountOfNodes = 5000;
            int[] _indexArray = new int[amountOfNodes*2];

            Random r = new Random();

            for (int i = 0; i < amountOfNodes; i++)
            {
                _indexArray[i] = r.Next();
                _indexArray[9999 - i] = _indexArray[i];
            }
            indexArray = _indexArray;

            Action ac0 = new Action(FunctionToTest0);
            Action ac1 = new Action(FunctionToTest1);
            Action ac2 = new Action(FunctionToTest2);

            var bb0 = MeasureTime.FunctionExecutionTime("List", 20, ac0, true); ;
            var bb1 = MeasureTime.FunctionExecutionTime("NodesCollection", 20, ac1, true); ;
            var bb2 = MeasureTime.FunctionExecutionTime("Dictionary", 20, ac2, true); ;
        }
    }
}
