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

        /// <returns>The coordinate system according to the Sap2000 rule for beams</returns>
        /// <remarks>
        /// <para>Axis-1 along the beam axis</para>
        /// <para>Axis-2 perpendicular to 1 and 3</para>
        /// <para>Axis-3 towards the positive Z global axis except if beam is parallel to global Z. In this case axis 2 is equal to global X</para>
        /// </remarks>
        public static CoordinateSystem GetBeamCoordinateSystem(Point3d pointStart, Point3d pointEnd, double angle)
        {
            // usiamo l'orientamento di SAP

            Vector3d v1 = new Vector3d(pointStart, pointEnd);
            v1.Unitize();


            Vector3d v3;
            if (Math.Abs(v1.DotProduct(Vector3d.ZAxis) - 1) < FemOptions.Instance.ToleranceLocalAxis)
            {
                v3 = Vector3d.YAxis;
            }
            else
            {
                v3 = v1 ^ Vector3d.ZAxis;
                v3.Unitize();
            }

            Vector3d v2 = v3 ^ v1;

            var coordinateSystem = new CoordinateSystem(pointStart, v1, v2, v3);
            coordinateSystem.RotateV1(angle);

            return coordinateSystem;
        }

    }
}
