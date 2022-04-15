using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Fem.Attributes;
using GPC.Model.Fem.Properties;
using GPC.Model.Fem.ElementStiffnessMatrices;
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

        public Section Section => (Section)_property;


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

            if (nodeStart.Position.SquareDistanceTo(nodeEnd.Position) < FemOptions.Instance.ToleranceNodeDistance * FemOptions.Instance.ToleranceNodeDistance)
            {
                throw new ArgumentException("Nodes are coincident");
            }
        }


        protected LinearElement(Node[] nodes)
            : base(nodes)
        {
            if (nodes.Where(i => i is null).Count() > 0)
            {
                throw new ArgumentNullException(nameof(nodes));
            }

            if (nodes.Skip(1).Where(i => i.Position.SquareDistanceTo(nodes[0].Position) < FemOptions.Instance.ToleranceNodeDistance * FemOptions.Instance.ToleranceNodeDistance).Count() > 0)
            {
                throw new ArgumentException("Nodes are coincident");
            }
        }

        protected LinearElement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {

        }


        #region Ovverride methods

        /// <inheritdoc cref="FiniteElement.SetProperty{T}(T)"/>
        internal override void SetProperty<Section>(Section property)
        {
            base.SetProperty(property);
        }

        protected override CoordinateSystem GetCoordinateSystem()
        {
            return FemHelpers.GetBeamCoordinateSystem(NodeStart.Position, NodeEnd.Position, _localAngle); ;
        }

        #endregion


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

        public abstract TransformationMatrix GetTransformationMatrix(NodalGlobalDegreeOfFreedom[] nodalDegreeOfFreedoms);
    }
}
