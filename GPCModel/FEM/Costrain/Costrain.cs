using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Costrain
{
    /// <summary>
    /// Multipoints costrains is when as example: gdl_i = f(gdl_1, ... , gld_K, ... gdl_N) + const with K and N != i and const can be = 0
    //  these are userful for rotated (not in Global Coordinates) restrains
    /// </summary>
    public class MultiPointCostrain
    { 
        Link[] _links;
        double _constValue;

        public Link[] Links => _links;
        public double ConstValue => _constValue;

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
            public string LabelNode;
            public LinearSolver.DOF GdlNode;
            public double Value;

            public Link(string labelNodeSlave, LinearSolver.DOF gdlNodeSlave, double val)
            {
                LabelNode = labelNodeSlave;
                GdlNode = gdlNodeSlave;
                Value = val;
            }

            public override string ToString()
            {
                return Value + " * (Node" + LabelNode + " " + GdlNode + ") ";
            }
        }
    }
}

