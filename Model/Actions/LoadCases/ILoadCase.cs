namespace GPC.Model.LoadCases
{
    /// <summary>
    /// The purpose of this interface is to group together the loadcases and the loadcombinations.
    /// </summary>
    public interface ILoadCase
    {
        /// <summary>
        /// The name of the load case or combination
        /// </summary>
        string Name { get; }
    }
}
