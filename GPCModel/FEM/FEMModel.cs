using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.FEM.Elements;

namespace GPC.Model.FEM
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
        public Node[] Nodes { get; set; }

        /// <summary>
        /// Unique elements in Models contains all the informations: node connectivity, material, property, LOAD as attribute, end releases...etc
        /// </summary>
        public FiniteElement[] Elements { get; set; }

        public FEMModel(FiniteElement[] inputElements)
        {
            #region NodeOfModel
            HashSet<Node> nodesModel = new HashSet<Node>();
            int iter = 0;
            for (int i = 0; i < inputElements.Count(); i++)
            {
                FiniteElement element = inputElements[i];

                for (int j = 0; j < element.GlobalNodesElement.Count(); j++)
                {
                    Node node = element.GlobalNodesElement[j];

                    var nodes = nodesModel.Where(n => n.X == node.X && n.Y == node.Y && n.Z == node.Z);

                    if (nodes.Count() > 1)
                    {
                        throw new Exception("Duplicate node!?");
                    } else if (nodes.Count() == 1) //Node already used in another element.
                    {
                        int ID = nodes.Single().ID;
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
                        nodesModel.Remove(nodes.Single());
                        nodesModel.Add(new Node(ID,node));
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
                        nodesModel.Add(new Node(iter,node));
                        iter++;
                    }
                }
            }
            Nodes = nodesModel.ToArray();
            int nrNodes = Nodes.Length;

            #endregion

            #region ElementsOfModel
            //Assumed that geometry has been meshed and forces and property applied inside elements
            
            HashSet<FiniteElement> elementsModel = new HashSet<FiniteElement>();
            for (int i = 0; i < inputElements.Count(); i++)
            {
                //check if some node need to be changed
                for (int j = 0; j < inputElements[i].GlobalNodesElement.Count(); j++)
                {
                    /*var nodes = Nodes.Where(x => x == inputElements[i].GlobalNodesElement[j]).ToList();
                    if (nodes.Count() == 0)
                    {
                        //the node in this element shiuld be updated:
                        //select the right note updated
                        Node n = inputElements.ElementAt(i).GlobalNodesElement.ElementAt(j);
                        Node rightNode = Nodes.Where(x => x.X == n.X && x.Y == n.Y && x.Z == n.Z).Single();
                        inputElements.ElementAt(i).GlobalNodesElement[j] = rightNode;
                    }*/
                    var nodes = Nodes.Where(x => x == inputElements[i].GlobalNodesElement[j]).ToList();
                    if (nodes.Count == 0 || nodes.Count > 1)
                    {
                        throw new Exception("Something wrong with nodes");
                    } else
                    {
                        inputElements.ElementAt(i).GlobalNodesElement[j] = nodes[0];
                    }
                }
                elementsModel.Add(inputElements[i]);
            }
            Elements = elementsModel.ToArray();
            #endregion

            #region AssemblyOfStiffnessMatrix
            //Assembling the Stiffness Matrix
            int dimensionKSystemMatrix = 0;
            for (int i = 0; i < Nodes.Length; i++)
            {
                dimensionKSystemMatrix = dimensionKSystemMatrix + Nodes.ElementAt(i).NrActiveDof;
            }
            _KGlobal = Matrix<double>.Build.Dense(dimensionKSystemMatrix, dimensionKSystemMatrix);
            int counter = 0;
            for (int el = 0; el < Elements.Count(); el++)
            {
                //element el
                FiniteElement element = Elements.ElementAt(el);
                int dofActive = element.NrDOFActive;

                element.BuildMatrix();
                //Stiffness Matrix of element in global coordinates, KElementGlobal = GlobalToLocal ^ T * [KeLocal] * [GlobalToLocal]
                Matrix<double> KElementGlobalCoord = element.DofGlobalToLocal.Transpose() * element.KElementLocalCoord * element.DofGlobalToLocal;
                Console.WriteLine("KElementGlobalCoord = " + KElementGlobalCoord.ToString());

                for (int i = 0; i < element.GlobalNodesElement.Count(); i++)
                {
                    //Node i
                    int idNodeI = element.GlobalNodesElement[i].ID;

                    for (int j = 0; j < dofActive; j++) //each node i have degree of freedom j
                    {
                        for (int k = 0; k < element.GlobalNodesElement.Count(); k++) //each node i with its degree of freedom j should be take in account with other node k.What hap in node k if force is applied in node i?
                        {
                            int idNodeK = element.GlobalNodesElement[k].ID;

                            for (int l = 0; l < dofActive; l++) //what hap to the degree of freedom of node k?
                            {
                                counter++;
                                Console.WriteLine(counter + " El=" + el + " Node " + idNodeI + " DOF: "+ j + " vs  Node " + idNodeK + " DOF: " + l + "");
                                Console.WriteLine( (i * dofActive + j) +"," + (k * dofActive + l) + " --> " + "[" + GetPositionInKGlobal(idNodeI, (DOF)j) + "," + GetPositionInKGlobal(idNodeK, (DOF)l) + "]");
                                int rowGlobal = GetPositionInKGlobal(idNodeI, (DOF)j);
                                int colGlobal = GetPositionInKGlobal(idNodeK, (DOF)l);
                                int rowLocal = i * dofActive + j;
                                int colLocal = k * dofActive + l;
                                _KGlobal[rowGlobal, colGlobal] = _KGlobal[rowGlobal, colGlobal] + KElementGlobalCoord[rowLocal, colLocal];
                            }
                        }
                    }
                }

#if FALSE
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
#endif

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
            for (int i = 0; i < Elements.Length; i++)
            {
                //select only interested displacements
                FiniteElement element = Elements[i];
                int[] pos = new int[element.NrDOFActive * element.GlobalNodesElement.Length];
                
                element.CalcResults(nodeDisplacements.ToArray());
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
            for (int i = 0; i < Nodes.Length; i++)
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

        public int GetPositionInKGlobal(int ID, DOF dof)
        {
            int counter = 0;
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (Nodes.ElementAt(i).ID == ID)
                {
                    i = Nodes.Length;
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
