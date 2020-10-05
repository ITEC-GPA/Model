using System;
using System.Collections.Generic;
using GPC.Geometry;

namespace GPC.Model.Elements
{
    public class Mesh : Element
    {
        private Nodes _nodes;
        private Shells _shells;
        private Edges _edges;

        public Mesh()
        {
            _nodes = new Nodes(this);
            _shells = new Shells(this);
            _edges = new Edges(this);
        }

        public Nodes Nodes
        {
            get { return _nodes; }
        }

        public Shells Shells
        {
            get { return _shells; }
        }

        public Edges Edges
        {
            get { return _edges; }
        }

        /// <summary>
        /// Add a shell to the mesh
        /// </summary>
        /// <param name="id">shell id</param>
        /// <param name="nodeIds">nodeIds of shell nodes. Nodes in rightHandOrder for the normal</param>
        /// <param name="nodePoints">node points of shell</param>
        public void AddShell(int id, int[] nodeIds, Point3d[] nodePoints)
        {
            bool add;
            List<Node> nodes;
            List<Edge> edges;

            //sanity check
            foreach (Shell other in _shells)
            {
                if (other.Id == id)
                {
                    throw new NotSupportedException("shell already present");
                }
            }

            //Nodes
            nodes = new List<Node>();
            for (int i = 0; i < nodeIds.Length; i++)
            {
                add = true;
                foreach (Node other in _nodes)
                {
                    if (other.Id == nodeIds[i])
                    {
                        if (Geom.SquareDistance(other.Point, nodePoints[i]) > 0.001)
                        {
                            throw new NotSupportedException("Bad node definition");
                        }
                        add = false;
                        nodes.Add(other);
                        break;
                    }
                }
                if (add)
                {
                    nodes.Add(_nodes.Add(nodeIds[i], nodePoints[i]));
                }
            }

            //edges
            edges = new List<Edge>();
            for (int i = 0; i < nodeIds.Length; i++)
            {
                int firstId, lastId;
                int nextIndex;
                add = true;
                firstId = nodeIds[i];
                if (i == nodeIds.Length - 1)
                {
                    nextIndex = 0;
                }
                else
                {
                    nextIndex = i + 1;
                }
                lastId = nodeIds[nextIndex];
                foreach (Edge other in _edges)
                {
                    if (((firstId == other.FirstNode.Id) && (lastId == other.LastNode.Id)) ||
                        ((firstId == other.LastNode.Id) && (lastId == other.FirstNode.Id)))
                    {
                        add = false;
                        edges.Add(other);
                        break;
                    }
                }
                if (add)
                {
                    edges.Add(_edges.Add(nodes[i], nodes[nextIndex]));
                    nodes[i].AddConnectedEdge(edges[edges.Count - 1]);
                    nodes[nextIndex].AddConnectedEdge(edges[edges.Count - 1]);
                }
            }

            //shells
            _shells.Add(id, nodes, edges);
            foreach (Node n in nodes)
            {
                n.AddConnectedShell(_shells[_shells.Count - 1]);
            }
        }

        /// <summary>
        /// Get the nodes staying on a number of edges
        /// </summary>
        /// <param name="connectionsNumber">number of connections of edges to be searched</param>
        /// <returns></returns>
        public Node[] GetBorderNodes(int connectionsNumber)
        {
            List<Node> supporteds;
            supporteds = new List<Node>();
            foreach (Edge edge in Edges)
            {
                int connections;
                connections = 0;
                foreach (Shell shell in Shells)
                {
                    Edge[] shellEdges;
                    shellEdges = shell.GetEdges();
                    foreach (Edge oe in shellEdges)
                    {
                        if (edge == oe)
                        {
                            connections += 1;
                        }
                    }
                }
                if (connections == connectionsNumber)
                {
                    if (!supporteds.Contains(edge.FirstNode))
                    {
                        supporteds.Add(edge.FirstNode);
                    }
                    if (!supporteds.Contains(edge.LastNode))
                    {
                        supporteds.Add(edge.LastNode);
                    }
                }
            }
            return supporteds.ToArray();
        }

