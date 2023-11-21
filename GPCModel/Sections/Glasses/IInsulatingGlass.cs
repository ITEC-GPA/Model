namespace GPC.Model.Sections.Glass
{
	/// <summary>
	/// Interface for generic insulating glass stratigraphy
	/// </summary>
	public interface IInsulatingGlass
	{
		/// <remarks>Order of the glass panels is from external to internal</remarks>
		IGlassPackage[][] GetGlassPackage();

		/// <returns>Total _thickness of the units package included interlayer</returns>
		double TotalThickness { get; }
	}
}