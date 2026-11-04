using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Actions.Moving;
using GPC.Model.Core;
using GPC.Model.Core.Identity;
using GPC.Model.Models;

namespace GPC.Converter.Straus7
{
    public interface IStraus7AdvancedMovingWriteApi
    {
        void SetMovingCentrifugal(int template,string offset,string factor,int[] flags,double[] limits);
        void AddMovingHeatSource(int template,int vehicle,int position,double[] values);
        void CreateMovingEntitySet(int id,int[] families,int[] entityIds);
    }
    /// <summary>Load-path API operations. Arrays follow the R3 SDK, after domain validation and explicit identity mapping.</summary>
    public interface IStraus7MovingWriteApi
    {
        void RequireEmptyMovingLoads();
        void CreateMovingTemplate(int id,string name,int[] flags,double[] values);
        void SetMovingLaneFactor(int template,int lane,double factor);
        void AddMovingVehicle(int template,int vehicle,string name,int[] flags,double[] values,string exclusiveSet,bool[] lanes);
        void AddMovingPointForce(int template,int vehicle,int position,int[] flags,double[] values);
        void AddMovingDistributedForce(int template,int vehicle,int position,int[] flags,double[] values);
        void SetMovingPath(int id,int[] flags,double[] coordinates,bool convertToStatic);
        IReadOnlyDictionary<int,string> ConvertSelectedMovingPaths();
    }

    /// <summary>Straus7 load-path controls. The four controls that change the applied loads are persisted in the GPC model
    /// (<see cref="MovingLoadPath.Options"/>); the display colour and the legacy native entity-set number are software data,
    /// persisted as "Straus7 API" preserved data by <see cref="Store"/>. Lengths use mm.</summary>
    public sealed class Straus7MovingPathOptions
    {
        public const string ColourRecord="MovingPathColour/",EntitySetRecord="MovingPathEntitySet/";
        public int Colour { get; set; } = 255;
        public bool TransitionTrailingLoad { get; set; }
        public bool BeamLateralTolerance { get; set; }
        public double RelativeTolerance { get; set; } = .01;
        /// <summary>Generate native static cases at the centres of the path divisions.
        /// The native conversion does not add endpoint positions or compute a continuous influence envelope.</summary>
        public bool ConvertToStaticCases { get; set; }
        /// <summary>Native entity set of a legacy export. The writer regenerates sets from <see cref="MovingLoadPath.TargetElementGuids"/>:
        /// a legacy set without a GPC selection is rejected instead of being dropped.</summary>
        public int? EntitySetId { get; set; }
        internal void Validate()
        {
            if(Colour<0||Colour>0xffffff||double.IsNaN(RelativeTolerance)||double.IsInfinity(RelativeTolerance)||RelativeTolerance<0||EntitySetId<1)
                throw new ArgumentException("InvalidStrausMovingOptions");
        }
        private bool SameLoadApplication(MovingPathOptions o)=>o.TransitionTrailingLoad==TransitionTrailingLoad&&o.BeamLateralTolerance==BeamLateralTolerance
            &&o.RelativeTolerance==RelativeTolerance&&o.ConvertToStaticCases==ConvertToStaticCases;

        /// <summary>Persists these options with the model: the load-application controls in the path, colour and entity set as preserved data.</summary>
        public void Store(global::GPC.Model.Models.Model model,Guid pathId)
        {
            Validate();var path=model?.MovingLoads?.Paths.SingleOrDefault(p=>p.Id==pathId)??throw new ArgumentException("UnknownMovingPath");
            path.Options=new MovingPathOptions {TransitionTrailingLoad=TransitionTrailingLoad,BeamLateralTolerance=BeamLateralTolerance,RelativeTolerance=RelativeTolerance,ConvertToStaticCases=ConvertToStaticCases};
            Preserve(model,ColourRecord+pathId.ToString("D"),Colour.ToString(System.Globalization.CultureInfo.InvariantCulture),"display colour of the native load path (no mechanical effect)");
            if(EntitySetId.HasValue)Preserve(model,EntitySetRecord+pathId.ToString("D"),EntitySetId.Value.ToString(System.Globalization.CultureInfo.InvariantCulture),"legacy native entity set; the writer uses TargetElementGuids");
        }
        private static void Preserve(global::GPC.Model.Models.Model model,string record,string value,string reason)
        {
            model.PreservedSourceData.RemoveAll(p=>p!=null&&p.Kind=="Straus7 API"&&p.SourceRecord==record);
            model.PreservedSourceData.Add(new GPC.Model.Structure.Assignments.PreservedAssignment {Kind="Straus7 API",SourceRecord=record,RawData=value,UnsupportedReason=reason});
        }

