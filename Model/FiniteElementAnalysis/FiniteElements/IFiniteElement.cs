using GPC.Model.ElementProperties;
using GPC.Model.Results.ElementResults;

namespace GPC.Model.FiniteElementAnalysis.FiniteElements
{
    /// <summary>
    /// A finite element of the analysis: stiffness matrix, nodal forces and results (a draft: implemented only by <see cref="Beam.FemBeam"/>, which throws)
    /// </summary>
    internal interface IFiniteElement
    {
        /// <summary>
        /// The property of the element
        /// </summary>
        ElementProperty ElementProperty { get; }

        /// <summary>
        /// Adds a result to the element
        /// </summary>
        /// <param name="result">The result</param>
        void AddResult(ElementResult result);

        /// <summary>
        /// Build Stiffness Matrix etc
        /// </summary>
        void BuildMatrix();

        /// <summary>
        /// Build vector of Forces in nodes due to internal action applied (shear stress, prestress etc) : integral N^T vectorPression dS, N = shape function matrix
        /// </summary>
        /// <returns>The nodal forces in the local coordinates</returns>
        MathNet.Numerics.LinearAlgebra.Vector<double> BuildFLocalCoord();

        /// <summary>
        /// The nodal forces in the global coordinates
        /// </summary>
        /// <returns>The forces</returns>
        MathNet.Numerics.LinearAlgebra.Vector<double> GetGlobalCoordF();

        /// <summary>
        /// Return global internal force for the element
        /// </summary>
        /// <param name="globalDisplacementsNodes">The displacements of the nodes in the global coordinates</param>
        /// <returns>KglobalElement * displGlobal</returns>
        MathNet.Numerics.LinearAlgebra.Vector<double> GetInternalGlobalForces(double[] globalDisplacementsNodes);

        /// <summary>
        /// Return local internal nodal force for the element
        /// </summary>
        /// <param name="localDisplacementsNodes">The displacements of the nodes in the local coordinates</param>
        /// <returns>Klocal * displLocal</returns>
        MathNet.Numerics.LinearAlgebra.Vector<double> GetInternalNodalLocalForces(MathNet.Numerics.LinearAlgebra.Vector<double> localDisplacementsNodes);

        /// <summary>
        /// Return local internal force for the element
        /// </summary>
        /// <param name="localDisplacementsNodes">The displacements of the nodes in the local coordinates</param>
        /// <returns>The internal forces</returns>
        MathNet.Numerics.LinearAlgebra.Vector<double> GetInternalLocalForces(double[] localDisplacementsNodes);

        /// <summary>
        /// Get displacements in local coordinates of the element
        /// </summary>
        /// <param name="globalDisplacementsNodes">The displacements of the nodes in the global coordinates</param>
        /// <returns>The displacements in the local coordinates</returns>
        MathNet.Numerics.LinearAlgebra.Vector<double> GetLocalDisplacementVector(double[] globalDisplacementsNodes);

        /// <summary>
        /// Get displacements in local coordinates of the element
        /// </summary>
        /// <param name="globalDisplacementsNodes">The displacements of the nodes in the global coordinates</param>
        /// <returns>The displacements in the local coordinates</returns>
        double[] GetLocalDisplacement(double[] globalDisplacementsNodes);
    }
}
