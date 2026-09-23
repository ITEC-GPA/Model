using GPC.Model.LoadCases;

namespace GPC.Model.Results.ResultLocations
{
    public interface IBeamResultLocation
    {
        string Name { get; }

        double ParametricDistance { get; }

        ResultType ResultTypes { get; }

        ILoadCase Case { get; }
    }
}
