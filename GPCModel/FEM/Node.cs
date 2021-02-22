using System;
using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;

namespace GPC.Model.FEM
{
    /// <summary>
    /// Nodo with unique ID, and X,Y,Z global coordinates
    /// </summary>
    public class Node : FEMObject, IEquatable<Node>
    {
        #region Variables
        private Point3d _position;

        private List<INodeLoadCaseAttribute> _attributesLoadCase;

        private List<INodeFreedomCaseAttribute> _attributesFreedomCase;


        #endregion

        #region Properties
        public Point3d Position => _position;
        /// <summary>
        /// Contains the degree of freedom active for the node
        /// </summary>
        public HashSet<LinearSolver.DOF> DOF { get; set; }

        public int NrActiveDof
        {
            get
            {
                int ris = 0;
                for (int i = 0; i < LinearSolver.MAXGDLPERNODE; i++)
                {
                    if (DOF.Contains((LinearSolver.DOF)i) == true)
                    {
                        ris++;
                    }  
                }
                return ris;
            }
        }

        public List<INodeFreedomCaseAttribute> AttributesFreedomCase => _attributesFreedomCase;
        public List<INodeLoadCaseAttribute> AttributesLoadCase => _attributesLoadCase;
        #endregion

        public Node(Point3d point, int ID, string label = "") : base(ID, label)
        {
            _position = point;
            
            DOF = new HashSet<LinearSolver.DOF>();
            
            _attributesLoadCase = new List<INodeLoadCaseAttribute>();
            _attributesFreedomCase = new List<INodeFreedomCaseAttribute>();
        }

        public Node(double X, double Y, double Z, int id, string label="") : this(new Point3d(X, Y, Z), id, label)
        {

        }

        public void SetID(int id)
        {
            base._index = id;
        }

        public override string ToString()
        {
            return "ID = " + Index + " Name = " + Name + "  X=" + Position.X + " Y=" + Position.Y + " Z=" + Position.Z;
        }

        public void AddAttribute(INodeFreedomCaseAttribute attribute)
        {
            _attributesFreedomCase.Add(attribute);
        }

        public void AddAttribute(INodeLoadCaseAttribute attribute)
        {
            _attributesLoadCase.Add(attribute);
        }

        public override bool Equals(object obj)
        {
            return obj is Node node &&
                   base.Equals(obj) &&
                   EqualityComparer<Point3d>.Default.Equals(_position, node._position) &&
                   EqualityComparer<List<INodeLoadCaseAttribute>>.Default.Equals(_attributesLoadCase, node._attributesLoadCase) &&
                   EqualityComparer<List<INodeFreedomCaseAttribute>>.Default.Equals(_attributesFreedomCase, node._attributesFreedomCase);
        }

        public override int GetHashCode()
        {
            int hashCode = -689368791;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Point3d>.Default.GetHashCode(_position);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<INodeLoadCaseAttribute>>.Default.GetHashCode(_attributesLoadCase);
            hashCode = hashCode * -1521134295 + EqualityComparer<List<INodeFreedomCaseAttribute>>.Default.GetHashCode(_attributesFreedomCase);
            return hashCode;
        }

        public bool Equals(Node other)
        {
            return Equals((object)other);
        }
    }
}
