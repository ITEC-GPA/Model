using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using GPC.Geometry;
using GPC.Model.Actions.Moving;

namespace GPC.Converter.Straus7
{
    /// <summary>Expanded library input. Persist Template with the GPC model: future exports do not resolve the library again.
    /// Library names are provenance, not a replacement for the resolved physical values.</summary>
    public sealed class Straus7ResolvedMovingTemplate
    {
        public string LibraryName { get; internal set; }
        public string ItemName { get; internal set; }
        public string ApiVersion { get; internal set; }
        public MovingLoadTemplate Template { get; internal set; }
        public Straus7MovingPathOptions Options { get; internal set; }
    }

    public static class Straus7MovingLibrary
    {
        /// <summary>Uses a unique new scratch model in the supplied existing directory. Does not alter the library.</summary>
        public static Straus7ResolvedMovingTemplate Resolve(string libraryName,string itemName,string scratchDirectory)
        {
            if(string.IsNullOrWhiteSpace(libraryName)||string.IsNullOrWhiteSpace(itemName))throw new ArgumentException("MissingMovingLibraryReference");
            string directory=Path.GetFullPath(scratchDirectory);
            if(!Directory.Exists(directory))throw new DirectoryNotFoundException(directory);
            using(var writer=new Straus7NativeWriter(Path.Combine(directory,"moving-library-"+Guid.NewGuid().ToString("N")+".st7"),directory))
            {
                writer.AssignMovingLibrary(1,libraryName,itemName);
                var result=writer.Reader.ReadExpandedMovingTemplate(1);result.LibraryName=libraryName;result.ItemName=itemName;
                return result;
            }
        }
    }

