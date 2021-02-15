using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.FEM.Elements;

namespace GPC.FEM
{
    public class FEMModel
    {
        public enum DOF
        {
            DX,   //0
            DY,   //1
            DZ,   //2
            RX,   //3
            RY,   //4
            RZ    //5
        }

        public static int MAXGDLPERNODE = Enum.GetNames(typeof(FEMModel.DOF)).Length;

        Matrix<double> _KGlobal;
        Matrix<double> _KGlobalRestrains;
        Vector<double> _F;
        Vector<double> _FRestrains;
        HashSet<MultiPointCostrain> _costrains;

        /// <summary>
        /// Unique nodes in model
        /// </summary>
        public static HashSet<Node> Nodes { get; set; }

        /// <summary>
        /// Unique elements in Models contains all the informations: node connectivity, material, property, LOAD as attribute, end releases...etc
        /// </summary>
        public HashSet<Elements.FiniteElement> Elements { get; set; }

        public FEMModel(FiniteElement[] inputElements)
        {
            #region NodeOfModel
            Nodes = new HashSet<Node>();
            int iter = 0;
            for (int i = 0; i < inputElements.Count(); i++)
            {
                FiniteElement element = inputElements[i];

                for (int j = 0; j < element.GlobalNodesElement.Count(); j++)
                {
                    Node node = element.GlobalNodesElement[j];

                    var nodes = Nodes.Where(n => n.X == node.X && n.Y == node.Y && n.Z == node.Z);

                    if (nodes.Count() > 1)
                    {
                        throw new Exception("Duplicate node!?");
                        
                    } else if (nodes.Count() == 1) //Node already used in another element.
                    {
                        node.ID = nodes.Single().ID;
                        if (node.Label != nodes.Single().Label)
                        {
                            node.Label = node.Label + "+" + nodes.Single().Label;
                        }

                        for (int k = 0; k < MAXGDLPERNODE; k++)
                        {
                            if (element.DOF[(FEM.FEMModel.DOF)k] == true)
                            {
                                node.DOF[(FEM.FEMModel.DOF)k] = true;
                            }

                            //save old DOF due to other element
                            if (nodes.Single().DOF[(FEM.FEMModel.DOF)k] == true)
                            {
                                node.DOF[(FEM.FEMModel.DOF)k] = true;
                            }
                        }

                        //Add Attribute of Duplicate node in original node

                        //update the HashSet
                        Nodes.Remove(nodes.Single());
                        Nodes.Add(node);
                    }
                    else
                    {
                        for (int k = 0; k < MAXGDLPERNODE; k++)
                        {
                            if (element.DOF[(FEM.FEMModel.DOF)k] == true)
                            {
                                node.DOF[(FEM.FEMModel.DOF)k] = true;
                            }
                        }
                        node.ID = iter;
                        Nodes.Add(node);
                        iter++;
                    }
                }
            }
            int nrNodes = Nodes.Count;

            #endregion

            #region ElementsOfModel
            //Assumed that geometry has been meshed and forces and property applied inside elements
            Elements = new HashSet<FiniteElement>();
            for (int i = 0; i < inputElements.Count(); i++)
            {
                //check if some node need to be changed
                for (int j = 0; j < inputElements.ElementAt(i).GlobalNodesElement.Count(); j++)
                {
                    var nodes = Nodes.Where(x => x == inputElements.ElementAt(i).GlobalNodesElement.ElementAt(j));
                    if (nodes.Count() == 0)
                    {
                        //the node in this element shiuld be updated:
                        //select the right note updated
                        Node n = inputElements.ElementAt(i).GlobalNodesElement.ElementAt(j);
                        Node rightNode = Nodes.Where(x => x.X == n.X && x.Y == n.Y && x.Z == n.Z).Single();
                        inputElements.ElementAt(i).GlobalNodesElement[j] = rightNode;
                    }
                }
                Elements.Add(inputElements.ElementAt(i));
            }
            #endregion

            #region AssemblyOfStiffnessMatrix
            //Assembling the Stiffness Matrix
            int dimensionKSystemMatrix = 0;
            for (int i = 0; i < Nodes.Count; i++)
            {
                dimensionKSystemMatrix = dimensionKSystemMatrix + Nodes.ElementAt(i).NrActiveDof;
            }
            _KGlobal = Matrix<double>.Build.Dense(dimensionKSystemMatrix, dimensionKSystemMatrix);
            for (int el = 0; el < Elements.Count(); el++)
            {
                //element el
                FiniteElement element = Elements.ElementAt(el);

                element.BuildMatrix();
                Matrix<double> Kelement = element.KElementGlobalCoord;
                
                for (int i = 0; i < element.PositionToGlobalSystemK.Count; i++)
                {
                    FiniteElement.Position local = element.PositionToGlobalSystemK.ElementAt(i).Key;
                    FiniteElement.Position global = element.PositionToGlobalSystemK.ElementAt(i).Value;
                    //Console.WriteLine(global.row + " " + global.col + " <- " + local.row + " " + local.col);

                    //double old = KGlobal[global.row, global.col];
                    _KGlobal[global.row, global.col] = _KGlobal[global.row, global.col] + Kelement[local.row, local.col];
                    /*if (global.row == 0 && global.col == 0)
                    {
                        Console.WriteLine("kGlob[" + global.row + "," + global.col + "] = " + old + " + " + Kelement[local.row, local.col]);
                    }*/
                }
                
            }
            Console.WriteLine("kGlobal System : " + _KGlobal.ToString());
            #endregion

            #region CalculationOfAppliedForcesF
            //Calculation of Forces vector
            _F = Vector<double>.Build.Dense(_KGlobal.RowCount);
            _F[GetPositionInKGlobal("4",DOF.DX).First()] = 1000;
            #endregion

            #region ApplyingRestrains
            _KGlobalRestrains = Matrix<double>.Build.Dense(_KGlobal.RowCount, _KGlobal.ColumnCount);
            _KGlobal.CopyTo(_KGlobalRestrains);
            _FRestrains = Vector<double>.Build.Dense(_F.Count);
            _F.CopyTo(_FRestrains);

            //PrescribeDisplacement("1", GDL.UX, 0);
            //PrescribeDisplacement("1", GDL.UY, 0);
            PrescribeDisplacement("1", DOF.DZ, 0);
            /*PrescribeDisplacement("1", DOF.RX, 0);
            PrescribeDisplacement("1", DOF.RY, 0);
            PrescribeDisplacement("1", DOF.RZ, 0);*/

            /*PrescribeDisplacement("2", GDL.UX, 0);
            PrescribeDisplacement("2", GDL.UY, 0);*/
            PrescribeDisplacement("2", DOF.DZ, 0);
            /*PrescribeDisplacement("2", DOF.RX, 0);
            PrescribeDisplacement("2", DOF.RY, 0);
            PrescribeDisplacement("2", DOF.RZ, 0);*/

            PrescribeDisplacement("3", DOF.DX, 0);
            PrescribeDisplacement("3", DOF.DY, 0);
            PrescribeDisplacement("3", DOF.DZ, 0);
            /*PrescribeDisplacement("3", DOF.RX, 0);
            PrescribeDisplacement("3", DOF.RY, 0);
            PrescribeDisplacement("3", DOF.RZ, 0);*/

            /*PrescribeDisplacement("4", GDL.UX, 0);
            PrescribeDisplacement("4", GDL.UY, 0);*/
            PrescribeDisplacement("4", DOF.DZ, 0);
            /*PrescribeDisplacement("4", DOF.RX, 0);
            PrescribeDisplacement("4", DOF.RY, 0);
            PrescribeDisplacement("4", DOF.RZ, 0);*/

            Console.WriteLine("kGlobal System + Restrains: " + _KGlobalRestrains.ToString());
            Console.WriteLine("Fmodified(Restrains): " + _FRestrains.ToString());
            #endregion

            #region ApplyingMultiPointCostrains
            _costrains = new HashSet<MultiPointCostrain>();
            //applying as example in node 1 : DX = DY (simply support with 45 degrees direction
            MultiPointCostrain.Link[] equations = new MultiPointCostrain.Link[2];
            equations[0] = new MultiPointCostrain.Link("1", DOF.DX, 1.0);
            equations[1] = new MultiPointCostrain.Link("1", DOF.DY, 1.0);
            MultiPointCostrain Costrain1 = new MultiPointCostrain(equations);
            _costrains.Add(Costrain1);

            /* Use: Lagrange multiplier method 
             * rewrite constrains as : 1.0 * GdL NodeMaster + ValI * GdL NodeSlaveI + ... + ValN * GdL NodeSlaveN = const
             * and modify K matrix and F vector
             */

            int nLagrangianMultiplier = _costrains.Count;
            for (int i = 0; i < _costrains.Count; i++)
            {
                MultiPointCostrain c = _costrains.ElementAt(i);
                Console.WriteLine(c.ToString());

                Vector<double> voidVector = Vector<double>.Build.Dense(_KGlobalRestrains.RowCount);
                _KGlobalRestrains = _KGlobalRestrains.InsertColumn(_KGlobalRestrains.ColumnCount, voidVector);
                voidVector = Vector<double>.Build.Dense(_KGlobalRestrains.ColumnCount);
                _KGlobalRestrains = _KGlobalRestrains.InsertRow(_KGlobalRestrains.RowCount, voidVector);

                //add a row to F vector and update it
                _FRestrains.CopySubVectorTo(voidVector,0,0,_FRestrains.Count);
                _FRestrains = voidVector;
                _FRestrains[_FRestrains.Count - 1] = c.ConstValue;

                for (int j = 0; j < c.Links.Length; j++) {

                    string labelNodeSlave = c.Links[j].LabelNode;
                    DOF gdlNodeSlave = c.Links[j].GdlNode;

                    int[] positionsGDLNodeSlave = GetPositionInKGlobal(labelNodeSlave, gdlNodeSlave);
                    if (positionsGDLNodeSlave.Length > 1)
                    {
                        throw new Exception("More than 1 nodes not yet supported");
                    }
                    int positionGDLNodeSlave = positionsGDLNodeSlave[0];
                    _KGlobalRestrains[positionGDLNodeSlave, _KGlobalRestrains.ColumnCount - 1] = c.Links[j].Value;
                    _KGlobalRestrains[_KGlobalRestrains.RowCount - 1, positionGDLNodeSlave] = c.Links[j].Value;
                }
            }
            Console.WriteLine("kGlobal System + Restrains + Constrains: " + _KGlobalRestrains.ToString());
            Console.WriteLine("Fmodified(Restrains + Constrains) = " + _FRestrains.ToString());
            #endregion

            #region SolveModel
            //Solve Matrix
            Vector<double> nodeDisplacements = _KGlobalRestrains.Solve(_FRestrains);
            Console.WriteLine("Node displacements results:" + nodeDisplacements.ToString());
            #endregion

            #region CalcResults
            for (int i = 0; i < Elements.Count; i++)
            {
                Elements.ElementAt(i).CalcResults(nodeDisplacements.ToArray());
            }
            #endregion
        }

