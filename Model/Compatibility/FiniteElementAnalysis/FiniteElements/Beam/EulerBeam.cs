using GPC.Model.Elements;

namespace GPC.Model.FiniteElementAnalysis.FiniteElements.Beam
{
    /// <summary>
    /// Euler-Bernoulli beam finite element (a draft: see <see cref="FemBeam"/>)
    /// </summary>
    public class EulerBeam : FemBeam, IFiniteElement
    {
        #region Variables

        #endregion

        #region Properties

        #endregion

        /// <summary>
        /// Creates the finite element of a beam
        /// </summary>
        /// <param name="beamElement">The beam</param>
        public EulerBeam(BeamElement beamElement)
            : base(beamElement)
        {

        }
    }
}