        /// <summary>Options of one path: explicit ones (which must agree with the persisted load-application controls), else the persisted
        /// controls and preserved display data, else the native defaults.</summary>
        internal static Straus7MovingPathOptions Effective(global::GPC.Model.Models.Model model,MovingLoadPath path,IReadOnlyDictionary<Guid,Straus7MovingPathOptions> explicitOptions)
        {
            Straus7MovingPathOptions result=null;
            if(explicitOptions!=null&&explicitOptions.TryGetValue(path.Id,out var requested))
            {
                result=requested??throw new ArgumentException("MissingStrausMovingOptions");
                if(path.Options!=null&&!result.SameLoadApplication(path.Options))throw new ArgumentException("MovingPathOptionsConflictWithModel");
                return result;
            }
            result=new Straus7MovingPathOptions();
            if(path.Options!=null)
            {result.TransitionTrailingLoad=path.Options.TransitionTrailingLoad;result.BeamLateralTolerance=path.Options.BeamLateralTolerance;result.RelativeTolerance=path.Options.RelativeTolerance;result.ConvertToStaticCases=path.Options.ConvertToStaticCases;}
            string colour=Preserved(model,ColourRecord+path.Id.ToString("D")),set=Preserved(model,EntitySetRecord+path.Id.ToString("D"));
            if(colour!=null)result.Colour=int.Parse(colour,System.Globalization.NumberStyles.Integer,System.Globalization.CultureInfo.InvariantCulture);
            if(set!=null)result.EntitySetId=int.Parse(set,System.Globalization.NumberStyles.Integer,System.Globalization.CultureInfo.InvariantCulture);
            return result;
        }
        private static string Preserved(global::GPC.Model.Models.Model model,string record)
            =>model.PreservedSourceData.Where(p=>p!=null&&p.Kind=="Straus7 API"&&p.SourceRecord==record).Select(p=>p.RawData).SingleOrDefault();
    }

    /// <summary>Detached mechanical moving-load export. Does not create topology or native load cases.
    /// Unsupported entity selections and static conversion of floating loads fail before any native write.</summary>
    public sealed class Straus7MovingWritePlan
    {
        private readonly Action<IStraus7MovingWriteApi>[] operations;
        private readonly bool convert;
        private readonly Guid modelGuid;
        private readonly bool advanced;
        internal bool RequiresAdvancedApi => advanced;
        public string AnalysisFingerprint { get; }
        public int TemplateCount { get; }
        public int PathCount { get; }
        private Straus7MovingWritePlan(Guid owner,string fingerprint,int templates,int paths,bool convert,bool advanced,List<Action<IStraus7MovingWriteApi>> operations)
        { modelGuid=owner; AnalysisFingerprint=fingerprint; TemplateCount=templates; PathCount=paths; this.convert=convert; this.advanced=advanced; this.operations=operations.ToArray(); }
        public static Straus7MovingWritePlan Prepare(global::GPC.Model.Models.Model model,IReadOnlyDictionary<Guid,int> loadCaseIds,
            IReadOnlyDictionary<Guid,Straus7MovingPathOptions> options=null)
            => Prepare(model,loadCaseIds,options,null);

        public static Straus7MovingWritePlan Prepare(global::GPC.Model.Models.Model model,IReadOnlyDictionary<Guid,int> loadCaseIds,
            IReadOnlyDictionary<Guid,Straus7MovingPathOptions> options,SolverIdentityMap identity)
        {
            if(model==null) throw new ArgumentNullException(nameof(model));
            model.RequireExportable(); // domain preflight of every exporter (review L4)
            return PrepareCore(model,loadCaseIds,options,identity);
        }

