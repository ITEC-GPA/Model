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
    public abstract class SolidElement : FiniteElement
    {

        public SolidElement(Node[] nodes) 
            : base(nodes)
        {

        }

        public SolidElement(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {

        }

        /// <inheritdoc cref="FiniteElement.SetProperty{T}(T)"/>
        internal override void SetProperty<BrickProperty>(BrickProperty property)
        {
            base.SetProperty(property);
        }


    }
}
