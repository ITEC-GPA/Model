using System;

namespace GPC.Model.Checking
{
    [Serializable]
    public sealed class SurfaceSelection
    {
        public string SurfaceId { get; set; }
        /// <summary>Null selects the whole surface; otherwise select an explicit zone.</summary>
        public string ZoneId { get; set; }
    }
}