        public static Mesh Create(Polygon poly, double? maxArea, IEnumerable<Polygon> holesCanBeNull, IEnumerable<Point2d> addingPointsCanBeNull, IEnumerable<Point2d[]> addingLinesCanBeNull, bool? conformingDelaunay)
        {
            Mesh result;
            TriangleNet.Geometry.Polygon tPoly;
            TriangleNet.Meshing.GenericMesher mr;
            TriangleNet.Meshing.IMesh mesh;
            TriangleNet.Meshing.ConstraintOptions cOptions;
            TriangleNet.Meshing.QualityOptions qOptions;

            tPoly = new TriangleNet.Geometry.Polygon();
            tPoly.Add(GetTriangleContour(poly), false);
            if (holesCanBeNull != null)
            {
                foreach (Polygon hole in holesCanBeNull)
                {
                    tPoly.Add(GetTriangleContour(hole), true);
                }
            }

            if (addingPointsCanBeNull != null)
            {
                foreach (Point2d p in addingPointsCanBeNull)
                {
                    tPoly.Add(new TriangleNet.Geometry.Vertex(p.X, p.Y));
                }
            }

            if (addingLinesCanBeNull != null)
            {
                double pointInterax;
                if (maxArea.HasValue)
                {
                    pointInterax = System.Math.Sqrt(maxArea.Value);
                }
                else
                {
                    pointInterax = double.MaxValue;
                }
                foreach (Point2d[] line in addingLinesCanBeNull)
                {
                    if (line.Length < 2)
                    {
                        throw new NotSupportedException();
                    }
                    for (int i = 0; i < line.Length - 1; i++)
                    {
                        Point2d p0, p1;
                        int number;
                        double distance;
                        double delta;
                        Point2d dir;
                        p0 = line[i];
                        p1 = line[i + 1];
                        dir = p1 - p0;
                        distance = Geom.Distance(p0, p1);
                        dir /= distance;
                        number = System.Convert.ToInt32(System.Math.Round(distance / pointInterax)) + 1;
                        delta = distance / number;
                        for (int j = 0; j < number; j++)
                        {
                            Point2d op;
                            op = p0 + dir * (j * delta);
                            tPoly.Add(new TriangleNet.Geometry.Vertex(op.X, op.Y));
                        }
                        if (i == line.Length - 1)
                        {
                            tPoly.Add(new TriangleNet.Geometry.Vertex(p1.X, p1.Y));
                        }
                    }
                }
            }

            mr = new TriangleNet.Meshing.GenericMesher();
            if (conformingDelaunay.HasValue || maxArea.HasValue)
            {
                cOptions = new TriangleNet.Meshing.ConstraintOptions();
                if (conformingDelaunay.HasValue)
                {
                    cOptions.ConformingDelaunay = conformingDelaunay.Value;
                }

                qOptions = new TriangleNet.Meshing.QualityOptions();
                if (maxArea.HasValue)
                {
                    qOptions.MaximumArea = maxArea.Value;
                }

                mesh = mr.Triangulate(tPoly, cOptions, qOptions);
            }
            else
            {
                mesh = mr.Triangulate(tPoly);
            }

            result = new Mesh();
            foreach (TriangleNet.Topology.Triangle t in mesh.Triangles)
            {
                int[] ids;
                Point3d[] points;

                ids = new int[3];
                points = new Point3d[3];
                for (int i = 0; i < 3; i++)
                {
                    TriangleNet.Geometry.Vertex v;
                    ids[i] = t.GetVertexID(i);
                    v = t.GetVertex(i);
                    points[i] = new Point3d(v.X, v.Y, 0);
                }
                result.AddShell(t.ID, ids, points);
            }

            return result;
        }

        public static Mesh Triangulate(Polygon poly, IEnumerable<Polygon> holesCanBeNull)
        {
            return Create(poly, null, holesCanBeNull, null, null, null);
        }

        private static TriangleNet.Geometry.Contour GetTriangleContour(Polygon poly)
        {
            List<TriangleNet.Geometry.Vertex> vs = new List<TriangleNet.Geometry.Vertex>();
            foreach (Point2d p in poly)
            {
                vs.Add(new TriangleNet.Geometry.Vertex(p.X, p.Y));
            }
            return new TriangleNet.Geometry.Contour(vs);
        }
    }
}