using GPC.Geometry;

namespace GPC.Model.Results
{
	/// <summary>
	/// A result that can be written in another coordinate system
	/// </summary>
	/// <typeparam name="T">The type of the result</typeparam>
	internal interface IResult<T>
	{
		/// <summary>
		/// The same result in another coordinate system
		/// </summary>
		/// <param name="coordinateSystem">The new coordinate system</param>
		/// <returns>The new result</returns>
		T ToCoordinateSystem(CoordinateSystem coordinateSystem);
	}
}