        public void PrescribeDisplacement(string labelNode, DOF dof, double val)
    {
            int[] positions = GetPositionInKGlobal(labelNode, dof);

            for (int i = 0; i < positions.Length; i++)
            {
                for (int j = 0; j < _F.Count; j++)
                {
                    _FRestrains[j] = _FRestrains[j] - _KGlobalRestrains[j, positions[i]] * val;
                    _KGlobalRestrains[j, positions[i]] = 0;
                    _KGlobalRestrains[positions[i], j] = 0;
                }
                _KGlobalRestrains[positions[i], positions[i]] = 1.0;
                _FRestrains[positions[i]] = val;
            }
            
        }
        /// <summary>
        /// Give the position of selected GDL from 0 to N where N is dimension of matrix KGloabl or the dimension of the vector of Forces or Displacments
        /// </summary>
        /// <param name="labelNode">Label of the node or nodes searched</param>
        /// <param name="gdl">GDL searched</param>
        /// <returns></returns>
        public int[] GetPositionInKGlobal(string labelNode, DOF dof)
        {
            HashSet<int> results = new HashSet<int>();
            int counter = 0;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes.ElementAt(i).Label == labelNode)
                {
                    results.Add(counter + (int)dof);
                }
                else
                {
                    counter = counter + Nodes.ElementAt(i).NrActiveDof;
                }
            }
            return results.ToArray();
        }

        public static int GetPositionInKGlobal(int ID, DOF dof)
        {
            int counter = 0;
            for (int i = 0; i < Nodes.Count; i++)
            {
                if (Nodes.ElementAt(i).ID == ID)
                {
                    i = Nodes.Count;
                }
                else
                {
                    counter = counter + Nodes.ElementAt(i).NrActiveDof;
                }
            }
            return counter + (int) dof;
        }

        /// <summary>
        /// Multipoints costrains is when as example: gdl_i = f(gdl_1, ... , gld_K, ... gdl_N) + const with K and N != i and const can be = 0
        //  these are userful for rotated (not in Global Coordinates) restrains
        /// </summary>
        protected class MultiPointCostrain
        {
            /*string _labelNodeMaster; //Not used in lagrange formulation
            GDL _nodeMasterGDL;*/      //Not used in lagrange formulation 
            Link[] _links;
            double _constValue;

            /*public string LabelNodeMaster => _labelNodeMaster; //Not used in lagrangian formulation
            public GDL NodeMasterGDL => _nodeMasterGDL;*/        //Not used in lagrangian formulation
            public Link[] Links => _links;
            public double ConstValue => _constValue;

            public MultiPointCostrain(Link[] links, double constValue = 0)
            {
                _links = links;
                _constValue = constValue;
            }

            /*public MultiPointCostrain(string labelNodeMaster, GDL gdlNodeMaster, Link[] links, double constValue = 0) //not used in lagrangian formulation
            {
                _labelNodeMaster = labelNodeMaster;
                _nodeMasterGDL = gdlNodeMaster;
                _links = links;
                _constValue = constValue;
            }*/

            public override string ToString()
            {
                string s = ""; // "Node " + _labelNodeMaster + " " + NodeMasterGDL + " = "; Not used in lagrangian formulation
                for (int i = 0; i < _links.Length; i++) {
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
                public DOF GdlNode;
                public double Value; 

                public Link(string labelNodeSlave, DOF gdlNodeSlave, double val)
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
}
