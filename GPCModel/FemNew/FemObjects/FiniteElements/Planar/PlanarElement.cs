using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Fem.Attributes;
using GPC.Model.Fem.Properties;

namespace GPC.Model.Fem.FemObjects.FiniteElements
{
    public abstract class PlanarElement : FiniteElement
    {

        public PlanarElement(Node[] nodes)
            : base(nodes)
        {

        }

        protected PlanarElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }


        /// <inheritdoc cref="FiniteElement.SetProperty{T}(T)"/>
        internal override void SetProperty<IPlateProperty>(IPlateProperty property)
        {
            base.SetProperty(property);
        }

        protected override CoordinateSystem GetCoordinateSystem(double angle)
        {
            throw new NotImplementedException();
        }

        public virtual bool AddLoadCaseAttribute(IPlateLoadCaseAttribute attribute, out bool replace)
        {
            return _attributesLoadCase.Add((LoadCaseAttribute)attribute, out replace);
        }

        public virtual bool AddLoadCaseAttribute(IPlateLoadCaseAttribute attribute)
        {
            return _attributesLoadCase.Add((LoadCaseAttribute)attribute);
        }

        public virtual bool AddFreedomCaseAttribute(IPlateFreedomCaseAttribute attribute, out bool replaced)
        {
            return _attributesFreedomCase.Add((FreedomCaseAttribute)attribute, out replaced);
        }

        public virtual bool AddFreedomCaseAttribute(IPlateFreedomCaseAttribute attribute)
        {
            return _attributesFreedomCase.Add((FreedomCaseAttribute)attribute);
        }
    }
}
