
using GPC.Model.FEM.Attributes;

namespace GPC.Model.FEM.FiniteElements
{
    public abstract class Beam : FiniteElement
    {
        public Beam(Node[] nodes) : base(nodes) { }

        public virtual void AddLoadCaseAttribute(IBeamLoadCaseAttribute attribute)
        {
            _attributesLoadCase.Add((LoadCaseAttribute)attribute);
        }


        public virtual void AddFreedomCaseAttribute(IBeamFreedomCaseAttribute attribute)
        {
            _attributesFreedomCase.Add((FreedomCaseAttribute)attribute);
        }
    }
}
