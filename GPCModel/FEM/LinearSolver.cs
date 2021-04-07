using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.FEM.FiniteElements;
using GPC.Geometry;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.FEM.Attributes;

namespace GPC.Model.FEM
{
    public class LinearSolver : Solver
    {
        #region variables
        public enum DOF
        {
            DX,   //0
            DY,   //1
            DZ,   //2
            RX,   //3
            RY,   //4
            RZ,   //5
        }

        public static int MAXDOFPERNODE = Enum.GetNames(typeof(DOF)).Length;

        protected mnl.Matrix<double> _KGlobal;
        protected mnl.Matrix<double> _KGlobalRestrains;
        protected mnl.Vector<double> _F;
        protected mnl.Vector<double> _FRestrains;
        protected HashSet<FEM.Costrain.MultiPointCostrain> _costrains;
        protected mnl.Vector<double> _nodeGlobalDisplacement;
        protected mnl.Vector<double> _reactions;
        #endregion

        #region Properties
        public mnl.Matrix<double> KGlobal => _KGlobalRestrains;
        public mnl.Vector<double> F => _F;

        /// <summary>
        /// Unique nodes in model
        /// </summary>
        public Node[] Nodes { get; }

        /// <summary>
        /// Unique elements in Models contains all the informations: node connectivity, material, property, LOAD as attribute, end releases...etc
        /// </summary>
        public FiniteElement[] Elements { get; }
        #endregion

        public LinearSolver(FiniteElement[] inputElements)
        {
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
                    } else if (nodes.Count() == 1) //Node already used in another element.
                    {
                        Node oldNode = nodes.Single();
                        int ID = oldNode.Id; //tra i 2 ID da poter scegliere uso quello del nodo già usato
                        
                        if (node.Name != oldNode.Name) //if the nodes have different name --> generally shuld not happen
                        {
                            node.Name = node.Name + "+" + oldNode.Name;
                        }

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
                                    node.AttributesFreedomCase.Add(new NodeRestrainAttribute(restrainAttribute.FreedomCase, restrainAttribute.CoordinateSystem, restrainAttribute.Restrains, restrainAttribute.Name, Guid.NewGuid()));
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
                    } else
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
            Console.WriteLine("kGlobal System :");
            FEMUtilities.WriteMatrix(_KGlobal);
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
                        double[] additionalForce = new double[6];
                        additionalForce[0] = nodeForceAttribute.F1 * dirX.DotProduct(X) + nodeForceAttribute.F2 * dirY.DotProduct(X) + nodeForceAttribute.F3 * dirZ.DotProduct(X); //fX
                        additionalForce[1] = nodeForceAttribute.F1 * dirX.DotProduct(Y) + nodeForceAttribute.F2 * dirY.DotProduct(Y) + nodeForceAttribute.F3 * dirZ.DotProduct(Y); //fY
                        additionalForce[2] = nodeForceAttribute.F1 * dirX.DotProduct(Z) + nodeForceAttribute.F2 * dirY.DotProduct(Z) + nodeForceAttribute.F3 * dirZ.DotProduct(Z); //fZ

                        additionalForce[3] = nodeForceAttribute.M1 * dirX.DotProduct(X) + nodeForceAttribute.M2 * dirY.DotProduct(X) + nodeForceAttribute.M3 * dirZ.DotProduct(X); //mX
                        additionalForce[4] = nodeForceAttribute.M1 * dirX.DotProduct(Y) + nodeForceAttribute.M2 * dirY.DotProduct(Y) + nodeForceAttribute.M3 * dirZ.DotProduct(Y); //mY
                        additionalForce[5] = nodeForceAttribute.M1 * dirX.DotProduct(Z) + nodeForceAttribute.M2 * dirY.DotProduct(Z) + nodeForceAttribute.M3 * dirZ.DotProduct(Z); //mZ

                        //Modify F vector adding the forces from the node
                        dofs.ForEach(
                            (stringDOF) => {
                                DOF dof = (DOF)Enum.Parse(typeof(DOF), stringDOF);
                                if (Nodes[i].DOF.Contains(dof) == true) {
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
            Console.WriteLine("Vector F");
            _F.ToList().ForEach(x => Console.WriteLine(x));
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
                            foreach(var restrain in restrainAttribute.Restrains)
                            {
                                //PrescribeDisplacement(Nodes[i].Id, restrain.Dof, restrain.ImposedDisplacement);
                                PrescribeDisplacement(Nodes[i], restrain.Dof, restrain.ImposedDisplacement);
                            }

                        } else
                        {
                            throw new NotImplementedException();
                        }
                        
                    }
                }
            }
            //Console.WriteLine("kGlobal System + Restrains: ");
            //Util.WriteMatrix(_KGlobalRestrains);
            //Console.WriteLine("Fmodified(Restrains):");
            //Util.WriteMatrix(_FRestrains);
            #endregion

            #region ApplyingMultiPointCostrains
            _costrains = new HashSet<FEM.Costrain.MultiPointCostrain>();
            //applying as example in node 1 : DX = DY (simply support with 45 degrees direction
            FEM.Costrain.MultiPointCostrain.Link[] equations = new FEM.Costrain.MultiPointCostrain.Link[2];
            equations[0] = new FEM.Costrain.MultiPointCostrain.Link("1", DOF.DX, 1.0);
            equations[1] = new FEM.Costrain.MultiPointCostrain.Link("1", DOF.DY, 1.0);
            FEM.Costrain.MultiPointCostrain Costrain1 = new FEM.Costrain.MultiPointCostrain(equations);
            //_costrains.Add(Costrain1);

