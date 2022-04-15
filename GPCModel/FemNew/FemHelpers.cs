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
        /// <remarks>
        /// <para>Axis-3 along the beam axis</para>
        /// <para>Axis-2 towards the positive Y global axis except if beam is parallel to global Z. In this case axis 2 is equal to global Z cross product axis-3</para>
        /// <para>Axis-1 perpendicular to 3 and 2</para>
        /// </remarks>
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

            Vector3d v1 = v2 ^ v3;

            var coordinateSystem = new CoordinateSystem(pointStart, v1, v2, v3);
            coordinateSystem.RotateV3(angle);

            return coordinateSystem;
        }

    }
}
