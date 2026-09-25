using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results.ResultLocations
{
    /// <summary>
    /// A result of a plate (area) element at a point
    /// </summary>
    public interface IPlateResultLocation
    {
        /// <summary>
        /// The name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The point of the result (2D coordinates in the plane of the element)
        /// </summary>
        Point2d Location { get; }

        /// <summary>
        /// The result values
        /// </summary>
        ResultType ResultTypes { get; }

        /// <summary>
        /// The load case or combination of the result
        /// </summary>
        ILoadCase Case { get; }
    }
}
