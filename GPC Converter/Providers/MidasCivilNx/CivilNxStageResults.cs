using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace GPC.Converter.CivilNx
{
    public static partial class CivilNxResults
    {
        public const string StageReaderVersion = "GPC.CivilNx.ConstructionStages/1";

        /// <summary>Reads the chosen stage states in bounded requests. The element filter is reused; its static cases are not requested.
        /// Reactions use selected nodes, since permanent restraints cannot identify supports that are active only in a construction phase.</summary>
        public static async Task<IReadOnlyList<CivilNxResponse>> ReadStagesAsync(CivilNxApiClient client, GPC.Model.Models.Model model,
            CivilNxStageSelection selection, ResultReadPlan elements = null, bool plateNodes = true, CancellationToken cancellationToken = default)
        {
            if (client == null || model == null || selection == null) throw new ArgumentNullException();
            var cases = selection.Validate(model); elements = elements ?? ResultFilter.All(model);
            var tables = new[] {
                Tuple.Create(CivilNxResultTable.BeamForce, Ids(elements.Beams)),
                Tuple.Create(CivilNxResultTable.PlateForcePerUnitLength, Ids(elements.Shells)),
                Tuple.Create(CivilNxResultTable.DisplacementGlobal, Ids(elements.Nodes)),
                Tuple.Create(CivilNxResultTable.ReactionGlobal, Ids(elements.Nodes))
            };
            var responses = new List<CivilNxResponse>();
            foreach (var step in selection.Steps)
                foreach (var sourceCase in selection.SourceCases)
                    foreach (var table in tables)
                        for (int start = 0; start < table.Item2.Length; start += 2000)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            responses.Add(await client.ReadResultTableAsync(new CivilNxTableRequest {
                                Table = table.Item1, ElementIds = table.Item2.Skip(start).Take(2000).ToArray(),
                                LoadCases = new[] { cases[sourceCase].SourceCaseName + "(CS)" }, StageSteps = new[] { step.RequestName },
                                PlateNodes = plateNodes,
                                DisplacementReference = table.Item1 == CivilNxResultTable.DisplacementGlobal ? selection.DisplacementReference : (CivilNxDisplacementReference?)null
                            }, cancellationToken).ConfigureAwait(false));
                        }
            return responses;
        }

        /// <summary>Imports stage tables using the same axes, signs and unit conversions as static tables. Missing requested states are
        /// rejected; missing elements within a returned state are not fabricated as zero results. Call with exclusive ownership of the model.</summary>
        public static ResultImportReport ImportStages(GPC.Model.Models.Model model, CivilNxSnapshot current, IReadOnlyList<CivilNxResponse> tables,
            string datasetId, CivilNxStageSelection selection, bool isSynthetic = false, CancellationToken cancellationToken = default)
        {
            if (selection == null) throw new ArgumentNullException(nameof(selection));
            return ImportCore(model, current, tables, datasetId, selection, isSynthetic, cancellationToken);
        }
    }
}
