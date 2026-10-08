using GPC.Model.ElementProperties;
using GPC.Model.Elements;
using GPC.Model.Results.ElementResults;
using MathNet.Numerics.LinearAlgebra;
using System;

namespace GPC.Model.FiniteElementAnalysis.FiniteElements.Beam
{
    /// <summary>
    /// Base of the beam finite elements (a draft: the methods of the analysis are not implemented)
    /// </summary>
    public abstract class FemBeam : BeamElement
    {
        #region Variables

        #endregion

        #region Properties

        /// <summary>
        /// The section of the beam
        /// </summary>
        public ElementProperty ElementProperty { get => _beamProperty; }

        #endregion

        /// <summary>
        /// Creates the finite element of a beam: same points, section, coordinate system, name and id
        /// </summary>
        /// <param name="beamElement">The beam</param>
        public FemBeam(BeamElement beamElement)
            : base(beamElement.StartPoint, beamElement.EndPoint, beamElement.BeamProperty, beamElement.CoordinateSystem, beamElement.Name, beamElement.Id)
        {

        }

        /// <summary>
        /// Adds a result of the beam
        /// </summary>
        /// <param name="result">The result</param>
        public void AddResult(BeamResult result)
        {
            base.AddResult(result);
        }

        /// <summary>
        /// Adds a result, that must be a <see cref="BeamResult"/>
        /// </summary>
        /// <param name="result">The result</param>
        /// <exception cref="ArgumentException">If the result is not a <see cref="BeamResult"/></exception>
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

        /// <summary>
        /// Build the stiffness matrix
        /// </summary>
        /// <exception cref="NotImplementedException">Always: the finite element beam is a draft</exception>
        public void BuildMatrix()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The nodal forces in the local coordinates
        /// </summary>
        /// <returns>The forces</returns>
        /// <exception cref="NotImplementedException">Always: the finite element beam is a draft</exception>
        public Vector<double> BuildFLocalCoord()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The nodal forces in the global coordinates
        /// </summary>
        /// <returns>The forces</returns>
        /// <exception cref="NotImplementedException">Always: the finite element beam is a draft</exception>
        public Vector<double> GetGlobalCoordF()
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal forces in the global coordinates
        /// </summary>
        /// <param name="globalDisplacementsNodes">The displacements of the nodes in the global coordinates</param>
        /// <returns>The forces</returns>
        /// <exception cref="NotImplementedException">Always: the finite element beam is a draft</exception>
        public Vector<double> GetInternalGlobalForces(double[] globalDisplacementsNodes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal nodal forces in the local coordinates
        /// </summary>
        /// <param name="localDisplacementsNodes">The displacements of the nodes in the local coordinates</param>
        /// <returns>The forces</returns>
        /// <exception cref="NotImplementedException">Always: the finite element beam is a draft</exception>
        public Vector<double> GetInternalNodalLocalForces(Vector<double> localDisplacementsNodes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The internal forces in the local coordinates
        /// </summary>
        /// <param name="localDisplacementsNodes">The displacements of the nodes in the local coordinates</param>
        /// <returns>The forces</returns>
        /// <exception cref="NotImplementedException">Always: the finite element beam is a draft</exception>
        public Vector<double> GetInternalLocalForces(double[] localDisplacementsNodes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The displacements in the local coordinates
        /// </summary>
        /// <param name="globalDisplacementsNodes">The displacements of the nodes in the global coordinates</param>
        /// <returns>The displacements</returns>
        /// <exception cref="NotImplementedException">Always: the finite element beam is a draft</exception>
        public Vector<double> GetLocalDisplacementVector(double[] globalDisplacementsNodes)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// The displacements in the local coordinates
        /// </summary>
        /// <param name="globalDisplacementsNodes">The displacements of the nodes in the global coordinates</param>
        /// <returns>The displacements</returns>
        /// <exception cref="NotImplementedException">Always: the finite element beam is a draft</exception>
        public double[] GetLocalDisplacement(double[] globalDisplacementsNodes)
        {
            throw new NotImplementedException();
        }
    }
}
