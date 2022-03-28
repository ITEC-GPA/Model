using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Model.FEM.Costrains;
using GPC.Model.FEM.FiniteElements;
using MathNet.Numerics;
using mnl = MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.FEM
{
    public class LinearSolver : Solver
    {
        #region variables
        protected mnl.Matrix<double> _KGlobal;
        protected mnl.Matrix<double> _KGlobalRestrains;
        protected mnl.Vector<double> _F;
        protected mnl.Vector<double> _FRestrains;
        protected MultiPointsCostrain[] _multiPointCostrains;
        protected mnl.Vector<double> _nodeGlobalDisplacements;
        protected mnl.Vector<double> _reactions;

        protected Dictionary<Point3d, int> _position = new Dictionary<Point3d, int>();
        #endregion

        #region Properties
        public mnl.Matrix<double> KGlobal => _KGlobal;
        public mnl.Matrix<double> KGlobalRestrains => _KGlobalRestrains;
        public mnl.Vector<double> F => _F;
        public mnl.Vector<double> FRestrains => _FRestrains;
        public mnl.Vector<double> NodeGlobalDisplacements => _nodeGlobalDisplacements;

        /// <summary>
        /// Unique nodes in model
        /// </summary>
        public Node[] Nodes { get; }

        /// <summary>
        /// Unique elements in Models contains all the informations: node connectivity, material, property, LOAD as attribute, end releases...etc
        /// </summary>
        public FiniteElement[] Elements { get; }
        #endregion

        #region Constructor
        public LinearSolver(FiniteElement[] inputElements) : this(inputElements, new Costrain[0], new MultiPointsCostrain[0])
        {
        }

        public LinearSolver(FiniteElement[] inputElements, MultiPointsCostrain[] multiPointsCostrains) : this(inputElements, new Costrain[0], multiPointsCostrains)
        {
        }

        public LinearSolver(FiniteElement[] inputElements, Costrain[] costrains) : this(inputElements, costrains, new MultiPointsCostrain[0])
        {
        }

        public LinearSolver(FiniteElement[] inputElements, Costrain[] costrains, MultiPointsCostrain[] multiPointCostrains)
        {
            Control.UseNativeMKL();

            #region NodeOfModel
            HashSet<Node> nodesModel = new HashSet<Node>();
            int iter = 0;
            for (int i = 0; i < inputElements.Count(); i++) //over element i
            {
                FiniteElement element = inputElements[i];

                for (int j = 0; j < element.Nodes.Count(); j++) //over node j
                {
                    Node node = element.Nodes[j];

                    var nodes = nodesModel.Where(n => n.Position.X == node.Position.X && n.Position.Y == node.Position.Y && n.Position.Z == node.Position.Z);

                    if (nodes.Count() > 1)
                    {
                        throw new Exception("Duplicate node!?");
                    }
                    else if (nodes.Count() == 1) //Node already used in another element.
                    {
                        Node oldNode = nodes.Single();
                        int ID = oldNode.Id; //tra i 2 ID da poter scegliere uso quello del nodo già usato

                        for (int k = 0; k < MAXDOFPERNODE; k++) //over degree of freedom
                        {
                            if (element.DOF.Contains((DOF)k))
                            {
                                node.DOF.Add((DOF)k);
                            }

                            //Merge old DOF due to other element
                            if (oldNode.DOF.Contains((DOF)k))
                            {
                                node.DOF.Add((DOF)k);
                            }
                        }

                        //Merge Attribute of old-duplicatenode the new node
                        foreach (FreedomCaseAttribute freedomCasecAttribute in oldNode.AttributesFreedomCase)
                        {
                            if (freedomCasecAttribute is NodeRestrainAttribute)
                            {
                                NodeRestrainAttribute restrainAttribute = (NodeRestrainAttribute)freedomCasecAttribute;
                                //check if already exist
                                if (node.AttributesFreedomCase.Contains(restrainAttribute) == false)
                                {
                                    //Copy
                                    node.AttributesFreedomCase.Add(new NodeRestrainAttribute(restrainAttribute.FreedomCaseName, restrainAttribute.CoordinateSystem, restrainAttribute.Restrains, restrainAttribute.Name, Guid.NewGuid()));
                                }
                            }
                        }

                        //update the HashSet
                        nodesModel.Remove(oldNode);
                        node.SetId(ID);
                        nodesModel.Add(node);
                    }
                    else
                    {
                        //New Node
                        for (int k = 0; k < MAXDOFPERNODE; k++)
                        {
                            if (element.DOF.Contains((DOF)k) == true)
                            {
                                node.DOF.Add((DOF)k);
                            }
                        }
                        node.SetId(iter);
                        nodesModel.Add(node);
                        iter++;
                    }
                }
            }

            //nodes due to Links
            #region NewNodeFromCostrain
            for (int i = 0; i < costrains.Length; i++)
            {
                if (costrains[i].GetType() == typeof(RigidLink))
                {
                    RigidLink rigidLink = (RigidLink)costrains[i];
                    for (int k = 0; k < rigidLink.Links.Count(); k++)
                    {
                        MultiPointsCostrain mpcostrain = rigidLink.Links[k];

                        for (int j = 0; j < mpcostrain.Equations.Length; j++)
                        {
                            Node node = mpcostrain.Equations[j].NodeSlave;
                            var nodes = nodesModel.Where(n => n.Position.X == node.Position.X && n.Position.Y == node.Position.Y && n.Position.Z == node.Position.Z);
                            if (nodes.Count() == 0)
                            {
                                //New node not used in elements
                                //Activate DOF in order to have place in stiffness matrix and Fvector
                                node.DOF.Add(DOF.DX);
                                node.DOF.Add(DOF.DY);
                                node.DOF.Add(DOF.DZ);
                                node.DOF.Add(DOF.RX);
                                node.DOF.Add(DOF.RY);
                                node.DOF.Add(DOF.RZ);
                                nodesModel.Add(node);
                            }
                        }
                    }
                }
            }
            #endregion

            #region NewNodeFromMultiPointCostrain
            for (int i = 0; i < multiPointCostrains.Length; i++)
            {
                MultiPointsCostrain mpcostrain = multiPointCostrains[i];

                for (int j = 0; j < mpcostrain.Equations.Length; j++)
                {
                    Node node = mpcostrain.Equations[j].NodeSlave;
                    var nodes = nodesModel.Where(n => n.Position.X == node.Position.X && n.Position.Y == node.Position.Y && n.Position.Z == node.Position.Z);
                    if (nodes.Count() == 0)
                    {
                        //New node not used in elements
                        //Activate DOF in order to have place in stiffness matrix and Fvector
                        node.DOF.Add(DOF.DX);
                        node.DOF.Add(DOF.DY);
                        node.DOF.Add(DOF.DZ);
                        node.DOF.Add(DOF.RX);
                        node.DOF.Add(DOF.RY);
                        node.DOF.Add(DOF.RZ);
                        nodesModel.Add(node);
                    }
                }
            }
            #endregion
            Nodes = nodesModel.ToArray();
            int nrNodes = Nodes.Length;
            #endregion

            #region ElementsOfModel
            //Assumed that geometry has been meshed and forces and property applied inside elements
            HashSet<FiniteElement> elementsModel = new HashSet<FiniteElement>();
            for (int i = 0; i < inputElements.Count(); i++) //over element i
            {
                //update node
                for (int j = 0; j < inputElements[i].Nodes.Count(); j++) //over node j
                {
                    var nodes = Nodes.Where(x => x.Position == inputElements[i].Nodes[j].Position).ToList();
                    if (nodes.Count == 0 || nodes.Count > 1)
                    {
                        throw new Exception("Something wrong with nodes");
                    }
                    else
                    {
                        inputElements[i].Nodes[j] = nodes[0]; //set updated node
                    }
                }
                elementsModel.Add(inputElements[i]); //add element with "new nodes" in model
            }
            Elements = elementsModel.ToArray();
            #endregion

            #region AssemblyOfStiffnessMatrix
            //Assembling the Stiffness Matrix

            #region CalcoloDimensioneMatrice
            int dimensionKSystemMatrix = 0;
            for (int i = 0; i < Nodes.Length; i++)
            {
                dimensionKSystemMatrix = dimensionKSystemMatrix + Nodes.ElementAt(i).NrActiveDof;
            }
            Console.WriteLine("dimensione K = " + dimensionKSystemMatrix);
            #endregion

            _KGlobal = mnl.Matrix<double>.Build.Dense(dimensionKSystemMatrix, dimensionKSystemMatrix);
            int counter = 0;
            for (int el = 0; el < Elements.Count(); el++) //over element el
            {
                FiniteElement element = Elements.ElementAt(el);
                int dofActive = element.NrDOFActive;

                element.BuildMatrix();
                //Stiffness Matrix of element in global coordinates, KElementGlobal = GlobalToLocal ^ T * [KeLocal] * [GlobalToLocal]
                mnl.Matrix<double> KElementGlobalCoord = element.KElementGlobalCoord;
                //Console.WriteLine("KElementGlobalCoord element " + el);
                //FEMUtilities.WriteMatrix(KElementGlobalCoord);

                for (int i = 0; i < element.Nodes.Count(); i++) //node i - over nodes of element
                {
                    Node nodeI = element.Nodes[i];  //Node i

                    for (int j = 0; j < dofActive; j++) //each node i have degree of freedom j
                    {
                        //TODO: fare check ed eventualemte fixare per gradi di libertà attivi non contigui ad esempio UX, UY, UZ, RY
                        for (int k = 0; k < element.Nodes.Count(); k++) //each node i with its degree of freedom j should be take in account with other node k. -> What hap in node k if force is applied in node i?
                        {
                            Node nodeK = element.Nodes[k];

                            for (int l = 0; l < dofActive; l++) //what hap to the degree of freedom of node k?
                            {
                                counter++;
                                //Console.WriteLine(counter + " El=" + el + " Node "+ element.Nodes[i].Name + " (id=" + idNodeI + ") DOF: " + (LinearSolver.DOF)j + "("+ j + ") vs  Node "+ element.Nodes[k].Name + " (id=" + idNodeK + ") DOF: " +(LinearSolver.DOF)l +"(" + l + ")");
                                //Console.WriteLine( (i * dofActive + j) +"," + (k * dofActive + l) + " --> " + "[" + GetPositionInKGlobal(idNodeI, (DOF)j) + "," + GetPositionInKGlobal(idNodeK, (DOF)l) + "]");

                                /*int rowGlobal = GetPositionInKGlobal(idNodeI, (DOF)j);
                                int colGlobal = GetPositionInKGlobal(idNodeK, (DOF)l);*/

                                int rowGlobal = GetPositionInKGlobal(nodeI, (DOF)j);
                                int colGlobal = GetPositionInKGlobal(nodeK, (DOF)l);

                                int rowLocal = i * dofActive + j;
                                int colLocal = k * dofActive + l;
                                _KGlobal[rowGlobal, colGlobal] = _KGlobal[rowGlobal, colGlobal] + KElementGlobalCoord[rowLocal, colLocal];
                            }
                            //Console.WriteLine();
                        }
                    }
                }
            }
#if DEBUG
            /*Console.WriteLine("kGlobal System :");
            FEMUtilities.WriteMatrix(_KGlobal, "F3");*/
#endif
            #endregion

            #region CalculationOfAppliedForcesF
            Dictionary<int, string> legend = new Dictionary<int, string>(); //Key = Position in vector F, Value = "Nodo XX DOFXXX", 
            Dictionary<int, DOF> legendDOF = new Dictionary<int, DOF>(); //Key = Position in vector F, Value = "DOFXXX", 
            //Calculation of Forces vector
            _F = mnl.Vector<double>.Build.Dense(_KGlobal.RowCount);

            Vector3d X = new Vector3d(1.0, 0.0, 0.0);
            Vector3d Y = new Vector3d(0.0, 1.0, 0.0);
            Vector3d Z = new Vector3d(0.0, 0.0, 1.0);

            #region ForceFromNodes
            for (int i = 0; i < Nodes.Count(); i++)
            {
                #region CreationLegendOfFvector
                Nodes[i].DOF.ToList().ForEach(dof =>
                {
                    legend.Add(GetPositionInKGlobal(Nodes[i], dof), Nodes[i].ToString() + " " + dof);
                    legendDOF.Add(GetPositionInKGlobal(Nodes[i], dof), dof);
                }
                );
                #endregion

                List<string> dofs = Enum.GetNames(typeof(DOF)).ToList();

                foreach (LoadCaseAttribute loadCaseAttribute in Nodes[i].AttributesLoadCase)
                {
                    if (loadCaseAttribute is NodeForceAttribute)
                    {
                        NodeForceAttribute nodeForceAttribute = (NodeForceAttribute)loadCaseAttribute;

                        Vector3d dirX = nodeForceAttribute.CoordinateSystem.V1;
                        dirX.Unitize();
                        Vector3d dirY = nodeForceAttribute.CoordinateSystem.V2;
                        dirY.Unitize();
                        Vector3d dirZ = nodeForceAttribute.CoordinateSystem.V3;
                        dirZ.Unitize();

                        //Set in global coordinates
                        double[] additionalForce = new double[dofs.Count];
                        additionalForce[0] = nodeForceAttribute.F1 * dirX.DotProduct(X) + nodeForceAttribute.F2 * dirY.DotProduct(X) + nodeForceAttribute.F3 * dirZ.DotProduct(X); //fX
                        additionalForce[1] = nodeForceAttribute.F1 * dirX.DotProduct(Y) + nodeForceAttribute.F2 * dirY.DotProduct(Y) + nodeForceAttribute.F3 * dirZ.DotProduct(Y); //fY
                        additionalForce[2] = nodeForceAttribute.F1 * dirX.DotProduct(Z) + nodeForceAttribute.F2 * dirY.DotProduct(Z) + nodeForceAttribute.F3 * dirZ.DotProduct(Z); //fZ

                        additionalForce[3] = nodeForceAttribute.M1 * dirX.DotProduct(X) + nodeForceAttribute.M2 * dirY.DotProduct(X) + nodeForceAttribute.M3 * dirZ.DotProduct(X); //mX
                        additionalForce[4] = nodeForceAttribute.M1 * dirX.DotProduct(Y) + nodeForceAttribute.M2 * dirY.DotProduct(Y) + nodeForceAttribute.M3 * dirZ.DotProduct(Y); //mY
                        additionalForce[5] = nodeForceAttribute.M1 * dirX.DotProduct(Z) + nodeForceAttribute.M2 * dirY.DotProduct(Z) + nodeForceAttribute.M3 * dirZ.DotProduct(Z); //mZ

                        //Modify F vector adding the forces from the node
                        dofs.ForEach(
                            (stringDOF) =>
                            {
                                DOF dof = (DOF)Enum.Parse(typeof(DOF), stringDOF);
                                if (Nodes[i].DOF.Contains(dof) == true)
                                {
                                    _F[GetPositionInKGlobal(Nodes[i], dof)] = _F[GetPositionInKGlobal(Nodes[i], dof)] + additionalForce[dofs.IndexOf(stringDOF)]; //modifico termine noto delle forze
                                }
                            });
                    }
                }
            }
            #endregion

            #region ForceFromElements
            for (int i = 0; i < Elements.Count(); i++) //cycle over elements
            {
                FiniteElement element = Elements[i];
                mnl.Vector<double> FElementGlobalCoord = element.GetGlobalCoordF();
                for (int j = 0; j < element.Nodes.Length; j++) //cycle over nodes of element
                {
                    Node node = element.Nodes[j];
                    Node[] nds = Nodes.Where(x => x.Position == node.Position).ToArray();

                    //just a check
                    if (nds.Length <= 0 || nds.Length > 1)
                    {
                        throw new Exception("something wrong with nodes of element");
                    }
                    Node nodeFound = nds[0];

                    //check which DOF are active for the node to put the force in the right position
                    for (int k = 0; k < nodeFound.NrActiveDof; k++)
                    {
                        //int pos = GetPositionInKGlobal(nodFound.Id, nds[0].DOF.ElementAt(k));
                        int pos = GetPositionInKGlobal(nodeFound, nds[0].DOF.ElementAt(k));

                        //search in local vector the value in DOF selected
                        double val = 0;
                        for (int l = 0; l < element.DOF.Count; l++) //un elemento puà avere attivo DX, DY, DZ ma nel nodo può essere attivo anche MX, MY, MZ se un altro elemento attaccato a quel nodo ha attivi quei gdl
                        {
                            DOF dof = element.DOF.ElementAt(l);

                            if (dof == nodeFound.DOF.ElementAt(k))
                            {
                                val = FElementGlobalCoord[j * element.DOF.Count + l];
                            }
                        }

                        _F[pos] = _F[pos] + val; //modifico il vettore noto delle forze
                    }
                }
            }
            #endregion


#if DEBUG
            /*Console.WriteLine("Vector F");
            _F.ToList().ForEach(x => Console.WriteLine(x));*/
#endif

            #endregion

            #region ApplyingRestrains
            _KGlobalRestrains = mnl.Matrix<double>.Build.Dense(_KGlobal.RowCount, _KGlobal.ColumnCount);
            _KGlobal.CopyTo(_KGlobalRestrains); //Copio matrice in modo da avere la KGlobal originale che mi servirà per il calcolo delle reazioni
            _FRestrains = mnl.Vector<double>.Build.Dense(_F.Count);
            _F.CopyTo(_FRestrains); //copio vettore termini noti / forze applicate

            for (int i = 0; i < Nodes.Count(); i++)
            {
                foreach (FreedomCaseAttribute freedomCasecAttribute in Nodes[i].AttributesFreedomCase)
                {
                    if (freedomCasecAttribute is NodeRestrainAttribute)
                    {
                        NodeRestrainAttribute restrainAttribute = (NodeRestrainAttribute)freedomCasecAttribute;
                        //check if is in Global Coordinate otherwise ...
                        Vector3d dirX = restrainAttribute.CoordinateSystem.V1;
                        dirX.Unitize();
                        Vector3d dirY = restrainAttribute.CoordinateSystem.V2;
                        dirY.Unitize();
                        Vector3d dirZ = restrainAttribute.CoordinateSystem.V3;
                        dirZ.Unitize();

                        if (dirX.DotProduct(X) == 1.0 && dirY.DotProduct(Y) == 1.0) //Coord sys == Global Coord
                        {
                            foreach (var restrain in restrainAttribute.Restrains)
                            {
                                //PrescribeDisplacement(Nodes[i].Id, restrain.Dof, restrain.ImposedDisplacement);
                                PrescribeDisplacement(Nodes[i], restrain.Dof, restrain.ImposedDisplacement);
                            }

                        }
                        else
                        {
                            throw new NotImplementedException();
                        }

                    }
                }
            }
            /*#if DEBUG
                        Console.WriteLine("kGlobal System + Restrains: ");
                        FEMUtilities.WriteMatrix(_KGlobalRestrains, "F3");
                        Console.WriteLine("Fmodified(Restrains):");
                        FEMUtilities.WriteVector(_FRestrains);
            #endif*/
            #endregion

            #region ApplyingMultiPointCostrains
            //applying as example in node 1 : DX = DY (simply support with 45 degrees direction

            /* EXAMPLE:
             * MultiPointCostrain.Link[] equations = new MultiPointCostrain.Link[2];
             * equations[0] = new MultiPointCostrain.Link("1", DOF.DX, 1.0);
             * equations[1] = new MultiPointCostrain.Link("1", DOF.DY, 1.0);
             * MultiPointCostrain Costrain1 = new MultiPointCostrain(equations);*/
            //_costrains.Add(Costrain1);

            /* Use: Lagrange multiplier method 
             * rewrite constrains as : 1.0 * GdL NodeMaster + ValI * GdL NodeSlaveI + ... + ValN * GdL NodeSlaveN = const
             * and modify K matrix and F vector
             */

            #region MultiPointCostrainDueToCostrainClass
            HashSet<MultiPointsCostrain> multiPointCostrainsSet = new HashSet<MultiPointsCostrain>();
            for (int i = 0; i < costrains.Length; i++)
            {
                if (costrains[i].GetType() == typeof(RigidLink))
                {
                    RigidLink rigidLink = (RigidLink)costrains[i];
                    for (int j = 0; j < rigidLink.Links.Length; j++)
                    {
                        multiPointCostrainsSet.Add(rigidLink.Links[j]);
                    }
                }
            }
            #endregion

            #region AddMultiPointCostrainDueToMultiPointManualAdded
            for (int i = 0; i < multiPointCostrains.Count(); i++)
            {
                multiPointCostrainsSet.Add(multiPointCostrains[i]);
            }
            #endregion

            _multiPointCostrains = multiPointCostrainsSet.ToArray();

            //check if all multipointcostrain has been added
            //if (_multiPointCostrains.Count() < multiPointCostrains.Length)

            int nLagrangianMultiplier = _multiPointCostrains.Length;
            for (int i = 0; i < _multiPointCostrains.Length; i++)
            {
                MultiPointsCostrain c = _multiPointCostrains[i]; //select equation constrain

#if DEBUG
                Console.WriteLine(c.ToString());
#endif

                #region ModificaStiffnessMatrixPerInserimentoCostrain
                mnl.Vector<double> voidVector = mnl.Vector<double>.Build.Dense(_KGlobalRestrains.RowCount);
                _KGlobalRestrains = _KGlobalRestrains.InsertColumn(_KGlobalRestrains.ColumnCount, voidVector);
                voidVector = mnl.Vector<double>.Build.Dense(_KGlobalRestrains.ColumnCount);
                _KGlobalRestrains = _KGlobalRestrains.InsertRow(_KGlobalRestrains.RowCount, voidVector);
                #endregion

                #region AggiornamentoTermineNoto
                //add a row to F vector and update it
                _FRestrains.CopySubVectorTo(voidVector, 0, 0, _FRestrains.Count);
                _FRestrains = voidVector;
                _FRestrains[_FRestrains.Count - 1] = c.ConstValue;

                /*_F.CopySubVectorTo(voidVector, 0, 0, _F.Count);
                _F = voidVector;*/
                #endregion

                #region AggiornamentoMatriceDiRigidezza
                for (int j = 0; j < c.Equations.Length; j++)
                {
                    Node nodeSlave = c.Equations[j].NodeSlave;
                    DOF gdlNodeSlave = c.Equations[j].GdlNode;

                    int positionGDLNodeSlave = GetPositionInKGlobal(nodeSlave, gdlNodeSlave);
                    _KGlobalRestrains[positionGDLNodeSlave, _KGlobalRestrains.ColumnCount - 1] = c.Equations[j].Value;
                    _KGlobalRestrains[_KGlobalRestrains.RowCount - 1, positionGDLNodeSlave] = c.Equations[j].Value;
                }
                #endregion
            }
            /*#if DEBUG
                        Console.WriteLine("kGlobal System + Restrains + Constrains:");
                        FEMUtilities.WriteMatrix(_KGlobalRestrains, "F5");
                        Console.WriteLine("F (Restrain + Costrains):");
                        FEMUtilities.WriteMatrix(_FRestrains, "F3");
            #endif*/
            #endregion

            #region SolveModel
            //Solve Matrix
            _nodeGlobalDisplacements = _KGlobalRestrains.Solve(_FRestrains);
            Console.WriteLine("Node displacements results:");

            for (int i = 0; i < _nodeGlobalDisplacements.Count; i++)
            {
                if (i < legend.Count)
                {
                    Console.WriteLine("Displ. " + legend[i] + " : \t " + _nodeGlobalDisplacements[i].ToString("F3"));
                }
                else
                {
                    Console.WriteLine("Lagrangian Multiplicator: : \t " + _nodeGlobalDisplacements[i].ToString("F3"));
                }
            }
            #endregion

            #region Reactions
            mnl.Vector<double> nodeGlobalDisplacementWithoutLagrangian = mnl.Vector<double>.Build.Dense(_nodeGlobalDisplacements.Count - nLagrangianMultiplier);
            _nodeGlobalDisplacements.CopySubVectorTo(nodeGlobalDisplacementWithoutLagrangian, 0, 0, _nodeGlobalDisplacements.Count - nLagrangianMultiplier);
            _reactions = _KGlobal * nodeGlobalDisplacementWithoutLagrangian - _F;
            double sumFX = 0.0;
            double sumFY = 0.0;
            double sumFZ = 0.0;

            Console.WriteLine();
            for (int i = 0; i < _reactions.Count; i++)
            {
#if DEBUG
                Console.WriteLine("React." + legend[i] + " : \t " + _reactions[i].ToString("F3"));
#endif
                if (legendDOF[i] == DOF.DX)
                {
                    sumFX = sumFX + _reactions[i];
                }
                else if (legendDOF[i] == DOF.DY)
                {
                    sumFY = sumFY + _reactions[i];
                }
                else if (legendDOF[i] == DOF.DZ)
                {
                    sumFZ = sumFZ + _reactions[i];
                }
            }
            Console.WriteLine("Sum of FX = " + sumFX);
            Console.WriteLine("Sum of FY = " + sumFY);
            Console.WriteLine("Sum of FZ = " + sumFZ);
            #endregion

            #region CalcResults
            #endregion
        }
        #endregion

        #region PublicFuction

        #region GetDisplacement
        /// <summary>
        /// Ritorna spostamento per un selezionato nodo e per un certp grado di libertà.
        /// </summary>
        /// <param name="node"></param>
        /// <param name="dof"></param>
        /// <returns></returns>
        public double GetNodeDisplacementGlobalCoordinates(Node node, DOF dof)
        {
            int pos = GetPositionInKGlobal(node, dof);
            return _nodeGlobalDisplacements[pos];
        }

        public Dictionary<DOF, double> GetNodeDisplacementGlobalCoordinates(Node node)
        {
            Dictionary<DOF, double> displ = new Dictionary<DOF, double>();
            foreach (DOF d in node.DOF)
            {
                int pos = GetPositionInKGlobal(node, d);
                displ.Add(d, _nodeGlobalDisplacements[pos]);
            }

            return displ;
        }

        public double[] GetDisplacementsAtNodesOfElementInGlobalCoordinates(FiniteElement e)
        {
            #region SelectGlobalDisplacementForElement
            var elements = Elements.Where(x => x == e);
            if (elements.Count() == 1)
            {
                FiniteElement element = elements.First();
                int[] pos = new int[element.NrDOFActive * element.Nodes.Length];

                int counter = 0;
                for (int j = 0; j < element.Nodes.Count(); j++)
                {
                    Node node = element.Nodes[j];
                    for (int k = 0; k < element.NrDOFActive; k++) //attenzione qui, se c'è un elemento con gdl attivi non contigui....
                    {
                        //pos[counter] = GetPositionInKGlobal(node.Id, (DOF)k);
                        pos[counter] = GetPositionInKGlobal(node, (DOF)k);
                        counter++;
                    }
                }

                double[] globalDisplacementsNodesElement = new double[element.NrDOFActive * element.Nodes.Length];
                for (int j = 0; j < element.NrDOFActive * element.Nodes.Length; j++)
                {
                    globalDisplacementsNodesElement[j] = _nodeGlobalDisplacements[pos[j]];
                    //Console.WriteLine("Element " + i + " Displacemente global coordintates DOF nr. " + j + " = " + globalDisplacementsNodesElement[j]);
                }

                return globalDisplacementsNodesElement;
                //get results of element
                //element.GetResults(globalDisplacementsNodesElement, out double[] localDisplacements, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
                #endregion
            }
            else
            {
                throw new Exception("More than 1 element selected");
            }
        }
        #endregion

        #region GetResultDK
        private void DKQGetCsiEta(int indexNode, out double csi, out double eta)
        {
            if (indexNode == 1)
            {
                csi = -1.0;
                eta = -1.0;
            }
            else if (indexNode == 2)
            {
                csi = 1.0;
                eta = -1.0;
            }
            else if (indexNode == 3)
            {
                csi = 1.0;
                eta = 1.0;
            }
            else if (indexNode == 4)
            {
                csi = -1.0;
                eta = 1.0;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        private void DKTGetCsiEta(int indexNode, out double csi, out double eta)
        {
            if (indexNode == 1)
            {
                csi = 0.0;
                eta = 0.0;
            }
            else if (indexNode == 2)
            {
                csi = 1.0;
                eta = 0.0;
            }
            else if (indexNode == 3)
            {
                csi = 0.0;
                eta = 1.0;
            }
            else
            {
                throw new ArgumentOutOfRangeException();
            }
        }

        public mnl.Matrix<double> GetDKCurvatures(DK element, int indexNode, CoordinateSystem newSys = null)
        {
            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            double csi, eta;
            if (element.Nodes.Count() == 3)
            {
                DKTGetCsiEta(indexNode, out csi, out eta);
            }
            else
            {
                DKQGetCsiEta(indexNode, out csi, out eta);
            }

            return element.GetCurvatures(csi, eta, globalDispl, newSys);
        }

        public mnl.Matrix<double> GetDKBending(DK element, int indexNode, CoordinateSystem newSys = null)
        {
            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            double csi, eta;
            if (element.Nodes.Count() == 3)
            {
                DKTGetCsiEta(indexNode, out csi, out eta);
            }
            else
            {
                DKQGetCsiEta(indexNode, out csi, out eta);
            }

            return element.GetBending(csi, eta, globalDispl, newSys);
        }

        public mnl.Matrix<double> GetDKStrains(DK element, int indexNode, Plate.Face face, CoordinateSystem newSys = null)
        {
            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            double csi, eta;
            if (element.Nodes.Count() == 3)
            {
                DKTGetCsiEta(indexNode, out csi, out eta);
            }
            else
            {
                DKQGetCsiEta(indexNode, out csi, out eta);
            }

            return element.GetStrains(face, csi, eta, globalDispl, newSys);
        }

        public mnl.Matrix<double> GetDKStress(DK element, int indexNode, Plate.Face face, CoordinateSystem newSys = null)
        {
            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            double csi, eta;
            if (element.Nodes.Count() == 3)
            {
                DKTGetCsiEta(indexNode, out csi, out eta);
            }
            else
            {
                DKQGetCsiEta(indexNode, out csi, out eta);
            }

            return element.GetStress(face, csi, eta, globalDispl, newSys);
        }
        #endregion

        #region GetResultTLG

        #region Glass
        public mnl.Vector<double> GetTLGGlassStrain(Quad4TriplexLaminatedGlassIvanov element, Quad4TriplexLaminatedGlassIvanov.Glass glass, Plate.Face face, int indexNode)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            double x = element.LocalNodes[indexNode - 1].Position.X;
            double y = element.LocalNodes[indexNode - 1].Position.Y;

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetStrainGlass(glass, face, x, y, globalDispl);
        }

        public mnl.Vector<double> GetTLGGlassStress(Quad4TriplexLaminatedGlassIvanov element, Quad4TriplexLaminatedGlassIvanov.Glass glass, Plate.Face face, int indexNode)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            double x = element.LocalNodes[indexNode - 1].Position.X;
            double y = element.LocalNodes[indexNode - 1].Position.Y;

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetStressGlass(glass, face, x, y, globalDispl);
        }

        public mnl.Vector<double> GetTLGGlassBending(Quad4TriplexLaminatedGlassIvanov element, int indexNode)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            double x = element.LocalNodes[indexNode - 1].Position.X;
            double y = element.LocalNodes[indexNode - 1].Position.Y;

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetGlassBending(x, y, globalDispl);
        }

        public mnl.Vector<double> GetTLGGlassForces(Quad4TriplexLaminatedGlassIvanov element, int indexNode)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            double x = element.LocalNodes[indexNode - 1].Position.X;
            double y = element.LocalNodes[indexNode - 1].Position.Y;

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetGlassForces(x, y, globalDispl);
        }
        #endregion

        #region Interlayer
        public mnl.Vector<double> GetTLGInterlayerStrain(Quad4TriplexLaminatedGlassIvanov element, int indexNode)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            double x = element.LocalNodes[indexNode - 1].Position.X;
            double y = element.LocalNodes[indexNode - 1].Position.Y;

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetStrainInterlayer(x, y, globalDispl);
        }

        public mnl.Vector<double> GetTLGInterlayerStress(Quad4TriplexLaminatedGlassIvanov element, int indexNode)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            double x = element.LocalNodes[indexNode - 1].Position.X;
            double y = element.LocalNodes[indexNode - 1].Position.Y;

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetStressInterlayer(x, y, globalDispl);
        }

        public mnl.Vector<double> GetTLGInterlayerBending(Quad4TriplexLaminatedGlassIvanov element, int indexNode)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            double x = element.LocalNodes[indexNode - 1].Position.X;
            double y = element.LocalNodes[indexNode - 1].Position.Y;

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetBendingInterlayer(x, y, globalDispl);
        }
        #endregion
        #endregion

        #region GetResultQuad4TLG2

        private void Quad4TLG2GetCsiEta(int indexNode, out double csi, out double eta)
        {
            switch (indexNode)
            {
                case 1:
                    csi = -1;
                    eta = -1;
                    break;
                case 2:
                    csi = 1;
                    eta = -1;
                    break;
                case 3:
                    csi = 1;
                    eta = 1;
                    break;
                case 4:
                    csi = -1;
                    eta = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Glass
        public mnl.Matrix<double> GetQuad4TLG2GlassStrain(Quad4TriplexLaminatedGlass element, Quad4TriplexLaminatedGlass.Glass glass, Plate.Face face, int indexNode, CoordinateSystem newSys = null)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Quad4TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetGlassStrains(glass, face, csi, eta, globalDispl, newSys);
        }

        public mnl.Matrix<double> GetQuad4TLG2GlassStress(Quad4TriplexLaminatedGlass element, Quad4TriplexLaminatedGlass.Glass glass, Plate.Face face, int indexNode, CoordinateSystem newSys = null)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Quad4TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetGlassStress(glass, face, csi, eta, globalDispl, newSys);
        }

        public mnl.Matrix<double> GetQuad4TLG2GlassBending(Quad4TriplexLaminatedGlass element, int indexNode, CoordinateSystem newSys = null)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Quad4TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetGlassBending(csi, eta, globalDispl, newSys);
        }

        public mnl.Matrix<double> GetQuad4TLG2GlassForces(Quad4TriplexLaminatedGlass element, int indexNode, CoordinateSystem newSys = null)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Quad4TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetGlassForces(csi, eta, globalDispl, newSys);
        }
        #endregion

        #region Interlayer
        public mnl.Matrix<double> GetQuad4TLG2InterlayerStrain(Quad4TriplexLaminatedGlass element, int indexNode, CoordinateSystem newSys = null)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Quad4TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetInterlayerStrains(csi, eta, globalDispl, newSys);
        }

        public mnl.Matrix<double> GetQuad4TLG2InterlayerStress(Quad4TriplexLaminatedGlass element, int indexNode, CoordinateSystem newSys = null)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Quad4TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetInterlayerStress(csi, eta, globalDispl, newSys);
        }

        public mnl.Vector<double> GetQuad4TLG2InterlayerBending(Quad4TriplexLaminatedGlass element, int indexNode)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Quad4TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetInterlayerLocalBending(csi, eta, globalDispl);
        }
        #endregion
        #endregion

        #region GetResultTri3TLG2

        private void Tri3TLG2GetCsiEta(int indexNode, out double csi, out double eta)
        {
            switch (indexNode)
            {
                case 1:
                    csi = 0;
                    eta = 0;
                    break;
                case 2:
                    csi = 1;
                    eta = 0;
                    break;
                case 3:
                    csi = 0;
                    eta = 1;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        #region Glass
        public mnl.Matrix<double> GetTri3TLG2GlassStrain(Tri3TriplexLaminatedGlass element, Tri3TriplexLaminatedGlass.Glass glass, Plate.Face face, int indexNode, CoordinateSystem newSys = null)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Tri3TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetGlassStrains(glass, face, csi, eta, globalDispl, newSys);
        }

        public mnl.Matrix<double> GetTri3TLG2GlassStress(Tri3TriplexLaminatedGlass element, Tri3TriplexLaminatedGlass.Glass glass, Plate.Face face, int indexNode, CoordinateSystem newSys = null)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Tri3TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetGlassStress(glass, face, csi, eta, globalDispl, newSys);
        }

        public mnl.Matrix<double> GetTri3TLG2GlassBending(Tri3TriplexLaminatedGlass element, int indexNode, CoordinateSystem newSys = null)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Tri3TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetGlassBending(csi, eta, globalDispl, newSys);
        }

        public mnl.Matrix<double> GetTri3TLG2GlassForces(Tri3TriplexLaminatedGlass element, int indexNode, CoordinateSystem newSys = null)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Tri3TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetGlassForces(csi, eta, globalDispl, newSys);
        }
        #endregion

        #region Interlayer
        public mnl.Matrix<double> GetTri3TLG2InterlayerStrain(Tri3TriplexLaminatedGlass element, int indexNode, CoordinateSystem newSys = null)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Tri3TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetInterlayerStrains(csi, eta, globalDispl, newSys);
        }

        public mnl.Matrix<double> GetTri3TLG2InterlayerStress(Tri3TriplexLaminatedGlass element, int indexNode, CoordinateSystem newSys = null)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Tri3TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetInterlayerStress(csi, eta, globalDispl, newSys);
        }

        public mnl.Vector<double> GetTri3TLG2InterlayerBending(Tri3TriplexLaminatedGlass element, int indexNode)
        {
            if (indexNode == 0)
            {
                throw new ArgumentOutOfRangeException("index node from 1 to 4");
            }

            Tri3TLG2GetCsiEta(indexNode, out double csi, out double eta);

            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(element);

            return element.GetInterlayerLocalBending(csi, eta, globalDispl);
        }
        #endregion
        #endregion

        #region GetResultsBeam

        #region forces
        //TODO: trasformare in classe Beam
        public Dictionary<Beam.InternalAction, double> GetBeamInternalForces(EulerBeam b, double station)
        {
            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(b);
            return b.GetInternalAction(station, globalDispl);
        }

        //TODO: trasformare in classe Beam
        public Dictionary<Beam.InternalAction, double> GetBeamInternalForces(EulerBeam b, int indexNode)
        {
            double station;
            if (indexNode == 1 || indexNode == 0)
            {
                station = 0.0;
            }
            else if (indexNode == 2)
            {
                station = b.L;
            }
            else
            {
                throw new ArgumentOutOfRangeException("Node I = 1 or J = 2?");
            }
            var globalDispl = GetDisplacementsAtNodesOfElementInGlobalCoordinates(b);
            return b.GetInternalAction(station, globalDispl);
        }

        //TODO: trasformare in classe Beam
        public double GetBeamInternalForces(EulerBeam b, int indexNode, Beam.InternalAction action)
        {
            return GetBeamInternalForces(b, indexNode)[action];
        }

        //TODO: trasformare in classe Beam
        public double GetBeamInternalForces(EulerBeam b, double station, Beam.InternalAction action)
        {
            var internalForces = GetBeamInternalForces(b, station);
            return internalForces[action];
        }
        #endregion

        #region displacements
        //TODO: trasformare in classe Beam
        public Dictionary<Beam.LocalDOF, double> GetBeamDisplacementInLocalCoordinatesAtNode(EulerBeam b, Beam.EndSide endSide)
        {
            var globalDisplNodes = GetDisplacementsAtNodesOfElementInGlobalCoordinates(b);

            return b.GetLocalDisplacementsAtNode(endSide, globalDisplNodes);
        }

        public Dictionary<Beam.LocalDOF, double> GetBeamDisplacementInLocalCoordinatesAtEnd(EulerBeam b, Beam.EndSide endSide)
        {
            var globalDisplNodes = GetDisplacementsAtNodesOfElementInGlobalCoordinates(b);

            return b.GetLocalDisplacementsAtNode(endSide, globalDisplNodes);
        }

        //TODO: trasformare in classe Beam
        public Dictionary<Beam.LocalDOF, double> GetBeamDisplacementInLocalCoordinates(EulerBeam b, double station)
        {
            var globalDisplNodes = GetDisplacementsAtNodesOfElementInGlobalCoordinates(b);

            return b.GetLocalDisplacements(station, globalDisplNodes);
        }

        //TODO: trasformare in classe Beam
        public double GetBeamDisplacementInLocalCoordinates(EulerBeam b, double station, Beam.LocalDOF dof)
        {
            return GetBeamDisplacementInLocalCoordinates(b, station)[dof];
        }

        //TODO: trasformare in classe Beam
        public Dictionary<Solver.DOF, double> GetBeamDisplacementInGlobalCoordinates(EulerBeam b, double station)
        {
            var localDispl = GetBeamDisplacementInLocalCoordinates(b, station);

            mnl.Matrix<double> rotationMatrix = mnl.Matrix<double>.Build.Dense(3, 3);
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    rotationMatrix[i, j] = b.DofGlobalToLocal[i, j];
                }
            }

            //traslation
            mnl.Vector<double> trasl = mnl.Vector<double>.Build.Dense(3);
            trasl[0] = localDispl[Beam.LocalDOF.AxialU1];
            trasl[1] = localDispl[Beam.LocalDOF.U2];
            trasl[2] = localDispl[Beam.LocalDOF.U3];

            //rotation
            mnl.Vector<double> rot = mnl.Vector<double>.Build.Dense(3);
            rot[0] = localDispl[Beam.LocalDOF.TorsionR1];
            rot[1] = localDispl[Beam.LocalDOF.R2];
            rot[2] = localDispl[Beam.LocalDOF.R3];

            mnl.Vector<double> traslGlobal = rotationMatrix.Transpose() * trasl;
            mnl.Vector<double> rotGlobal = rotationMatrix.Transpose() * rot;

            Dictionary<DOF, double> globalResult = new Dictionary<DOF, double>
            {
                { Solver.DOF.DX, traslGlobal[0] },
                { Solver.DOF.DY, traslGlobal[1] },
                { Solver.DOF.DZ, traslGlobal[2] },

                { Solver.DOF.RX, rotGlobal[0] },
                { Solver.DOF.RY, rotGlobal[1] },
                { Solver.DOF.RZ, rotGlobal[2] }
            };

            return globalResult;
        }

        public double GetBeamDisplacementInGlobalCoordinates(EulerBeam b, double station, Solver.DOF dof)
        {
            return GetBeamDisplacementInGlobalCoordinates(b, station)[dof];
        }
        #endregion
        #endregion

        #region PrescribeDisplacement
        /// <summary>
        /// Modifica la matrice K e il termine noto F per l'inserimento di un spostamento imposto nei nodi con label "labelNode", grado di libertà dof e con spostamento = value;
        /// </summary>
        /// <param name="labelNode"></param>
        /// <param name="dof"></param>
        /// <param name="val"></param>
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
        /// Modifica la matrice K e il termine noto F per l'inserimento di un spostamento imposto nel nodo node, grado di libertà dof e con spostamento = value;
        /// </summary>
        /// <param name="node"></param>
        /// <param name="dof"></param>
        /// <param name="val"></param>
        public void PrescribeDisplacement(Node node, DOF dof, double val)
        {
            int position = GetPositionInKGlobal(node, dof);

            for (int j = 0; j < _F.Count; j++)
            {
                _FRestrains[j] = _FRestrains[j] - _KGlobalRestrains[j, position] * val;
                _KGlobalRestrains[j, position] = 0;
                _KGlobalRestrains[position, j] = 0;
            }
            _KGlobalRestrains[position, position] = 1.0;
            _FRestrains[position] = val;
        }
        #endregion

        #region GetReactions
        public double GetReaction(Node node, DOF dof)
        {
            int pos = GetPositionInKGlobal(node, dof);
            return _reactions[pos];
        }

        public Dictionary<DOF, double> GetReaction(Node node)
        {
            Dictionary<DOF, double> results = new Dictionary<DOF, double>();
            for (int i = 0; i < node.DOF.Count; i++)
            {
                DOF dof = node.DOF.ElementAt(i);
                int pos = GetPositionInKGlobal(node, dof);
                results.Add(dof, _reactions[pos]);
            }
            return results;
        }
        #endregion
        #endregion

        #region PrivateFunction

        #region GetPosition
        /// <summary>
        /// Give the position of selected GDL from 0 to N where N is dimension of matrix KGloabl or the dimension of the vector of Forces or Displacments
        /// </summary>
        /// <param name="labelNode">Label of the node or nodes searched</param>
        /// <param name="dof">degree of freedom searched</param>
        /// <returns></returns>
        private int[] GetPositionInKGlobal(string labelNode, DOF dof = 0)
        {
            HashSet<int> results = new HashSet<int>();
            int counter = 0;
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (Nodes.ElementAt(i).Name == labelNode)
                {
                    results.Add(counter + (int)dof);
                }
                else
                {
                    counter = counter + Nodes.ElementAt(i).NrActiveDof; //add the dof of previous nodes
                }
            }
            return results.ToArray();
        }

        /// <summary>
        /// return the position in Gloab System Matrix of in F vector or Displacement vector usign geometric position of the node (after a general clear mesh, no double node in same place should exist)
        /// </summary>
        /// <param name="node">ID node</param>
        /// <param name="dof">Searched dof</param>
        /// <returns></returns>
        private int GetPositionInKGlobal(Node node, DOF dof = 0)
        {
#if TRUE
            #region new
            var searchIndex = node.Position;
            if (_position.ContainsKey(searchIndex))
            {
                if (node.DOF.Contains(dof))
                {
                    return _position[searchIndex] + (int)dof;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Dof: " + dof.ToString() + "  not active in this node: " + node.ToString());
                }
            }
            else
            {
                int contatore = 0;
                for (int i = 0; i < Nodes.Length; i++)
                {
                    Point3d posCurrentNode = Nodes[i].Position;
                    if (node.Position != posCurrentNode)
                    {
                        contatore = contatore + Nodes[i].DOF.Count;
                    }
                    else
                    {
                        _position.Add(node.Position, contatore);
                        return contatore;
                    }
                }
            }
            #endregion
            throw new Exception("never here");
#endif
            /*
#region CounterForPreviousNodes
            int counter = 0;
            int posNode = -1;
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (Nodes[i].Position.X == node.Position.X && Nodes[i].Position.Y == node.Position.Y && Nodes[i].Position.Z == node.Position.Z)
                {
                    posNode = i;
                    i = Nodes.Length; //exit from the cycle
                }
                else
                {
                    counter = counter + Nodes.ElementAt(i).NrActiveDof; //add the dof of previous nodes
                }
            }
#endregion
#region CounterForDOFofCurrentNode
            int counter2 = 0;
            bool found = false;
            for (int i = 0; i < Nodes[posNode].DOF.Count; i++)
            {
                if (dof != Nodes[posNode].DOF.ElementAt(i))
                {
                    counter2++;
                }
                else
                {
                    found = true;
                    i = Nodes[posNode].DOF.Count; //exit from cycle
                }
            }
            if (found == true) {
                return counter + counter2;
            } else
            {
                throw new Exception(dof + " for node " + node.ToString() + " not found");
            }
#endregion
            */
        }
        #endregion

        #endregion
    }
}
