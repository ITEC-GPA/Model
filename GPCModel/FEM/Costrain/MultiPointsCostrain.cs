using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Costrain
{
    /// <summary>
    /// Multipoints costrains is when as example: gdl_i = f(gdl_1, ... , gld_K, ... gdl_N) + const with K and N != i and const can be = 0
    //  these are userful for rotated restrains
    /* EXAMPLE:
             * MultiPointCostrain.Link[] equations = new MultiPointCostrain.Link[2];
             * equations[0] = new MultiPointCostrain.Link("1", DOF.DX, 1.0);
             * equations[1] = new MultiPointCostrain.Link("1", DOF.DY, 1.0);
             * MultiPointCostrain Costrain1 = new MultiPointCostrain(equations);*/
    /// </summary>
    public class MultiPointsCostrain : Costrain
    {
        #region Variables
        Link[] _links;
        double _constValue;
        #endregion

        #region Properties
        public Link[] Links => _links;
        public double ConstValue => _constValue;
        #endregion

        #region Costruttori
        public MultiPointsCostrain(Link[] links, double constValue = 0)
        {
            _links = links;
            _constValue = constValue;
        }

        public MultiPointsCostrain(Link[] links, double constValue = 0, string name = "", int id = 0) : this(links, constValue)
        {
            this._name = name;
            this.SetId(id);
        }
        #endregion

        public override string ToString()
        {
            string s = ""; // "Node " + _labelNodeMaster + " " + NodeMasterGDL + " = "; Not used in lagrangian formulation
            for (int i = 0; i < _links.Length; i++)
            {
                s = s + _links[i].ToString();
            }
            s = s + " = " + _constValue;
            return s;
        }

        /// <summary>
        /// Costrain = Value * GDLNode
        /// </summary>
        public struct Link
        {
            public Node NodeSlave;
            public LinearSolver.DOF GdlNode;
            public double Value;

            public Link(Node nodeSlave, LinearSolver.DOF gdlNodeSlave, double factor)
            {
                NodeSlave = nodeSlave;
                GdlNode = gdlNodeSlave;
                Value = factor;
            }

            public override string ToString()
            {
                if (Value > 0)
                {
                    return "+" + Value + " * (Node" + NodeSlave.ToString() + " " + GdlNode + ") ";
                } else
                {
                    return Value + " * (Node" + NodeSlave.ToString() + " " + GdlNode + ") ";
                }
            }
        }

        /// <summary>
        /// Ritorna le equazioni per rigid link tra 2 nodi
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        public static MultiPointsCostrain[] RigidLink(Node node1, Node node2)
        {
            double LX = Math.Abs(node1.Position.X - node2.Position.X);
            double LY = Math.Abs(node1.Position.Y - node2.Position.Y);
            double LZ = Math.Abs(node1.Position.Z - node2.Position.Z);

            double factorRX;
            double factorRY;
            double factorRZ;

            //Y' = Y + cos(Theta) * L
            //0 = Y' + Y + cos(Theta) * L = Y' + Y + 1.0 * L

            #region traslations
            Link[] eqtnDX = new Link[4];
            factorRY = (node2.Position.Z - node1.Position.Z < 0) ? -1.0 : 1.0;
            factorRZ = (node2.Position.Y - node1.Position.Y < 0) ? 1.0 : -1.0;
           
            eqtnDX[0] = new Link(node2, LinearSolver.DOF.DX, -1.0);
            eqtnDX[1] = new Link(node1, LinearSolver.DOF.DX, 1.0);
            eqtnDX[2] = new Link(node1, LinearSolver.DOF.RY, factorRY * LZ);
            eqtnDX[3] = new Link(node1, LinearSolver.DOF.RZ, factorRZ * LY);

            Link[] eqtnDY = new Link[4];
            factorRX = (node2.Position.Z - node1.Position.Z < 0) ? 1.0 : -1.0;
            factorRZ = (node2.Position.X - node1.Position.X < 0) ? -1.0 : 1.0;
            
            eqtnDY[0] = new Link(node2, LinearSolver.DOF.DY, -1.0);
            eqtnDY[1] = new Link(node1, LinearSolver.DOF.DY, 1.0);
            eqtnDY[2] = new Link(node1, LinearSolver.DOF.RX, factorRX * LZ);
            eqtnDY[3] = new Link(node1, LinearSolver.DOF.RZ, factorRZ * LX);

            Link[] eqtnDZ = new Link[4];
            factorRX = (node2.Position.Y - node1.Position.Y < 0) ? -1.0 : 1.0;
            factorRY = (node2.Position.X - node1.Position.X < 0) ? 1.0 : -1.0;
            eqtnDZ[0] = new Link(node2, LinearSolver.DOF.DZ, -1.0);
            eqtnDZ[1] = new Link(node1, LinearSolver.DOF.DZ, 1.0);
            eqtnDZ[2] = new Link(node1, LinearSolver.DOF.RX, factorRX * LY);
            eqtnDZ[3] = new Link(node1, LinearSolver.DOF.RY, factorRY * LX);
            #endregion

            #region rotation
            //RX node1 = RX node2
            Link[] eqtnRX = new Link[2];
            eqtnRX[0] = new Link(node2, LinearSolver.DOF.RX, -1.0);
            eqtnRX[1] = new Link(node1, LinearSolver.DOF.RX, 1.0);

            //RY node1 = RY node2
            Link[] eqtnRY = new Link[2];
            eqtnRY[0] = new Link(node2, LinearSolver.DOF.RY, -1.0);
            eqtnRY[1] = new Link(node1, LinearSolver.DOF.RY, 1.0);

            //RZ node1 = RZ node2
            Link[] eqtnRZ = new Link[2];
            eqtnRZ[0] = new Link(node2, LinearSolver.DOF.RZ, -1.0);
            eqtnRZ[1] = new Link(node1, LinearSolver.DOF.RZ, 1.0);
            #endregion

            #region equations
            MultiPointsCostrain[] links = new MultiPointsCostrain[6];
            links[0] = new MultiPointsCostrain(eqtnDX, 0.0);
            links[1] = new MultiPointsCostrain(eqtnDY, 0.0);
            links[2] = new MultiPointsCostrain(eqtnDZ, 0.0);
            links[3] = new MultiPointsCostrain(eqtnRX, 0.0);
            links[4] = new MultiPointsCostrain(eqtnRY, 0.0);
            links[5] = new MultiPointsCostrain(eqtnRZ, 0.0);
            #endregion

            return links;
        }
    }
}

