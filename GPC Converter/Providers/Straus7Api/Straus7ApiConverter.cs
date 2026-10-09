using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Models;
using GPC.Model.PostProcessing;

namespace GPC.Converter.Straus7
{
    /// <summary>Acquires an isolated read-only native session, then maps a detached candidate.
    /// API initialization, licence ownership and all reads are serialized for this converter.</summary>
    public sealed class Straus7ApiConverter
    {
        public const string InputBindingKind = "Straus7 input binding";
        private static readonly SemaphoreSlim Gate = new SemaphoreSlim(1, 1);
        private readonly Func<IStraus7ReadApi> openApi;
        private const int MaximumEntities = 1000000;
        public Straus7ApiConverter(string libraryPath = null) : this(() => new Straus7NativeApi(libraryPath)) { }
        public Straus7ApiConverter(Func<IStraus7ReadApi> apiFactory) { openApi = apiFactory ?? throw new ArgumentNullException(nameof(apiFactory)); }

        public ImportReport Import(Straus7ImportRequest request, CancellationToken cancellationToken = default)
        {
            var report = new ImportReport { Status = ImportStatus.Rejected }; bool acquired = false;
            try
            {
                if (request == null || string.IsNullOrWhiteSpace(request.ModelRevision)) throw new ArgumentException("Model revision required.");
                var path = ExistingPath(request.ModelPath); var revision = request.ModelRevision; var analysis = request.AnalysisId;
                cancellationToken.ThrowIfCancellationRequested(); Gate.Wait(cancellationToken); acquired = true;
                ImportBatch batch;
                using (var source = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    report.SourceHash = Hash(source, cancellationToken);
                    report.Preserved.Add(new PreservedAssignment { Kind = "Straus7 native source", SourceRecord = path, RawData = report.SourceHash,
                        UnsupportedReason = "SHA256 of original .st7 file; source binary is not embedded and remains at this path. Unqueried assignments require separate mapping." });
                    using (var api = openApi())
                    {
                        api.OpenModelReadOnly(path, Path.GetTempPath());
                        batch = ReadGeometry(api, revision, analysis, report, cancellationToken);
                        api.CloseModel();
                    }
                    if (Hash(source, cancellationToken) != report.SourceHash) throw new InvalidDataException("Model changed during API acquisition.");
                }
                var mapped = ModelMapper.Map(batch, cancellationToken);
                if (mapped.Model != null)
                {
                    var binding = SourceBinding.Create(InputBindingKind, report.SourceHash, mapped.Model,
                        "Imported Model fingerprints for subsequent API result binding.");
                    mapped.Model.PreservedSourceData.Add(binding); mapped.Preserved.Add(binding);
                }
                return mapped;
            }
            catch (OperationCanceledException) { report.Status = ImportStatus.Cancelled; }
            catch (Exception ex) when (Handled(ex)) { report.Diagnostics.Add(Diagnostic(ex)); }
            finally { if (acquired) Gate.Release(); }
            return report;
        }

