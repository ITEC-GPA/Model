using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using GPC.Geometry;

namespace GPC.Model.Elements
{
    [DebuggerDisplay("Id={Id}")]
    public class Shell : Element
    {
        private int _id;
        private List<Node> _nodes;
        private List<Edge> _edges;
        private Shells _shells;

        internal Shell(int id, List<Node> nodes, List<Edge> edges, Shells shells)
        {
            _id = id;
            _nodes = nodes;
            _edges = edges;
            _shells = shells;
        }

        public int Id
        {
            get { return _id; }
        }

        public Node[] GetNodes()
        {
            return _nodes.ToArray();
        }

        public Edge[] GetEdges()
        {
            return _edges.ToArray();
        }

        public Shells Shells
        {
            get { return _shells; }
        }

        /// <summary>
        /// Give a trans rightHandOriented with the surface polygon
        /// </summary>
        /// <returns>trans</returns>
        public Trans3d GetRightHandTrans()
        {
            Node[] nodes = this.GetNodes();
            Point3d origin = nodes[0].Point;
            Point3d diff0 = nodes[1].Point - origin;
            Point3d diff1 = nodes[2].Point - origin;
            Point3d normal = diff0 ^ diff1;//sign not important here, tested later

            Trans3d result = new Trans3d(origin, nodes[1].Point, origin, origin + normal);
            Polygon poly = new Polygon();
            foreach (Node n in nodes)
            {
                Point3d localP = result.PointLocal(n.Point);
                poly.Add(localP.X, localP.Y);
            }
            if (!poly.IsRightHandOrdered())
            {
                //reversing zDir
                result = new Trans3d(result.Origin, result.Origin + result.XDir, result.Origin, result.Origin - result.ZDir);
            }
            return result;
        }
    }

    public class Shells : IEnumerable<Shell>
    {
        private List<Shell> _shells;
        private Mesh _mesh;

        internal Shells(Mesh mesh)
        {
            _shells = new List<Shell>();
            _mesh = mesh;
        }

        public Shell this[int index]
        {
            get { return _shells[index]; }
        }

        public Mesh Mesh
        {
            get { return _mesh; }
        }

        public int Count
        {
            get { return _shells.Count; }
        }

        internal Shell Add(int id, List<Node> nodes, List<Edge> edges)
        {
            Shell s = new Shell(id, nodes, edges, this);
            _shells.Add(s);
            return s;
        }

        public IEnumerator<Shell> GetEnumerator()
        {
            return _shells.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}