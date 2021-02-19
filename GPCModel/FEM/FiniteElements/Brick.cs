using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Elements;
using GPC.Model.Materials;

namespace GPC.Model.FEM.FiniteElements
{
    class Brick : FiniteElement
    {
        public Brick(Node[] nodes, BrickProperty property, int id) : base(nodes, property, id) { }

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
