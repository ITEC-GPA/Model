using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;

namespace GPC.Model.FEM
{
    /// <summary>
    /// Nodo with unique ID, and X,Y,Z global coordinates
    /// </summary>
    public class Node : Point3d
    {
        #region Variables
        private int _ID;
        #endregion

        #region Properties
        public int ID => _ID;

        public string Label { get; set; }
        public Dictionary<DOF, bool> DOF { get; set; } //used for GlobalSystemMatrix

        public int NrActiveDof
        {
            get
            {
                int ris = 0;
                for (int i = 0; i < FEMModel.MAXGDLPERNODE; i++)
                {
                    if (DOF[(DOF)i] == true)
                    {
                        ris++;
                    }
                   
                }
                return ris;
            }
        }
        #endregion

        /// <summary>
        /// 
        /// </summary>
        /// <param name="X">Global coordinate X</param>
        /// <param name="Y">Global coordinate Y</param>
        /// <param name="Z">Global coordinate Z</param>
        public Node(double globalX, double globalY, double globalZ, int ID, string label = "") : base(globalX, globalY, globalZ)
        {
            _ID = ID;
            Label = label;

            DOF = new Dictionary<DOF, bool>(FEMModel.MAXGDLPERNODE);
            for (int i = 0; i < FEMModel.MAXGDLPERNODE; i++)
            {
                DOF.Add((DOF)i, false);
            }
        }

        public Node(int ID, Node node) : this(node.X, node.Y, node.Z, ID, node.Label)
        {
            for (int i = 0; i < FEMModel.MAXGDLPERNODE; i++)
            {
                DOF[(DOF) i] = node.DOF[(DOF) i];
            }
        }

        public override string ToString()
        {
            return "ID = " + ID + " Label = " + Label + "  X=" + X + " Y=" + Y + " Z=" + Z;
        }
    }
}
