using System;
using System.Threading;
using GPC.Model.Checking.Contracts;
using GPC.Model.Checking.Preparation;

namespace GPC.Model.Checking.Execution
{
    /// <summary>
    /// The section checks of one run of an orchestrator over one model (GPCModelChecker: the rows of one job), as
    /// <see cref="SectionCheckExecution.Run(BeamPreparation, CheckMechanism, IConcreteSectionVerifier, CancellationToken)"/> makes them: the
    /// same steps, comparisons and diagnostics, in the same order. Only the reads of the model differ. SectionCheckExecution reads the whole
    /// model again in every check (the verification, analysis, compatibility, design and dataset digests: twice per row, and once more to
    /// capture the provenance of the row), so a run cost its rows times its elements. The checks of a run read the model through one read
    /// session, which the following checks reuse until code that may change the model runs:
    /// <list type="bullet">
    /// <item>a verifier: after each of its calls the next read is a new one, unless the caller declares that the verifier reads nothing
    /// but its input and changes nothing (<c>verifierReadsOnlyItsInput</c>; for example an engine that receives detached copies only);</item>
    /// <item>the code of the caller between two checks that may change the model (a progress report, an engine factory): the caller
    /// declares it with <see cref="ModelMayHaveChanged"/> after it ran.</item>
    /// </list>
    /// Each check therefore reads the model as it is when the check runs, and a change is seen by exactly the checks that saw it before,
    /// as long as the code the caller does not declare (its own stamping, which reads the model) changes nothing and no other thread
    /// writes the model during the run (as before, such a write was seen or not depending on the timing). No read session spans a
    /// verifier or a callback. A run belongs to one thread and lives for one loop of its caller.
    /// </summary>
    public sealed class SectionCheckRun
    {
        private ValidationReadScope.Session _reads;

        public SectionCheckRun(Models.Model model) { Model = model ?? throw new ArgumentNullException(nameof(model)); }

        /// <summary>The model whose reads the checks share; an input of another model is read as by SectionCheckExecution.</summary>
        public Models.Model Model { get; }

        /// <summary>Code that may have changed the model ran since the last check: the next check reads the model again.</summary>
        public void ModelMayHaveChanged() { _reads = null; }

        /// <summary><see cref="SectionCheckExecution.Run(BeamPreparation, CheckMechanism, IConcreteSectionVerifier, CancellationToken)"/> with the
        /// reads of the run.</summary>
        /// <param name="verifierReadsOnlyItsInput">True only for a verifier that reads nothing of the model but its input and changes
        /// neither: its calls do not end the reads of the run.</param>
        public CheckResult Run(BeamPreparation preparation, CheckMechanism mechanism, IConcreteSectionVerifier verifier, bool verifierReadsOnlyItsInput,
            CancellationToken cancellationToken = default(CancellationToken))
            => SectionCheckExecution.Run(preparation, mechanism, null, verifier, cancellationToken, this, verifierReadsOnlyItsInput);

        /// <summary><see cref="SectionCheckExecution.Run(BeamPreparation, SectionCheckSpecification, IConcreteSectionVerifier, CancellationToken)"/>
        /// with the reads of the run.</summary>
        public CheckResult Run(BeamPreparation preparation, SectionCheckSpecification check, IConcreteSectionVerifier verifier, bool verifierReadsOnlyItsInput,
            CancellationToken cancellationToken = default(CancellationToken))
            => SectionCheckExecution.Run(preparation, check, verifier, cancellationToken, this, verifierReadsOnlyItsInput);

        /// <summary><see cref="ShellCheckInput.IsCurrent"/> with the reads of the run.</summary>
        public bool IsCurrent(ShellCheckInput input)
        {
            if (input == null) throw new ArgumentNullException(nameof(input));
            using (Enter(input.Model)) return input.IsCurrent;
        }

        /// <summary>The read scope of the run for a check of <paramref name="model"/>; null (no scope) for another model.</summary>
        internal IDisposable Enter(Models.Model model)
        {
            if (!ReferenceEquals(model, Model)) return null;
            if (_reads == null) _reads = new ValidationReadScope.Session(Model);
            return _reads.Enter();
        }
    }
}
