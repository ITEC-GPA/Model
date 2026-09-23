using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results.ResultLocations
{
    public interface IPlateResultLocation
    {
        string Name { get; }

        Point2d Location { get; }

        ResultType ResultTypes { get; }

        ILoadCase Case { get; }
    }
}
