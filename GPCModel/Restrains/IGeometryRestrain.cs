
using GPC.Geometry;
using GPC.Model.FEM;
using System.Collections.Generic;

namespace GPC.Model.Restrains
{
    public interface IGeometryRestrain
    {
        Vector3d GetV1();

        Vector3d GetV2();

        Vector3d GetV3();

        Point3d GetCoordinateSystemOrigin();

        KeyValuePair<LinearSolver.DOF, bool>[] GetRestrains();

        KeyValuePair<LinearSolver.DOF, double>[] GetStiffnesses();

        GeometryBase GetGeometry();
    }
}