            /* Use: Lagrange multiplier method 
             * rewrite constrains as : 1.0 * GdL NodeMaster + ValI * GdL NodeSlaveI + ... + ValN * GdL NodeSlaveN = const
             * and modify K matrix and F vector
             */
            int nLagrangianMultiplier = _costrains.Count;
            for (int i = 0; i < _costrains.Count; i++)
            {
                FEM.Costrain.MultiPointCostrain c = _costrains.ElementAt(i);
                Console.WriteLine(c.ToString());

                mnl.Vector<double> voidVector = mnl.Vector<double>.Build.Dense(_KGlobalRestrains.RowCount);
                _KGlobalRestrains = _KGlobalRestrains.InsertColumn(_KGlobalRestrains.ColumnCount, voidVector);
                voidVector = mnl.Vector<double>.Build.Dense(_KGlobalRestrains.ColumnCount);
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
            Console.WriteLine("kGlobal System + Restrains + Constrains:");
            FEMUtilities.WriteMatrix(_KGlobalRestrains, "F3");
            //Console.WriteLine("Fmodified(Restrains + Constrains) = " + _FRestrains.ToString());
            #endregion

            #region SolveModel
            //Solve Matrix
            _nodeGlobalDisplacement = _KGlobalRestrains.Solve(_FRestrains);
            Console.WriteLine("Node displacements results:");

            for (int i = 0; i < _nodeGlobalDisplacement.Count; i++)
            {
                Console.WriteLine("Displ. " + legend[i] + " : \t " + _nodeGlobalDisplacement[i].ToString("F3"));
            }
            #endregion

            #region Reactions
            _reactions = _KGlobal * _nodeGlobalDisplacement - _F; //Or Fmodified?
            double sumFX = 0.0;
            double sumFY = 0.0;
            double sumFZ = 0.0;
            
            for (int i = 0; i < _reactions.Count; i++)
            {
                Console.WriteLine("React." + legend[i] + " : \t " + _reactions[i].ToString("F3"));
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

        #region PublicFuction
        /// <summary>
        /// Ritorna spostamento per un selezionato nodo e per un certp grado di libertà.
        /// Per selezionare il nodo è usata la label in quanto l'index potrebbe essere stato modificato rispetto a fase di input...
        /// (forse è meglio dare un errore in fase di costruzione e non cambiare index?)
        /// </summary>
        /// <param name="node"></param>
        /// <param name="dof"></param>
        /// <returns></returns>
        public double GetDisplacementGlobalCoordinates(Node node, DOF dof)
        {
            int pos = GetPositionInKGlobal(node, dof);
            return _nodeGlobalDisplacement[pos];
        }

        public double[] GetDisplacementGlobalCoordinates(string labelNode, DOF dof)
        {
            if (labelNode== "" || labelNode == null)
            {
                throw new Exception("Select a node with a name!");
            }
            int[] pos = GetPositionInKGlobal(labelNode, dof);
            double[] ris = new double[pos.Length];
            for (int i = 0; i < pos.Length; i++)
            {
                ris[i] = _nodeGlobalDisplacement[pos[i]];
            }
            return ris;
        }

        public double[] GetDisplacementsGlobalCoordinates(FiniteElement e)
        {
            #region SelectGlobalDisplacementForElement
            var elements = Elements.Where(x => x == e);
            if (elements.Count() > 1)
            {
                throw new Exception("More than 1 element selected");
            }
            else
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
                    globalDisplacementsNodesElement[j] = _nodeGlobalDisplacement[pos[j]];
                    //Console.WriteLine("Element " + i + " Displacemente global coordintates DOF nr. " + j + " = " + globalDisplacementsNodesElement[j]);
                }

                return globalDisplacementsNodesElement;
                //get results of element
                //element.GetResults(globalDisplacementsNodesElement, out double[] localDisplacements, out mnl.Matrix<double>[] gloabalPseudoDeformation, out mnl.Matrix<double>[] localPseudoDeformation, out mnl.Matrix<double>[] globalForces, out mnl.Matrix<double>[] localForces, out mnl.Matrix<double>[] globalStress, out mnl.Matrix<double>[] localStress, out mnl.Matrix<double>[] globalEpsilon, out mnl.Matrix<double>[] localEpsilon);
                #endregion
            }
        }
        #endregion

        #region PrivateFunction
        /// <summary>
        /// Modifica la matrice K e il termine noto F per l'inserimento di un spostamento imposto nei nodi con label "labelNode", grado di libertà dof e con spostamento = value;
        /// </summary>
        /// <param name="labelNode"></param>
        /// <param name="dof"></param>
        /// <param name="val"></param>
        private void PrescribeDisplacement(string labelNode, DOF dof, double val)
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
        /// <param name="index"></param>
        /// <param name="dof"></param>
        /// <param name="val"></param>
        private void PrescribeDisplacement(Node node, DOF dof, double val)
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
        /// <param name="IdNode">ID node</param>
        /// <param name="dof">Searched dof</param>
        /// <returns></returns>
        private int GetPositionInKGlobal(Node node, DOF dof = 0)
        {
            #region CounterForPreviousNodes
            int counter = 0;
            int posNode = -1;
            for (int i = 0; i < Nodes.Length; i++)
            {
                if (Nodes.ElementAt(i).Position.X == node.Position.X && Nodes.ElementAt(i).Position.Y == node.Position.Y && Nodes.ElementAt(i).Position.Z == node.Position.Z)
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
            for (int i = 0; i < Nodes[posNode].DOF.Count; i++)
            {
                if (dof != Nodes[posNode].DOF.ElementAt(i))
                {
                    counter2++;
                }
                else
                {
                    i = Nodes[posNode].DOF.Count; //exit from cycle
                }
            }
            return counter + counter2;
            //return counter + (int)dof;
            #endregion
        }
        #endregion       
    }
}
