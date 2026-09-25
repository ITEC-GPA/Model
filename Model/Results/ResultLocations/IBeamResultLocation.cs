using GPC.Model.LoadCases;

namespace GPC.Model.Results.ResultLocations
{
    /// <summary>
    /// A result of a beam element at a station
    /// </summary>
    public interface IBeamResultLocation
    {
        /// <summary>
        /// The name
        /// </summary>
        string Name { get; }

        /// <summary>
        /// The position of the station along the beam: 0 at the start point, 1 at the end point
        /// </summary>
        double ParametricDistance { get; }

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