        private static ImportBatch ReadGeometry(IStraus7ReadApi api, string revision, string analysis, ImportReport report, CancellationToken token)
        {
            var batch = new ImportBatch { Program = "Straus7", SolverVersion = api.Version, ModelRevision = revision, AnalysisId = analysis, SourceHash = report.SourceHash };
            batch.Uninterpreted.AddRange(report.Preserved);
            void Preserve<T>(string record, T data, string reason = null)
            {
                var item = Evidence(record, data, reason); batch.Uninterpreted.Add(item); report.Preserved.Add(item);
            }
            var unitCodes = api.ReadUnits(); var units = Units(unitCodes); Preserve("St7GetUnits", unitCodes);
            int nodes = Bounded(api.Count(Straus7Entity.Node)), beams = Bounded(api.Count(Straus7Entity.Beam)), plates = Bounded(api.Count(Straus7Entity.Plate));
            int bricks = Bounded(api.Count(Straus7Entity.Brick)), links = Bounded(api.Count(Straus7Entity.Link));
            if ((long)nodes + beams + plates > MaximumEntities) throw new InvalidDataException("Native entity import limit exceeded.");
            if (bricks != 0 || links != 0) throw new NotSupportedException("This reader requires a node/beam/plate model; bricks or links are present. No entities have been silently dropped.");
            var points = new Dictionary<int, Point3d>();
            for (int i = 1; i <= nodes; i++)
            {
                token.ThrowIfCancellationRequested(); var node = api.ReadNode(i);
                if (node == null || node.Number != i) throw new InvalidDataException("Node API number mismatch.");
                Finite(node.Coordinates, 3, "Node " + i);
                var p = new Point3d(units.Length(node.Coordinates[0]), units.Length(node.Coordinates[1]), units.Length(node.Coordinates[2]));
                points.Add(i, p); batch.Nodes.Add(new NodeRecord { Id = Id(i), GlobalPosition = p, Record = "St7GetNodeXYZ " + i });
                Preserve("Node/" + i, node, "Connectivity uses the API number. UserId is independent metadata, not the registry key.");
            }
            var groups = new Dictionary<int, GroupRecord>(); var nativeGroups = new List<Straus7Group>();
            int groupCount = Bounded(api.GroupCount);
            for (int i = 1; i <= groupCount; i++)
            {
                token.ThrowIfCancellationRequested(); var g = api.ReadGroup(i);
                if (g == null || g.Id < 1 || string.IsNullOrWhiteSpace(g.Name)) throw new InvalidDataException("Invalid group metadata.");
                // Group labels need not be unique. The stable native ID disambiguates them in the common named registry.
                var group = new GroupRecord { Name = "Group " + Id(g.Id) + ": " + g.Name, Record = "St7GetGroupByIndex " + i };
                groups.Add(g.Id, group); batch.Groups.Add(group); nativeGroups.Add(g); Preserve("Group/" + g.Id, g);
            }
            foreach (var g in nativeGroups)
                if (g.ParentId != -1) groups[g.Id].ParentName = groups.TryGetValue(g.ParentId, out var parent) ? parent.Name : throw new InvalidDataException("Missing group parent " + g.ParentId);
            int cases = Bounded(api.LoadCaseCount); var caseNames = new Dictionary<int, string>();
            for (int i = 1; i <= cases; i++)
            {
                token.ThrowIfCancellationRequested(); string name = api.ReadLoadCase(i);
                caseNames.Add(i, CaseName(i, name));
                batch.LoadCases.Add(new LoadCaseRecord { Name = CaseName(i, name), Record = "St7GetLoadCaseName " + i });
                Preserve("LoadCase/" + i, name);
            }
            var assignments = new Straus7Assignments(batch, unitCodes, api as IStraus7AssignmentReadApi, caseNames);
            var seenProperties = new Dictionary<string, (Straus7Property Property, string Material, string Other)>();
            foreach (var entity in new[] { Straus7Entity.Beam, Straus7Entity.Plate })
            {
                int count = entity == Straus7Entity.Beam ? beams : plates;
                for (int i = 1; i <= count; i++)
                {
                    token.ThrowIfCancellationRequested(); var e = api.ReadElement(entity, i); var record = entity + "/" + i;
                    if (e == null || e.Number != i || e.Property <= 0) throw new InvalidDataException("Invalid element API number/property: " + record);
                    Preserve(record, e, "Property reference and UserId preserved.");
                    bool beam = entity == Straus7Entity.Beam;
                    if (e.Formulation != (beam ? 6 : 4)) throw new NotSupportedException("Unsupported Straus7 formulation " + e.Formulation + " at " + record);
                    var propertyKey = entity + "/" + e.Property;
                    if (!seenProperties.TryGetValue(propertyKey, out var mapped))
                    {
                        var property = api.ReadProperty(entity, e.Property);
                        if (property == null || property.Number != e.Property || property.Formulation != e.Formulation) throw new InvalidDataException("Property identity/formulation mismatch: " + record);
                        if (property.Geometry != null) Finite(property.Geometry, property.Geometry.Length, propertyKey);
                        if (property.Material != null) Finite(property.Material, property.Material.Length, propertyKey);
                        if (property.SectionProperties != null) Finite(property.SectionProperties, property.SectionProperties.Length, propertyKey);
                        Preserve("Property/" + propertyKey, property, "Native elastic/section data in source units; strengths come only from a recognised grade of the material name.");
                        var ids = beam ? assignments.Beam(property) : assignments.Plate(property);
                        mapped = (property, ids.Item1, ids.Item2); seenProperties.Add(propertyKey, mapped);
                    }
                    if (e.Nodes == null || (beam ? e.Nodes.Length != 2 : e.Nodes.Length != 3 && e.Nodes.Length != 4))
                        throw new NotSupportedException("Unsupported higher-order/orientation-node connectivity at " + record);
                    if (e.Nodes.Distinct().Count() != e.Nodes.Length || e.Nodes.Any(n => !points.ContainsKey(n))) throw new InvalidDataException("Invalid connectivity at " + record);
                    Finite(e.InitialAxes, 9, record + " initial axes");
                    Finite(e.Centroid, 3, record + " centroid");
                    var origin = beam ? points[e.Nodes[0]] : new Point3d(units.Length(e.Centroid[0]), units.Length(e.Centroid[1]), units.Length(e.Centroid[2]));
                    var axes = CoordinateSystem(origin, e.InitialAxes); Axes.Validate(axes);
                    if (beam)
                    {
                        Vector3d direction = points[e.Nodes[1]] - points[e.Nodes[0]]; double length = Axes.Length(direction);
                        if (length <= 1e-9 || !Finite(length) || Axes.Length(direction / length - axes.V3) > 1e-8)
                            throw new InvalidDataException("Beam axis 3 must follow I to J: " + record);
                        var b = new BeamRecord { Id = Id(i), I = Id(e.Nodes[0]), J = Id(e.Nodes[1]), CoordinateSystem = axes, Record = record,
                            MaterialId = mapped.Material, SectionId = mapped.Material == null ? null : mapped.Other };
                        b.OtherAssignments.Add(Evidence(record, e, "Element record; releases and other unmapped attributes stay in the source file.")); batch.Beams.Add(b);
                        assignments.BeamElement(i, b, e.Property);
                    }
                    else
                    {
                        Vector3d normal = ((Vector3d)(points[e.Nodes[1]] - points[e.Nodes[0]])).CrossProduct(points[e.Nodes[2]] - points[e.Nodes[0]]);
                        if (Axes.Length(normal) <= 1e-9 || Axes.Dot(normal / Axes.Length(normal), axes.V3) < 1 - 1e-8)
                            throw new InvalidDataException("Plate axis normal conflicts with connectivity: " + record);
                        var shell = new ShellRecord { Id = Id(i), Nodes = e.Nodes.Select(Id).ToArray(), CoordinateSystem = axes, Record = record,
                            MaterialId = mapped.Material, ThicknessId = mapped.Material == null ? null : mapped.Other };
                        batch.Shells.Add(shell);
                        assignments.PlateElement(i, shell, mapped.Property);
                    }
                    if (!groups.TryGetValue(e.GroupId, out var group)) throw new InvalidDataException("Missing element group: " + record);
                    group.Members.Add(new SourceIdentity("Straus7", revision, beam ? EntityFamily.Beam : EntityFamily.Shell, Id(i)));
                }
            }
            assignments.Nodes(points); assignments.Gravity(cases); assignments.Finish();
            int stages = Bounded(api.StageCount); Preserve("St7GetNumStages", stages, "Construction-stage definitions/activation are not mapped.");
            batch.Diagnostics.Add(assignments.Available
                ? new ModelDiagnostic { Code = "Straus7AssignmentsPartlyMapped", Severity = DiagnosticSeverity.Warning,
                    Message = "Properties, offsets, restraints of a single freedom case, nodal/beam/plate loads and gravity imported; releases, springs, links, temperatures and stages are not mapped." }
                : new ModelDiagnostic { Code = "Straus7AssignmentsNotMapped", Severity = DiagnosticSeverity.Warning,
                    Message = "The API binding reads no element attributes: properties are imported, loads, restraints and offsets are not." });
            return batch;
        }

