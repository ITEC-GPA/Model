using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using GPC.Model.FEM.Collections;
using GPC.Model.FEM;
using GPC.Utilities.Time;
using System.Collections.Generic;
using GPC.TestUtilities;
using System.Diagnostics;
using System.Linq;

namespace GeneralTest
{
    [TestClass]
    public class PerformanceTest : UnitTestBase
    {

        private int[] indexArray;

        private void FunctionToTest0()
        {
            int amountOfNodes = 10000;
            List<Node> nodesCollection = new List<Node>();
            for (int i = 0; i < amountOfNodes; i++)
            {
                nodesCollection.Add(new Node(10.0, 20, 30, string.Empty, indexArray[i]));
            }
        }

        private void FunctionToTest1()
        {
            int amountOfNodes = 10000;
            FemObjectCollection<Node> nodesCollection = new FemObjectCollection<Node>();

            for (int i = 0; i < amountOfNodes; i++)
            {
                nodesCollection.AddUnique(new Node(10.0 + 10 * i, 20 + 15 * i, 30 + 5 * i, string.Empty, indexArray[i]));
            }
        }

        private void FunctionToTest2()
        {
            int amountOfNodes = 10000;
            Dictionary<int, Node> nodesCollection = new Dictionary<int, Node>();

            for (int i = 0; i < amountOfNodes; i++)
            {
                nodesCollection.Add(i, new Node(10.0, 20, 30, string.Empty, indexArray[i]));
            }
        }

        [TestMethod]
        public void TestMethod1()
        {
            int amountOfNodes = 5000;
            int[] _indexArray = new int[amountOfNodes * 2];

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

            var bb0 = MeasureTime.FunctionExecutionTime(20, ac0, true, "List"); ;
            var bb1 = MeasureTime.FunctionExecutionTime(20, ac1, true, "NodesCollection"); ;
            var bb2 = MeasureTime.FunctionExecutionTime(20, ac2, true, "Dictionary"); ;
        }


        [TestMethod]
        public void CollectionCast()
        {
            ICollection<int> collection = new HashSet<int>();
            HashSet<int> set = new HashSet<int>();

            collection.Add(1);
            collection.Add(2);
            collection.Add(1);
            collection.Add(4);
            collection.Add(5);

            set.Add(1);
            set.Add(2);
            set.Add(1);
            set.Add(4);
            set.Add(5);

            Action action1 = new Action(() =>
            {
                (collection as HashSet<int>).TryGetValue(2, out int found);
            });

            Action action2 = new Action(() =>
            {
                set.TryGetValue(2, out int found);
            });

            Action action3 = new Action(() =>
            {
                ((HashSet<int>)collection).TryGetValue(2, out int found);
            });


            var cast1Time = MeasureTime.FunctionExecutionTime(100, action1, true, "CastAs"); ;
            var cast2Time = MeasureTime.FunctionExecutionTime(100, action3, true, "CastParenthesis"); ;
            var setTime = MeasureTime.FunctionExecutionTime(100, action2, true, "Set"); ;

            Assert.IsTrue(cast1Time > setTime);
            Assert.IsTrue(cast2Time < cast1Time);
        }

        private void Shuffle<T>(ref T[] list)
        {
            Random rnd = new Random();
            int n = list.Length;
            while (n > 1)
            {
                n--;
                int k = rnd.Next(n + 1);
                T value = list[k];
                list[k] = list[n];
                list[n] = value;
            }
        }

        [TestMethod]
        public void AddUnique()
        {
            int amountOfNodes = 10000;
            FemObjectCollection<Node> nodesCollection = new FemObjectCollection<Node>();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < amountOfNodes; i++)
            {
                nodesCollection.Add(new Node(10.0 + 10 * i, 20 + 15 * i, 30 + 5 * i, string.Empty));
            }
            stopWatch.Stop();
            Debug.WriteLine(stopWatch.ElapsedMilliseconds, "R1");
            // Aggiunta elementi alla collection vuota senza test "contains": aumento drastico da 6.5s a 30-40ms

            // Lista casuale
            int[] randomIdx = Enumerable.Range(0, amountOfNodes).ToArray();
            Shuffle(ref randomIdx);

            // Prova a reinserire tutti gli stessi elementi ma in ordine casuale
            int id = 0;
            stopWatch.Restart();
            for (int i = 0; i < amountOfNodes; i++)
            {
                int n = randomIdx[i];
                id = nodesCollection.AddUnique(new Node(10.0 + 10 * n, 20 + 15 * n, 30 + 5 * n, string.Empty));
            }
            stopWatch.Stop();
            Debug.WriteLine(stopWatch.ElapsedMilliseconds, "Elapsed");
            // Con test "contains" che usa Parallel: prestazioni migliorate 2.5s-3.0s (prima erano 20s)

            // Il numero di elementi non dovrebbe variare
            Assert.IsTrue(nodesCollection.Count == amountOfNodes);
        }

        [TestMethod]
        public void NodeCollection1()
        {
            int amountOfNodes = 10000;
            // Nuova lista auto ordinata
            NodeCollection nodesCollection = new NodeCollection();

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < amountOfNodes; i++)
            {
                nodesCollection.Add(new Node(10.0 + 10 * i, 20 + 15 * i, 30 + 5 * i, string.Empty));
            }
            stopWatch.Stop();
            Debug.WriteLine(stopWatch.ElapsedMilliseconds, "R1");
            // Aggiunta elementi alla collection vuota con nuovo algoritmo di test "contains": 30-40ms

