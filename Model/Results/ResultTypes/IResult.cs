using GPC.Geometry;

namespace GPC.Model.Results
{
	internal interface IResult<T>
	{
		T ToCoordinateSystem(CoordinateSystem coordinateSystem);
	}
}
