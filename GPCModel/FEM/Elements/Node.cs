using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.FEM
{
    /// <summary>
    /// Nodo with unique ID, and X,Y,Z global coordinates
    /// </summary>
    public class Node
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
        public int ID { get; set; }
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="X">Global coordinate X</param>
        /// <param name="Y">Global coordinate Y</param>
        /// <param name="Z">Global coordinate Z</param>
        public Node(double globalX, double globalY, double globalZ, string label = "")
        {
            X = globalX;
            Y = globalY;
            Z = globalZ;
            Label = label;

            DOF = new Dictionary<FEMModel.DOF, bool>(6);
            DOF.Add(FEM.FEMModel.DOF.DX, false);
            DOF.Add(FEM.FEMModel.DOF.DY, false);
            DOF.Add(FEM.FEMModel.DOF.DZ, false);
            DOF.Add(FEM.FEMModel.DOF.RX, false);
            DOF.Add(FEM.FEMModel.DOF.RY, false);
            DOF.Add(FEM.FEMModel.DOF.RZ, false);
        }

        public override string ToString()
        {
            return "ID = " + ID + " Label = " + Label + "  X=" + X + " Y=" + Y + " Z=" + Z;
        }

        public bool GeometryEquals(Node other)
        {
            if (other.X == X && other.Y == Y && other.Z == Z) { return true; } else { return false; }
        }
    }
}
