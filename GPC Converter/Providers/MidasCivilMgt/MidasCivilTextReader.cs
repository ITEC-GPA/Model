using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GPC.Converter.CivilNx;
using GPC.Model.Analysis;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.MidasCivil
{
    /// <summary>MIDAS Civil text model (MCT; also an MGT-named Civil export). The commands are translated into the Civil NX database records
    /// and mapped by <see cref="CivilNxModelProfile"/>, the reader of the API: same conventions for axes, properties, supports and loads.
    /// No release whitelist and no claim of validation against every solver version.</summary>
    public sealed class MidasCivilTextReader : IModelFileReader
    {
        private readonly MidasTextModelReader reader;
        public string Program => reader.Program;
        public IReadOnlyDictionary<string, CapabilityStatus> Capabilities => reader.Capabilities;

        /// <param name="modelRevision">Caller-supplied revision, never inferred from element IDs.</param>
        /// <param name="analysisId">Optional externally attested analysis identity; an MCT file does not prove it.</param>
        /// <param name="textEncoding">Defaults to strict UTF-8 (with or without BOM). Specify the original encoding for legacy ANSI exports.</param>
        public MidasCivilTextReader(string modelRevision, string analysisId = null, Encoding textEncoding = null)
        { reader = new MidasTextModelReader("MIDAS Civil", modelRevision, analysisId, textEncoding); }

        public ImportReport Import(Stream source, CancellationToken cancellationToken = default(CancellationToken)) =>
            new SolverFileAdapter(Program, this).Import(source, cancellationToken);

        public ImportBatch Read(Stream source, CancellationToken cancellationToken) => reader.Read(source, cancellationToken);
    }

    /// <summary>MIDAS Gen text model (MGT): the Civil command set plus the Gen commands; stories, rigid floor diaphragms and wall elements
    /// are preserved with diagnostics, the rest is mapped as for <see cref="MidasCivilTextReader"/>.</summary>
    public sealed class MidasGenTextReader : IModelFileReader
    {
        private readonly MidasTextModelReader reader;
        public string Program => reader.Program;
        public IReadOnlyDictionary<string, CapabilityStatus> Capabilities => reader.Capabilities;

        /// <param name="modelRevision">Caller-supplied revision, never inferred from element IDs.</param>
        /// <param name="analysisId">Optional externally attested analysis identity; an MGT file does not prove it.</param>
        /// <param name="textEncoding">Defaults to strict UTF-8 (with or without BOM). Specify the original encoding for legacy ANSI exports.</param>
        public MidasGenTextReader(string modelRevision, string analysisId = null, Encoding textEncoding = null)
        { reader = new MidasTextModelReader("MIDAS Gen", modelRevision, analysisId, textEncoding); }

        public ImportReport Import(Stream source, CancellationToken cancellationToken = default(CancellationToken)) =>
            new SolverFileAdapter(Program, this).Import(source, cancellationToken);

        public ImportBatch Read(Stream source, CancellationToken cancellationToken) => reader.Read(source, cancellationToken);
    }

    /// <summary>Choice of the MIDAS text reader from the file: the export header (";  MIDAS/Civil Text(MCT) File.", ";  MIDAS CIVIL NX Text(MCT) File.",
    /// ";  midas Gen Text(MGT) File.") decides; without a header the extension does (.mct Civil, .mgt Gen).</summary>
    public static class MidasTextFiles
    {
        public const string CivilProgram = "MIDAS Civil", GenProgram = "MIDAS Gen";

        /// <summary>"MIDAS Civil", "MIDAS Gen", or null when neither the header nor the extension identifies the program.</summary>
        /// <param name="head">The first bytes of the file (the first kilobyte is enough); may be null.</param>
        public static string DetectProgram(string fileName, byte[] head)
        {
            if (head != null)
            {
                var text = Encoding.ASCII.GetString(head, 0, Math.Min(head.Length, 4096)).ToUpperInvariant();
                foreach (var line in text.Split('\n').Select(l => l.Trim()).Where(l => l.StartsWith(";", StringComparison.Ordinal)).Take(10))
                {
                    if (line.Contains("TEXT(MCT) FILE") && line.Contains("CIVIL")) return CivilProgram;
                    if (line.Contains("TEXT(MGT) FILE") && line.Contains("GEN")) return GenProgram;
                }
            }
            var extension = Path.GetExtension(fileName ?? "").ToUpperInvariant();
            return extension == ".MCT" ? CivilProgram : extension == ".MGT" ? GenProgram : null;
        }

        /// <summary>The reader of <see cref="DetectProgram"/>; throws <see cref="NotSupportedException"/> for an unidentified file.</summary>
        public static IModelFileReader CreateReader(string fileName, byte[] head, string modelRevision, string analysisId = null, Encoding textEncoding = null)
        {
            var program = DetectProgram(fileName, head) ?? throw new NotSupportedException("Not a MIDAS Civil (.mct) or Gen (.mgt) text model: " + Path.GetFileName(fileName));
            return program == GenProgram ? (IModelFileReader)new MidasGenTextReader(modelRevision, analysisId, textEncoding) : new MidasCivilTextReader(modelRevision, analysisId, textEncoding);
        }

        /// <summary>The Civil NX databases translated from a MIDAS text file, as JSON with the names and the form of the API responses
        /// (<c>{"NODE":{"1":{"X":...}}}</c>), with the mapped batch and the diagnostics. See <see cref="MidasTextTables"/>.</summary>
        public static MidasTextTables ReadTables(string path, string modelRevision, string analysisId = null, Encoding textEncoding = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Source path required.", nameof(path));
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                return ReadTables(stream, path, modelRevision, analysisId, textEncoding, cancellationToken);
        }

        /// <summary>As <see cref="ReadTables(string, string, string, Encoding, CancellationToken)"/> for a stream; the file name only chooses the program
        /// when the text has no export header.</summary>
        public static MidasTextTables ReadTables(Stream source, string fileName, string modelRevision, string analysisId = null, Encoding textEncoding = null,
            CancellationToken cancellationToken = default(CancellationToken))
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            byte[] bytes;
            using (var memory = new MemoryStream())
            {
                var buffer = new byte[65536]; int count;
                while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
                {
                    cancellationToken.ThrowIfCancellationRequested(); memory.Write(buffer, 0, count);
                    if (memory.Length > 128L * 1024 * 1024) throw new InvalidDataException("Source exceeds configured import size limit.");
                }
                bytes = memory.ToArray();
            }
            var program = DetectProgram(fileName, bytes.Take(4096).ToArray()) ?? throw new NotSupportedException("Not a MIDAS Civil (.mct) or Gen (.mgt) text model: " + Path.GetFileName(fileName));
            var result = new MidasTextTables { Program = program };
            using (var memory = new MemoryStream(bytes)) result.SourceHash = SourceEvidence.Sha256(memory);
            try
            {
                using (var memory = new MemoryStream(bytes))
                {
                    var (batch, tables) = new MidasTextModelReader(program, modelRevision, analysisId, textEncoding).ReadWithTables(memory, cancellationToken);
                    batch.SourceHash = result.SourceHash;
                    result.Batch = batch; result.Diagnostics = batch.Diagnostics.ToArray();
                    result.Tables = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(tables.Names.ToDictionary(n => n, tables.Json, StringComparer.Ordinal));
                }
            }
            catch (Exception ex) when (ex is FormatException || ex is InvalidDataException || ex is NotSupportedException || ex is DecoderFallbackException)
            {
                result.Diagnostics = new[] { new ModelDiagnostic { Code = "SourceReadFailed", Severity = DiagnosticSeverity.Error, Record = (ex as ModelFileReadException)?.Record,
                    Message = ex.Message, SuggestedAction = "Correct the source record or select an encoding matching the export." } };
            }
            return result;
        }

        /// <summary>Imports a file read once: program detected from its header/extension, original bytes and hash retained by <see cref="SolverFileAdapter"/>.</summary>
        public static ImportReport Import(string path, string modelRevision, string analysisId = null, Encoding textEncoding = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Source path required.", nameof(path));
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var head = new byte[4096]; int read = stream.Read(head, 0, head.Length); stream.Position = 0;
                Array.Resize(ref head, read);
                var reader = CreateReader(path, head, modelRevision, analysisId, textEncoding);
                return new SolverFileAdapter(reader.Program, reader).Import(stream, cancellationToken);
            }
        }
    }

    /// <summary>Tables of a MIDAS text file in the form of the Civil NX API responses. <see cref="Tables"/>: table name → <c>{"TABLE":{"key":{...}}}</c>,
    /// with the API field names (NODE, ELEM with NODE padded to eight entries, MATL, SECT, THIK, GRUP, STLD, CONS, SKEW, CNLD, BMLD, PRES, BODF, FRLS,
    /// OFFS, ELNK, RIGD, LCOM-*, STAG), plus SPRING (point springs, not an API database) and GPC-TEXT (program, version and the marker that makes
    /// <see cref="CivilNxModelProfile"/> apply the text conventions to these tables). **Values are in N, mm and °C, as declared by the UNIT table
    /// {"UNIT":{"1":{"FORCE":"N","DIST":"MM","TEMPER":"C"}}}**, not in the units of the file; distances of beam loads are relative and angles in degrees.
    /// Members without a value are omitted. <see cref="Batch"/> is the mapped batch of the text reader (null when the file is rejected).</summary>
    public sealed class MidasTextTables
    {
        public string Program { get; internal set; }
        public string SourceHash { get; internal set; }
        public IReadOnlyDictionary<string, string> Tables { get; internal set; } = new System.Collections.ObjectModel.ReadOnlyDictionary<string, string>(new Dictionary<string, string>());
        public ImportBatch Batch { get; internal set; }
        public IReadOnlyList<ModelDiagnostic> Diagnostics { get; internal set; } = new ModelDiagnostic[0];
        /// <summary>The tables as API responses (endpoint "mct/TABLE"), e.g. for <see cref="CivilNxGeometryReader.Import"/> with the program "MIDAS Civil NX":
        /// the model is that of the text reader, except REF beams (default axes), unresolved sections (kept empty) and pair offsets.</summary>
        public CivilNxSnapshot ToSnapshot() => new CivilNxSnapshot(Tables.ToDictionary(t => t.Key, t => new CivilNxResponse("mct/" + t.Key, t.Value), StringComparer.Ordinal));
    }

    internal sealed class MidasTextModelReader
    {
        private readonly string revision, analysisId;
        private readonly Encoding encoding;
        public string Program { get; }
        public IReadOnlyDictionary<string, CapabilityStatus> Capabilities { get; } =
            new ReadOnlyDictionary<string, CapabilityStatus>(new Dictionary<string, CapabilityStatus>
            {
                ["Nodes, beam/truss/tension/compression/cable and plate connectivity"] = CapabilityStatus.ImplementedSyntheticTests,
                ["Units, beam beta axes and plate axes with ANGLE (Civil NX conventions)"] = CapabilityStatus.ImplementedSyntheticTests,
                ["Groups, static cases, combinations, construction schedule (metadata)"] = CapabilityStatus.ImplementedSyntheticTests,
                ["Materials (code grades and user data), DB/user/value sections, thicknesses"] = CapabilityStatus.ImplementedSyntheticTests,
                ["Supports (also in node local axes), linear point springs, releases, offsets, rigid and linear elastic links"] = CapabilityStatus.ImplementedSyntheticTests,
                ["Nodal, beam, pressure and self-weight loads"] = CapabilityStatus.ImplementedSyntheticTests,
                ["Native Civil NX 9.5.5 MCT exports equal to the API model (steel frames)"] = CapabilityStatus.ImplementedSyntheticTests,
                ["Walls, plane/solid elements, stories and floor diaphragms, tapered/PSC/SRC sections, nonlinear links and springs"] = CapabilityStatus.PreservedOnly,
                ["Analysis results"] = CapabilityStatus.NotSupported
            });

        public MidasTextModelReader(string program, string modelRevision, string analysisId, Encoding textEncoding)
        {
            if (string.IsNullOrWhiteSpace(modelRevision)) throw new ArgumentException("Model revision required.", nameof(modelRevision));
            Program = program; revision = modelRevision; this.analysisId = analysisId;
            encoding = (Encoding)(textEncoding ?? new UTF8Encoding(false, true)).Clone();
            encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
        }

        public ImportBatch Read(Stream source, CancellationToken cancellationToken) => ReadWithTables(source, cancellationToken).Batch;

        public (ImportBatch Batch, CivilNxTables Tables) ReadWithTables(Stream source, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var blocks = MidasCivilTextSyntax.Read(source, encoding, cancellationToken);
            var translator = new MidasTextTranslator(blocks, Program, cancellationToken);
            translator.Run();
            ImportBatch batch;
            try
            {
                batch = CivilNxModelProfile.ReadTables(translator.Tables, new AnalysisSource { Program = Program, SolverVersion = translator.Version,
                    ModelRevision = revision, AnalysisId = analysisId }, cancellationToken);
            }
            catch (ArgumentException ex) { throw new InvalidDataException(ex.Message, ex); }
            batch.Diagnostics.InsertRange(0, translator.Diagnostics);
            batch.Uninterpreted.InsertRange(0, translator.Evidence);
            batch.Nodes.AddRange(translator.Duplicates); // Identical duplicates are reported by ModelMapper.
            Finish(batch, translator);
            return (batch, translator.Tables);
        }

        /// <summary>Conventions of the text reader that the API does not have: beams by reference point keep no axes, unresolved sections are
        /// left out, pair offsets other than the centroid are not qualified, and every beam keeps its element record.</summary>
        private void Finish(ImportBatch batch, MidasTextTranslator translator)
        {
            void Warn(string code, string record, string message) => batch.Diagnostics.Add(new ModelDiagnostic { Code = code, Record = record, Severity = DiagnosticSeverity.Warning, Message = message });
            foreach (var beam in batch.Beams.Where(b => translator.ReferenceBeams.Contains(b.Id)))
            {
                var placeholder = beam.CoordinateSystem; beam.CoordinateSystem = null;
                Warn("BeamReferenceAxesPreserved", beam.Record, "REF orientation retained; no section coordinate system inferred.");
                // Local loads were given in the placeholder axes of a zero angle: they are not the source axes.
                int loads = batch.BeamLoads.RemoveAll(l => l.BeamId == beam.Id && ReferenceEquals(l.CoordinateSystem, placeholder));
                int releases = batch.BeamReleases.RemoveAll(r => r.BeamId == beam.Id);
                if (loads + releases != 0) Warn("BeamReferenceAssignmentsPreserved", beam.Record, "Local loads and end releases of a beam without section axes are preserved only.");
            }
            var empty = new HashSet<string>(batch.Sections.Where(s => s.IdenticalPair == null && s.CatalogDesignation == null && s.Shape == SectionShapeKind.Unknown
                && s.Values == null && s.GeometryComponents == null).Select(s => s.Id), StringComparer.Ordinal);
            batch.Sections.RemoveAll(s => empty.Contains(s.Id));
            foreach (var beam in batch.Beams.Where(b => b.SectionId != null && empty.Contains(b.SectionId))) { beam.SectionId = null; beam.MaterialId = null; }
            foreach (var section in batch.Sections.Where(s => s.IdenticalPair != null && s.Reference != null && !s.Reference.IsUnresolved
                && (s.Reference.Horizontal != SectionHorizontalReference.Centroid || s.Reference.Vertical != SectionVerticalReference.Centroid)))
            {
                section.Reference = new SectionReference { IsUnresolved = true };
                Warn("CivilPairOffsetUnresolved", section.Record, "Pair outline imported; this source section offset is not qualified.");
            }
            foreach (var beam in batch.Beams)
            {
                translator.ElementLines.TryGetValue(beam.Id, out var line);
                beam.OtherAssignments.Add(new PreservedAssignment { Kind = "MIDAS material/section/orientation references", SourceRecord = beam.Record, RawData = line,
                    UnsupportedReason = beam.SectionId != null ? "Material/section mapped; original references and orientation retained." : "Material/section references retained without a mapped property." });
            }
            if (batch.Beams.Any(b => b.SectionId == null) || batch.Shells.Any(s => s.ThicknessId == null))
                Warn("CivilPropertiesPreserved", "*ELEMENT", "Unmapped material/section/thickness references remain preserved; no strength inferred from elastic data.");
            Warn("CivilInputWithoutResults", Program == MidasTextFiles.GenProgram ? "MGT" : "MCT", "Input model read. Analysis results require a separately matched export; verification remains disabled.");
        }
    }
}
