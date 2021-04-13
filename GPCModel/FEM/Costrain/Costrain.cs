using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Costrain
{
    abstract public class Costrain : FEMObject
    {
        public Costrain(string name) : base(name) { }

        public Costrain() : base() { }
    }
}
