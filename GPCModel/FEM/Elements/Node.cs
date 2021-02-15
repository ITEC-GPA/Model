using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM
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
