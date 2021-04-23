
using GPC.Model.FEM.Attributes;

namespace GPC.Model.FEM.FiniteElements
{
    public abstract class Beam : FiniteElement
    {
        public enum InternalAction
        {
            N,
            V2,
            V3,
            T,
            M2,
            M3
        }

        public enum LocalDOF
        {
            AxialU1,
            U2,
            U3,
            TorsionR1,
            R2,
            R3
        }

        public enum EndSide
        {
            End1,
            End2
        }

        #region Variables
        protected double _length;
        protected double _axisAngleRadians;
        #endregion

        #region Properties
        public double L => _length;
        public double AxisAngleRad => _axisAngleRadians;
        #endregion

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
