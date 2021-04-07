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
    public class MultiPointCostrain
    {
        #region Variables
        Link[] _links;
        double _constValue;
        #endregion

        #region Properties
        public Link[] Links => _links;
        public double ConstValue => _constValue;
        #endregion

        public MultiPointCostrain(Link[] links, double constValue = 0)
        {
            _links = links;
            _constValue = constValue;
        }

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

        public static MultiPointCostrain[] RigidLink(Node node1, Node node2)
        {
            double L = node1.Position.DistanceTo(node2.Position);

            //sin(x) = x - x^3/6 + x^5/120 - x^7/5040 .... (Taylor) 
            //cos(x) = 1 - x^2/2 + x^4/24 -x^6/720 + x^8/40320 .... (Taylor) 

            //Y' = Y + cos(Theta) * L
            //0 = Y' + Y + cos(Theta) * L = Y' + Y + 1.0 * L

            #region traslations
            MultiPointCostrain.Link[] eqtnsDX = new MultiPointCostrain.Link[2];
            eqtnsDX[0] = new Link(node2, LinearSolver.DOF.DX, -1.0);
            eqtnsDX[1] = new Link(node1, LinearSolver.DOF.DX, 1.0);
            /*eqtnsDX[2] = new MultiPointCostrain.Link(node1, LinearSolver.DOF.RY, L);
            eqtnsDX[3] = new MultiPointCostrain.Link(node1, LinearSolver.DOF.RZ, L);*/

            MultiPointCostrain.Link[] eqtnsDY = new MultiPointCostrain.Link[4];
            double signRZ = 1.0;
            if (node2.Position.X - node1.Position.X < 0)
            {
                signRZ = -1.0;
            }
            double signRX = -1.0;
            if (node2.Position.Z - node1.Position.Z < 0)
            {
                signRX = 1.0;
            }
            eqtnsDY[0] = new Link(node2, LinearSolver.DOF.DY, -1.0);
            eqtnsDY[1] = new Link(node1, LinearSolver.DOF.DY, 1.0);
            eqtnsDY[2] = new Link(node1, LinearSolver.DOF.RX, signRX * L);
            eqtnsDY[3] = new Link(node1, LinearSolver.DOF.RZ, signRZ * L);

            Link[] eqtnsDZ = new Link[4];
            eqtnsDZ[0] = new Link(node2, LinearSolver.DOF.DZ, -1.0);
            eqtnsDZ[1] = new Link(node1, LinearSolver.DOF.DZ, 1.0);
            eqtnsDZ[2] = new Link(node1, LinearSolver.DOF.RX, L);
            eqtnsDZ[3] = new Link(node1, LinearSolver.DOF.RY, L);
            #endregion

            #region rotation
            Link[] eqtnsRX = new Link[2];
            eqtnsRX[0] = new Link(node2, LinearSolver.DOF.RX, -1.0);
            eqtnsRX[1] = new Link(node1, LinearSolver.DOF.RX, 1.0);

            Link[] eqtnsRY = new Link[2];
            eqtnsRY[0] = new Link(node2, LinearSolver.DOF.RY, -1.0);
            eqtnsRY[1] = new Link(node1, LinearSolver.DOF.RY, 1.0);

            Link[] eqtnsRZ = new Link[2];
            eqtnsRZ[0] = new Link(node2, LinearSolver.DOF.RZ, -1.0);
            eqtnsRZ[1] = new Link(node1, LinearSolver.DOF.RZ, 1.0);
            #endregion

            MultiPointCostrain[] links = new MultiPointCostrain[6];
            links[0] = new MultiPointCostrain(eqtnsDX, 0.0);
            links[1] = new MultiPointCostrain(eqtnsDY, 0.0);
            links[2] = new MultiPointCostrain(eqtnsDZ, 0.0);
            links[3] = new MultiPointCostrain(eqtnsRX, 0.0);
            links[4] = new MultiPointCostrain(eqtnsRY, 0.0);
            links[5] = new MultiPointCostrain(eqtnsRZ, 0.0);

            return links;
        }
    }
}

