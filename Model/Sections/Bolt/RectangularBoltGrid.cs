using GPC.Geometry;
using GPC.Model.Materials;
using System.Collections.Generic;

namespace GPC.Model.Sections.Bolt
{
    /// <summary>
	/// Obsolete: it is no longer used, instead the generic BoltGrid is used.
    /// Maintained only for testing.
	/// </summary>
    public class RectangularBoltGrid : BoltGrid
    {
        /// <summary>
        /// The steps in X
        /// </summary>
        protected IEnumerable<double> _stepX;
        /// <summary>
        /// The steps in Y
        /// </summary>
        protected IEnumerable<double> _stepY;

        /// <summary>
        /// The steps in X
        /// </summary>
        public IEnumerable<double> StepX => _stepX;
        /// <summary>
        /// The steps in Y
        /// </summary>
        public IEnumerable<double> StepY => _stepY;

        #region Public Constructors

        /// <summary>
        /// Creates a rectangular grid of bolts.
        /// </summary>
        /// <param name="stepX">Steps in X.</param>
        /// <param name="stepY">Steps in Y.</param>
        /// <param name="diameter">The diameter of the bolts</param>
        /// <param name="mat">The material of the bolts</param>
        /// <param name="origin">Starting point, bottom left corner (null: the origin).</param>
        /// <param name="name">The name</param>
        public RectangularBoltGrid(IEnumerable<double> stepX, IEnumerable<double> stepY, double diameter, SteelMaterial mat, Point2d origin = default(Point2d), string name = "")
            : base(name)
        {
            _stepX = stepX;
            _stepY = stepY;

            List<BoltPosition> bolts = AddBoltsRectangularGrid(stepX, stepY, diameter, mat, origin);
        }

        #endregion
    }
}
