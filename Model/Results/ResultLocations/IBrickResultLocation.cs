using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results.ResultLocations
{
    /// <summary>
    /// A result of a brick (volume) element at a point
    /// </summary>
    public interface IBrickResultLocation
    {
        /// <summary>
        /// The name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The point of the result
        /// </summary>
        Point3d Location { get; }

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
