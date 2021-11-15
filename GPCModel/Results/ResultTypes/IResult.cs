using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;

namespace GPC.Model.Results
{
    internal interface IResult<T>
    {

        T ToCoordinateSystem(CoordinateSystem coordinateSystem);

    }
}
