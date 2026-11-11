using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;

namespace GPC.Converter.Straus7
{
    /// <summary>
    /// Optional metadata of the Straus7 file, as the BridGHe 0.3.0 exporter (StrausR3Helper) wrote it. The default (no options, or a new
    /// instance) writes the file of today: no section label, no plate material name, one native plate property per plate named after the
    /// plate. None of the options changes a value the solver uses (C2, 11/11/2026, points P26-01, P26-02 and P26-03). C4 (11/11/2026): the
    /// axis data of a rotated section as 0.3.0 wrote them (a model refused before), the order of the unstaged nonlinear increments. C5
    /// (11/11/2026): the end releases of a rotated section that differ about its two transverse axes, with the axis data (refused before).
    /// </summary>
    public sealed class Straus7WriteOptions
    {
        /// <summary>P26-01: the label of the beam section (St7SetBeamSectionName) by beam GUID, written on the native property of the beam right
        /// after its creation, before the section geometry, as 0.3.0 did (SolidRound, HollowRound, SolidRect, HollowRect, IBeam, TBeam,
        /// Lipped Channel, Angle). Straus7 keeps the label through the geometry, the property update and the section calculation. Beams with
        /// different labels do not share a native property. Null or absent beam: no label.</summary>
        public IReadOnlyDictionary<Guid, string> BeamSectionLabels { get; set; }
        /// <summary>P26-02: the name of the material of every plate property with a material (St7SetMaterialName on ptPLATEPROP), as 0.3.0
        /// wrote it; load patches and per-width membranes have none.</summary>
        public bool PlateMaterialNames { get; set; }
        /// <summary>P26-03: one native plate property per distinct GPC plate property, named as the property (as 0.3.0: one property per
        /// Plate Property, St7SetElementConnection on its number), instead of one per plate named after the plate. Plates share a native
        /// property when their GPC properties have the same name and the same native data; the axes and the offset are element data and are
        /// written on each plate as before.</summary>
        public bool PlatePropertiesByProperty { get; set; }
        /// <summary>C4 (point 1): a beam with a principal angle (an angle, a double angle, any rotated section) with distributed supports,
        /// temperature gradients or a non-uniform taper is written as BridGHe 0.3.0 wrote it (StrausR3Helper, BeamSupportExport): the axes 1 and
        /// 2 of its section frame are the native axes 1 and 2, with no turn by the principal angle (St7SetBeamSupport2 ±1/±2 from the direction
        /// along the section axes, St7SetBeamTempGradient2 with the gradients along 1 and 2, St7SetBeamTaper2 on the axes 1 and 2). These are
        /// the calls of a beam without a principal angle. Default: refused (PrincipalAngleWithTaperGradientOrSupportRequiresExplicitAxisConvention).
        /// C5 (11/11/2026), the same convention for the end releases: a release in the section frame whose kind or value differs along or about
        /// the two transverse axes is written as 0.3.0 wrote it (StrausR3Helper.cs:894-947: St7SetBeamTRelease3/St7SetBeamRRelease3 with the
        /// components along and about the section axes 1, 2 and 3 on the native axes 1, 2 and 3, as given). Default: refused
        /// (SectionFrameBeamReleaseDiffersInPrincipalAxes). A release equal about both transverse axes, or given in the principal axes, is
        /// written as before with or without the option.</summary>
        public bool SectionAxisDataOnNativeAxes { get; set; }
        /// <summary>C4 (point 3): the nonlinear increments of a model without stages (one per combination, stage 0) in the order of the
        /// combinations of the model (Model.Combinations), as BridGHe 0.3.0 wrote them (its list of combinations); default: by name.</summary>
        public bool NonlinearIncrementsInModelOrder { get; set; }

        internal bool Any => (BeamSectionLabels?.Count ?? 0) != 0 || PlateMaterialNames || PlatePropertiesByProperty || SectionAxisDataOnNativeAxes || NonlinearIncrementsInModelOrder;
        internal string Label(Guid beam) => BeamSectionLabels != null && BeamSectionLabels.TryGetValue(beam, out var label) && !string.IsNullOrEmpty(label) ? label : null;
    }

