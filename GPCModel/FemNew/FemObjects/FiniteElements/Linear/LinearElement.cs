using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Fem.Attributes;

namespace GPC.Model.Fem.FemObjects.FiniteElements
{
    public abstract class LinearElement : FiniteElement
    {
        protected double _localAngle;


        public double Length => _nodes[0].Position.DistanceTo(_nodes.Last().Position);

        public double LengthSquare => _nodes[0].Position.SquareDistanceTo(_nodes.Last().Position);

        protected double LocalAngle { get => _localAngle; set => _localAngle = value; }


        public LinearElement(Node node1, Node node2)
            : this(new Node[] { node1, node2 })
        {
            if (node1 is null)
            {
                throw new ArgumentNullException(nameof(node1));
            }

            if (node2 is null)
            {
                throw new ArgumentNullException(nameof(node2));
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
