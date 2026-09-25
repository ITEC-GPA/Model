using GPC.Geometry;

namespace GPC.Model.Loads
{
    /// <summary>
    /// A load applied in a point
    /// </summary>
    public interface IPointLoad
    {
        /// <summary>
        /// The point of the load
        /// </summary>
        /// <returns>The point, in the global coordinates</returns>
        Point3d GetGeometry();
    }
}
