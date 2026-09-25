using GPC.Geometry;

namespace GPC.Model.Loads
{
    /// <summary>
    /// A load applied along a line
    /// </summary>
    public interface ILineLoad
    {
        /// <summary>
        /// The line of the load
        /// </summary>
        /// <returns>The segment, in the global coordinates</returns>
        Line3d GetGeometry();
    }
}
