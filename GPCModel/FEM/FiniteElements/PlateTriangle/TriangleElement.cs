using GPC.Geometry;
using GPC.Model.FEM.Properties;

namespace GPC.Model.FEM.FiniteElements
{
    public abstract class TriangleElement : Plate
    {
        //calculated and used in BuildMatrix and used also in BuildF
        protected double _areaElement;

        public TriangleElement(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
        {

        }

        /// <summary>
        /// Node I,J,K are in Global System Coordinate, out node1,2,3 are in local element system coordinate
        /// </summary>
        /// <param name="nodeI"></param>
        /// <param name="nodeJ"></param>
        /// <param name="nodeK"></param>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <param name="node3"></param>
        protected void LocalNodes(Node nodeI, Node nodeJ, Node nodeK, out Node node1, out Node node2, out Node node3)
        {
            #region CalculationOfLocalCoordinates
            //Search for 3 local axis
            Vector3d y = new Vector3d(nodeJ.Position.X - nodeI.Position.X, nodeJ.Position.Y - nodeI.Position.Y, nodeJ.Position.Z - nodeI.Position.Z);
            Vector3d vecy = new Vector3d(y);
            vecy.Unitize();

            Vector3d x = new Vector3d(nodeK.Position.X - nodeI.Position.X, nodeK.Position.Y - nodeI.Position.Y, nodeK.Position.Z - nodeI.Position.Z);
            Vector3d vecx = new Vector3d(x);
            vecx.Unitize();

            Vector3d z = x.CrossProduct(y);
            //UnitVector3D vecz = z.Normalize();
            //_vecZLocal = vecz.ToVector().ToArray();
            Vector3d vecz = new Vector3d(z);
            vecz.Unitize();

            //recalculation of x that can be non-ortogonal
            x = y.CrossProduct(z);
            vecx = new Vector3d(x);
            vecx.Unitize();
            //_vecXLocal = vecx.ToVector().ToArray();
            _localCoordinateSystem = new Geometry.CoordinateSystem(new Point3d(0, 0, 0), vecx, vecy);

            //move to local axis
            //calculation in local nodes
            Vector3d v12 = new Vector3d(nodeJ.Position.X - nodeI.Position.X, nodeJ.Position.Y - nodeI.Position.Y, nodeJ.Position.Z - nodeI.Position.Z);
            Vector3d v13 = new Vector3d(nodeK.Position.X - nodeI.Position.X, nodeK.Position.Y - nodeI.Position.Y, nodeK.Position.Z - nodeI.Position.Z);

            node1 = new Node(0, 0, 0, nodeI.Index, nodeI.Name); //Origin GlobalNodes.ElementAt(1 - 1);
            node2 = new Node(v12.DotProduct(vecx), v12.DotProduct(vecy), v12.DotProduct(vecz), nodeJ.Index, nodeJ.Name); //Axis y GlobalNodes.ElementAt(2 - 1);
            node3 = new Node(v13.DotProduct(vecx), v13.DotProduct(vecy), v13.DotProduct(vecz), nodeK.Index, nodeK.Name); //GlobalNodes.ElementAt(3 - 1);
            #endregion
        }
    }
}
