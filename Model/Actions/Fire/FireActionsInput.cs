using System.Collections.Generic;
using GPC.Model.Actions.Climate;

namespace GPC.Model.Actions.Fire
{
    public enum FireCurve { Standard, External, Hydrocarbon, AssignedHistory }
    public enum FireExposure { Exposed, Unexposed, Adiabatic }
    public enum FireSurfaceState { ConstantReference, AssignedHistory }
    public sealed class FireTemperaturePoint
    {
        public string Name { get; set; } = "Punto";
        public double TimeMinutes { get; set; }
        public double TemperatureC { get; set; } = 20;
    }
    public sealed class FireSurfacePoint
    {
        public string Name { get; set; } = "Punto superficie";
        public int Face { get; set; } = 1;
        public double TimeMinutes { get; set; }
        public double TemperatureC { get; set; } = 20;
    }
    public sealed class FireFaceInput
    {
        public string Name { get; set; } = "Faccia";
        public FireExposure Exposure { get; set; } = FireExposure.Exposed;
        public double StartXM { get; set; }
        public double StartYM { get; set; }
        public double EndXM { get; set; } = .3;
        public double EndYM { get; set; }
        public double MemberEmissivity { get; set; } = .8;
        public double ConfigurationFactor { get; set; } = 1;
        public bool AssignedConvection { get; set; }
        public double ConvectionWm2K { get; set; } = 25;
        /// <summary>EN unexposed face only: alpha=9 includes radiation, which must not be added again.</summary>
        public bool CombinedUnexposedTransfer { get; set; }
        public FireSurfaceState SurfaceState { get; set; }
        public double ReferenceSurfaceC { get; set; } = 20;
        public string SurfaceSource { get; set; } = "";
    }
    public sealed class FireActionsInput
    {
        public ClimateStandard Standard { get; set; }
        public string NationalParameters { get; set; } = "";
        public FireCurve Curve { get; set; }
        public double DurationMinutes { get; set; } = 120;
        public double TimeStepMinutes { get; set; } = .25;
        public double LengthM { get; set; } = 1;
        public double AmbientC { get; set; } = 20;
        public double FireEmissivity { get; set; } = 1;
        public string FireScenarioSource { get; set; } = "";
        public string BoundaryParametersSource { get; set; } = "";
        public bool CalculateBoundaryFlux { get; set; }
        public bool PrescribedSurfaceStateConfirmed { get; set; }
        public bool RadiationEqualsGasConfirmed { get; set; }
        public List<FireTemperaturePoint> GasTemperatures { get; set; } = new List<FireTemperaturePoint>
        {new FireTemperaturePoint{Name="Inizio",TimeMinutes=0},new FireTemperaturePoint{Name="Picco",TimeMinutes=60,TemperatureC=900},new FireTemperaturePoint{Name="Fine",TimeMinutes=120,TemperatureC=20}};
        public List<FireSurfacePoint> SurfaceTemperatures { get; set; } = new List<FireSurfacePoint>();
        public List<FireFaceInput> FireFaces { get; set; } = new List<FireFaceInput>
        {
            new FireFaceInput{Name="Inferiore",StartXM=0,StartYM=0,EndXM=.3,EndYM=0},
            new FireFaceInput{Name="Destra",StartXM=.3,StartYM=0,EndXM=.3,EndYM=.5},
            new FireFaceInput{Name="Superiore",StartXM=.3,StartYM=.5,EndXM=0,EndYM=.5,Exposure=FireExposure.Unexposed},
            new FireFaceInput{Name="Sinistra",StartXM=0,StartYM=.5,EndXM=0,EndYM=0}
        };
    }
    public sealed class FireFluxPoint
    {
        public double TimeMinutes { get; set; }
        public double GasC { get; set; }
        public double? SurfaceC { get; set; }
        public double? ConvectionWm2 { get; set; }
        public double? RadiationWm2 { get; set; }
        public double? NetWm2 { get; set; }
    }
    public sealed class FireFaceResult
    {
        public string Name { get; set; } = "";
        public FireExposure Exposure { get; set; }
        public double LengthM { get; set; }
        public double AreaM2 { get; set; }
        public double ConvectionWm2K { get; set; }
        public List<FireFluxPoint> History { get; set; } = new List<FireFluxPoint>();
    }
    public sealed class FireActionsResult
    {
        public ClimateResult Summary { get; set; } = new ClimateResult();
        public List<FireTemperaturePoint> GasHistory { get; set; } = new List<FireTemperaturePoint>();
        public List<FireFaceResult> Faces { get; set; } = new List<FireFaceResult>();
    }
}
