
using GPC.Model.FEM.Attributes;
using GPC.Model.Results;
using System;

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

        public Beam(Node[] nodes) 
            : base(nodes) 
        {
            _length = nodes[0].Position.DistanceTo(nodes[1].Position);
        }



        public virtual bool AddLoadCaseAttribute(IBeamLoadCaseAttribute attribute)
        {
            return _attributesLoadCase.Add((LoadCaseAttribute)attribute);
        }


        public virtual bool AddFreedomCaseAttribute(IBeamFreedomCaseAttribute attribute)
        {
            return _attributesFreedomCase.Add((FreedomCaseAttribute)attribute);
        }


        public void AddResult(BeamResult result)
        {
            base.AddResult(result);
        }

        public override void AddResult(FiniteElementResult result)
        {
            if (result is BeamResult)
            {
                base.AddResult(result);
            }
            else
            {
                throw new ArgumentException();
            }
        }


    }
}
