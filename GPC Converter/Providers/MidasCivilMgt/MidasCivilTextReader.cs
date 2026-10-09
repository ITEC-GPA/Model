using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Models;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.MidasCivil
{
    /// <summary>Documented Civil MCT command subset, also usable with an MGT-named Civil export.
    /// No release whitelist and no claim of validation against every solver version.</summary>
    public sealed class MidasCivilTextReader : IModelFileReader
    {
        private readonly string revision;
        private readonly string analysisId;
        private readonly Encoding encoding;
        private const int MaximumExpandedReferences = 1000000;
        private static readonly Regex IdRange = new Regex(@"\G\s*(\d+)(?:\s*to\s*(\d+)(?:\s*by\s*(\d+))?)?(?=\s|$)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));

        public string Program => "MIDAS Civil";
        public IReadOnlyDictionary<string, CapabilityStatus> Capabilities { get; } =
            new ReadOnlyDictionary<string, CapabilityStatus>(new Dictionary<string, CapabilityStatus>
            {
                ["Nodes/beam/plate connectivity"] = CapabilityStatus.ImplementedSyntheticTests,
                ["Units and beam beta axes"] = CapabilityStatus.ImplementedSyntheticTests,
                ["Structural groups and static cases"] = CapabilityStatus.ImplementedSyntheticTests,
                ["Global nodal loads and permanent global constraints"] = CapabilityStatus.ImplementedSyntheticTests,
                ["Materials/sections/thickness/releases/springs/stages"] = CapabilityStatus.PreservedOnly,
                ["Plate axes and beam REF axes"] = CapabilityStatus.PreservedOnly,
                ["Analysis results"] = CapabilityStatus.NotSupported
            });

        /// <param name="modelRevision">Caller-supplied revision, never inferred from element IDs.</param>
        /// <param name="analysisId">Optional externally attested analysis identity; an MCT file does not prove it.</param>
        /// <param name="textEncoding">Defaults to strict UTF-8 (with or without BOM). Specify the original encoding for legacy ANSI exports.</param>
        public MidasCivilTextReader(string modelRevision, string analysisId = null, Encoding textEncoding = null)
        {
            if (string.IsNullOrWhiteSpace(modelRevision)) throw new ArgumentException("Model revision required.", nameof(modelRevision));
            revision = modelRevision; this.analysisId = analysisId;
            encoding = (Encoding)(textEncoding ?? new UTF8Encoding(false, true)).Clone();
            encoding.DecoderFallback = DecoderFallback.ExceptionFallback;
        }

        public ImportReport Import(Stream source, CancellationToken cancellationToken = default(CancellationToken)) =>
            new SolverFileAdapter(Program, this).Import(source, cancellationToken);

        public ImportBatch Read(Stream source, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var blocks = MidasCivilTextSyntax.Read(source, encoding, cancellationToken);
            var batch = new ImportBatch { Program = Program, ModelRevision = revision, AnalysisId = analysisId };
            var points = new Dictionary<string, Point3d>(StringComparer.Ordinal);
            var elements = new Dictionary<string, EntityFamily>(StringComparer.Ordinal);
            var beamAngles = new Dictionary<BeamRecord, double>();
            var groups = new List<Tuple<GroupRecord, string[]>>();
            var restraints = new Dictionary<string, NodeRestrainRecord>(StringComparer.Ordinal);
            var caseNames = new HashSet<string>(StringComparer.Ordinal);
            var selectedCases = new List<Tuple<string, string>>();
            int expanded = 0; double force = 0, length = 0; string unitLabel = "not declared", activeCase = null;
            // Nodal local axes and construction-stage activation need their own complete mapping.
            // In their presence no constraints are flattened into permanent global supports.
            bool withholdConstraints = blocks.Any(b => b.Command == "LOCALAXIS" || b.Command.StartsWith("STAGE", StringComparison.Ordinal));

            foreach (var block in blocks)
            {
                cancellationToken.ThrowIfCancellationRequested();
                string command = block.Command;
                var evidence = new PreservedAssignment { Kind = "MIDAS Civil *" + command,
                    SourceRecord = block.Header.Record(command), RawData = block.Raw.ToString(), UnitsAndAxes = unitLabel };
                batch.Uninterpreted.Add(evidence);
                if (command == "UNIT")
                {
                    HeaderOnly(block); SingleLine(block);
                    var f = Fields(block, block.Lines[0], 2, 4);
                    if (f.Length != 2 && f.Length != 4) throw block.Lines[0].Error(command, "Expected FORCE,LENGTH or FORCE,LENGTH,HEAT,TEMPERATURE.");
                    force = ForceFactor(f[0], block.Lines[0]); length = LengthFactor(f[1], block.Lines[0]);
                    unitLabel = f[0] + "," + f[1] + "; coordinates GCS; canonical N/mm; beta degrees";
                    evidence.UnitsAndAxes = unitLabel;
                }
                else if (command == "VERSION")
                {
                    HeaderOnly(block); SingleLine(block);
                    if (batch.SolverVersion != null) throw block.Header.Error(command, "Duplicate version declaration.");
                    batch.SolverVersion = block.Lines[0].Text.Trim();
                }
                else if (command == "NODE")
                {
                    HeaderOnly(block); RequireUnits(block, length);
                    foreach (var line in block.Lines)
                    {
                        cancellationToken.ThrowIfCancellationRequested(); var f = Fields(block, line, 4, 4); var id = Id(f[0], line, command);
                        var p = new Point3d(Scaled(f[1], length, line, command), Scaled(f[2], length, line, command), Scaled(f[3], length, line, command));
                        if (points.TryGetValue(id, out var old) && (old.X != p.X || old.Y != p.Y || old.Z != p.Z))
                            throw line.Error(command, "Conflicting node ID " + id);
                        points[id] = p; batch.Nodes.Add(new NodeRecord { Id = id, GlobalPosition = p, Record = line.Record(command) });
                    }
                }
                else if (command == "ELEMENT")
                {
                    HeaderOnly(block); RequireUnits(block, length);
                    foreach (var line in block.Lines)
                    {
                        cancellationToken.ThrowIfCancellationRequested(); var f = Fields(block, line, 8, 12); var id = Id(f[0], line, command);
                        if (elements.ContainsKey(id)) throw line.Error(command, "Duplicate element ID " + id);
                        var type = f[1].ToUpperInvariant();
                        Id(f[2], line, command); Id(f[3], line, command); // Retain material/property references, never synthesize strengths.
                        if (type == "BEAM")
                        {
                            bool reference = f[6].Equals("REF", StringComparison.OrdinalIgnoreCase);
                            if ((!reference && f.Length != 8 && f.Length != 9) || (reference && f.Length != 11 && f.Length != 12))
                                throw line.Error(command, "Unsupported beam record layout.");
                            int sub = reference ? 10 : 7;
                            if (Number(f[sub], line, command) != 0 || (f.Length > sub + 1 && f[sub + 1].Length != 0 && Number(f[sub + 1], line, command) != 0))
                                throw line.Error(command, "Unsupported beam subtype/EXVAL.");
                            var beam = new BeamRecord { Id = id, I = Id(f[4], line, command), J = Id(f[5], line, command), Record = line.Record(command) };
                            beam.OtherAssignments.Add(new PreservedAssignment { Kind = "MIDAS material/section/orientation references", SourceRecord = beam.Record,
                                RawData = line.Text, UnitsAndAxes = unitLabel, UnsupportedReason = "Material " + f[2] + ", section " + f[3] + ": properties require separate mapping." });
                            batch.Beams.Add(beam); elements.Add(id, EntityFamily.Beam);
                            if (reference)
                            {
                                for (int i = 7; i <= 9; i++) Scaled(f[i], length, line, command);
                                Warn(batch, "BeamReferenceAxesPreserved", beam.Record, "REF orientation retained; no section coordinate system inferred.");
                            }
                            else beamAngles.Add(beam, Number(f[6], line, command));
                        }
                        else if (type == "PLATE")
                        {
                            if (f.Length != 9 && f.Length != 10) throw line.Error(command, "Unsupported plate record layout.");
                            if (f[8] != "1" && f[8] != "2") throw line.Error(command, "Only thick/thin 3/4-node plates are supported.");
                            if (f.Length == 10 && f[9].Length != 0) Number(f[9], line, command);
                            var ids = new List<string> { Id(f[4], line, command), Id(f[5], line, command), Id(f[6], line, command) };
                            if (f[7] != "0") ids.Add(Id(f[7], line, command));
                            batch.Shells.Add(new ShellRecord { Id = id, Nodes = ids.ToArray(), Record = line.Record(command) }); elements.Add(id, EntityFamily.Shell);
                        }
                        else throw line.Error(command, "Unsupported element type " + type + "; original file retained, no elements silently omitted.");
                    }
                    evidence.UnsupportedReason = "Material/section/thickness references and plate-axis suffix retained without interpretation.";
                }
                else if (command == "GROUP")
                {
                    HeaderOnly(block);
                    foreach (var line in block.Lines)
                    {
                        var f = Fields(block, line, 3, 4);
                        var g = new GroupRecord { Name = Required(f[0], line, command), Record = line.Record(command) };
                        foreach (var id in Expand(f[1], line, command, ref expanded, cancellationToken)) g.Members.Add(Source(EntityFamily.Node, id));
                        groups.Add(Tuple.Create(g, Expand(f[2], line, command, ref expanded, cancellationToken))); batch.Groups.Add(g);
                    }
                    evidence.UnsupportedReason = "Structural membership mapped; optional display-plane field retained only.";
                }
                else if (command == "STLDCASE")
                {
                    HeaderOnly(block);
                    foreach (var line in block.Lines)
                    {
                        var f = Fields(block, line, 3, 3); var name = Required(f[0], line, command);
                        if (!caseNames.Add(name)) throw line.Error(command, "Duplicate static case " + name);
                        batch.LoadCases.Add(new LoadCaseRecord { Name = name, Record = line.Record(command) });
                    }
                    evidence.UnsupportedReason = "Case names mapped; solver category/description retained in this source record.";
                }
                else if (command == "USE-STLD")
                {
                    if (block.Arguments.Length != 2 || block.Lines.Count != 0) throw block.Header.Error(command, "Expected *USE-STLD,case-name with no data records.");
                    activeCase = Required(block.Arguments[1], block.Header, command); selectedCases.Add(Tuple.Create(activeCase, block.Header.Record(command)));
                }
                else if (command == "CONLOAD")
                {
                    HeaderOnly(block); RequireUnits(block, length);
                    if (activeCase == null) throw block.Header.Error(command, "Missing *USE-STLD case selection.");
                    foreach (var line in block.Lines)
                    {
                        var f = Fields(block, line, 7, 8); var v = new double[6];
                        for (int i = 0; i < 6; i++) v[i] = Scaled(f[i + 1], i < 3 ? force : force * length, line, command);
                        foreach (var id in Expand(f[0], line, command, ref expanded, cancellationToken)) batch.NodeLoads.Add(new NodeLoadRecord {
                            NodeId = id, Case = activeCase, Components = (double[])v.Clone(), CoordinateSystem = CoordinateSystem.Global, Record = line.Record(command) });
                    }
                    evidence.UnsupportedReason = "Global force/moment components mapped; load-group activation retained only.";
                }
                else if (command == "CONSTRAINT" && !withholdConstraints)
                {
                    HeaderOnly(block);
                    foreach (var line in block.Lines)
                    {
                        var f = Fields(block, line, 2, 3);
                        if (f[1].Length != 6 || f[1].Any(c => c != '0' && c != '1')) throw line.Error(command, "Expected six binary DOF flags DX,DY,DZ,RX,RY,RZ.");
                        foreach (var id in Expand(f[0], line, command, ref expanded, cancellationToken))
                        {
                            if (!restraints.TryGetValue(id, out var r))
                            {
                                r = new NodeRestrainRecord { NodeId = id, FixedDofs = new bool[6], CoordinateSystem = CoordinateSystem.Global, Record = line.Record(command) };
                                restraints.Add(id, r); batch.NodeRestrains.Add(r);
                            }
                            else r.Record += "; " + line.Record(command);
                            for (int i = 0; i < 6; i++) r.FixedDofs[i] |= f[1][i] == '1';
                        }
                    }
                    evidence.UnsupportedReason = "Permanent GCS restraints mapped; boundary-group label retained only.";
                }
                else if (command == "ENDDATA") { HeaderOnly(block); }
                else
                {
                    evidence.UnsupportedReason = command == "CONSTRAINT" ? "Nodal local axes or construction stages prevent permanent GCS mapping."
                        : "This command has no semantic mapper in the current Civil reader.";
                    Warn(batch, "CivilCommandPreserved", evidence.SourceRecord, "*" + command + ": " + evidence.UnsupportedReason);
                }
            }
            if (points.Count == 0) throw new FormatException("Civil model contains no *NODE records.");
            foreach (var entry in beamAngles)
            {
                cancellationToken.ThrowIfCancellationRequested(); var beam = entry.Key;
                if (!points.TryGetValue(beam.I, out var i) || !points.TryGetValue(beam.J, out var j)) throw new ModelFileReadException(beam.Record, "Missing beam node.");
                try { beam.CoordinateSystem = BeamAxes(i, j, entry.Value); }
                catch (ArgumentException ex) { throw new ModelFileReadException(beam.Record, ex.Message); }
            }
            foreach (var entry in groups)
                foreach (var id in entry.Item2)
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    if (!elements.TryGetValue(id, out var family)) throw new ModelFileReadException(entry.Item1.Record, "Missing group element " + id);
                    entry.Item1.Members.Add(Source(family, id));
                }
            foreach (var selected in selectedCases)
                if (!caseNames.Contains(selected.Item1)) throw new ModelFileReadException(selected.Item2, "Undeclared static case " + selected.Item1);
            if (elements.Count != 0) Warn(batch, "CivilPropertiesPreserved", "*ELEMENT", "Material and section/thickness references are retained; no structural properties or strengths have been inferred.");
            if (batch.Shells.Count != 0) Warn(batch, "PlateAxesAndThicknessPreserved", "*ELEMENT", "Plate axes and physical thickness remain unassigned; no effective thickness is assumed to be a design thickness.");
            Warn(batch, "CivilInputWithoutResults", "MCT", "Input model read. Analysis results require a separately matched export; verification remains disabled.");
            return batch;
        }

        private SourceIdentity Source(EntityFamily family, string id) => new SourceIdentity(Program, revision, family, id);
        private static CoordinateSystem BeamAxes(Point3d i, Point3d j, double betaDegrees)
        {
            Vector3d x = j - i; var norm = Axes.Length(x);
            if (norm <= 1e-9 || double.IsInfinity(norm)) throw new ArgumentException("Degenerate beam.");
            x = x / norm;
            // MIDAS x = longitudinal, z = projected GCS Z; for exactly vertical members z = GCS X.
            // GPC (V1,V2,V3) = MIDAS (y,z,x). Rotation is right-handed about longitudinal x.
            var z = x.X == 0 && x.Y == 0 ? new Vector3d(1, 0, 0) : new Vector3d(0, 0, 1) - x * x.Z;
            if (Axes.Length(z) <= 1e-10) throw new ArgumentException("Near-vertical beta convention requires an explicit solver axis export.");
            z = z / Axes.Length(z);
            return Axes.Beam(i, j, z.CrossProduct(x), (betaDegrees % 360) * Math.PI / 180);
        }
        private static string[] Fields(CivilTextBlock block, CivilTextLine line, int min, int max)
        {
            var f = MidasCivilTextSyntax.Fields(line.Text, line.Record(block.Command));
            if (f.Length < min || f.Length > max) throw line.Error(block.Command, "Unexpected field count: " + f.Length);
            return f;
        }
        private static void HeaderOnly(CivilTextBlock block)
        { if (block.Arguments.Length != 1) throw block.Header.Error(block.Command, "Unexpected header arguments."); }
        private static void SingleLine(CivilTextBlock block)
        { if (block.Lines.Count != 1) throw block.Header.Error(block.Command, "Expected exactly one data record."); }
        private static void RequireUnits(CivilTextBlock block, double length)
        { if (length == 0) throw block.Header.Error(block.Command, "Explicit *UNIT required before dimensional data."); }
        private static string Required(string value, CivilTextLine line, string command)
        { if (string.IsNullOrWhiteSpace(value)) throw line.Error(command, "Missing required value."); return value; }
        private static string Id(string text, CivilTextLine line, string command)
        {
            if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value <= 0)
                throw line.Error(command, "Expected a positive integer ID: " + text);
            return value.ToString(CultureInfo.InvariantCulture);
        }
        private static double Number(string text, CivilTextLine line, string command)
        {
            if (!double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) || double.IsNaN(value) || double.IsInfinity(value))
                throw line.Error(command, "Expected a finite invariant-culture number: " + text);
            return value;
        }
        private static double Scaled(string text, double factor, CivilTextLine line, string command)
        {
            double value = Number(text, line, command) * factor;
            if (double.IsNaN(value) || double.IsInfinity(value)) throw line.Error(command, "Unit conversion overflow.");
            return value;
        }
        private static string[] Expand(string list, CivilTextLine line, string command, ref int expanded, CancellationToken token)
        {
            var result = new HashSet<string>(StringComparer.Ordinal); int at = 0;
            while (at < list.Length)
            {
                token.ThrowIfCancellationRequested(); Match match;
                try { match = IdRange.Match(list, at); }
                catch (RegexMatchTimeoutException) { throw line.Error(command, "ID list parsing time limit exceeded."); }
                if (!match.Success) throw line.Error(command, "Invalid ID list near: " + list.Substring(at));
                long start = int.Parse(Id(match.Groups[1].Value, line, command), CultureInfo.InvariantCulture);
                long end = match.Groups[2].Success ? int.Parse(Id(match.Groups[2].Value, line, command), CultureInfo.InvariantCulture) : start;
                long step = match.Groups[3].Success ? int.Parse(Id(match.Groups[3].Value, line, command), CultureInfo.InvariantCulture) : 1;
                if (end < start) throw line.Error(command, "Descending ID ranges are not supported.");
                long count = (end - start) / step + 1;
                if (count > MaximumExpandedReferences - expanded) throw line.Error(command, "Expanded ID reference limit exceeded.");
                expanded += (int)count;
                for (long id = start; id <= end; id += step) { token.ThrowIfCancellationRequested(); result.Add(id.ToString(CultureInfo.InvariantCulture)); }
                at = match.Index + match.Length;
            }
            return result.OrderBy(id => int.Parse(id, CultureInfo.InvariantCulture)).ToArray();
        }
        private static double ForceFactor(string unit, CivilTextLine line)
        {
            switch (unit.ToUpperInvariant())
            {
                case "N": return 1; case "KN": return 1000; case "KGF": return 9.80665; case "TONF": return 9806.65;
                case "LBF": return 4.4482216152605; case "KIPS": return 4448.2216152605;
                default: throw line.Error("UNIT", "Unsupported force unit " + unit);
            }
        }
        private static double LengthFactor(string unit, CivilTextLine line)
        {
            switch (unit.ToUpperInvariant())
            {
                case "MM": return 1; case "CM": return 10; case "M": return 1000; case "IN": return 25.4; case "FT": return 304.8;
                default: throw line.Error("UNIT", "Unsupported length unit " + unit);
            }
        }
        private static void Warn(ImportBatch batch, string code, string record, string message) => batch.Diagnostics.Add(
            new ModelDiagnostic { Code = code, Record = record, Severity = DiagnosticSeverity.Warning, Message = message });
    }
}
