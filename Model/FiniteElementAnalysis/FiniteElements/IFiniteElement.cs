using GPC.Model.ElementProperties;
using GPC.Model.Results.ElementResults;

namespace GPC.Model.FiniteElementAnalysis.FiniteElements
{
    internal interface IFiniteElement
    {
        ElementProperty ElementProperty { get; }

        void AddResult(ElementResult result);

        /// <summary>
        /// Build Stiffness Matrix etc
        /// </summary>
        void BuildMatrix();

        /// <summary>
        /// Build vector of Forces in nodes due to internal action applied (shear stress, prestress etc) : integral N^T vectorPression dS, N = shape function matrix
        /// </summary>
        MathNet.Numerics.LinearAlgebra.Vector<double> BuildFLocalCoord();

        MathNet.Numerics.LinearAlgebra.Vector<double> GetGlobalCoordF();

        /// <summary>
        /// Return global internal force for the element
        /// </summary>
        /// <param name="globalDisplacementsNodes"></param>
        /// <returns>KglobalElement * displGlobal</returns>
        MathNet.Numerics.LinearAlgebra.Vector<double> GetInternalGlobalForces(double[] globalDisplacementsNodes);

        /// <summary>
        /// Return local internal force for the element
        /// </summary>
        /// <param name="localDisplacementsNodes"></param>
        /// <returns>KglobalElement * displGlobal</returns>
        MathNet.Numerics.LinearAlgebra.Vector<double> GetInternalNodalLocalForces(MathNet.Numerics.LinearAlgebra.Vector<double> localDisplacementsNodes);

        MathNet.Numerics.LinearAlgebra.Vector<double> GetInternalLocalForces(double[] localDisplacementsNodes);

        /// <summary>
        /// Get displacements in local coordinates of the element
        /// </summary>
        /// <returns></returns>
        MathNet.Numerics.LinearAlgebra.Vector<double> GetLocalDisplacementVector(double[] globalDisplacementsNodes);

        double[] GetLocalDisplacement(double[] globalDisplacementsNodes);
    }
}
