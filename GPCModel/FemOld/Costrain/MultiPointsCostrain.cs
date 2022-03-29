using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static GPC.Model.Fem.Solver;

namespace GPC.Model.Fem.Costrains
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
    public class MultiPointsCostrain : FemObject
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

        public override bool Equals(object obj)
        {
            return obj is MultiPointsCostrain costrain &&
                   base.Equals(obj) &&
                   EqualityComparer<Equation[]>.Default.Equals(_equations, costrain._equations) &&
                   _constValue == costrain._constValue;
        }

        public override int GetHashCode()
        {
            int hashCode = -1066499299;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Equation[]>.Default.GetHashCode(_equations);
            hashCode = hashCode * -1521134295 + _constValue.GetHashCode();
            return hashCode;
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

