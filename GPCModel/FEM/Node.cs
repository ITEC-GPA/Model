using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public string Label { get; set; }
        public Dictionary<FEMModel.DOF, bool> DOF { get; set; } //used for GlobalSystemMatrix

        public int NrActiveDof
        {
            get
            {
                int ris = 0;
                for (int i = 0; i < FEMModel.MAXGDLPERNODE; i++)
                {
                    if (DOF[(FEMModel.DOF)i] == true)
                    {
                        ris++;
                    }
                   
                }
                return ris;
            }
        }
        #endregion

        public Node(Point3d point, int ID, string label = "") : base(ID)
        {
            Label = label;
            _position = point;
            
            DOF = new Dictionary<FEMModel.DOF, bool>(FEMModel.MAXGDLPERNODE);
            for (int i = 0; i < FEMModel.MAXGDLPERNODE; i++)
            {
                DOF.Add((FEMModel.DOF)i, false);
            }

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
            return "ID = " + Index + " Label = " + Label + "  X=" + Position.X + " Y=" + Position.Y + " Z=" + Position.Z;
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
                   _x == node._x &&
                   _y == node._y &&
                   _z == node._z &&
                   _ID == node._ID &&
                   Label == node.Label &&
                   EqualityComparer<Dictionary<FEM.FEMModel.DOF, bool>>.Default.Equals(DOF, node.DOF);
        }

        public override int GetHashCode()
        {
            int hashCode = 1581907463;
            hashCode = hashCode * -1521134295 + base.GetHashCode();
            hashCode = hashCode * -1521134295 + _x.GetHashCode();
            hashCode = hashCode * -1521134295 + _y.GetHashCode();
            hashCode = hashCode * -1521134295 + _z.GetHashCode();
            hashCode = hashCode * -1521134295 + _ID.GetHashCode();
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Label);
            hashCode = hashCode * -1521134295 + EqualityComparer<Dictionary<DOF, bool>>.Default.GetHashCode(DOF);
            return hashCode;
        }

        public bool Equals(Node other)
        {
            return Equals((object)other);
        }
    }
}
