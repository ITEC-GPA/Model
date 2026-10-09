using GPC.Geometry;

namespace GPC.Model.Loads
{
    /// <summary>
    /// A point or line load that can be converted into an equivalent area load, spreading it on a width
    /// </summary>
    public interface IConvertibleLoad
    {
        /// <summary>
        /// Convert this load into an area load: the force is spread on a square (point) or on a strip (line) of the given width on the reference plane
        /// </summary>
        /// <param name="referencePlane">The plane where the area is built</param>
        /// <param name="width">is used to convert the 0D/1D geometry into a 2D area</param>
        /// <returns>The area load with the same total force</returns>
        AreaLoad ConvertToAreaLoad(Plane referencePlane, double width);

        /// <summary>
        /// Convert this load into a normal area load (see <see cref="ConvertToAreaLoad(Plane, double)"/>)
        /// </summary>
        /// <param name="referencePlane">The plane where the area is built</param>
        /// <param name="width">is used to convert the 0D/1D geometry into a 2D area</param>
        /// <returns>The pressure normal to the plane with the same total normal force</returns>
        /// <remarks>The not normal portion will be lost</remarks>
        NormalAreaLoad ConvertToNormalAreaLoad(Plane referencePlane, double width);
    }
}
