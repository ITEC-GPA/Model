using GPC.Model.LoadCases;

namespace GPC.Model.Results.Locations
{
    /// <summary>
    /// A result of a node
    /// </summary>
    public interface INodeResultLocation
    {
        /// <summary>
        /// The name
        /// </summary>
        string Name { get; }

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
