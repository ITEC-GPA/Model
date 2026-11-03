using System;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Actions.Moving;
using GPC.Model.Core;
using GPC.Model.Models;

namespace GPC.Converter.Straus7
{
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

    /// <summary>Software-specific controls, separate from physical moving-load inputs. Lengths use mm.</summary>
    public sealed class Straus7MovingPathOptions
    {
        public int Colour { get; set; } = 255;
        public bool TransitionTrailingLoad { get; set; }
        public bool BeamLateralTolerance { get; set; }
        public double RelativeTolerance { get; set; } = .01;
        public bool ConvertToStaticCases { get; set; }
        internal void Validate()
        {
            if(Colour<0||Colour>0xffffff||double.IsNaN(RelativeTolerance)||double.IsInfinity(RelativeTolerance)||RelativeTolerance<0)
                throw new ArgumentException("InvalidStrausMovingOptions");
        }
    }

    /// <summary>Detached mechanical moving-load export. Does not create topology or native load cases.
    /// Unsupported entity selections and static conversion of floating loads fail before any native write.</summary>
    public sealed class Straus7MovingWritePlan
    {
        private readonly Action<IStraus7MovingWriteApi>[] operations;
        private readonly bool convert;
        private readonly Guid modelGuid;
        public string AnalysisFingerprint { get; }
        public int TemplateCount { get; }
        public int PathCount { get; }
        private Straus7MovingWritePlan(Guid owner,string fingerprint,int templates,int paths,bool convert,List<Action<IStraus7MovingWriteApi>> operations)
        { modelGuid=owner; AnalysisFingerprint=fingerprint; TemplateCount=templates; PathCount=paths; this.convert=convert; this.operations=operations.ToArray(); }
        public static Straus7MovingWritePlan Prepare(global::GPC.Model.Models.Model model,IReadOnlyDictionary<Guid,int> loadCaseIds,
            IReadOnlyDictionary<Guid,Straus7MovingPathOptions> options=null)
        {
            if(model==null||loadCaseIds==null) throw new ArgumentNullException();
            if(model.MovingLoads==null) throw new ArgumentException("MissingMovingLoadDefinition");
            model.MovingLoads.Validate(model);
            if(loadCaseIds.Values.Any(i=>i<1)||loadCaseIds.Values.Distinct().Count()!=loadCaseIds.Count) throw new ArgumentException("InvalidStrausLoadCaseMap");
            var copy=ModelValues.CopyValue(model.MovingLoads); var ops=new List<Action<IStraus7MovingWriteApi>>(); var templates=new Dictionary<string,int>(); bool convert=false;
            if(options!=null && options.Keys.Any(id=>!copy.Paths.Any(p=>p.Id==id))) throw new ArgumentException("UnknownMovingPathOptions");
            for(int pathIndex=0;pathIndex<copy.Paths.Count;pathIndex++)
            {
                var path=copy.Paths[pathIndex]; var template=path.Template;
                if(!loadCaseIds.TryGetValue(path.CaseGuid,out int loadCase)) throw new ArgumentException("MissingStrausMovingCaseIdentity");
                if(path.Target==MovingPathTarget.Selection) throw new NotSupportedException("StrausMovingEntitySelectionRequiresEntitySetWriter");
                var settings=options!=null && options.TryGetValue(path.Id,out var requested) ? requested : new Straus7MovingPathOptions();
                if(settings==null) throw new ArgumentException("MissingStrausMovingOptions"); settings.Validate();
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
                            var f=new[]{(int)force.Mobility,(int)force.Axes,force.ApplyAdjacencyFactor?1:0,0};
                            var d=new[]{force.X,force.Y,force.Force.X,force.Force.Y,force.Force.Z};
                            ops.Add(a=>a.AddMovingPointForce(id,number,position,(int[])f.Clone(),(double[])d.Clone()));
                        }
                        for(int i=0;i<vehicle.DistributedForces.Length;i++)
                        {
                            var force=vehicle.DistributedForces[i]; int position=i+1;
                            var f=new[]{(int)force.Mobility,(int)force.Axes,force.ApplyAdjacencyFactor?1:0,0};
                            var d=new[]{force.X1,force.X2,force.Y1,force.Y2,force.ForcePerLength.X,force.ForcePerLength.Y,force.ForcePerLength.Z};
                            ops.Add(a=>a.AddMovingDistributedForce(id,number,position,(int[])f.Clone(),(double[])d.Clone()));
                        }
                    }
                }
                int target=path.Target==MovingPathTarget.AllStructuralElements?0:path.Target==MovingPathTarget.Beams?2:path.Target==MovingPathTarget.Shells?3:4;
                int[] pathFlags={loadCase,templateId,(int)path.Shape,(int)path.Surface,target,path.Divisions,0};
                double[] coordinates={path.Start.X,path.Start.Y,path.Start.Z,path.End.X,path.End.Y,path.End.Z,path.Reference.X,path.Reference.Y,path.Reference.Z};
                int pathId=pathIndex+1; ops.Add(a=>a.SetMovingPath(pathId,(int[])pathFlags.Clone(),(double[])coordinates.Clone(),toStatic)); convert|=toStatic;
            }
            return new Straus7MovingWritePlan(model.Guid,model.AnalysisFingerprint(),templates.Count,copy.Paths.Count,convert,ops);
        }
        public IReadOnlyDictionary<int,string> Write(IStraus7MovingWriteApi api,global::GPC.Model.Models.Model currentInput)
        {
            if(api==null||currentInput==null) throw new ArgumentNullException();
            if(currentInput.Guid!=modelGuid||currentInput.AnalysisFingerprint()!=AnalysisFingerprint) throw new InvalidOperationException("StaleStrausMovingPlan");
            api.RequireEmptyMovingLoads(); foreach(var operation in operations) operation(api);
            return convert ? api.ConvertSelectedMovingPaths() : new Dictionary<int,string>();
        }
    }
}
