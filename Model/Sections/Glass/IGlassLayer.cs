namespace GPC.Model.Sections.Glass
{
    /// <summary>
    /// Interface for generic glass layer. It may be Monolithic, Interlayer or AirChamber
    /// </summary>
    public interface IGlassLayer
    {
        /// <summary>
        /// The thickness of the layer
        /// </summary>
        double Thickness { get; set; }
    }
}