        /// <summary>The moving-load part of a full export, whose preflight already ran (Straus7ModelWritePlan.Prepare).</summary>
        internal static Straus7MovingWritePlan PrepareCore(global::GPC.Model.Models.Model model,IReadOnlyDictionary<Guid,int> loadCaseIds,
            IReadOnlyDictionary<Guid,Straus7MovingPathOptions> options,SolverIdentityMap identity)
        {
            if(model==null||loadCaseIds==null) throw new ArgumentNullException();
            if(model.MovingLoads==null) throw new ArgumentException("MissingMovingLoadDefinition");
            model.MovingLoads.Validate(model);
            if(loadCaseIds.Values.Any(i=>i<1)||loadCaseIds.Values.Distinct().Count()!=loadCaseIds.Count) throw new ArgumentException("InvalidStrausLoadCaseMap");
            identity?.ValidateAgainst(model);
            var copy=ModelValues.CopyValue(model.MovingLoads); var ops=new List<Action<IStraus7MovingWriteApi>>(); var templates=new Dictionary<string,int>(); bool convert=false,advanced=false;int entitySetCount=0;
            if(options!=null && options.Keys.Any(id=>!copy.Paths.Any(p=>p.Id==id))) throw new ArgumentException("UnknownMovingPathOptions");
            for(int pathIndex=0;pathIndex<copy.Paths.Count;pathIndex++)
            {
                var path=copy.Paths[pathIndex]; var template=path.Template;
                if(!loadCaseIds.TryGetValue(path.CaseGuid,out int loadCase)) throw new ArgumentException("MissingStrausMovingCaseIdentity");
                int set=0;
                if(path.Target==MovingPathTarget.Selection)
                {
                    if(identity==null)throw new ArgumentException("MovingSelectionRequiresSolverIdentityMap");
                    var selected=path.TargetElementGuids.Select(g=>identity.Entries.Single(e=>e.EntityGuid==g)).ToArray();
                    var families=selected.Select(e=>e.Family==EntityFamily.Beam?1:e.Family==EntityFamily.Shell?2:e.Family==EntityFamily.Solid?3:throw new ArgumentException("InvalidMovingTargetFamily")).ToArray();
                    var ids=selected.Select(e=>e.SolverId).ToArray();set=++entitySetCount;int setId=set;advanced=true;
                    ops.Add(a=>((IStraus7AdvancedMovingWriteApi)a).CreateMovingEntitySet(setId,(int[])families.Clone(),(int[])ids.Clone()));
                }
                var settings=Straus7MovingPathOptions.Effective(model,path,options); settings.Validate();
                if(settings.EntitySetId.HasValue&&path.Target!=MovingPathTarget.Selection) throw new NotSupportedException("LegacyMovingEntitySetRequiresTargetSelection");
                if(template.LaneFactors.Length>16) throw new NotSupportedException("StrausMovingMaximum16Lanes");
                bool toStatic=settings.ConvertToStaticCases;
                if(toStatic && template.Vehicles.Any(v=>v.PointForces.Any(f=>f.Mobility==MovingPointMobility.Floating)
                    ||v.DistributedForces.Any(f=>f.Mobility==MovingDistributionMobility.Floating)))
                    throw new NotSupportedException("StrausStaticConversionWouldOmitFloatingLoads");
                int[] flags={settings.Colour,template.LaneFactors.Length,(int)template.LaneFactorRule,settings.TransitionTrailingLoad?1:0,settings.BeamLateralTolerance?1:0};
                double[] values={settings.RelativeTolerance,template.MinimumLaneWidth};
                string key=ModelValues.Fingerprint(new object[]{template,flags,values});
                if(!templates.TryGetValue(key,out int templateId))
                {
                    templateId=templates.Count+1; templates.Add(key,templateId); int id=templateId;
                    ops.Add(a=>a.CreateMovingTemplate(id,template.Name,(int[])flags.Clone(),(double[])values.Clone()));
                    if(template.Centrifugal!=null)
                    {
                        // The N-mm GPC law becomes R3 expressions in mm and N (B8): the dialect and its unit codes stay in the Converter.
                        var law=Straus7CentrifugalExpression.FromDomain(template.Centrifugal,model);
                        int length=Straus7CentrifugalExpression.LengthUnits.ToList().IndexOf(law.LengthUnit),force=Straus7CentrifugalExpression.ForceUnits.ToList().IndexOf(law.ForceUnit);
                        int[] cf={law.LimitFactor?1:0,length,force};double[] limits={law.MinimumFactor,law.MaximumFactor};advanced=true;
                        ops.Add(a=>((IStraus7AdvancedMovingWriteApi)a).SetMovingCentrifugal(id,law.OffsetExpression,law.FactorExpression,(int[])cf.Clone(),(double[])limits.Clone()));
                    }
                    for(int i=0;i<template.LaneFactors.Length;i++) { int lane=i+1; double factor=template.LaneFactors[i]; ops.Add(a=>a.SetMovingLaneFactor(id,lane,factor)); }
                    for(int v=0;v<template.Vehicles.Length;v++)
                    {
                        var vehicle=template.Vehicles[v]; int number=v+1;
                        var vehicleFlags=new[]{(int)vehicle.Placement,(int)vehicle.Direction}; var vehicleValues=new[]{vehicle.Speed,vehicle.StartTime};
                        var lanes=Enumerable.Range(1,template.LaneFactors.Length).Select(l=>vehicle.AllowedLanes.Length==0||vehicle.AllowedLanes.Contains(l)).ToArray();
                        ops.Add(a=>a.AddMovingVehicle(id,number,vehicle.Name,(int[])vehicleFlags.Clone(),(double[])vehicleValues.Clone(),vehicle.ExclusiveSet??"",(bool[])lanes.Clone()));
                        for(int i=0;i<vehicle.PointForces.Length;i++)
                        {
                            var force=vehicle.PointForces[i]; int position=i+1;
                            var f=new[]{(int)force.Mobility,(int)force.Axes,force.ApplyAdjacencyFactor?1:0,force.ApplyCentrifugal==true?1:0};
                            var d=new[]{force.X,force.Y,force.Force.X,force.Force.Y,force.Force.Z};
                            ops.Add(a=>a.AddMovingPointForce(id,number,position,(int[])f.Clone(),(double[])d.Clone()));
                        }
                        for(int i=0;i<vehicle.DistributedForces.Length;i++)
                        {
                            var force=vehicle.DistributedForces[i]; int position=i+1;
                            var f=new[]{(int)force.Mobility,(int)force.Axes,force.ApplyAdjacencyFactor?1:0,force.ApplyCentrifugal==true?1:0};
                            var d=new[]{force.X1,force.X2,force.Y1,force.Y2,force.ForcePerLength.X,force.ForcePerLength.Y,force.ForcePerLength.Z};
                            ops.Add(a=>a.AddMovingDistributedForce(id,number,position,(int[])f.Clone(),(double[])d.Clone()));
                        }
                        if(vehicle.HeatSources!=null)for(int i=0;i<vehicle.HeatSources.Length;i++)
                        {
                            var h=vehicle.HeatSources[i];int position=i+1;advanced=true;
                            var data=new[]{h.X,h.Y,h.SizeX,h.SizeY,h.Power/1000}; // N mm/s -> J/s, native energy unit J.
                            ops.Add(a=>((IStraus7AdvancedMovingWriteApi)a).AddMovingHeatSource(id,number,position,(double[])data.Clone()));
                        }
                    }
                }
                int target=path.Target==MovingPathTarget.Selection?1:path.Target==MovingPathTarget.AllStructuralElements?0:path.Target==MovingPathTarget.Beams?2:path.Target==MovingPathTarget.Shells?3:4;
                int[] pathFlags={loadCase,templateId,(int)path.Shape,(int)path.Surface,target,path.Divisions,set};
                double[] coordinates={path.Start.X,path.Start.Y,path.Start.Z,path.End.X,path.End.Y,path.End.Z,path.Reference.X,path.Reference.Y,path.Reference.Z};
                int pathId=pathIndex+1; ops.Add(a=>a.SetMovingPath(pathId,(int[])pathFlags.Clone(),(double[])coordinates.Clone(),toStatic)); convert|=toStatic;
            }
            return new Straus7MovingWritePlan(model.Guid,model.AnalysisFingerprint(),templates.Count,copy.Paths.Count,convert,advanced,ops);
        }
        public IReadOnlyDictionary<int,string> Write(IStraus7MovingWriteApi api,global::GPC.Model.Models.Model currentInput)
        {
            if(api==null||currentInput==null) throw new ArgumentNullException();
            if(currentInput.Guid!=modelGuid||currentInput.AnalysisFingerprint()!=AnalysisFingerprint) throw new InvalidOperationException("StaleStrausMovingPlan");
            if(advanced&&!(api is IStraus7AdvancedMovingWriteApi))throw new NotSupportedException("MissingAdvancedMovingWriteApi");
            api.RequireEmptyMovingLoads(); foreach(var operation in operations) operation(api);
            return convert ? api.ConvertSelectedMovingPaths() : new Dictionary<int,string>();
        }
    }
}
