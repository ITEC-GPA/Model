using GPC.Geometry;

namespace GPC.Model.Loads
{
    public interface IPointLoad
    {
        Point3d GetGeometry();
    }
}
