
using GPC.Geometry;

namespace GPC.Model.Elements
{
    public interface IRestrain
    {
        Vector3d GetV1();

        Vector3d GetV2();

        Vector3d GetV3();

        Point3d GetCoordinateSystemOrigin();

        bool[] GetRestrains();

        double[] GetStiffnesses();
    }
}
