using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GPC.Geometry;


namespace GPC.Model.Elements
{
    [DebuggerDisplay("Id={Id}, Point=({Point.X},{Point.Y},{Point.Z})")]
    public class Node
    {
        private int _id;
        private Point3d _point;
        private List<Edge> _connectedEdges;
        private List<Shell> _connectedShells;
        private Nodes _nodes;

        internal Node(int id, Point3d point, Nodes nodes)
        {
            _id = id;
            _point = new Point3d(point);
            _nodes = nodes;
            _connectedEdges = new List<Edge>();
            _connectedShells = new List<Shell>();
        }

        public int Id
        {
            get { return _id; }
        }

        public Point3d Point
        {
            get { return _point; }
        }

        public Nodes Nodes
        {
            get { return _nodes; }
        }

        public Edge[] GetConnectedEdges()
        {
            return _connectedEdges.ToArray();
        }

        public Shell[] GetConnectedShells()
        {
            return _connectedShells.ToArray();
        }

        internal void AddConnectedEdge(Edge edge)
        {
            _connectedEdges.Add(edge);
        }

        internal void AddConnectedShell(Shell shell)
        {
            _connectedShells.Add(shell);
        }
    }

    public class Nodes : IEnumerable<Node>
    {
        private List<Node> _nodes;
        private Mesh _mesh;

        internal Nodes(Mesh mesh)
        {
            _nodes = new List<Node>();
            _mesh = mesh;
        }

        public Node this[int index]
        {
            get { return _nodes[index]; }
        }

        public Mesh Mesh
        {
            get { return _mesh; }
        }

        public int Count
        {
            get { return _nodes.Count; }
        }

        public int IndexOf(Node item)
        {
            return _nodes.IndexOf(item);
        }

        internal Node Add(int id, Point3d point)
        {
            Node node;
            node = new Node(id, point, this);
            _nodes.Add(node);
            return node;
        }

        public IEnumerator<Node> GetEnumerator()
        {
            return _nodes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}