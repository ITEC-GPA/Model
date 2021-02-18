using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Elements;

namespace GPC.Model.FEM.FiniteElements
{
    public class Plate : FiniteElement
    {
        public Plate(Node[] nodes, PlateProperty property, int id) : base (nodes, id)
        {
           
        }

        public override void BuildF()
        {
            throw new NotImplementedException();
        }

        public override void BuildMatrix()
        {
            throw new NotImplementedException();
        }
    }
}
