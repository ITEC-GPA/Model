using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.Attributes;
using GPC.Model.Fem.Properties;
using GPC.Model.Sections;

namespace GPC.Model.Fem.FemObjects.FiniteElements
{
    public abstract class LinearElement : FiniteElement
    {
        protected double _localAngle;


        public double Length => _nodes[0].Position.DistanceTo(_nodes.Last().Position);

        public double LengthSquare => _nodes[0].Position.SquareDistanceTo(_nodes.Last().Position);

        protected double LocalAngle { get => _localAngle; set => _localAngle = value; }


        public Node NodeStart => _nodes[0];
        public Node NodeEnd => _nodes.Last();



        public LinearElement(Node nodeStart, Node nodeEnd)
            : base(new Node[] { nodeStart, nodeEnd })
        {
            if (nodeStart is null)
            {
                throw new ArgumentNullException(nameof(nodeStart));
            }

            if (nodeEnd is null)
            {
                throw new ArgumentNullException(nameof(nodeEnd));
            }

        }


        protected LinearElement(Node[] nodes)
            : base(nodes)
        {

        }

        protected LinearElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }


        /// <inheritdoc cref="FiniteElement.SetProperty{T}(T)"/>
        internal override void SetProperty<Section>(Section property)
        {
            base.SetProperty(property);
        }


        public virtual bool AddLoadCaseAttribute(IBeamLoadCaseAttribute attribute, out bool replace)
        {
            return _attributesLoadCase.Add((LoadCaseAttribute)attribute, out replace);
        }

        public virtual bool AddFreedomCaseAttribute(IBeamFreedomCaseAttribute attribute, out bool replace)
        {
            return _attributesFreedomCase.Add((FreedomCaseAttribute)attribute, out replace);
        }

        public virtual bool AddLoadCaseAttribute(IBeamLoadCaseAttribute attribute)
        {
            return _attributesLoadCase.Add((LoadCaseAttribute)attribute);
        }

        public virtual bool AddFreedomCaseAttribute(IBeamFreedomCaseAttribute attribute)
        {
            return _attributesFreedomCase.Add((FreedomCaseAttribute)attribute);
        }

    }
}
