using GPC.Model.Elements;
using GPC.Model.Restrains;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Costrains
{
    /// <summary>
    /// A multipoint constraint: a linear equation between degrees of freedom, sum(Value_k * dof_k) = const (e.g. gdl_i = f(gdl_1, ... , gdl_N) + const).
    /// These are useful for rigid links and rotated restrains
    /// <example>
    /// MultiPointsCostrain.Equation[] equations = new MultiPointsCostrain.Equation[2];
    /// equations[0] = new MultiPointsCostrain.Equation(node1, DOF.DX, 1.0);
    /// equations[1] = new MultiPointsCostrain.Equation(node1, DOF.DY, 1.0);
    /// MultiPointsCostrain costrain1 = new MultiPointsCostrain(equations);
    /// </example>
    /// </summary>
    [Serializable]
    public class MultiPointsCostrain : ModelObject
    {
        #region Variables
        /// <summary>
        /// The terms of the equation
        /// </summary>
        Equation[] _equations;
        /// <summary>
        /// The constant term
        /// </summary>
        double _constValue;
        #endregion

        #region Properties
        /// <summary>
        /// The terms of the equation (the array of the constraint)
        /// </summary>
        public Equation[] Equations => _equations;
        /// <summary>
        /// The constant term
        /// </summary>
        public double ConstValue => _constValue;
        #endregion

        #region Costruttori
        /// <summary>
        /// Creates a constraint (the array is kept, not copied)
        /// </summary>
        /// <param name="equations">The terms of the equation</param>
        /// <param name="constValue">The constant term</param>
        public MultiPointsCostrain(Equation[] equations, double constValue = 0)
        {
            _equations = equations;
            _constValue = constValue;
        }

        protected MultiPointsCostrain(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _equations = (Equation[])info.GetValue("Equations", typeof(Equation[]));
            _constValue = info.GetDouble("ConstValue");
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Equations", _equations);
            info.AddValue("ConstValue", _constValue);
        }
        #endregion

        /// <summary>
        /// The equation as text: the terms, " = " and the constant
        /// </summary>
        /// <returns>The text</returns>
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
        /// Equality of name, terms (the same array instance) and constant
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal constraint</returns>
        public override bool Equals(object obj)
        {
            return obj is MultiPointsCostrain costrain &&
                   base.Equals(obj) &&
                   EqualityComparer<Equation[]>.Default.Equals(_equations, costrain._equations) &&
                   _constValue == costrain._constValue;
        }

        /// <summary>
        /// The hash code of name, array of the terms (as instance) and constant
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = -1066499299;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Equation[]>.Default.GetHashCode(_equations);
            hashCode = hashCode * -1521134295 + _constValue.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// A term of the equation: Value * degree of freedom of a node
        /// </summary>
        [Serializable]
        public struct Equation
        {
            /// <summary>
            /// The node
            /// </summary>
            public NodeElement NodeSlave;
            /// <summary>
            /// The degree of freedom of the node
            /// </summary>
            public GeometryRestrain.DOF GdlNode;
            /// <summary>
            /// The coefficient
            /// </summary>
            public double Value;

            /// <summary>
            /// Creates a term
            /// </summary>
            /// <param name="nodeSlave">The node</param>
            /// <param name="gdlNodeSlave">The degree of freedom</param>
            /// <param name="factor">The coefficient</param>
            public Equation(NodeElement nodeSlave, GeometryRestrain.DOF gdlNodeSlave, double factor)
            {
                NodeSlave = nodeSlave;
                GdlNode = gdlNodeSlave;
                Value = factor;
            }

            /// <summary>
            /// The term as text: "±Value * (Node node dof)"
            /// </summary>
            /// <returns>The text</returns>
            public override string ToString()
            {
                if (Value > 0)
                {
                    return "+" + Value + " * (Node" + NodeSlave.ToString() + " " + GdlNode + ") ";
                }
                else
                {
                    return Value + " * (Node" + NodeSlave.ToString() + " " + GdlNode + ") ";
                }
            }
        }
    }
}

