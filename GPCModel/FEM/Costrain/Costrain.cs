using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Costrains
{
    abstract public class Costrain : FEMObject
    {
        public Costrain(Node nodo1, Node[] nodes, string name = "") : base(name) { }
    }
}
