using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GPC.Model.FEM.Solver;

namespace GPC.Model.FEM.Costrains
{
    /// <summary>
    /// Multipoints costrains is when as example: gdl_i = f(gdl_1, ... , gld_K, ... gdl_N) + const with K and N != i and const can be = 0
    ///  these are userful for rotated restrains
    ///<example>
    /// MultiPointCostrain.Link[] equations = new MultiPointCostrain.Link[2];
    /// equations[0] = new MultiPointCostrain.Link(node1, DOF.DX, 1.0);
    /// equations[1] = new MultiPointCostrain.Link(node1, DOF.DY, 1.0);
    /// MultiPointCostrain Costrain1 = new MultiPointCostrain(equations);
    ///</example>
    /// </summary>
    public class MultiPointsCostrain : FEMObject
    {
        #region Variables
        Equation[] _equations;
        double _constValue;
        #endregion

        #region Properties
        public Equation[] Equations => _equations;
        public double ConstValue => _constValue;
        #endregion

        #region Costruttori
        public MultiPointsCostrain(Equation[] equations, double constValue = 0)
        {
            _equations = equations;
            _constValue = constValue;
        }
        #endregion

        public override string ToString()
        {
            string s = ""; // "Node " + _labelNodeMaster + " " + NodeMasterGDL + " = "; Not used in lagrangian formulation
            for (int i = 0; i < _equations.Length; i++)
            {
                s = s + _equations[i].ToString();
            }
            s = s + " = " + _constValue;
            return s;
        }

        /// <summary>
        /// Costrain = Value * GDLNode
        /// </summary>
        public struct Equation
        {
            public Node NodeSlave;
            public DOF GdlNode;
            public double Value;

            public Equation(Node nodeSlave, DOF gdlNodeSlave, double factor)
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
    }
}

