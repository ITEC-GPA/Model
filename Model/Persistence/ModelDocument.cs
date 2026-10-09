using System;
using GPC.Model.Structure;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Model.Collections;
using GPC.Model.Attributes;
using GPC.Model.Elements;
using GPC.Model.ElementProperties;
using GPC.Model.PostProcessing;

namespace GPC.Model.Persistence
{
    // Root contract owns its names/order/version independently of Models.Model.
    // Leaf contracts retain the closed legacy vocabulary (including custom section serializers).
    [DataContract(Name = "ModelDocument", Namespace = "urn:gpc:model:archive:4")]
    internal sealed class ModelDocument
    {
        [DataMember(Order = 1, IsRequired = true)] internal int Schema = 1;
        [DataMember(Order = 2, IsRequired = true)] internal Guid Id;
        [DataMember(Order = 3)] internal string Name;
        [DataMember(Order = 4, IsRequired = true)] internal SortedCollection<NodeElement> Nodes;
        [DataMember(Order = 5, IsRequired = true)] internal SortedCollection<BeamElement> Beams;
        [DataMember(Order = 6, IsRequired = true)] internal SortedCollection<AreaElement> Plates;
        [DataMember(Order = 7, IsRequired = true)] internal SortedCollection<VolumeElement> Solids;
        [DataMember(Order = 8, IsRequired = true)] internal UniqueIdCollection<Costrains.Costrain> Constraints;
        [DataMember(Order = 9, IsRequired = true)] internal UniqueNameCollection<BeamProperty> BeamProperties;
        [DataMember(Order = 10, IsRequired = true)] internal UniqueNameCollection<PlateProperty> PlateProperties;
        [DataMember(Order = 11, IsRequired = true)] internal UniqueNameCollection<BrickProperty> SolidProperties;
        [DataMember(Order = 12, IsRequired = true)] internal UniqueNameCollection<LoadCases.LoadCaseBase> Cases;
        [DataMember(Order = 13, IsRequired = true)] internal UniqueNameCollection<FreedomCases.FreedomCase> FreedomCases;
        [DataMember(Order = 14, IsRequired = true)] internal UniqueNameCollection<Combinations.Combination> Combinations;
        [DataMember(Order = 15, IsRequired = true)] internal Dictionary<int, HashSet<string>> StageCombinations;
        [DataMember(Order = 16, IsRequired = true)] internal UniqueNameCollection<Group> Groups;
        [DataMember(Order = 17, IsRequired = true)] internal UniqueIdCollection<Stages.Stage> Stages;
        [DataMember(Order = 18, IsRequired = true)] internal UniqueIdCollection<Loads.Load> ModelLoads;
        [DataMember(Order = 19, IsRequired = true)] internal Dictionary<string, AnalysisDataset> Datasets;
        [DataMember(Order = 20)] internal AnalysisSource Source;
        [DataMember(Order = 21, IsRequired = true)] internal PreservedAssignment[] Preserved;
        [DataMember(Order = 22, IsRequired = true)] internal PhysicalMemberDefinition[] Members;
        [DataMember(Order = 23)] internal AnalysisSnapshot Analysis;
        [DataMember(Order = 24)] internal VerificationProvenance Verification;
        [DataMember(Order = 25, IsRequired = true)] internal VerificationScenario[] Scenarios;
        [DataMember(Order = 26, IsRequired = true)] internal CheckReport[] Reports;

        [DataMember(Order=27,EmitDefaultValue=false)] internal PhysicalSurfaceDefinition[] Surfaces;

        internal static ModelDocument Capture(Models.Model model) => new ModelDocument {
            Schema=model.PhysicalSurfaces.Count==0 ? 1 : 2, Surfaces=model.PhysicalSurfaces.Count==0 ? null : model.PhysicalSurfaces.Values.ToArray(),
            Id = model.Guid, Name = model.Name, Nodes = model.NodesElements, Beams = model.BeamElements,
            Plates = model.AreaElements, Solids = model.VolumeElements, Constraints = model.Costrains,
            BeamProperties = model.BeamProperties, PlateProperties = model.PlateProperties, SolidProperties = model.BrickProperties,
            Cases = model.LoadCases, FreedomCases = model.FreedomCases, Combinations = model.Combinations,
            StageCombinations = model.StageCombinationsMap, Groups = model.Groups, Stages = model.Stages,
            ModelLoads = model.ModelLoads, Datasets = model.Datasets, Source = model.AnalysisSource,
            Preserved = model.PreservedSourceData.ToArray(), Members = model.PhysicalMembers.Values.ToArray(),
            Analysis = model.Analysis, Verification = model.VerificationContext,
            Scenarios = model.VerificationScenarios.Values.ToArray(), Reports = model.CheckReports.ToArray() };

        internal Models.Model Restore()
        {
            if (Schema != 1 && Schema != 2) throw new SerializationException("Unsupported ModelDocument schema.");
            if (Schema==2 && (Surfaces==null || Surfaces.Length==0) || Schema==1 && Surfaces!=null && Surfaces.Length!=0) throw new SerializationException("PhysicalSurfaceSchemaMismatch");
            if (Nodes == null || Beams == null || Plates == null || Solids == null || Constraints == null || BeamProperties == null
                || PlateProperties == null || SolidProperties == null || Cases == null || FreedomCases == null || Combinations == null
                || StageCombinations == null || Groups == null || Stages == null || ModelLoads == null || Datasets == null
                || Preserved == null || Members == null || Scenarios == null || Reports == null)
                throw new SerializationException("Missing ModelDocument collection.");
            return Models.Model.FromDocument(this);
        }
    }
}
