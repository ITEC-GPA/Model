using System.Collections.Generic;
using GPC.Model.Actions.Climate;
using GPC.Model.Actions.General;
using GPC.Model.Stages;

namespace GPC.Model.Actions.Construction
{
    public enum ConstructionLoadModel { Assigned, SelfWeight, EnPersonnel, EnBridgeStorage, EnConcreteCasting }
    public sealed class ConstructionStageInput
    {
        public string Name { get; set; } = "Fase";
        public double DurationDays { get; set; } = 7;
    }
    /// <summary>Loads in kN, m and kPa. Vertical load positive downwards, horizontal components along +x/+y.</summary>
    public sealed class ConstructionLoadInput
    {
        public string Name { get; set; } = "Carico";
        public ConstructionLoadModel Model { get; set; } = ConstructionLoadModel.SelfWeight;
        public ActionNature Nature { get; set; } = ActionNature.Variable;
        public int StartStage { get; set; } = 1;
        /// <summary>First inactive stage; zero means still active at the end of the schedule.</summary>
        public int StopStage { get; set; }
        public double LeftXM { get; set; }
        public double BottomYM { get; set; }
        public double LengthM { get; set; } = 10;
        public double WidthM { get; set; } = 5;
        public double ThicknessM { get; set; } = .2;
        public double UnitWeightKnM3 { get; set; } = 25;
        public double FormworkKpa { get; set; } = .5;
        public double AssignedPressureKpa { get; set; }
        public double PointLoadKn { get; set; }
        public double PointXM { get; set; }
        public double PointYM { get; set; }
        public double HorizontalXKn { get; set; }
        public double HorizontalYKn { get; set; }
        public double HorizontalHeightM { get; set; }
        public double WorkXOffsetM { get; set; }
        public double WorkYOffsetM { get; set; }
        public double DynamicFactor { get; set; } = 1;
        public string Source { get; set; } = "";
    }
    public sealed class ConstructionActionsInput
    {
        public ClimateStandard Standard { get; set; }
        public string NationalParameters { get; set; } = "";
        public bool RecommendedEnLoadsConfirmed { get; set; }
        public string ExecutionPlan { get; set; } = "";
        public List<ConstructionStageInput> ConstructionStages { get; set; } = new List<ConstructionStageInput>
        { new ConstructionStageInput { Name="Getto", DurationDays=14 }, new ConstructionStageInput { Name="Disarmo", DurationDays=14 } };
        public List<ConstructionLoadInput> ConstructionLoads { get; set; } = new List<ConstructionLoadInput>
        {
            new ConstructionLoadInput { Name="Calcestruzzo fresco", StartStage=1, StopStage=2 },
            new ConstructionLoadInput { Name="Peso permanente", StartStage=2, Nature=ActionNature.PermanentG1 }
        };
    }
    public sealed class ConstructionPressurePatch
    {
        public string Name { get; set; } = "";
        public double LeftXM { get; set; }
        public double BottomYM { get; set; }
        public double LengthM { get; set; }
        public double WidthM { get; set; }
        public double PressureKpa { get; set; }
    }
    public sealed class ConstructionLoadResult
    {
        public string Name { get; set; } = "";
        public ActionNature Nature { get; set; }
        public List<ConstructionPressurePatch> Patches { get; set; } = new List<ConstructionPressurePatch>();
        public double PointLoadKn { get; set; }
        public double PointXM { get; set; }
        public double PointYM { get; set; }
        public double VerticalDownKn { get; set; }
        public double HorizontalXKn { get; set; }
        public double HorizontalYKn { get; set; }
        public double MomentXKnM { get; set; }
        public double MomentYKnM { get; set; }
        public double MomentZKnM { get; set; }
    }
    public sealed class ConstructionStageResult
    {
        public string Name { get; set; } = "";
        public double StartDay { get; set; }
        public double EndDay { get; set; }
        public StageDefinition Definition { get; set; } = new StageDefinition(1,0);
        public List<ConstructionLoadResult> Loads { get; set; } = new List<ConstructionLoadResult>();
    }
    public sealed class ConstructionActionsResult
    {
        public ClimateResult Summary { get; set; } = new ClimateResult();
        public List<ConstructionStageResult> Stages { get; set; } = new List<ConstructionStageResult>();
    }
}
