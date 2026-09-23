namespace GPC.Model.Sections.Glass
{
    /// <summary>
    /// Interface for generic glass layer. It may be Monolithic, Interlayer or AirChamber
    /// </summary>
    public interface IGlassLayer
    {
        double Thickness { get; set; }
    }
}
