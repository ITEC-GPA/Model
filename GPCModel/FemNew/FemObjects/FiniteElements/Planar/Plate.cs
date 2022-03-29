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
    internal class Plate : PlanarElement
    {

        public Plate(Node[] nodes) 
            : base(nodes)
        {

        }

        protected Plate(SerializationInfo info, StreamingContext context) 
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
