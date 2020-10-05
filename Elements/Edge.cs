using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Collections;
using GPC.Geometry;
#if _never
namespace GPC.Model
{
    [DebuggerDisplay("FirstNode={FirstNode.Id}, LastNode={LastNode.Id}")]
    public class Edge
    {
        private Node _firstNode;
        private Node _lastNode;
        private Edges _edges;

        internal Edge(Node firstNode, Node lastNode, Edges edges)
        {
            _firstNode = firstNode;
            _lastNode = lastNode;
            _edges = edges;
        }

        public Node FirstNode
        {
            get { return _firstNode; }
        }

        public Node LastNode
        {
            get { return _lastNode; }
        }

        public Edges Edges
        {
            get { return _edges; }
        }

        public double GetLength()
        {
            return Geom.Distance(FirstNode.Point, LastNode.Point);
        }
    }

    public class Edges : IEnumerable<Edge>
    {
        private List<Edge> _edges;
        private Mesh _mesh;

        internal Edges(Mesh mesh)
        {
            _edges = new List<Edge>();
            _mesh = mesh;
        }

        public Edge this[int index]
        {
            get { return _edges[index]; }
        }

        public Mesh Mesh
        {
            get { return _mesh; }
        }

        public int Count
        {
            get { return _edges.Count; }
        }

        internal Edge Add(Node firstNode, Node lastNode)
        {
            Edge e;
            e = new Edge(firstNode, lastNode, this);
            _edges.Add(e);
            return e;
        }

        public IEnumerator<Edge> GetEnumerator()
        {
            return _edges.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}
#endif