    public sealed partial class Straus7NativeApi
    {
        public IReadOnlyDictionary<string,string[]> MovingLibraryCatalog()
        {
            int count=0;Check(Call<LibraryCount>("St7GetNumLibraries")(5,ref count),"St7GetNumLibraries");
            var result=new Dictionary<string,string[]>(StringComparer.Ordinal);
            for(int library=1;library<=count;library++)
            {
                var name=new StringBuilder(1024);Check(Call<LibraryName>("St7GetLibraryName")(5,library,name,name.Capacity),"St7GetLibraryName");
                int items=0;Check(Call<LibraryItems>("St7GetNumLibraryItems")(5,library,ref items),"St7GetNumLibraryItems");var names=new string[items];
                for(int item=1;item<=items;item++){var text=new StringBuilder(1024);Check(Call<LibraryItemName>("St7GetLibraryItemName")(5,library,item,text,text.Capacity),"St7GetLibraryItemName");names[item-1]=text.ToString();}
                result.Add(name.ToString(),names);
            }
            return result;
        }
        public Straus7ResolvedMovingTemplate ReadExpandedMovingTemplate(int id)
        {
            var units=ReadUnits();if(units[0]!=2||units[1]!=0||units[5]!=0)throw new NotSupportedException("MovingTemplateReadRequiresNmmJoule");
            var parameters=ReadMovingTemplate(id,out var flags);
            var template=new MovingLoadTemplate {Name=Name("St7GetLoadPathTemplateName",id),MinimumLaneWidth=parameters[1],LaneFactorRule=(MovingLaneFactorRule)flags[2],
                LaneFactors=Enumerable.Range(1,flags[1]).Select(l=>ReadMovingLaneFactor(id,l)).ToArray()};
            int count=0;Check(Call<LibraryItems>("St7GetNumLoadPathTemplateVehicles")(FileId,id,ref count),"St7GetNumLoadPathTemplateVehicles");
            var vehicles=new List<MovingVehicle>();
            for(int v=1;v<=count;v++)
            {
                var data=ReadMovingVehicle(id,v,out var vf);var vehicle=new MovingVehicle {Name=VehicleName("St7GetLoadPathTemplateVehicleName",id,v),
                    ExclusiveSet=VehicleName("St7GetLoadPathTemplateVehicleSet",id,v),Placement=(MovingVehiclePlacement)vf[0],Direction=(MovingVehicleDirection)vf[1],Speed=data[0],StartTime=data[1]};
                var lanes=new List<int>();for(int lane=1;lane<=flags[1];lane++){byte enabled=0;Check(Call<MovingSetRead>("St7GetLoadPathTemplateVehicleLaneState")(FileId,id,v,lane,ref enabled),"St7GetLoadPathTemplateVehicleLaneState");if(enabled!=0)lanes.Add(lane);}
                if(lanes.Count==0)throw new NotSupportedException("LibraryVehicleHasNoEnabledLanes"); // empty domain selection means all lanes.
                vehicle.AllowedLanes=lanes.ToArray();
                vehicle.PointForces=Enumerable.Range(1,ForceCount("PointForces",id,v)).Select(p=>{
                    var d=ReadMovingPointForce(id,v,p,out var f);return new MovingPointForce {Mobility=(MovingPointMobility)f[0],Axes=(MovingLoadAxes)f[1],ApplyAdjacencyFactor=f[2]!=0,ApplyCentrifugal=f[3]!=0?(bool?)true:null,X=d[0],Y=d[1],Force=new Vector3d(d[2],d[3],d[4])};}).ToArray();
                vehicle.DistributedForces=Enumerable.Range(1,ForceCount("DistributedForces",id,v)).Select(p=>{
                    var d=ReadMovingDistributedForce(id,v,p,out var f);return new MovingDistributedForce {Mobility=(MovingDistributionMobility)f[0],Axes=(MovingLoadAxes)f[1],ApplyAdjacencyFactor=f[2]!=0,ApplyCentrifugal=f[3]!=0?(bool?)true:null,X1=d[0],X2=d[1],Y1=d[2],Y2=d[3],ForcePerLength=new Vector3d(d[4],d[5],d[6])};}).ToArray();
                int heat=ForceCount("HeatSources",id,v);
                if(heat!=0)vehicle.HeatSources=Enumerable.Range(1,heat).Select(p=>{var d=ReadMovingHeatSource(id,v,p);return new MovingHeatSource {X=d[0],Y=d[1],SizeX=d[2],SizeY=d[3],Power=d[4]*1000};}).ToArray();
                vehicles.Add(vehicle);
            }
            template.Vehicles=vehicles.ToArray();template.Centrifugal=ReadMovingCentrifugal(id);
            if(string.IsNullOrWhiteSpace(template.Centrifugal.OffsetExpression)&&string.IsNullOrWhiteSpace(template.Centrifugal.FactorExpression))template.Centrifugal=null;
            template.Validate();return new Straus7ResolvedMovingTemplate {Template=template,ApiVersion=Version,
                Options=new Straus7MovingPathOptions {Colour=flags[0],TransitionTrailingLoad=flags[3]!=0,BeamLateralTolerance=flags[4]!=0,RelativeTolerance=parameters[0]}};
        }
        private int ForceCount(string kind,int template,int vehicle)
        {int count=0;string op="St7GetNumLoadPathTemplate"+kind;Check(Call<LibraryForceCount>(op)(FileId,template,vehicle,ref count),op);return count;}
        private string VehicleName(string op,int template,int vehicle)
        {var name=new StringBuilder(1024);Check(Call<LibraryItemName>(op)(FileId,template,vehicle,name,name.Capacity),op);return name.ToString();}
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int LibraryCount(int kind,ref int count);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int LibraryItems(int kind,int library,ref int count);
        [UnmanagedFunctionPointer(CallingConvention.Winapi)] private delegate int LibraryForceCount(int file,int template,int vehicle,ref int count);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int LibraryName(int kind,int library,StringBuilder name,int max);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int LibraryItemName(int kind,int library,int item,StringBuilder name,int max);
    }
    public sealed partial class Straus7NativeWriter
    {
        internal void AssignMovingLibrary(int template,string libraryName,string itemName)
        {
            int library=0,item=0;
            api.CheckWrite(api.WriteFunction<LibraryId>("St7GetLibraryID")(5,libraryName,ref library),"St7GetLibraryID");
            api.CheckWrite(api.WriteFunction<LibraryItemId>("St7GetLibraryItemID")(5,library,itemName,ref item),"St7GetLibraryItemID");
            api.CheckWrite(api.WriteFunction<MovingName>("St7NewLoadPathTemplate")(Id,template,itemName),"St7NewLoadPathTemplate");
            api.CheckWrite(api.WriteFunction<Four>("St7AssignLibraryLoadPathTemplate")(Id,template,library,item),"St7AssignLibraryLoadPathTemplate");
        }
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int LibraryId(int kind,string name,ref int id);
        [UnmanagedFunctionPointer(CallingConvention.Winapi,CharSet=CharSet.Ansi)] private delegate int LibraryItemId(int kind,int library,string name,ref int id);
    }
}
