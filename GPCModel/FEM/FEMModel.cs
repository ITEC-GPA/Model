using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.FEM.FiniteElements;
using GPC.Geometry;

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
            RZ,   //5
        }

        public static int MAXGDLPERNODE = Enum.GetNames(typeof(DOF)).Length;

        Matrix<double> _KGlobal;
        Matrix<double> _KGlobalRestrains;
        Vector<double> _F;
        Vector<double> _FRestrains;
        HashSet<Costrain.MultiPointCostrain> _costrains;

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

                    var nodes = nodesModel.Where(n => n.Position.X == node.Position.X && n.Position.Y == node.Position.Y && n.Position.Z == node.Position.Z);

                    if (nodes.Count() > 1)
                    {
                        throw new Exception("Duplicate node!?");
                    } else if (nodes.Count() == 1) //Node already used in another element.
                    {
                        int ID = nodes.Single().Index;
                        if (node.Label != nodes.Single().Label)
                        {
                            node.Label = node.Label + "+" + nodes.Single().Label;
                        }

                        for (int k = 0; k < MAXGDLPERNODE; k++)
                        {
                            if (element.DOF[(DOF)k] == true)
                            {
                                node.DOF[(DOF)k] = true;
                            }

                            //Merge old DOF due to other element
                            if (nodes.Single().DOF[(DOF)k] == true)
                            {
                                node.DOF[(DOF)k] = true;
                            }
                        }

                        //Merge Attribute of node in other element in the node

                        //update the HashSet
                        nodesModel.Remove(nodes.Single());
                        node.SetID(ID);
                        nodesModel.Add(node);
                    }
                    else
                    {
                        for (int k = 0; k < MAXGDLPERNODE; k++)
                        {
                            if (element.DOF[(DOF)k] == true)
                            {
                                node.DOF[(DOF)k] = true;
                            }
                        }
                        node.SetID(iter);
                        nodesModel.Add(node);
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
                    var nodes = Nodes.Where(x => x == inputElements[i].GlobalNodesElement[j]).ToList();
                    if (nodes.Count == 0 || nodes.Count > 1)
                    {
                        throw new Exception("Something wrong with nodes");
                    } else
                    {
                        inputElements[i].GlobalNodesElement[j] = nodes[0];
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
                    int idNodeI = element.GlobalNodesElement[i].Index;

                    for (int j = 0; j < dofActive; j++) //each node i have degree of freedom j
                    {
                        //WARNING fare check ed eventualemte fixare per gradi di libertà attivi non contigui ad esempio UX, UY, UZ, RY
                        for (int k = 0; k < element.GlobalNodesElement.Count(); k++) //each node i with its degree of freedom j should be take in account with other node k.What hap in node k if force is applied in node i?
                        {
                            int idNodeK = element.GlobalNodesElement[k].Index;

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
            }
            Console.WriteLine("kGlobal System : " + _KGlobal.ToString());
            #endregion

            #region CalculationOfAppliedForcesF
            //Calculation of Forces vector
            _F = Vector<double>.Build.Dense(_KGlobal.RowCount);
            _F[GetPositionInKGlobal("3",DOF.DX).First()] = 1000;
            #endregion

            #region ApplyingRestrains
            _KGlobalRestrains = Matrix<double>.Build.Dense(_KGlobal.RowCount, _KGlobal.ColumnCount);
            _KGlobal.CopyTo(_KGlobalRestrains);
            _FRestrains = Vector<double>.Build.Dense(_F.Count);
            _F.CopyTo(_FRestrains);

            PrescribeDisplacement("1", DOF.DX, 0);
            PrescribeDisplacement("1", DOF.DY, 0);
            PrescribeDisplacement("1", DOF.DZ, 0);
            /*PrescribeDisplacement("1", DOF.RX, 0);
            PrescribeDisplacement("1", DOF.RY, 0);
            PrescribeDisplacement("1", DOF.RZ, 0);*/

            PrescribeDisplacement("2", DOF.DX, 0);
            PrescribeDisplacement("2", DOF.DY, 0);
            PrescribeDisplacement("2", DOF.DZ, 0);
            /*PrescribeDisplacement("2", DOF.RX, 0);
            PrescribeDisplacement("2", DOF.RY, 0);
            PrescribeDisplacement("2", DOF.RZ, 0);*/

            /*PrescribeDisplacement("3", DOF.DX, 0);
            PrescribeDisplacement("3", DOF.DY, 0);*/
            PrescribeDisplacement("3", DOF.DZ, 0);
            /*PrescribeDisplacement("3", DOF.RX, 0);
            PrescribeDisplacement("3", DOF.RY, 0);
            PrescribeDisplacement("3", DOF.RZ, 0);*/

            /*PrescribeDisplacement("4", GDL.UX, 0);
            PrescribeDisplacement("4", GDL.UY, 0);*/
            //PrescribeDisplacement("4", DOF.DZ, 0);
            /*PrescribeDisplacement("4", DOF.RX, 0);
            PrescribeDisplacement("4", DOF.RY, 0);
            PrescribeDisplacement("4", DOF.RZ, 0);*/

            Console.WriteLine("kGlobal System + Restrains: " + _KGlobalRestrains.ToString());
            Console.WriteLine("Fmodified(Restrains): " + _FRestrains.ToString());
            #endregion

            #region ApplyingMultiPointCostrains
            _costrains = new HashSet<Costrain.MultiPointCostrain>();
            //applying as example in node 1 : DX = DY (simply support with 45 degrees direction
            Costrain.MultiPointCostrain.Link[] equations = new Costrain.MultiPointCostrain.Link[2];
            equations[0] = new Costrain.MultiPointCostrain.Link("1", DOF.DX, 1.0);
            equations[1] = new Costrain.MultiPointCostrain.Link("1", DOF.DY, 1.0);
            Costrain.MultiPointCostrain Costrain1 = new Costrain.MultiPointCostrain(equations);
            //_costrains.Add(Costrain1);

            /* Use: Lagrange multiplier method 
             * rewrite constrains as : 1.0 * GdL NodeMaster + ValI * GdL NodeSlaveI + ... + ValN * GdL NodeSlaveN = const
             * and modify K matrix and F vector
             */

            int nLagrangianMultiplier = _costrains.Count;
            for (int i = 0; i < _costrains.Count; i++)
            {
                Costrain.MultiPointCostrain c = _costrains.ElementAt(i);
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
                #region SelectGlobalDisplacementForElement
                FiniteElement element = Elements[i];
                int[] pos = new int[element.NrDOFActive * element.GlobalNodesElement.Length];

                counter = 0;
                for (int j = 0; j < element.GlobalNodesElement.Count(); j++) {
                    Node node = element.GlobalNodesElement[j];
                    for (int k = 0; k < element.NrDOFActive; k++) {
                        pos[counter] = GetPositionInKGlobal(node.Index, (DOF)k);
                        counter++;
                    }
                }

                double[] globalDisplacementsNodesElement = new double[element.NrDOFActive * element.GlobalNodesElement.Length];
                for (int j = 0; j < element.NrDOFActive * element.GlobalNodesElement.Length; j++) {
                    globalDisplacementsNodesElement[j] = nodeDisplacements[pos[j]];
                    Console.WriteLine("Element " + i + " Displacemente global coordintates DOF nr. " + j + " = " + globalDisplacementsNodesElement[j]);
                }
                #endregion

                #region ConvertGlobalDisplacementToLocalDisplacement
                Vector<double> vecLocalDispl = element.DofGlobalToLocal * Vector<double>.Build.Dense(globalDisplacementsNodesElement);
                Console.WriteLine("Displacement in Local coordinates:" + vecLocalDispl.ToString());
                #endregion

                if (element is TriangularMembranal) {
                    #region CalculationOfStressAndDeformationsInLocalCoordinates
                    Vector<double> epsilon = Vector<double>.Build.Dense(3); //epsilon_xx; epsilon_yy; epsilon_xy
                    Vector<double> stress = Vector<double>.Build.Dense(epsilon.Count); //sigma_xx; sigma_yy; tau_xy

                    epsilon = element.B * vecLocalDispl;
                    stress = element.D * epsilon;
                    Console.WriteLine("Strains in Local coordinates:" + epsilon.ToString());
                    Console.WriteLine("Stress in Local coordinates:" + stress.ToString());
                    #endregion

                    #region ConvertInGlobalCoordinates
                    //Define Couchy Tensor
                    Matrix<double> epsilonCouchy = Matrix<double>.Build.Dense(3, 3);
                    epsilonCouchy[0, 0] = epsilon[0]; //epsilon_xx
                    epsilonCouchy[1, 1] = epsilon[1]; //epsilon_yy
                    epsilonCouchy[0, 1] = epsilon[2]; //epsilon_xy
                    epsilonCouchy[1, 0] = epsilon[2]; //epsilon_yx
                    Console.WriteLine("Epsilon local coordinate:" + epsilonCouchy.ToString());

                    Matrix<double> stressCouchy = Matrix<double>.Build.Dense(3, 3);
                    stressCouchy[0, 0] = stress[0]; //sigma_xx
                    stressCouchy[1, 1] = stress[1]; //sigma_yy
                    stressCouchy[0, 1] = stress[2]; //sigma_xy
                    stressCouchy[1, 0] = stress[2]; //sigma_yx
                    Console.WriteLine("Stress local coordinate:" + stressCouchy.ToString());

                    //Rotation matrix
                    Matrix<double> rotation = Matrix<double>.Build.Dense(3, 3);
                    CoordinateSystem versorsLocalAxis = element.LocalCoordinateSystem;
                    Vector3d xVersor = versorsLocalAxis.V11;
                    Vector3d yVersor = versorsLocalAxis.V22;
                    Vector3d zVersor = versorsLocalAxis.V33;

                     rotation[0, 0] = xVersor.X; 
                    rotation[0, 1] = yVersor.X;
                    rotation[0, 2] = zVersor.X;

                    rotation[1, 0] = xVersor.Y;
                    rotation[1, 1] = yVersor.Y;
                    rotation[1, 2] = zVersor.Y;

                    rotation[2, 0] = xVersor.Z;
                    rotation[2, 1] = yVersor.Z;
                    rotation[2, 2] = zVersor.Z;
                    Console.WriteLine("Rotation matrix tensor:" + rotation.ToString());

                    //Second order tensor -> Trotated = Q * T * Q^T
                    Matrix<double> epsilonGlobalCoord = rotation * epsilonCouchy * rotation.Transpose();
                    Console.WriteLine("Epsilon in global coordinates = " + epsilonGlobalCoord);
                    Matrix<double> sigmaGlobalCoord = rotation * stressCouchy * rotation.Transpose();
                    Console.WriteLine("Stress in global coordinates = " + sigmaGlobalCoord);
                    #endregion
                } else
                {
                    throw new NotImplementedException("retrieve result not implemented");
                }
            }
            #endregion
        }

        #region PublicFuction
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

        public double GetDisplacementGlobalCoordinates(FiniteElement element, Node node, DOF dof)
        {
            //read in saved result
            throw new NotImplementedException();
        }
        #endregion

        #region PrivateFunction
        /// <summary>
        /// Give the position of selected GDL from 0 to N where N is dimension of matrix KGloabl or the dimension of the vector of Forces or Displacments
        /// </summary>
        /// <param name="labelNode">Label of the node or nodes searched</param>
        /// <param name="gdl">GDL searched</param>
        /// <returns></returns>
        private int[] GetPositionInKGlobal(string labelNode, DOF dof = 0)
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

        /// <summary>
        /// return the position in Gloab System Matrix of in F vector or Displacement vector
        /// </summary>
        /// <param name="IdNode">ID node</param>
        /// <param name="dof">Searched dof</param>
        /// <returns></returns>
        private int GetPositionInKGlobal(int IdNode, DOF dof = 0)
        {
            int counter = 0;
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (Nodes.ElementAt(i).Index == IdNode)
                {
                    i = Nodes.Length;
                }
                else
                {
                    counter = counter + Nodes.ElementAt(i).NrActiveDof;
                }
            }
            return counter + (int)dof;
        }
        #endregion       
    }
}
