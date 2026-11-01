using System.Collections.Generic;
using GPC.Model.Actions.Climate;

namespace GPC.Model.Actions.Hydraulics
{
    /// <summary>Actual pressure ordinates for drawing and export. Position is elevation for walls/piers, x for slabs.</summary>
    public sealed class HydraulicDiagramPoint
    {
        public double PositionM { get; set; }
        public double PositivePressureKpa { get; set; }
        public double OpposingPressureKpa { get; set; }
        public double NetPressureKpa { get; set; }
        public double NetLineLoadKnM { get; set; }
    }
    public sealed class HydraulicCaseResult
    {
        public string Name { get; set; } = "";
        public double PositiveForceKn { get; set; }
        public double OpposingForceKn { get; set; }
        public double NetForceKn { get; set; }
        /// <summary>First moment about bottom elevation (wall/pier) or x=0 (slab), with the force sign.</summary>
        public double MomentKnM { get; set; }
        /// <summary>Absolute elevation or slab x; null for a zero resultant, including a pure couple.</summary>
        public double? ApplicationCoordinateM { get; set; }
        public List<HydraulicDiagramPoint> Diagram { get; set; } = new List<HydraulicDiagramPoint>();
    }
    public sealed class HydraulicActionsResult
    {
        public ClimateResult Summary { get; set; } = new ClimateResult();
        public List<HydraulicCaseResult> Cases { get; set; } = new List<HydraulicCaseResult>();
    }
}
