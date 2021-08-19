using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;

namespace GPC.Model.Loads
{
    interface IConvertibleLoad
    {

        /// <summary>
        /// Convert this load into an area loads.
        /// </summary>
        /// <param name="referencePlane"></param>
        /// <param name="width">is used to convert the 0D/1D geometry into a 2D area</param>
        AreaLoad ConvertToAreaLoad(Plane referencePlane, double width);


        /// <summary>
        /// Convert this load into a normal area loads.
        /// </summary>
        /// <param name="referencePlane"></param>
        /// <param name="width">is used to convert the 0D/1D geometry into a 2D area</param>
        /// <remarks>The not normal portion will be lost</remarks>
        NormalAreaLoad ConvertToNormalAreaLoad(Plane referencePlane, double width);

    }
}