        /// <summary>Reads explicitly selected cases/entities; empty selections list primary result cases only.
        /// No analysis, combinations, averaging, saving, or result regeneration occurs.</summary>
        public Straus7ResultsReport ReadResults(Straus7ResultsRequest request, CancellationToken cancellationToken = default)
        {
            var report = new Straus7ResultsReport { Status = ImportStatus.Rejected }; bool acquired = false;
            try
            {
                if (request == null) throw new ArgumentNullException(nameof(request));
                var modelPath = ExistingPath(request.ModelPath); var resultPath = ExistingPath(request.ResultPath);
                if (string.Equals(modelPath, resultPath, StringComparison.OrdinalIgnoreCase)) throw new ArgumentException("Model and results must be different files.");
                var cases = Selection(request.CaseNumbers ?? new int[0]); var beams = Selection(request.BeamNumbers); var plates = Selection(request.PlateNumbers); var nodes = Selection(request.NodeNumbers);
                if (request.MinimumBeamStations < 2 || request.MinimumBeamStations > 100) throw new ArgumentOutOfRangeException(nameof(request.MinimumBeamStations));
                int stations = request.MinimumBeamStations;
                if (cases.Length == 0 && beams.Length + plates.Length + nodes.Length > 0) throw new ArgumentException("Select explicit result case numbers.");
                cancellationToken.ThrowIfCancellationRequested(); Gate.Wait(cancellationToken); acquired = true;
                using (var modelSource = new FileStream(modelPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                using (var resultSource = new FileStream(resultPath, FileMode.Open, FileAccess.Read, FileShare.Read))
                {
                    report.ModelHash = Hash(modelSource, cancellationToken); report.ResultHash = Hash(resultSource, cancellationToken);
                    using (var api = openApi())
                    {
                        report.ApiVersion = api.Version; api.OpenModelReadOnly(modelPath, Path.GetTempPath());
                        var elementApi = api as IStraus7ElementNodeReadApi;
                        if (request.IncludeElementNodeForces && elementApi == null) throw new NotSupportedException("The API binding does not provide element node forces.");
                        report.ModelStageCount = Bounded(api.StageCount);
                        report.Units = api.ReadUnits(); Units(report.Units);
                        ValidateSelection(beams, Bounded(api.Count(Straus7Entity.Beam))); ValidateSelection(plates, Bounded(api.Count(Straus7Entity.Plate))); ValidateSelection(nodes, Bounded(api.Count(Straus7Entity.Node)));
                        report.File = api.ValidateResults(resultPath);
                        if (report.File == null || report.File.ValidationFlags != 0) throw new InvalidDataException("St7ValidateResultFile rejected the model/result association; flags=" + report.File?.ValidationFlags);
                        var opened = api.OpenResults(resultPath);
                        report.File.PrimaryCases = Bounded(opened.PrimaryCases); report.File.SecondaryCases = Bounded(opened.SecondaryCases);
                        if (cases.Length == 0) cases = Enumerable.Range(1, report.File.PrimaryCases).ToArray();
                        ValidateSelection(cases, report.File.PrimaryCases); // No implicit combinations/envelopes.
                        long values = 0;
                        void Add(Straus7ResultTable table, Straus7Entity entity, int number, int c, string quantity)
                        {
                            cancellationToken.ThrowIfCancellationRequested();
                            if (table == null || table.Entity != entity || table.Number != number || table.CaseNumber != c || table.Quantity != quantity)
                                throw new InvalidDataException("Native result table identity mismatch.");
                            if (table.Rows <= 0 || table.Columns <= 0 || table.Values == null || (long)table.Rows * table.Columns != table.Values.Length)
                                throw new InvalidDataException("Invalid result dimensions.");
                            Finite(table.Values, table.Values.Length, quantity);
                            if (quantity == "ElementNodeForceGlobal" && (table.NodeNumbers == null || table.NodeNumbers.Length != table.Rows
                                || table.NodeNumbers.Any(n => n < 1) || table.NodeNumbers.Distinct().Count() != table.Rows))
                                throw new InvalidDataException("Invalid element node-force row identities.");
                            if (entity == Straus7Entity.Beam)
                            {
                                Finite(table.Positions, table.Rows, "Beam stations");
                                if (table.Positions.Any(p => p < 0 || p > 1) || !table.Positions.SequenceEqual(table.Positions.OrderBy(p => p))) throw new InvalidDataException("Invalid beam station order/domain.");
                            }
                            if (entity != Straus7Entity.Node && (table.ElementState == null || table.ElementState.Length != 3 || table.ElementState[0] != 1 || table.ElementState[1] != 1))
                                throw new InvalidDataException("Inactive element or unavailable results.");
                            values += table.Values.Length; if (values > 10000000) throw new InvalidDataException("Result import size limit exceeded.");
                            report.Tables.Add(table);
                        }
                        foreach (int c in cases)
                        {
                            cancellationToken.ThrowIfCancellationRequested(); var resultCase = api.ReadResultCase(c);
                            if (resultCase == null || resultCase.Number != c) throw new InvalidDataException("Native case number mismatch.");
                            report.Cases.Add(resultCase);
                            foreach (int b in beams)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                Add(api.ReadBeamForces(b, c, stations), Straus7Entity.Beam, b, c, "BeamForceGlobal");
                                if (request.IncludeElementNodeForces)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    Add(elementApi.ReadElementNodeForces(Straus7Entity.Beam, b, c), Straus7Entity.Beam, b, c, "ElementNodeForceGlobal");
                                }
                            }
                            foreach (int p in plates)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                Add(api.ReadPlateResult(p, c, false), Straus7Entity.Plate, p, c, "PlateForceLocalCentroid");
                                cancellationToken.ThrowIfCancellationRequested();
                                Add(api.ReadPlateResult(p, c, true), Straus7Entity.Plate, p, c, "PlateMomentLocalCentroid");
                                if (request.IncludeElementNodeForces)
                                {
                                    cancellationToken.ThrowIfCancellationRequested();
                                    Add(elementApi.ReadElementNodeForces(Straus7Entity.Plate, p, c), Straus7Entity.Plate, p, c, "ElementNodeForceGlobal");
                                }
                            }
                            foreach (int n in nodes)
                            {
                                cancellationToken.ThrowIfCancellationRequested();
                                Add(api.ReadNodeResult(n, c, false), Straus7Entity.Node, n, c, "NodeDisplacementGlobal");
                                cancellationToken.ThrowIfCancellationRequested();
                                Add(api.ReadNodeResult(n, c, true), Straus7Entity.Node, n, c, "NodeReactionGlobal");
                            }
                        }
                        api.CloseResults(); api.CloseModel();
                    }
                    if (Hash(modelSource, cancellationToken) != report.ModelHash || Hash(resultSource, cancellationToken) != report.ResultHash)
                        throw new InvalidDataException("Source files changed during results acquisition.");
                }
                report.Status = ImportStatus.Completed;
                report.Diagnostics.Add(new ModelDiagnostic { Code = "Straus7NativeResults", Severity = DiagnosticSeverity.Information,
                    Message = "Native result tables acquired. Their units, component conventions, cases and stage/element states are retained; acquisition is not a structural verification." });
            }
            catch (OperationCanceledException) { report.Status = ImportStatus.Cancelled; report.Tables.Clear(); report.Cases.Clear(); }
            catch (Exception ex) when (Handled(ex)) { report.Tables.Clear(); report.Cases.Clear(); report.Diagnostics.Add(Diagnostic(ex)); }
            finally { if (acquired) Gate.Release(); }
            return report;
        }
        /// <summary>Reads the results of a plan resolved on a model imported from <paramref name="modelPath"/> (<see cref="ResultFilter"/>): lists the
        /// primary result cases, maps the planned static cases to them by name (<see cref="Straus7LinearStaticResults.CaseMap"/>), then reads the
        /// forces of the planned beams and plates and the displacements and reactions of the planned nodes.</summary>
        public Straus7ResultsReport ReadResults(GPC.Model.Models.Model model, ResultReadPlan plan, string modelPath, string resultPath, bool includeElementNodeForces = false,
            int minimumBeamStations = 3, CancellationToken cancellationToken = default)
        {
            Straus7ResultsRequest request;
            try
            {
                if (model == null || plan == null) throw new ArgumentNullException();
                var listing = ReadResults(new Straus7ResultsRequest { ModelPath = modelPath, ResultPath = resultPath }, cancellationToken);
                if (listing.Status != ImportStatus.Completed) return listing;
                var map = Straus7LinearStaticResults.CaseMap(model, listing.Cases);
                var numbers = plan.StaticCases.Select(name => map.Where(p => p.Value == name).Select(p => (int?)p.Key).SingleOrDefault()
                    ?? throw new InvalidDataException("No primary result case of " + Path.GetFileName(resultPath) + " for " + name + "; result cases: "
                        + string.Join(", ", listing.Cases.Select(c => c.Number.ToString(CultureInfo.InvariantCulture) + " '" + c.Name + "'")))).ToArray();
                int[] Numbers(IEnumerable<GPC.Model.Elements.Element> elements) => elements.Select(e => e.Source?.Program == "Straus7"
                    ? int.Parse(e.Source.OriginalId, NumberStyles.None, CultureInfo.InvariantCulture) : throw new ArgumentException("Element " + e.Id + " was not imported from Straus7.")).ToArray();
                request = new Straus7ResultsRequest { ModelPath = modelPath, ResultPath = resultPath, CaseNumbers = numbers, BeamNumbers = Numbers(plan.Beams),
                    PlateNumbers = Numbers(plan.Shells), NodeNumbers = Numbers(plan.Nodes), MinimumBeamStations = minimumBeamStations, IncludeElementNodeForces = includeElementNodeForces };
            }
            catch (OperationCanceledException) { return new Straus7ResultsReport { Status = ImportStatus.Cancelled }; }
            catch (Exception ex) when (Handled(ex))
            {
                var rejected = new Straus7ResultsReport { Status = ImportStatus.Rejected }; rejected.Diagnostics.Add(Diagnostic(ex)); return rejected;
            }
            return ReadResults(request, cancellationToken);
        }

