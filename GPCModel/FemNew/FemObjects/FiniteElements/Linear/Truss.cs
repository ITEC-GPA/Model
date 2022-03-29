using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.Attributes;
using GPC.Model.Fem.Properties;

namespace GPC.Model.Fem.FemObjects.FiniteElements
{
    public class Truss : LinearElement
    {
        public Truss(Node node1, Node node2) 
            : base(node1, node2)
        {

        }

        protected Truss(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }

        public override void BuildMatrix()
        {
            throw new NotImplementedException();
        }

        public override FiniteElement Duplicate(ElementProperty property, List<LoadCaseAttribute> lcAttributes, List<FreedomCaseAttribute> fcAttributes)
        {
            throw new NotImplementedException();
        }

        public override FiniteElement Duplicate()
        {
            throw new NotImplementedException();
        }
    }
}
