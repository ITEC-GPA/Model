using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Fem;
using GPC.Model.Fem.FemObjects;

namespace GPC.Model.Fem
{
    internal static class FemHelpers
    {


        /// <returns>The coordinate system according to the Straus7 rule for beams</returns>
        public static CoordinateSystem GetBeamCoordinateSystem(Point3d pointStart, Point3d pointEnd, double angle)
        {
            // usiamo l'orientamento di Straus

            Vector3d v3 = new Vector3d(pointStart, pointEnd);

            Vector3d v2;
            if (v3.DotProduct(Vector3d.ZAxis) < FemOptions.Instance.ToleranceLocalAxis)
            {
                v2 = Vector3d.YAxis;
            }
            else
            {
                v2 = Vector3d.ZAxis ^ v3;
            }

            var coordinateSystem = new CoordinateSystem(pointStart, v3, v2);
            coordinateSystem.RotateV3(angle);

            return coordinateSystem;
        }

    }
}
