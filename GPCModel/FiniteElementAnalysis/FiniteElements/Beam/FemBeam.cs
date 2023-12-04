using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.Results.ElementResults;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace GPC.Model.FiniteElementAnalysis.FiniteElements.Beam
{
    public abstract class FemBeam : BeamElement
    {
        #region Variables

        #endregion

        #region Properties

        public ElementProperty ElementProperty { get => _beamProperty; }

        #endregion

        public FemBeam(BeamElement beamElement)
            : base(beamElement.StartPoint, beamElement.EndPoint, beamElement.BeamProperty, beamElement.CoordinateSystem, beamElement.Name, beamElement.Id)
        {

        }

        public void AddResult(BeamResult result)
        {
            base.AddResult(result);
        }

        public override void AddResult(ElementResult result)
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

        public void BuildMatrix()
        {
            throw new NotImplementedException();
        }

        public Vector<double> BuildFLocalCoord()
        {
            throw new NotImplementedException();
        }

        public Vector<double> GetGlobalCoordF()
        {
            throw new NotImplementedException();
        }

        public Vector<double> GetInternalGlobalForces(double[] globalDisplacementsNodes)
        {
            throw new NotImplementedException();
        }

        public Vector<double> GetInternalNodalLocalForces(Vector<double> localDisplacementsNodes)
        {
            throw new NotImplementedException();
        }

        public Vector<double> GetInternalLocalForces(double[] localDisplacementsNodes)
        {
            throw new NotImplementedException();
        }

        public Vector<double> GetLocalDisplacementVector(double[] globalDisplacementsNodes)
        {
            throw new NotImplementedException();
        }

        public double[] GetLocalDisplacement(double[] globalDisplacementsNodes)
        {
            throw new NotImplementedException();
        }
    }
}
