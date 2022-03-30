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

    [Obsolete("Only a dummy class at the moment")]
    internal class Brick : SolidElement
    {
        public Brick(Node[] nodes)
            : base(nodes)
        {
        }

        protected Brick(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        protected override NodalDegreeOfFreedom[] GetNodalDegreeOfFreedom()
        {
            throw new NotImplementedException();
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
