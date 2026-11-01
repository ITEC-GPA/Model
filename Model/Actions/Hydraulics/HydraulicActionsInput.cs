using System.Collections.Generic;

namespace GPC.Model.Actions.Hydraulics
{
    public enum HydraulicElement { VerticalWall, HorizontalSlab, RiverPier }
    public enum UpliftDistribution { HydrostaticHead, AssignedPressureProfile }

    /// <summary>Independent water scenario. Levels are absolute elevations in the same datum as the geometry.</summary>
    public sealed class HydraulicWaterCase
    {
        public string Name { get; set; } = "Ordinario";
        public double LeftLevelM { get; set; } = 3;
        public double RightLevelM { get; set; }
        public double CurrentVelocityMs { get; set; } = 2;
        public double ProfileMultiplier { get; set; } = 1;
    }
    public sealed class HydraulicPressurePoint
    {
        public double PositionM { get; set; }
        public double PressureKpa { get; set; }
    }
    public sealed class HydraulicVelocityPoint
    {
        public double ElevationM { get; set; }
        public double VelocityMs { get; set; }
    }

    /// <summary>
    /// Water actions on one planar vertical wall, rectangular horizontal slab, or vertical river pier.
    /// m, kN, kPa, m/s; positive wall force is left to right, positive slab force is upwards.
    /// Pier width is the projected width normal to the assigned current; Cd is supplied by the hydraulic study.
    /// </summary>
    public sealed class HydraulicActionsInput
    {
        public HydraulicElement Element { get; set; }
        public double UnitWeightKnM3 { get; set; } = 9.81;
        public double BottomElevationM { get; set; }
        public double TopElevationM { get; set; } = 5;
        public double WidthM { get; set; } = 2;
        public double SlabLengthM { get; set; } = 10;
        public double SlabThicknessM { get; set; } = .5;
        public UpliftDistribution Uplift { get; set; }
        public string PressureSource { get; set; } = "";
        public List<HydraulicPressurePoint> PressureProfile { get; set; } = new List<HydraulicPressurePoint>
        {
            new HydraulicPressurePoint { PositionM = 0, PressureKpa = 30 },
            new HydraulicPressurePoint { PositionM = 10, PressureKpa = 10 }
        };
        public double DragCoefficient { get; set; } = 1;
        public string DragSource { get; set; } = "";
        public bool UseVelocityProfile { get; set; }
        public List<HydraulicVelocityPoint> VelocityProfile { get; set; } = new List<HydraulicVelocityPoint>
        {
            new HydraulicVelocityPoint { ElevationM = 0, VelocityMs = 0 },
            new HydraulicVelocityPoint { ElevationM = 5, VelocityMs = 2 }
        };
        public List<HydraulicWaterCase> WaterCases { get; set; } = new List<HydraulicWaterCase> { new HydraulicWaterCase() };
        public string ProjectSource { get; set; } = "";
    }
}