        public static string CaseName(int number, string name) => "Case " + Id(number) + ": " + (name ?? "");
        internal static string Id(int n) => n.ToString(CultureInfo.InvariantCulture);
        internal static ResultUnits Units(int[] codes)
        {
            if (codes == null || codes.Length != 6) throw new InvalidDataException("Expected six native unit codes.");
            double[] lengths = { 1000, 10, 1, 304.8, 25.4 }; double[] forces = { 1, 1000, 1000000, 9.80665, 4.4482216152605, 9806.65, 4448.2216152605 };
            if (codes[0] < 0 || codes[0] >= lengths.Length || codes[1] < 0 || codes[1] >= forces.Length) throw new NotSupportedException("Unknown Straus7 length/force unit code.");
            return new ResultUnits(forces[codes[1]], lengths[codes[0]], forces[codes[1]] * lengths[codes[0]]);
        }
        internal static CoordinateSystem CoordinateSystem(Point3d origin, double[] a)
        {
            var x = new Vector3d(a[0], a[1], a[2]); var y = new Vector3d(a[3], a[4], a[5]); var z = new Vector3d(a[6], a[7], a[8]);
            // Validate before CoordinateSystem normalizes its constructor vectors: malformed source axes must not be repaired silently.
            double error = Math.Abs(Axes.Dot(x, x) - 1) + Math.Abs(Axes.Dot(y, y) - 1) + Math.Abs(Axes.Dot(z, z) - 1)
                + Math.Abs(Axes.Dot(x, y)) + Math.Abs(Axes.Dot(x, z)) + Math.Abs(Axes.Dot(y, z)) + Math.Abs(Axes.Dot(x.CrossProduct(y), z) - 1);
            if (!Finite(error) || error > 1e-8) throw new InvalidDataException("Native axes are not a right-handed orthonormal basis.");
            return new CoordinateSystem(origin, x, y, z);
        }
        internal static bool Finite(double v) => !double.IsNaN(v) && !double.IsInfinity(v);
        internal static void Finite(double[] values, int count, string record)
        { if (values == null || values.Length != count || values.Any(v => !Finite(v))) throw new InvalidDataException("Missing/nonfinite values at " + record); }
        private static int Bounded(int count)
        { if (count < 0 || count > MaximumEntities) throw new InvalidDataException("Invalid native count."); return count; }
        private static int[] Selection(int[] values)
        {
            if (values == null || values.Length > MaximumEntities || values.Any(n => n < 1) || values.Distinct().Count() != values.Length) throw new ArgumentException("Select distinct positive API numbers.");
            return values.OrderBy(n => n).ToArray();
        }
        private static void ValidateSelection(int[] selection, int count)
        { if (selection.Any(n => n > count)) throw new ArgumentException("API number outside the native entity/case range."); }
        private static string ExistingPath(string path)
        { if (string.IsNullOrWhiteSpace(path)) throw new ArgumentException("Source path required."); path = Path.GetFullPath(path); if (!File.Exists(path)) throw new FileNotFoundException("Source file not found.", path); return path; }
        private static string Hash(Stream stream, CancellationToken token)
        {
            token.ThrowIfCancellationRequested(); stream.Position = 0;
            using (var hash = System.Security.Cryptography.SHA256.Create())
            {
                var buffer = new byte[65536]; int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) != 0) { token.ThrowIfCancellationRequested(); hash.TransformBlock(buffer, 0, read, buffer, 0); }
                hash.TransformFinalBlock(new byte[0], 0, 0); stream.Position = 0; return BitConverter.ToString(hash.Hash).Replace("-", "");
            }
        }
        internal static PreservedAssignment Evidence<T>(string record, T data, string reason = null)
        {
            using (var stream = new MemoryStream())
            {
                new DataContractJsonSerializer(typeof(T)).WriteObject(stream, data);
                return new PreservedAssignment { Kind = "Straus7 API", SourceRecord = record, RawData = Encoding.UTF8.GetString(stream.ToArray()),
                    UnitsAndAxes = "Native model units; axes and component order follow the named R3 API call.", UnsupportedReason = reason };
            }
        }
        private static bool Handled(Exception ex) => ex is ArgumentException || ex is InvalidOperationException || ex is IOException || ex is InvalidDataException
            || ex is NotSupportedException || ex is Straus7ApiException || ex is DllNotFoundException || ex is EntryPointNotFoundException || ex is BadImageFormatException;
        private static ModelDiagnostic Diagnostic(Exception ex) => new ModelDiagnostic { Code = ex is DllNotFoundException || ex is EntryPointNotFoundException || ex is BadImageFormatException
            ? "Straus7MissingDependency" : ex is Straus7ApiException ? "Straus7ApiError" : "Straus7ImportRejected", Severity = DiagnosticSeverity.Error,
            Record = (ex as Straus7ApiException)?.Operation, Message = ex.Message,
            SuggestedAction = "Inspect the native error, source schema, R3 x64 installation and API licence; the active Model has not been modified." };
    }
}
