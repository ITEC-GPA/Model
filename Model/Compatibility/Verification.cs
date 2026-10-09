using System.Collections.Generic;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Checking;
using GPC.Model.Elements;
using GPC.Model.Results;
using GPC.Model.Results.ResultLocations;

namespace GPC.Model.PostProcessing
{
    /// <summary>Compatibility facade over result queries, input preparation and verifier execution.</summary>
    public static class Verification
    {
        public static IReadOnlyList<StationResultBeamForces> BeamSamples(BeamElement beam, string dataset, string caseName)
            => ResultQueries.BeamSamples(beam, dataset, caseName);
        public static StationResultBeamForces BeamSample(BeamElement beam, string dataset, string caseName, double station, SectionSide side)
            => ResultQueries.BeamSample(beam, dataset, caseName, station, side);
        public static BeamPreparation PrepareBeam(Models.Model model, int beamId, StationResultBeamForces sample, string settings)
            => BeamCheckPreparation.Prepare(model, beamId, sample, settings);
        public static CheckResult Run(BeamPreparation preparation, CheckMechanism mechanism, IConcreteSectionVerifier verifier, CancellationToken cancellationToken = default(CancellationToken))
            => SectionCheckExecution.Run(preparation, mechanism, verifier, cancellationToken);
        public static CheckResult Run(BeamPreparation preparation, SectionCheckSpecification check, IConcreteSectionVerifier verifier, CancellationToken cancellationToken = default(CancellationToken))
            => SectionCheckExecution.Run(preparation, check, verifier, cancellationToken);
        internal static bool IsCurrent(BeamCheckInput input) => BeamCheckPreparation.IsCurrent(input);
    }
    /// <summary>Compatibility facade. Strip extraction is an explicit design operation, not a tensor rotation.</summary>
    public static class ShellPreparation
    {
        public static ResultBeamForces StripDirection1(ResultPlateForces designActions, double widthMm, CoordinateSystem sectionAxes)
            => Design.Plate.ShellStripActions.StripDirection1(designActions, widthMm, sectionAxes);
    }
}