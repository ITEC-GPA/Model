using GPC.Geometry;

namespace GPC.Model.Loads
{
    /// <summary>
    /// A load applied on an area
    /// </summary>
    public interface IAreaLoad
    {
        /// <summary>
        /// The area of the load
        /// </summary>
        /// <returns>The shape, in the global coordinates</returns>
        Shape GetGeometry();
    }
}