    /// <summary>
    /// Options of a linear static run (C2, P26-04 and P26-07). The default (null, or a new instance) solves every load case of the file and
    /// reads only the primary cases, as today.
    /// </summary>
    public sealed class Straus7LinearRunOptions
    {
        /// <summary>P26-04: the GPC load cases solved (by GUID); the other load cases of the file are disabled (St7DisableLSALoadCase), as
        /// BridGHe 0.3.0 solved only the requested cases and those with a non-zero factor in the requested combinations. Null: every load case.
        /// The result cases are the solved ones, in native order, and the import reads them only.</summary>
        public IReadOnlyCollection<Guid> LoadCases { get; set; }
        /// <summary>P26-04: whether the static cases generated from the moving paths are solved (true, today). BridGHe 0.3.0 solved them only
        /// in a Moving Static Analysis, and disabled them in any other analysis.</summary>
        public bool SolveGeneratedMovingCases { get; set; } = true;
        /// <summary>P26-07: the GPC combinations whose results are read from the LSA combination cases that Straus7 computes when the result file
        /// is opened with kGenerateNewCombinations, as BridGHe 0.3.0 read them (StrausResultsReader on the combination case). They are attached
        /// to the combination itself (IsCombined), with every family of the import. Every case with a non-zero factor must be solved.
        /// Null or empty: no combination is read (the combinations stay superpositions of the consumer, as today).</summary>
        public IReadOnlyCollection<Guid> Combinations { get; set; }
        /// <summary>P26-06: the nonlinear switches of the new file are left as they are (geometry and material on), as BridGHe 0.3.0 left them;
        /// today they are set off. A linear solve does not use them: only the saved model file changes.</summary>
        public bool FileSolverDefaults { get; set; }
    }

    /// <summary>
    /// Options of a staged nonlinear run (C2, P26-06). The default (null, or <see cref="Explicit"/>) sets the controls of today: small
    /// displacements, material nonlinearity as requested, the given convergence tolerances and iterations.
    /// </summary>
    public sealed class Straus7NonlinearRunOptions
    {
        /// <summary>P26-06: the solver settings of a new Straus7 file are left as they are (St7NewFile: nonlinear geometry and material on,
        /// displacement tolerance 1e-4, residual tolerance 1e-3, 20 iterations at most, read natively on R31 on 11/11/2026), as BridGHe 0.3.0
        /// left them: it set only St7SetNLAStagedAnalysis. The other fields are then not used.</summary>
        public bool FileDefaults { get; set; }
        public bool NonlinearMaterial { get; set; }
        public double DisplacementTolerance { get; set; } = 1e-8;
        public double ResidualTolerance { get; set; } = 1e-8;
        public int MaximumIterations { get; set; } = 100;
        /// <summary>The settings of today (small displacements).</summary>
        public static Straus7NonlinearRunOptions Explicit(bool nonlinearMaterial = false, double displacementTolerance = 1e-8, double residualTolerance = 1e-8, int maximumIterations = 100) =>
            new Straus7NonlinearRunOptions { NonlinearMaterial = nonlinearMaterial, DisplacementTolerance = displacementTolerance, ResidualTolerance = residualTolerance, MaximumIterations = maximumIterations };
        /// <summary>The settings of a new Straus7 file, as BridGHe 0.3.0 solved.</summary>
        public static Straus7NonlinearRunOptions OfFile() => new Straus7NonlinearRunOptions { FileDefaults = true };
    }

    /// <summary>What a solve enables and sets (C2): the LSA load cases by native number (null: all, as <see cref="Straus7NativeWriter.Solve(bool,bool,bool)"/>)
    /// and the nonlinear switches (null: left as the file has them).</summary>
    public sealed class Straus7SolveOptions
    {
        public bool StagedNonlinear { get; set; }
        /// <summary>C4 (point 3): a nonlinear static solve without stages (NLA, staged flag off, the increments of stage 0), as BridGHe 0.3.0
        /// solved the nonlinear increments (solver 1). Not with <see cref="StagedNonlinear"/>.</summary>
        public bool UnstagedNonlinear { get; set; }
        /// <summary>Linear solve: the native load case numbers enabled (St7EnableLSALoadCase); the others are disabled. Null: every load case.</summary>
        public IReadOnlyCollection<int> EnabledLoadCases { get; set; }
        /// <summary>St7SetSolverNonlinearGeometry; null: not set (the setting of the file).</summary>
        public bool? NonlinearGeometry { get; set; } = false;
        /// <summary>St7SetSolverNonlinearMaterial; null: not set (the setting of the file).</summary>
        public bool? NonlinearMaterial { get; set; } = false;
    }

    /// <summary>The native names of P26-01 and P26-02 (<see cref="Straus7WriteOptions"/>).</summary>
    public interface IStraus7PropertyNameWriteApi
    {
        /// <summary>St7SetBeamSectionName.</summary>
        void SetBeamSectionName(int property, string label);
        /// <summary>St7SetMaterialName on a plate property (ptPLATEPROP).</summary>
        void SetPlateMaterialName(int property, string name);
    }

    public sealed partial class Straus7NativeWriter : IStraus7PropertyNameWriteApi
    {
        public void SetBeamSectionName(int property, string label) =>
            api.CheckWrite(api.WriteFunction<SectionLabel>("St7SetBeamSectionName")(Id, property, label), "St7SetBeamSectionName");
        public void SetPlateMaterialName(int property, string name) =>
            api.CheckWrite(api.WriteFunction<PlateMaterialName>("St7SetMaterialName")(Id, 2, property, name), "St7SetMaterialName/plate");
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate int SectionLabel(int file, int property, string label);
        [UnmanagedFunctionPointer(CallingConvention.Winapi, CharSet = CharSet.Ansi)] private delegate int PlateMaterialName(int file, int entity, int property, string name);
    }
}
