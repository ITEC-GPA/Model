
namespace GPC.Model.Glasses
{
    public interface IInsulatingGlass
    {
        /// <remarks>Order of the glass panels is from external to internal</remarks>
        IGlassPackage[][] GetGlassPackage();
    }
}