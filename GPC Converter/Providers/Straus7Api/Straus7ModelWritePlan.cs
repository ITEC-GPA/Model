using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Identity;
using GPC.Model.Elements;
using GPC.Model.Models;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.Straus7
{
    /// <summary>Preflight and detached export of a GPC model into a fresh Straus7 N-mm model.
    /// Preparation has no native side effects; unsupported physical inputs fail before file creation.</summary>
    public sealed partial class Straus7ModelWritePlan
    {
        private readonly Action<IStraus7ModelWriteApi>[] operations;
        private readonly Straus7PhysicalWritePlan physical;
        private readonly Straus7MovingWritePlan moving;
        private readonly Straus7StageWritePlan stages;
        private readonly string groupFingerprint;
        private readonly bool requiresMaterialApi;
        private readonly bool requiresSectionGeometryApi;
        private readonly bool requiresSectionMeshApi;
        private readonly bool requiresNativeSectionApi;
        private readonly bool requiresInertiaApi;
        private readonly bool requiresConstraintApi;
        private readonly bool requiresSurfacePressureApi;
        internal readonly Guid[] StaticMovingParentCases;
        public SolverIdentityMap IdentityMap { get; }
        public IReadOnlyDictionary<Guid,int> LoadCaseIds { get; }
        public IReadOnlyDictionary<Guid,int> FreedomCaseIds { get; }
        public IReadOnlyDictionary<Guid,int> GroupIds { get; }
        public IReadOnlyDictionary<int,int> MinimumOutputStations => physical.MinimumOutputStations;
        public IReadOnlyDictionary<Guid,Straus7SectionDefinition> NativeSections { get; }
        public IReadOnlyDictionary<Guid,int> BeamIntegrationSlices { get; }
        public string ConfigurationFingerprint { get; }
        /// <summary>Warnings of the preparation that do not stop the export, for example shear areas of a section of separate parts
        /// (SeparateSectionPartsShearAreas).</summary>
        public IReadOnlyList<GPC.Model.Core.Diagnostics.ModelDiagnostic> Diagnostics { get; }

        private Straus7ModelWritePlan(Builder builder, IReadOnlyDictionary<Guid,Straus7MovingPathOptions> movingOptions)
        {
            Diagnostics=builder.Diagnostics.AsReadOnly();
            StaticMovingParentCases=builder.Model.MovingLoads?.Paths.Where(p=>Straus7MovingPathOptions.Effective(builder.Model,p,movingOptions).ConvertToStaticCases).Select(p=>p.CaseGuid).Distinct().ToArray()??Array.Empty<Guid>();
            IdentityMap=SolverIdentityMap.Create(builder.Model,"Straus7-R3",builder.Entities);
            LoadCaseIds=new ReadOnlyDictionary<Guid,int>(builder.Loads);
            FreedomCaseIds=new ReadOnlyDictionary<Guid,int>(builder.Freedoms);
            GroupIds=new ReadOnlyDictionary<Guid,int>(builder.Groups);
            groupFingerprint=GroupFingerprint(builder.Model);
            requiresMaterialApi=builder.RequiresMaterialApi;
            requiresSectionGeometryApi=builder.RequiresSectionGeometryApi;
            requiresSectionMeshApi=builder.RequiresSectionMeshApi;
            requiresNativeSectionApi=builder.NativeSections.Count!=0;
            NativeSections=new ReadOnlyDictionary<Guid,Straus7SectionDefinition>(builder.NativeSections);
            BeamIntegrationSlices=new ReadOnlyDictionary<Guid,int>(builder.IntegrationSlices.ToDictionary(p=>p.Key,p=>p.Value));
            ConfigurationFingerprint=ModelValues.Fingerprint(new object[]{builder.ConfigurationValues()});
            requiresInertiaApi=builder.RequiresInertiaApi;
            requiresConstraintApi=builder.RequiresConstraintApi;
            requiresSurfacePressureApi=builder.RequiresSurfacePressureApi;
            physical=Straus7PhysicalWritePlan.PrepareForFullModel(builder.Model,IdentityMap,
                builder.Model.LoadCases.Values.ToDictionary(c=>c.Name,c=>builder.Loads[c.Guid]),
                builder.Model.FreedomCases.Values.ToDictionary(c=>c.Name,c=>builder.Freedoms[c.Guid]));
            // The parts of this export: the domain preflight ran once in Prepare.
            if(builder.Model.MovingLoads!=null) moving=Straus7MovingWritePlan.PrepareCore(builder.Model,LoadCaseIds,movingOptions,IdentityMap);
            if(builder.Model.Stages.Count!=0) stages=Straus7StageWritePlan.PrepareCore(builder.Model,GroupIds,LoadCaseIds,FreedomCaseIds,1);
            operations=builder.Operations.ToArray();
        }
        public static Straus7ModelWritePlan Prepare(global::GPC.Model.Models.Model model,
            IReadOnlyDictionary<Guid,Straus7MovingPathOptions> movingOptions=null)
            => Prepare(model,movingOptions,null);
        /// <param name="beamIntegrationSlices">Explicit lengthwise numerical integration count by beam GUID;
        /// omitted beams retain the native solver default. This numerical setting is not a physical section property.</param>
        public static Straus7ModelWritePlan Prepare(global::GPC.Model.Models.Model model,
            IReadOnlyDictionary<Guid,Straus7MovingPathOptions> movingOptions,IReadOnlyDictionary<Guid,int> beamIntegrationSlices)
            =>Prepare(model,movingOptions,beamIntegrationSlices,null);
        /// <summary>Resolved native geometry is an immutable Converter input, separate from the GPC physical domain.
        /// Persist each definition with Save(); results record its complete snapshot and configuration fingerprint.</summary>
        public static Straus7ModelWritePlan Prepare(global::GPC.Model.Models.Model model,
            IReadOnlyDictionary<Guid,Straus7MovingPathOptions> movingOptions,IReadOnlyDictionary<Guid,int> beamIntegrationSlices,
            IReadOnlyDictionary<Guid,Straus7SectionDefinition> nativeSections)
        {
            if(model==null)throw new ArgumentNullException(nameof(model));
            // Domain preflight of every exporter (review L4): dangling stage/moving-load references, invalid elastic properties and form finding.
            model.RequireExportable();
            if(model.Stages.Count!=0 && model.MovingLoads!=null && model.MovingLoads.Paths.Any(p=>Straus7MovingPathOptions.Effective(model,p,movingOptions)?.ConvertToStaticCases==true))
                throw new NotSupportedException("GeneratedMovingCasesNeedExplicitStageFactors");
            var builder=new Builder(model,beamIntegrationSlices,nativeSections);builder.Build();return new Straus7ModelWritePlan(builder,movingOptions);
        }
        private static string GroupFingerprint(global::GPC.Model.Models.Model model) => ModelValues.Fingerprint(new object[] {
            model.Name,
            model.Groups.Values.OrderBy(g=>g.Guid).Select(g=>new object[]{g.Guid,g.Name,g.Parent?.Guid}).ToArray(),
            model.AllElements.OrderBy(e=>e.Guid).Select(e=>new object[]{e.Guid,e.Name,e.Groups.Values.Select(g=>g.Guid).OrderBy(id=>id).ToArray()}).ToArray() });

        public IReadOnlyDictionary<int,string> Write(IStraus7ModelWriteApi api, global::GPC.Model.Models.Model currentInput)
        {
            if(api==null||currentInput==null)throw new ArgumentNullException();
            if(currentInput.Guid!=IdentityMap.ModelGuid || currentInput.AnalysisFingerprint()!=IdentityMap.AnalysisFingerprint || GroupFingerprint(currentInput)!=groupFingerprint)
                throw new InvalidOperationException("StaleStrausModelPlan");
            if(!(api is IStraus7WriteApi physicalApi) || moving!=null && !(api is IStraus7MovingWriteApi) || stages!=null && !(api is IStraus7StageWriteApi))
                throw new ArgumentException("IncompleteStrausWriterCapabilities");
            if(physical.RequiresDistributionApi&&!(api is IStraus7DistributionWriteApi))throw new ArgumentException("MissingStrausDistributionWriteApi");
            if(moving?.RequiresAdvancedApi==true&&!(api is IStraus7AdvancedMovingWriteApi))throw new ArgumentException("MissingAdvancedMovingWriteApi");
            if(requiresMaterialApi&&!(api is IStraus7MaterialWriteApi))throw new ArgumentException("MissingMaterialWriteApi");
            if(requiresSectionGeometryApi&&!(api is IStraus7SectionGeometryWriteApi))throw new ArgumentException("MissingSectionGeometryWriteApi");
            if(requiresSectionMeshApi&&!(api is IStraus7SectionMeshWriteApi))throw new ArgumentException("MissingSectionMeshWriteApi");
            if(requiresNativeSectionApi&&!(api is IStraus7NativeSectionWriteApi))throw new ArgumentException("MissingNativeSectionWriteApi");
            if(requiresInertiaApi&&!(api is IStraus7InertiaWriteApi))throw new ArgumentException("MissingInertiaWriteApi");
            if(requiresConstraintApi&&!(api is IStraus7ConstraintWriteApi))throw new ArgumentException("MissingConstraintWriteApi");
            if(requiresSurfacePressureApi&&!(api is IStraus7SurfacePressureWriteApi))throw new ArgumentException("MissingSurfacePressureWriteApi");
            api.RequireEmptyTopology();foreach(var operation in operations)operation(api);
            physical.Write(physicalApi,currentInput);
            var generated=moving?.Write((IStraus7MovingWriteApi)api,currentInput)??new Dictionary<int,string>();
            if(stages!=null && generated.Count!=0)throw new InvalidOperationException("GeneratedMovingCasesNeedExplicitStageFactors");
            stages?.Write((IStraus7StageWriteApi)api,currentInput);return generated;
        }

        private sealed partial class Builder
        {
            internal readonly global::GPC.Model.Models.Model Model;
            internal readonly Dictionary<Guid,int> Entities=new Dictionary<Guid,int>(),Loads=new Dictionary<Guid,int>(),Freedoms=new Dictionary<Guid,int>(),Groups=new Dictionary<Guid,int>();
            internal readonly List<Action<IStraus7ModelWriteApi>> Operations=new List<Action<IStraus7ModelWriteApi>>();
            private int nextUcs=2;
            private readonly IReadOnlyDictionary<Guid,int> beamIntegrationSlices;
            internal IReadOnlyDictionary<Guid,int> IntegrationSlices=>beamIntegrationSlices;
            internal readonly Dictionary<Guid,Straus7SectionDefinition> NativeSections;
            internal object[] ConfigurationValues()=>new object[]{"Straus7 section configuration/1",NativeSections.OrderBy(p=>p.Key).Select(p=>new object[]{p.Key,p.Value.Fingerprint()}).ToArray(),beamIntegrationSlices.OrderBy(p=>p.Key).Select(p=>new object[]{p.Key,p.Value}).ToArray()};
            internal Builder(global::GPC.Model.Models.Model model,IReadOnlyDictionary<Guid,int> slices,IReadOnlyDictionary<Guid,Straus7SectionDefinition> nativeSections)
            {
                Model=model;beamIntegrationSlices=slices==null?new Dictionary<Guid,int>():slices.ToDictionary(p=>p.Key,p=>p.Value);
                if(beamIntegrationSlices.Any(p=>p.Value<2||p.Value>10||!model.BeamElements.Values.Any(b=>b.Guid==p.Key)))throw new ArgumentException("InvalidBeamIntegrationSlices");
                NativeSections=nativeSections==null?new Dictionary<Guid,Straus7SectionDefinition>():nativeSections.ToDictionary(p=>p.Key,p=>p.Value);
                if(NativeSections.Any(p=>p.Value==null||!p.Value.IsResolved||!model.BeamElements.Values.Any(b=>b.Guid==p.Key)))throw new ArgumentException("InvalidResolvedNativeSections");
            }
            internal void Build()
            {
                foreach(var item in Model.PreservedSourceData)
                {
                    var prefixes=new[]{"ResultFile/","ResultUnits/","ResultCases/","ResultValidation/","ExportIdentityMap/","GeneratedMovingCases/","GeneratedMovingCaseParents/","NativeSectionDefinitions/","SectionExportConfiguration/",Straus7MovingPathOptions.ColourRecord,Straus7MovingPathOptions.EntitySetRecord,Straus7CentrifugalExpression.RecordPrefix};
                    if(item==null||item.Kind!="Straus7 API"||item.SourceRecord==null||!prefixes.Any(p=>item.SourceRecord.StartsWith(p,StringComparison.Ordinal)))
                        throw new NotSupportedException("UnmappedPreservedModelInput: "+item?.Kind+"/"+item?.SourceRecord+"; "+item?.UnsupportedReason);
                }
                if(Model.FormFinding!=null)throw new NotSupportedException("FormFindingRequiresItsDedicatedSolver");
                LegacyStageCombinations();
                if(Model.VolumeElements.Count!=0)throw new NotSupportedException("StrausSolidWriterRequired");
                if(Model.ValidateTopology(supportsWarpedQuads:true,planarityToleranceMm:1e-6).Any(d=>d.Severity==GPC.Model.Core.Diagnostics.DiagnosticSeverity.Error))
                    throw new ArgumentException("InvalidStrausModelTopology");
                if(Model.ValidateGroups().Count!=0)throw new ArgumentException("InvalidStrausGroups");
                foreach(var e in Model.AllElements) PhysicalAssignments.Validate(e);
                int index=0;
                foreach(var c in Model.LoadCases.Values.OrderBy(c=>c.Name,StringComparer.Ordinal))
                {int id=++index;string name=c.Name;Loads.Add(c.Guid,id);Operations.Add(a=>a.CreateLoadCase(id,name));}
                if(Loads.Count==0)Operations.Add(a=>a.CreateLoadCase(1,"Unloaded"));
                index=0;
                foreach(var c in Model.FreedomCases.Values.OrderBy(c=>c.Name,StringComparer.Ordinal))
                {int id=++index;string name=c.Name;Freedoms.Add(c.Guid,id);Operations.Add(a=>a.CreateFreedomCase(id,name));}
                if(Freedoms.Count==0)Operations.Add(a=>a.CreateFreedomCase(1,"Default restraints"));
                if(Model.Stages.Count!=0 && (Freedoms.Count==0||Loads.Count==0))throw new ArgumentException("StagedExportRequiresExplicitCases");
                string modelName=string.IsNullOrWhiteSpace(Model.Name)?"Model":Model.Name;Operations.Add(a=>a.CreateGroup(1,0,modelName));
                var pending=Model.Groups.Values.OrderBy(g=>g.Name,StringComparer.Ordinal).ToList();
                while(pending.Count!=0)
                {
                    var ready=pending.Where(g=>g.Parent==null||Groups.ContainsKey(g.Parent.Guid)).ToArray();
                    if(ready.Length==0)throw new ArgumentException("CyclicStrausGroups");
                    foreach(var g in ready)
                    {int id=Groups.Count+2,parent=g.Parent==null?1:Groups[g.Parent.Guid];string name=NativeGroupName(g);Groups.Add(g.Guid,id);Operations.Add(a=>a.CreateGroup(id,parent,name));pending.RemoveAll(candidate=>ReferenceEquals(candidate,g));}
                }
                Number(Model.NodesElements.Values);Number(Model.BeamElements.Values);Number(Model.AreaElements.Values);
                PrepareShearAreas();
                foreach(var node in Model.NodesElements.Values.OrderBy(n=>Entities[n.Guid])) Node(node);
                foreach(var beam in Model.BeamElements.Values.OrderBy(n=>Entities[n.Guid])) Beam(beam);
                foreach(var plate in Model.AreaElements.Values.OrderBy(n=>Entities[n.Guid])) Plate(plate);
                UserIds();
                Constraints();
                ModelActions();
                NonlinearCombinationIncrements();
                ValidatePrescribedCases();
            }
            /// <summary>GPC group names are unique in the model; Straus7 names are unique only among siblings. A child named
            /// "&lt;parent name&gt;\&lt;leaf&gt;" (path names, as BridGHe groups) is written with its leaf name under its native parent.</summary>
            internal static string NativeGroupName(GPC.Model.Attributes.Group group)
            {
                string name=group.Name,parent=group.Parent?.Name;
                return parent!=null&&name.Length>parent.Length+1&&name.StartsWith(parent+"\\",StringComparison.Ordinal)?name.Substring(parent.Length+1):name;
            }
            /// <summary>Legacy behaviour: native user IDs carry the GPC element Id (St7SetNodeID/BeamID/PlateID).</summary>
            private void UserIds()
            {
                foreach(var family in new[]{(Type:0,Items:Model.NodesElements.Values.Cast<Element>()),(Type:1,Items:Model.BeamElements.Values.Cast<Element>()),(Type:2,Items:Model.AreaElements.Values.Cast<Element>())})
                    foreach(var e in family.Items.OrderBy(e=>Entities[e.Guid])){int type=family.Type,number=Entities[e.Guid],user=e.Id;Operations.Add(a=>a.SetEntityUserId(type,number,user));}
            }
            /// <summary>Legacy behaviour: without stages every LSA combination is also an unstaged NLA increment (stage 0).
            /// Freedom-case factors keep the native default; staged models write only their explicit stage increments.</summary>
            private void NonlinearCombinationIncrements()
            {
                if(Model.Stages.Count!=0)return;
                foreach(var combination in Model.Combinations.Values.OrderBy(c=>c.Name,StringComparer.Ordinal))
                {
                    string name=combination.Name;var pairs=combination.GetLoadCaseCoefficientsPair();
                    var cases=pairs.Select(c=>Load(c.Key)).ToArray();var factors=pairs.Select(c=>c.Value).ToArray();Finite(factors);
                    Operations.Add(a=>a.AddNonlinearIncrement(name,(int[])cases.Clone(),(double[])factors.Clone()));
                }
            }
            /// <summary>The legacy stage-combination matrix has no meaning without stages and is not exported: reject it explicitly.</summary>
            private void LegacyStageCombinations()
            {
                if(Model.Stages.Count==0&&Model.StageCombinationsMap!=null&&Model.StageCombinationsMap.Values.Any(v=>v!=null&&v.Count!=0))
                    throw new NotSupportedException("LegacyStageCombinationsMapWithoutStagesIsNotExported");
            }
            private void Number<T>(IEnumerable<T> entities) where T:Element
            {int id=0;foreach(var e in entities.OrderBy(e=>e.Id).ThenBy(e=>e.Guid)) {if(e.Guid==Guid.Empty||e.Id<=0)throw new ArgumentException("UnregisteredStrausEntity");Entities.Add(e.Guid,++id);}}
            private int Load(GPC.Model.LoadCases.LoadCaseBase c)
            {if(c==null || !Model.LoadCases.TryGetValue(c.Name,out var registered) || registered.Guid!=c.Guid || !Loads.TryGetValue(c.Guid,out int id))throw new ArgumentException("UnknownStrausLoadCase");return id;}
            private int Freedom(GPC.Model.FreedomCases.FreedomCase c)
            {if(c==null){if(Freedoms.Count>1)throw new ArgumentException("AmbiguousStrausFreedomCase");return 1;}if(!Model.FreedomCases.TryGetValue(c.Name,out var registered)||registered.Guid!=c.Guid||!Freedoms.TryGetValue(c.Guid,out int id))throw new ArgumentException("UnknownStrausFreedomCase");return id;}
            private void Group(Element element,int nativeType)
            {
                if(element.Groups.Count>1)throw new NotSupportedException("StrausElementRequiresOneGroup");
                int group=element.Groups.Count==0?1:Groups[element.Groups.Values.Single().Guid],id=Entities[element.Guid];
                Operations.Add(a=>a.SetEntityGroup(nativeType,id,group));
            }
            private int Ucs(CoordinateSystem axes)
            {
                if(axes==null)axes=CoordinateSystem.Global;Axes.Validate(axes);
                if(Same(axes,CoordinateSystem.Global))return 1;
                int id=nextUcs++;var p=new[]{0d,0,0,axes.V1.X,axes.V1.Y,axes.V1.Z,axes.V2.X,axes.V2.Y,axes.V2.Z,0};
                Operations.Add(a=>a.CreateUcs(id,(double[])p.Clone()));return id;
            }
            private static bool Same(CoordinateSystem a,CoordinateSystem b) => a!=null&&b!=null&&
                Math.Abs(Axes.Dot(a.V1,b.V1)-1)<1e-10&&Math.Abs(Axes.Dot(a.V2,b.V2)-1)<1e-10&&Math.Abs(Axes.Dot(a.V3,b.V3)-1)<1e-10;
            private static double[] Values(Vector3d v) {var a=new[]{v.X,v.Y,v.Z};Finite(a);return a;}
            private static void Finite(IEnumerable<double> a) {if(a.Any(v=>double.IsNaN(v)||double.IsInfinity(v)))throw new ArgumentException("NonFiniteStrausInput");}
            private static double[] AxesData(CoordinateSystem a) {Axes.Validate(a);return Values(a.V1).Concat(Values(a.V2)).Concat(Values(a.V3)).ToArray();}
        }
    }
}
