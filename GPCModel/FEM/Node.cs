using System;
using System.Collections.Generic;
using GPC.Geometry;
using GPC.Model.FEM.Attributes;
using GPC.Utilities.Extensions;

namespace GPC.Model.FEM
{
    /// <summary>
    /// Nodo with unique ID, and X,Y,Z global coordinates
    /// </summary>
    public class Node : FEMObject
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

        public Node(Point3d point, string label = "") : base(label)
        {
            _position = point;
            
            DOF = new HashSet<LinearSolver.DOF>();
            
            _attributesLoadCase = new List<INodeLoadCaseAttribute>();
            _attributesFreedomCase = new List<INodeFreedomCaseAttribute>();
        }

        internal Node(double X, double Y, double Z, string label="") : this(new Point3d(X, Y, Z), label)
        {

        }

        internal Node(double X, double Y, double Z, int id, string name) : this(new Point3d(X, Y, Z), name, id)
        {

        }
        internal Node(double X, double Y, double Z, string name, int id) : this(new Point3d(X, Y, Z), name, id)
        {

        }

        internal Node(Point3d point, string name, int id) : this(point)
        {
            Name = name;
            SetId(id);
        }

        internal Node(Point3d point, int id) : this(point)
        {
            Name = string.Empty;
            SetId(id);
        }

        public override string ToString()
        {
            return "ID = " + Id + " Name = " + Name + "  X=" + Position.X + " Y=" + Position.Y + " Z=" + Position.Z;
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
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            Node node = obj as Node;
            return !(node is null) && _position.Equals(node._position)
                                    && _attributesFreedomCase.ScrambledEquals(node._attributesFreedomCase)
                                    && _attributesLoadCase.ScrambledEquals(node._attributesLoadCase)
                                    && base.Equals(node);
        }

        public override int GetHashCode()
        {
            int hashCode = -689368791;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<Point3d>.Default.GetHashCode(_position);

            foreach (var element in _attributesLoadCase)
            {
                hashCode = hashCode + EqualityComparer<INodeLoadCaseAttribute>.Default.GetHashCode(element);
            }
            foreach (var element in _attributesFreedomCase)
            {
                hashCode = hashCode + EqualityComparer<INodeFreedomCaseAttribute>.Default.GetHashCode(element);
            }

            return hashCode;
        }

    }
}
