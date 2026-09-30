using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using System.Globalization;
using System.Runtime.Serialization;
using GPC.Checkers.Concrete.Attributes;
using GPC.Checkers.Concrete.Checkers;
using GPC.Checkers.Concrete.SectionSolvers;
using GPC.Geometry;
using GPC.Model.PostProcessing;
using GPC.Model.Persistence;
using GPC.Model.Results;
using GPC.Model.Sections.Concrete;
using GPC.Model.Standards;

namespace GPC.Model.Checker
{
    /// <summary>Optional headless adapter to the installed Checker. No FEM solving or resistance formula is implemented here.</summary>
    public sealed class ConcreteSectionVerifier : IConfiguredSectionVerifier
    {
        private static readonly object Sync = new object();
        private readonly StandardModelCode2010 _standard;
        private readonly SectionSolver.FailureAnalysisTypes _criterion;
        private readonly bool _considerTension;
        private readonly int _angularDivisions;
        private readonly double _psiRebar, _psiTendon;
        private readonly Dictionary<string, Tuple<ReinforcedConcreteSection, SectionCheckerModelCode2010>> _checkers = new Dictionary<string, Tuple<ReinforcedConcreteSection, SectionCheckerModelCode2010>>();
        private int _createdCheckers;
        public int CreatedCheckers { get { lock (Sync) return _createdCheckers; } }
        public string Version => typeof(SectionCheckerModelCode2010).Assembly.GetName().Version.ToString();
        public IReadOnlyCollection<CheckMechanism> Capabilities { get; } = Array.AsReadOnly(new[] {CheckMechanism.UlsBiaxialSection});
        public ConcreteSectionVerifier(StandardModelCode2010 standard, SectionSolver.FailureAnalysisTypes criterion, bool considerTension,
            int angularDivisions, double psiRebar, double psiTendon)
        {
            _standard=standard ?? throw new ArgumentNullException(nameof(standard));_criterion=criterion;_considerTension=considerTension;
            if (!Enum.IsDefined(typeof(SectionSolver.FailureAnalysisTypes), criterion)) throw new ArgumentOutOfRangeException(nameof(criterion));
            if(angularDivisions<4) throw new ArgumentOutOfRangeException(nameof(angularDivisions));
            if(double.IsNaN(psiRebar)||double.IsInfinity(psiRebar)||double.IsNaN(psiTendon)||double.IsInfinity(psiTendon)) throw new ArgumentException("Finite psi values required.");
            _angularDivisions=angularDivisions;_psiRebar=psiRebar;_psiTendon=psiTendon;
        }

        public string Configuration
        {
            get
            {
                var info=new SerializationInfo(_standard.GetType(),new FormatterConverter());
                _standard.GetObjectData(info,new StreamingContext());
                var entries=new SortedDictionary<string,string>(StringComparer.Ordinal);
                foreach(SerializationEntry entry in info)
                    if(entry.Value is double || entry.Value is int || entry.Value is bool || entry.Value is string)
                        entries[entry.Name]=Convert.ToString(entry.Value,CultureInfo.InvariantCulture);
                entries["StandardType"]=_standard.GetType().FullName;entries["Criterion"]=_criterion.ToString();
                entries["ConsiderTensileConcrete"]=_considerTension.ToString();entries["AngularDivisions"]=_angularDivisions.ToString(CultureInfo.InvariantCulture);
                entries["PsiRebar"]=_psiRebar.ToString("R",CultureInfo.InvariantCulture);entries["PsiTendon"]=_psiTendon.ToString("R",CultureInfo.InvariantCulture);
                entries["FailureDomain"]="Plastic";entries["StressAnalysis"]="NonLinear";entries["WorkingRatioForceScaleN"]="1000000";entries["WorkingRatioLengthScaleMm"]="1000";
                return string.Join("\n",entries.Select(p=>p.Key+"="+p.Value));
            }
        }

        public CheckResult Verify(BeamCheckInput input, CheckMechanism mechanism, CancellationToken cancellationToken)
        {
            if(mechanism!=CheckMechanism.UlsBiaxialSection) return new CheckResult {Data=DataStatus.NotSupported,Outcome=EngineeringOutcome.NotEvaluated};
            cancellationToken.ThrowIfCancellationRequested();
            // Section geometry is expressed in its 2D coordinates; input already contains components in those section axes.
            // The section force reduction point must be explicitly the section centroid for this adapter.
            var reference=new CoordinateSystem(input.Section.Centroid,new Vector3d(1,0,0),new Vector3d(0,1,0));
            var f=input.Forces;
            var forces=new ResultBeamForces(f.N,f.V1,f.V2,f.T,f.M1,f.M2,reference);
            lock(Sync)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var key = ModelArchive.Fingerprint(new object[] { input.Section, Configuration });
                if (!_checkers.TryGetValue(key, out var entry) || ModelArchive.Fingerprint(new object[] { entry.Item1, Configuration }) != key)
                {
                    var options=new SectionCheckerModelCode2010.SectionOptionsModelCode2010(reference,_criterion,
                        SectionSolver.FailureDomainTypes.Plastic,SectionSolver.StressAnalysisTypes.NonLinear,_psiRebar,_psiTendon,_considerTension,_angularDivisions);
                    var created=new SectionCheckerModelCode2010(new SectionCheckerAttribute(input.Section),options,_standard,_considerTension);
                    entry = Tuple.Create(input.Section, created); _checkers[key] = entry; _createdCheckers++;
                }
                var point=entry.Item2.CalculateFailureDomainPoint(forces);
                cancellationToken.ThrowIfCancellationRequested();
                if(point==null) return Failure("CheckerDomainPointMissing");
                double ratio=point.CalculateWorkingRatio(_criterion,forces,1e6,1000);
                if(double.IsNaN(ratio)||double.IsInfinity(ratio)||ratio<0) return Failure("CheckerInvalidRatio");
                return new CheckResult {Execution=ExecutionStatus.Completed,Data=DataStatus.Ready,
                    Outcome=ratio<=1 ? EngineeringOutcome.Satisfied : EngineeringOutcome.NotSatisfied,Utilization=ratio,EngineVersion=Version};
            }
        }
        private static CheckResult Failure(string code) => new CheckResult {Execution=ExecutionStatus.Error,Data=DataStatus.Insufficient,
            Outcome=EngineeringOutcome.NotEvaluated,Diagnostics=new List<ModelDiagnostic>{ModelDiagnostic.Error(code)}};
    }
}
