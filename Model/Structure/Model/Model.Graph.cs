using System;
using System.Linq;
using GPC.Model.Analysis;
using GPC.Model.Compatibility;
using GPC.Model.Structure.Members;

namespace GPC.Model.Models
{
    public partial class Model
    {
        internal static Model FromGraph(ModelGraph data)
        {
            var model = new Model(data.Name) {
                _guid = data.Id, _nodesElements = data.Nodes, _beamElements = data.Beams, _areaElements = data.Plates,
                _volumeElements = data.Solids, _costrains = data.Constraints, _beamProperties = data.BeamProperties,
                _areaProperties = data.PlateProperties, _volumeProperties = data.SolidProperties, _loadCases = data.Cases,
                _freedomCases = data.FreedomCases, _combinations = data.Combinations, _stageCombinationsMap = data.StageCombinations,
                _groups = data.Groups, _stages = data.Stages, ModelLoads = data.ModelLoads, Datasets = data.Datasets,
                AnalysisSource = data.Source, PreservedSourceData = data.Preserved.ToList(),
                PhysicalMembers = data.Members.ToDictionary(m => m.Id, StringComparer.Ordinal),
                PhysicalSurfaces=(data.Surfaces??Array.Empty<global::GPC.Model.Structure.Members.PhysicalSurfaceDefinition>()).ToDictionary(s=>s.Id,StringComparer.Ordinal),
                Analysis = data.Analysis, VerificationContext = data.Verification,
                VerificationScenarios = data.Scenarios.ToDictionary(s => s.Id, StringComparer.Ordinal), CheckReports = data.Reports.ToList() };
            model._groups.ItemRenamed += model.OnGroupRenamed;
            model._groups.ItemRenaming += model.OnGroupRenaming;
            return model;
        }
    }
}
