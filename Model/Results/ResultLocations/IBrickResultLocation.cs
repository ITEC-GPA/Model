using GPC.Geometry;
using GPC.Model.LoadCases;

namespace GPC.Model.Results.ResultLocations
{
    public interface IBrickResultLocation
    {
        string Name { get; }

        Point3d Location { get; }

        ResultType ResultTypes { get; }

        ILoadCase Case { get; }
    }
}
