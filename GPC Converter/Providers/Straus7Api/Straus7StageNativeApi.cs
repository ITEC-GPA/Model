using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace GPC.Converter.Straus7
{
    public sealed partial class Straus7NativeApi
    {
        public bool ReadStagedAnalysis()
        { byte value = 0; Check(Call<StageBool>("St7GetNLAStagedAnalysis")(FileId, ref value), "St7GetNLAStagedAnalysis"); return value != 0; }
        public bool ReadStageEnabled(int stage) => StageState("St7GetNLAStageState", stage);
        public bool ReadResultConverged(int resultCase) => StageState("St7GetResultCaseConvergence", resultCase);
        public bool ReadStagePseudoTimeEnabled(int stage) => StageState("St7GetNLAPseudoTimeState", stage);
        private bool StageState(string operation, int stage)
        { byte value = 0; Check(Call<StageIndexBool>(operation)(FileId, stage, ref value), operation); return value != 0; }
        public double ReadStageFluidLevel(int stage)
        { double value = 0; Check(Call<StageDouble>("St7GetStageFluidLevel")(FileId, stage, ref value), "St7GetStageFluidLevel"); return value; }
        public int ReadStageIncrementCount(int stage)
        { int value = 0; Check(Call<StageInt>("St7GetNumNLAIncrements")(FileId, stage, ref value), "St7GetNumNLAIncrements"); return value; }
        public string ReadStageIncrementName(int stage, int increment)
        { var name = new StringBuilder(1024); Check(Call<StageIncrementName>("St7GetNLAIncrementName")(FileId, stage, increment, name, name.Capacity), "St7GetNLAIncrementName"); return name.ToString(); }
        public bool ReadStageCaseEnabled(int stage, int caseNumber, bool freedom)
        {
            byte value = 0; string operation = freedom ? "St7GetNLAFreedomCaseState" : "St7GetNLALoadCaseState";
            Check(Call<StageCaseBool>(operation)(FileId, stage, caseNumber, ref value), operation); return value != 0;
        }
        public double ReadStageFactor(int stage, int increment, int caseNumber, bool freedom)
        {
            double value = 0; string operation = freedom ? "St7GetNLAFreedomIncrementFactor" : "St7GetNLALoadIncrementFactor";
            Check(Call<StageFactor>(operation)(FileId, stage, increment, caseNumber, ref value), operation); return value;
        }
        public double ReadStagePseudoTime(int stage, int increment)
        { double value = 0; Check(Call<StageTime>("St7GetNLAPseudoTime")(FileId, stage, increment, ref value), "St7GetNLAPseudoTime"); return value; }
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StageBool(int a, ref byte value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StageIndexBool(int a, int b, ref byte value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StageCaseBool(int a, int b, int c, ref byte value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StageDouble(int a, int b, ref double value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StageInt(int a, int b, ref int value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StageFactor(int a, int b, int c, int d, ref double value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StageTime(int a, int b, int c, ref double value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate int StageIncrementName(int a, int b, int c, StringBuilder name, int capacity);
    }

    public sealed partial class Straus7NativeWriter : IStraus7StageWriteApi
    {
        public void RequireStageInputs(int[] groupIds, int[] loadCaseIds, int[] freedomCaseIds)
        {
            if (api.StageCount != 0) throw new InvalidOperationException("StrausStageWriterRequiresEmptyStages");
            var nativeGroups = Enumerable.Range(1, api.GroupCount).Select(i => api.ReadGroup(i).Id).OrderBy(i => i);
            if (groupIds == null || loadCaseIds == null || freedomCaseIds == null
                || !nativeGroups.SequenceEqual(groupIds.OrderBy(i => i))
                || !Enumerable.Range(1, api.LoadCaseCount).SequenceEqual(loadCaseIds.OrderBy(i => i))
                || !Enumerable.Range(1, api.FreedomCaseCount).SequenceEqual(freedomCaseIds.OrderBy(i => i)))
                throw new InvalidOperationException("StrausStageNativeInventoryMismatch");
        }
        public void SetStagedAnalysis() => api.CheckWrite(api.WriteFunction<StageBool>("St7SetNLAStagedAnalysis")(Id, 1), "St7SetNLAStagedAnalysis");
        public void CreateStage(int number, string name, int[] flags, double? fluidLevelMm)
        {
            if (flags == null || flags.Length != 5 || flags.Any(f => f != 0 && f != 1)) throw new ArgumentException("FiveStageFlagsRequired");
            if (api.StageCount != number - 1) throw new InvalidOperationException("NonSequentialStrausStage");
            api.CheckWrite(api.WriteFunction<StageCreate>("St7AddStage")(Id, name, flags), "St7AddStage");
            api.CheckWrite(api.WriteFunction<MovingTwo>("St7EnableNLAStage")(Id, number), "St7EnableNLAStage");
            if (fluidLevelMm.HasValue)
                api.CheckWrite(api.WriteFunction<StageLevel>("St7SetStageFluidLevel")(Id, number, fluidLevelMm.Value), "St7SetStageFluidLevel");
            // Only this newly created stage is touched: discard any native auto-generated increment defaults.
            for (int i = api.ReadStageIncrementCount(number); i > 0; i--)
                api.CheckWrite(api.WriteFunction<MovingThree>("St7DeleteNLAIncrement")(Id, number, i), "St7DeleteNLAIncrement");
        }
        public void SetStageGroup(int stage, int group, bool active)
        {
            string operation = active ? "St7EnableStageGroup" : "St7DisableStageGroup";
            api.CheckWrite(api.WriteFunction<MovingThree>(operation)(Id, stage, group), operation);
        }
        public void SetStageCase(int stage, int caseNumber, bool freedom, bool active)
        {
            string operation = "St7" + (active ? "Enable" : "Disable") + "NLA" + (freedom ? "Freedom" : "Load") + "Case";
            api.CheckWrite(api.WriteFunction<MovingThree>(operation)(Id, stage, caseNumber), operation);
        }
        public void SetStagePseudoTime(int stage, bool enabled)
        {
            string operation = enabled ? "St7EnableNLAPseudoTime" : "St7DisableNLAPseudoTime";
            api.CheckWrite(api.WriteFunction<MovingTwo>(operation)(Id, stage), operation);
        }
        public void AddStageIncrement(int stage, int increment, string name, double? pseudoTimeSeconds)
        {
            if (api.ReadStageIncrementCount(stage) != increment - 1) throw new InvalidOperationException("NonSequentialStrausStageIncrement");
            api.CheckWrite(api.WriteFunction<MovingName>("St7AddNLAIncrement")(Id, stage, name), "St7AddNLAIncrement");
            if (pseudoTimeSeconds.HasValue)
                api.CheckWrite(api.WriteFunction<StageTime>("St7SetNLAPseudoTime")(Id, stage, increment, pseudoTimeSeconds.Value), "St7SetNLAPseudoTime");
        }
        public void SetStageFactor(int stage, int increment, int caseNumber, bool freedom, double totalFactor)
        {
            string operation = freedom ? "St7SetNLAFreedomIncrementFactor" : "St7SetNLALoadIncrementFactor";
            api.CheckWrite(api.WriteFunction<StageFactor>(operation)(Id, stage, increment, caseNumber, totalFactor), operation);
        }
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StageBool(int a, byte value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate int StageCreate(int a, string name, int[] flags);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StageLevel(int a, int b, double value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StageTime(int a, int b, int c, double value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int StageFactor(int a, int b, int c, int d, double value);
    }
}
