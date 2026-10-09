using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter
{
    /// <summary>Solver-specific entry points use an explicitly supplied, version-qualified reader.
    /// Without one, bytes are retained and the operation is rejected, never labelled as imported geometry.</summary>
    public sealed class SolverFileAdapter
    {
        public string Program { get; }
        public IModelFileReader Reader { get; }
        public SolverFileAdapter(string program, IModelFileReader reader = null)
        {
            if (!new[] { "MIDAS Civil NX", "MIDAS Civil", "MIDAS Gen NX", "MIDAS Gen", "Straus7", "Strand7", "SAP2000" }.Contains(program)) throw new ArgumentException("Unknown program.");
            if (reader != null && reader.Program != program) throw new ArgumentException("Reader program mismatch.");
            Program = program; Reader = reader;
        }
        public ImportReport Import(Stream source, CancellationToken cancellationToken = default(CancellationToken))
        {
            var rejected = new ImportReport { Status = ImportStatus.Rejected };
            byte[] original = null;
            try
            {
                using (var memory = new MemoryStream())
                {
                    var buffer = new byte[8192]; int count;
                    while ((count = source.Read(buffer, 0, buffer.Length)) != 0)
                    {
                        cancellationToken.ThrowIfCancellationRequested(); memory.Write(buffer, 0, count);
                        if (memory.Length > 128L * 1024 * 1024) throw new InvalidDataException("Source exceeds configured import size limit.");
                    }
                    memory.Position = 0; rejected.SourceHash = SourceEvidence.Sha256(memory); memory.Position = 0;
                    original = memory.ToArray();
                    if (Reader == null)
                    {
                        PreserveOriginal(rejected, original, "No validated reader for this solver/version is installed.");
                        rejected.Diagnostics.Add(ModelDiagnostic.Error("MissingValidatedReader", message: "Original bytes retained as Base64; no solver format or sign convention has been inferred.")); return rejected;
                    }
                    var batch = Reader.Read(memory, cancellationToken); batch.SourceHash = rejected.SourceHash;
                    var report = ModelMapper.Map(batch, cancellationToken);
                    if (report.Status == ImportStatus.Rejected) PreserveOriginal(report, original, "Mapping rejected; complete original bytes retained.");
                    return report;
                }
            }
            catch (OperationCanceledException) { rejected.Status = ImportStatus.Cancelled; return rejected; }
            catch (Exception ex) when (ex is FormatException || ex is InvalidDataException || ex is NotSupportedException || ex is DecoderFallbackException)
            {
                if (original != null) PreserveOriginal(rejected, original, "Reader rejected the source; complete original bytes retained.");
                rejected.Diagnostics.Add(new ModelDiagnostic { Code = "SourceReadFailed", Severity = DiagnosticSeverity.Error,
                    Record = (ex as ModelFileReadException)?.Record, Message = ex.Message,
                    SuggestedAction = "Correct the source record or select a reader/encoding matching the export schema." }); return rejected;
            }
        }
        private void PreserveOriginal(ImportReport report, byte[] original, string reason) => report.Preserved.Add(
            new PreservedAssignment { Kind = Program + " file", RawData = Convert.ToBase64String(original), UnsupportedReason = reason });
    }
}
