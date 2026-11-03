using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Linq;

namespace GPC.Converter.Straus7
{
    public interface IStraus7MovingReadApi
    {
        int MovingTemplateCount { get; }
        double[] ReadMovingPath(int id,out int[] flags);
        double[] ReadMovingTemplate(int id,out int[] flags);
        double ReadMovingLaneFactor(int template,int lane);
        double[] ReadMovingVehicle(int template,int vehicle,out int[] flags);
        double[] ReadMovingPointForce(int template,int vehicle,int position,out int[] flags);
        double[] ReadMovingDistributedForce(int template,int vehicle,int position,out int[] flags);
    }
    public sealed partial class Straus7NativeApi : IStraus7MovingReadApi
    {
        public double[] ReadMovingHeatSource(int template,int vehicle,int position)
        {var flags=new int[1];var values=new double[5];Check(Call<MovingForceData>("St7GetLoadPathTemplateHeatSourceData")(FileId,template,vehicle,position,flags,values),"St7GetLoadPathTemplateHeatSourceData");return values;}
        public GPC.Model.Actions.Moving.MovingCentrifugalLaw ReadMovingCentrifugal(int template)
        {
            var offset=new StringBuilder(1024);var factor=new StringBuilder(1024);var flags=new int[3];var values=new double[2];
            Check(Call<MovingCentrifugalRead>("St7GetLoadPathTemplateCentrifugalData")(FileId,template,offset,factor,1024,flags,values),"St7GetLoadPathTemplateCentrifugalData");
            return new GPC.Model.Actions.Moving.MovingCentrifugalLaw {ExpressionLanguage="Straus7-R3",OffsetExpression=offset.ToString(),FactorExpression=factor.ToString(),
                LengthUnit=new[]{"m","cm","mm","ft","in"}[flags[1]],ForceUnit=new[]{"N","kN","MN","kgf","lbf","tf","kip"}[flags[2]],
                LimitFactor=flags[0]!=0,MinimumFactor=values[0],MaximumFactor=values[1]};
        }
        public bool IsInMovingEntitySet(int family,int entity,int set)
        {byte included=0;Check(Call<MovingSetRead>("St7GetEntitySetEntityState")(FileId,family,entity,set,ref included),"St7GetEntitySetEntityState");return included!=0;}
        public int MovingTemplateCount
        { get { int count=0,last=0; Check(Call<MovingTotals>("St7GetTotalLoadPathTemplates")(FileId,ref count,ref last),"St7GetTotalLoadPathTemplates"); return count; } }
        public double[] ReadMovingPath(int id,out int[] flags)
        { flags=new int[7]; var values=new double[9]; Check(Call<MovingData>("St7GetLoadPath")(FileId,id,flags,values),"St7GetLoadPath"); return values; }
        public double[] ReadMovingTemplate(int id,out int[] flags)
        { flags=new int[5]; var values=new double[2]; Check(Call<MovingData>("St7GetLoadPathTemplateParameters")(FileId,id,flags,values),"St7GetLoadPathTemplateParameters"); return values; }
        public double ReadMovingLaneFactor(int template,int lane)
        { double factor=0; Check(Call<MovingFactor>("St7GetLoadPathTemplateLaneFactor")(FileId,template,lane,ref factor),"St7GetLoadPathTemplateLaneFactor"); return factor; }
        public double[] ReadMovingVehicle(int template,int vehicle,out int[] flags)
        { flags=new int[2]; var values=new double[2]; Check(Call<MovingVehicleData>("St7GetLoadPathTemplateVehicleData")(FileId,template,vehicle,flags,values),"St7GetLoadPathTemplateVehicleData"); return values; }
        public double[] ReadMovingPointForce(int template,int vehicle,int position,out int[] flags) => ReadMovingForce("St7GetLoadPathTemplatePointForceData",template,vehicle,position,5,out flags);
        public double[] ReadMovingDistributedForce(int template,int vehicle,int position,out int[] flags) => ReadMovingForce("St7GetLoadPathTemplateDistributedForceData",template,vehicle,position,7,out flags);
        private double[] ReadMovingForce(string operation,int template,int vehicle,int position,int count,out int[] flags)
        { flags=new int[4]; var values=new double[count]; Check(Call<MovingForceData>(operation)(FileId,template,vehicle,position,flags,values),operation); return values; }
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingTotals(int a,ref int b,ref int c);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingData(int a,int b,[Out] int[] flags,[Out] double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingVehicleData(int a,int b,int c,[Out] int[] flags,[Out] double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingForceData(int a,int b,int c,int d,[Out] int[] flags,[Out] double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingFactor(int a,int b,int c,ref double value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int MovingCentrifugalRead(int a,int b,StringBuilder offset,StringBuilder factor,int max,int[] flags,double[] limits);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingSetRead(int a,int family,int entity,int set,ref byte included);
    }
    public sealed partial class Straus7NativeWriter : IStraus7MovingWriteApi,IStraus7AdvancedMovingWriteApi
    {
        public void SetMovingCentrifugal(int template,string offset,string factor,int[] flags,double[] limits)
            =>api.CheckWrite(api.WriteFunction<MovingCentrifugalWrite>("St7SetLoadPathTemplateCentrifugalData")(Id,template,offset,factor,flags,limits),"St7SetLoadPathTemplateCentrifugalData");
        public void AddMovingHeatSource(int template,int vehicle,int position,double[] values)
        {
            if(api.ReadUnits()[5]!=0)throw new NotSupportedException("MovingHeatRequiresNativeJouleEnergyUnit");
            AddMovingForce("HeatSource",template,vehicle,position,new int[1],values);
        }
        public void CreateMovingEntitySet(int id,int[] families,int[] entityIds)
        {
            if(families==null||entityIds==null||families.Length!=entityIds.Length||families.Length==0||families.Any(f=>f<1||f>3)||entityIds.Any(i=>i<=0))throw new ArgumentException("InvalidMovingNativeSelection");
            int count=0;api.CheckWrite(api.WriteFunction<MovingSetCount>("St7GetNumEntitySets")(Id,ref count),"St7GetNumEntitySets");
            if(count!=id-1)throw new InvalidOperationException("MovingEntitySetsRequireFreshConsecutiveIds");
            api.CheckWrite(api.WriteFunction<MovingSetName>("St7NewEntitySet")(Id,"GPC moving selection "+id),"St7NewEntitySet");
            foreach(int family in families.Distinct())
            {
                api.CheckWrite(api.WriteFunction<MovingSelectAll>("St7SetAllEntitySelectState")(Id,family,0),"St7SetAllEntitySelectState");
                for(int i=0;i<families.Length;i++)if(families[i]==family)
                    api.CheckWrite(api.WriteFunction<MovingSelect>("St7SetEntitySelectState")(Id,family,entityIds[i],0,1),"St7SetEntitySelectState");
                api.CheckWrite(api.WriteFunction<MovingThree>("St7AddSelectedToEntitySet")(Id,family,id),"St7AddSelectedToEntitySet");
                api.CheckWrite(api.WriteFunction<MovingSelectAll>("St7SetAllEntitySelectState")(Id,family,0),"St7SetAllEntitySelectState");
            }
        }
        public void RequireEmptyMovingLoads()
        {
            if(api.Count((Straus7Entity)8)!=0||api.MovingTemplateCount!=0) throw new InvalidOperationException("StrausMovingWriterRequiresEmptyPathsAndTemplates");
        }
        public void CreateMovingTemplate(int id,string name,int[] flags,double[] values)
        {
            api.CheckWrite(api.WriteFunction<MovingName>("St7NewLoadPathTemplate")(Id,id,name),"St7NewLoadPathTemplate");
            api.CheckWrite(api.WriteFunction<MovingData>("St7SetLoadPathTemplateParameters")(Id,id,flags,values),"St7SetLoadPathTemplateParameters");
        }
        public void SetMovingLaneFactor(int template,int lane,double factor) => api.CheckWrite(api.WriteFunction<MovingFactor>("St7SetLoadPathTemplateLaneFactor")(Id,template,lane,factor),"St7SetLoadPathTemplateLaneFactor");
        public void AddMovingVehicle(int template,int vehicle,string name,int[] flags,double[] values,string exclusiveSet,bool[] lanes)
        {
            api.CheckWrite(api.WriteFunction<MovingTwo>("St7AddLoadPathTemplateVehicle")(Id,template),"St7AddLoadPathTemplateVehicle");
            api.CheckWrite(api.WriteFunction<MovingVehicleName>("St7SetLoadPathTemplateVehicleName")(Id,template,vehicle,name),"St7SetLoadPathTemplateVehicleName");
            api.CheckWrite(api.WriteFunction<MovingVehicleData>("St7SetLoadPathTemplateVehicleData")(Id,template,vehicle,flags,values),"St7SetLoadPathTemplateVehicleData");
            api.CheckWrite(api.WriteFunction<MovingVehicleName>("St7SetLoadPathTemplateVehicleSet")(Id,template,vehicle,exclusiveSet),"St7SetLoadPathTemplateVehicleSet");
            for(int i=0;i<lanes.Length;i++)
            {
                string operation=lanes[i]?"St7EnableLoadPathTemplateVehicleLane":"St7DisableLoadPathTemplateVehicleLane";
                api.CheckWrite(api.WriteFunction<Four>(operation)(Id,template,vehicle,i+1),operation);
            }
        }
        public void AddMovingPointForce(int template,int vehicle,int position,int[] flags,double[] values) => AddMovingForce("PointForce",template,vehicle,position,flags,values);
        public void AddMovingDistributedForce(int template,int vehicle,int position,int[] flags,double[] values) => AddMovingForce("DistributedForce",template,vehicle,position,flags,values);
        private void AddMovingForce(string kind,int template,int vehicle,int position,int[] flags,double[] values)
        {
            string add="St7AddLoadPathTemplate"+kind,set="St7SetLoadPathTemplate"+kind+"Data";
            api.CheckWrite(api.WriteFunction<MovingThree>(add)(Id,template,vehicle),add);
            api.CheckWrite(api.WriteFunction<MovingForceData>(set)(Id,template,vehicle,position,flags,values),set);
        }
        public void SetMovingPath(int id,int[] flags,double[] coordinates,bool convertToStatic)
        {
            api.CheckWrite(api.WriteFunction<MovingData>("St7SetLoadPath")(Id,id,flags,coordinates),"St7SetLoadPath");
            api.CheckWrite(api.WriteFunction<MovingSelect>("St7SetEntitySelectState")(Id,8,id,0,(byte)(convertToStatic?1:0)),"St7SetEntitySelectState");
        }
        public IReadOnlyDictionary<int,string> ConvertSelectedMovingPaths()
        {
            int before=api.LoadCaseCount;
            api.CheckWrite(api.WriteFunction<MovingConvert>("St7ConvertLoadPathsToLoadCases")(Id,1,1,1),"St7ConvertLoadPathsToLoadCases");
            int after=api.LoadCaseCount; var cases=new Dictionary<int,string>();
            for(int i=before+1;i<=after;i++) cases.Add(i,api.ReadLoadCase(i));
            if(cases.Count==0) throw new InvalidOperationException("StrausMovingConversionProducedNoCases"); return cases;
        }
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingTwo(int a,int b);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingThree(int a,int b,int c);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int MovingName(int a,int b,string name);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int MovingVehicleName(int a,int b,int c,string name);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingData(int a,int b,int[] flags,double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingVehicleData(int a,int b,int c,int[] flags,double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingForceData(int a,int b,int c,int d,int[] flags,double[] values);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingFactor(int a,int b,int c,double value);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingSelect(int a,int b,int c,int d,byte selected);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingConvert(int a,byte points,byte distributed,byte heat);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int MovingCentrifugalWrite(int a,int b,string offset,string factor,int[] flags,double[] limits);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingSetCount(int a,ref int count);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int MovingSetName(int a,string name);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int MovingSelectAll(int a,int family,byte selected);
    }
}