            // Lista casuale
            int[] randomIdx = Enumerable.Range(0, amountOfNodes).ToArray();
            Shuffle(ref randomIdx);

            // Prova a reinserire tutti gli stessi elementi ma in ordine casuale
            int id = 0;
            stopWatch.Restart();
            for (int i = 0; i < amountOfNodes; i++)
            {
                int n = randomIdx[i];
                id = nodesCollection.Add(new Node(10.0 + 10 * n, 20 + 15 * n, 30 + 5 * n, string.Empty));
            }
            stopWatch.Stop();
            Debug.WriteLine(stopWatch.ElapsedMilliseconds, "Elapsed");
            // Risultato: ~ 20ms

            // Il numero di elementi non dovrebbe variare
            Assert.IsTrue(nodesCollection.Count == amountOfNodes);
        }

        [TestMethod]
        public void NodeCollection2()
        {
            // Test più intensivo con coordinate casuali e non più progressive
            int amountOfNodes = 10000;
            NodeCollection nodesCollection = new NodeCollection();
            int[] randomIdx = new int[amountOfNodes];

            // Genera dei punti in coordinate casuali
            List<(double x, double y, double z)> points = new List<(double x, double y, double z)>();
            Random rnd = new Random();
            for (int i = 0; i < amountOfNodes; i++)
            {
                randomIdx[i] = i;
                int k = rnd.Next(1000, 5000);
                double x = rnd.NextDouble() * k;
                double y = rnd.NextDouble() * k;
                double z = rnd.NextDouble() * k;
                points.Add((x, y, z));
            }
            // Randomizza la lista degli indici
            Shuffle(ref randomIdx);

            // Genera la prima lista di nodi
            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < amountOfNodes; i++)
            {
                nodesCollection.Add(new Node(points[i].x, points[i].y, points[i].z, string.Empty));
            }
            stopWatch.Stop();
            Debug.WriteLine(stopWatch.ElapsedMilliseconds, "R1");
            // Aggiunta elementi alla collection vuota con nuovo algoritmo di test "contains": ~50ms

            // Prova a reinserire gli stessi nodi ma in ordine casuale
            int id = 0;
            stopWatch.Restart();
            for (int i = amountOfNodes - 1; i >= 0; i--)
            {
                int n = randomIdx[i];
                id = nodesCollection.Add(new Node(points[n].x, points[n].y, points[n].z, string.Empty));
            }
            stopWatch.Stop();
            Debug.WriteLine(stopWatch.ElapsedMilliseconds, "Elapsed");
            // Il risultato resta ~ 20ms

            // Il numero di elementi non dovrebbe variare
            Assert.IsTrue(nodesCollection.Count == amountOfNodes);

            // Genera un'altra lista di punti casuali
            points.Clear();
            for (int i = 0; i < amountOfNodes; i++)
            {
                int k = rnd.Next(1000, 5000);
                double x = rnd.NextDouble() * k;
                double y = rnd.NextDouble() * k;
                double z = rnd.NextDouble() * k;
                points.Add((x, y, z));
            }

            // Prova ad aggiungrli alla lista
            stopWatch.Restart();
            for (int i = amountOfNodes - 1; i >= 0; i--)
            {
                id = nodesCollection.Add(new Node(points[i].x, points[i].y, points[i].z, string.Empty));
            }
            stopWatch.Stop();
            Debug.WriteLine(stopWatch.ElapsedMilliseconds, "Elapsed");
            // Risultato ~60ms

            // Il numero di elementi iniziale dovrebbe radoppiare
            Assert.IsTrue(nodesCollection.Count == amountOfNodes * 2);
        }

        [TestMethod]
        public void NodeCollectionEditing()
        {
            NodeCollection nodesCollection = new NodeCollection();
            nodesCollection.Add(new Node(10, 20, 30, string.Empty));
            nodesCollection.Add(new Node(15, 25, 35, string.Empty));

            Node changedNode = nodesCollection[1];
            changedNode.Position.Move(1, 1, 1);
            int index = nodesCollection.Update(changedNode);

            Debug.WriteLine(index, "First move index");

            changedNode = nodesCollection[1];
            changedNode.Position.Move(9, 9, 9);
            index = nodesCollection.Update(changedNode);

            Debug.WriteLine(index, "Second move insex");
        }

        [TestMethod]
        public void NodeCollectionEditing1()
        {
            int amountOfNodes = 10000;
            NodeCollection nodesCollection = new NodeCollection();
            int[] ids = new int[amountOfNodes];

            // Riempie con punti random
            Random rnd = new Random();
            for (int i = 0; i < amountOfNodes; i++)
            {
                int k = rnd.Next(1000, 5000);
                double x = rnd.NextDouble() * k;
                double y = rnd.NextDouble() * k;
                double z = rnd.NextDouble() * k;
                ids[i] = nodesCollection.Add(new Node(x, y, z, string.Empty));
            }

            Stopwatch stopWatch = new Stopwatch();
            stopWatch.Start();
            for (int i = 0; i < amountOfNodes; i++)
            {
                int k = rnd.Next(50, 100);
                Node node = nodesCollection[ids[i]];
                double dx = rnd.NextDouble() * k;
                double dy = rnd.NextDouble() * k;
                double dz = rnd.NextDouble() * k;
                node.Position.Move(dx, dy, dz);

                nodesCollection.Update(node);
            }
            stopWatch.Stop();
            Debug.WriteLine(stopWatch.ElapsedMilliseconds, "Elapsed time");

        }
    }
}
