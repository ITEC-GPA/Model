using System;
using GPC.Model.Structure;
using System.Collections.Generic;
using System.Linq;
using GPC.Model.Collections;
using GPC.Model.Attributes;
using GPC.Model.Elements;
using GPC.Model.ElementProperties;
using GPC.Model.Analysis;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Reports;
using GPC.Model.Checking.Scenarios;
using GPC.Model.Compatibility;
using GPC.Model.Constraints;
using GPC.Model.Structure.Assignments;
using GPC.Model.Structure.Members;

namespace GPC.Model.Models
{
    // Trusted graph reconstruction data. No archive schema, XML envelope or serializer dependency.
    internal sealed class ModelGraph
    {
        internal Guid Id;
        internal string Name;
        internal SortedCollection<NodeElement> Nodes;
        internal SortedCollection<BeamElement> Beams;
        internal SortedCollection<AreaElement> Plates;
        internal SortedCollection<VolumeElement> Solids;
        internal UniqueIdCollection<global::GPC.Model.Constraints.Costrain> Constraints;
        internal UniqueNameCollection<BeamProperty> BeamProperties;
        internal UniqueNameCollection<PlateProperty> PlateProperties;
        internal UniqueNameCollection<BrickProperty> SolidProperties;
        internal UniqueNameCollection<LoadCases.LoadCaseBase> Cases;
        internal UniqueNameCollection<FreedomCases.FreedomCase> FreedomCases;
        internal UniqueNameCollection<Combinations.Combination> Combinations;
        internal Dictionary<int, HashSet<string>> StageCombinations;
        internal UniqueNameCollection<Group> Groups;
        internal UniqueIdCollection<Stages.Stage> Stages;
        internal UniqueIdCollection<Loads.Load> ModelLoads;
        internal Dictionary<string, AnalysisDataset> Datasets;
        internal AnalysisSource Source;
        internal PreservedAssignment[] Preserved;
        internal PhysicalMemberDefinition[] Members;
        internal AnalysisSnapshot Analysis;
        internal VerificationProvenance Verification;
        internal VerificationScenario[] Scenarios;
        internal CheckReport[] Reports;
        internal PhysicalSurfaceDefinition[] Surfaces;
    }
}
