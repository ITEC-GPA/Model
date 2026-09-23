using GPC.Model.LoadCases;

namespace GPC.Model.Results.ResultLocations
{
    public interface INodeResultLocation
    {
        string Name { get; }

        ResultType ResultTypes { get; }

        ILoadCase Case { get; }
    }
}
