using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using GPC.Converter.CivilNx;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.MidasCivil
{
    /// <summary>Translates the MCT/MGT commands into the Civil NX database records read by <see cref="CivilNxModelProfile"/>: one mapping for
    /// the API and the text files. Values are converted to N, mm and °C record by record (a file may change *UNIT between commands).
    /// Record layouts follow the comment templates written by MIDAS Civil/Civil NX/Gen exports; anything else is preserved with a diagnostic.</summary>
    internal sealed class MidasTextTranslator
    {
        private const int MaximumExpandedReferences = 1000000;
        private static readonly Regex IdRange = new Regex(@"\G\s*(\d+)(?:\s*to\s*(\d+)(?:\s*by\s*(\d+))?)?(?=\s|$)",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant, TimeSpan.FromSeconds(1));
        private static readonly Dictionary<string, string> CombinationTables = new Dictionary<string, string>(StringComparer.Ordinal)
            { ["GEN"] = "LCOM-GEN", ["CONC"] = "LCOM-CONC", ["STEEL"] = "LCOM-STEEL", ["SRC"] = "LCOM-SRC", ["STLCOMP"] = "LCOM-STLCOMP", ["SEISMIC"] = "LCOM-SEISMIC" };
        private static readonly HashSet<string> SectionTypes = new HashSet<string>(StringComparer.Ordinal)
            { "DBUSER", "VALUE", "SRC", "COMBINED", "TAPERED", "COMPOSITE", "PSC", "CONSTRUCT", "COMPOSITE-B", "COMPOSITE-I", "COMPOSITE-TUB", "STEEL", "PSC-CMPW" };

        private readonly IReadOnlyList<CivilTextBlock> blocks;
        private readonly string program;
        private readonly CancellationToken token;
        private int expanded;
        private double force, length;
        private string unitLabel = "not declared", activeCase;
        private readonly HashSet<string> caseNames = new HashSet<string>(StringComparer.Ordinal);
        private readonly List<Tuple<string, string>> selectedCases = new List<Tuple<string, string>>();
        private readonly Dictionary<string, (double X, double Y, double Z)> nodes = new Dictionary<string, (double, double, double)>(StringComparer.Ordinal);

        public CivilNxTables Tables { get; } = new CivilNxTables { Lenient = true };
        public List<PreservedAssignment> Evidence { get; } = new List<PreservedAssignment>();
        public List<ModelDiagnostic> Diagnostics { get; } = new List<ModelDiagnostic>();
        public List<NodeRecord> Duplicates { get; } = new List<NodeRecord>();
        /// <summary>Beams oriented by a reference point or vector (REF): no section axes are inferred.</summary>
        public HashSet<string> ReferenceBeams { get; } = new HashSet<string>(StringComparer.Ordinal);
        /// <summary>Element records (line text) by element ID, kept with each beam as evidence of its source references.</summary>
        public Dictionary<string, string> ElementLines { get; } = new Dictionary<string, string>(StringComparer.Ordinal);
        public string Version { get; private set; }

        public MidasTextTranslator(IReadOnlyList<CivilTextBlock> blocks, string program, CancellationToken token)
        { this.blocks = blocks; this.program = program; this.token = token; }

        public void Run()
        {
            Tables.Table<CivilNxModelProfile.UnitData>("UNIT")["1"] = new CivilNxModelProfile.UnitData { Force = "N", Distance = "MM", Temperature = "C" };
            Tables.TableRecord("UNIT", "*UNIT");
            // Nodal local axes by angles have no verified convention, and constraints on them are not flattened into global supports.
            var localAxes = blocks.Where(b => b.Command == "LOCALAXIS" || b.Command == "SKEW").ToArray();
            bool unresolvedAxes = localAxes.Any(b => b.Lines.Any(l => Fields(b, l, 2, 14).ElementAtOrDefault(1) != "2" && Fields(b, l, 2, 14).ElementAtOrDefault(1) != "3"));
            bool stages = blocks.Any(b => b.Command == "STAGE" && b.Lines.Count != 0);
            foreach (var block in blocks)
            {
                token.ThrowIfCancellationRequested();
                string command = block.Command;
                var evidence = new PreservedAssignment { Kind = program + " *" + command, SourceRecord = block.Header.Record(command), RawData = block.Raw.ToString(), UnitsAndAxes = unitLabel };
                Evidence.Add(evidence);
                string reason = null;
                switch (command)
                {
                    case "UNIT": Unit(block); evidence.UnitsAndAxes = unitLabel; break;
                    case "VERSION":
                        HeaderOnly(block); SingleLine(block);
                        if (Version != null) throw block.Header.Error(command, "Duplicate version declaration.");
                        Version = block.Lines[0].Text.Trim(); break;
                    case "NODE": Node(block); break;
                    case "ELEMENT": Element(block); reason = "Element records mapped through the Civil NX reader; unsupported families are preserved."; break;
                    case "GROUP": Group(block); reason = "Structural membership mapped; optional display-plane field retained only."; break;
                    case "STLDCASE": Cases(block); reason = "Case names mapped; solver category/description retained in this source record."; break;
                    case "USE-STLD":
                        if (block.Arguments.Length != 2 || block.Lines.Count != 0) throw block.Header.Error(command, "Expected *USE-STLD,case-name with no data records.");
                        activeCase = Required(block.Arguments[1], block.Header, command); selectedCases.Add(Tuple.Create(activeCase, block.Header.Record(command))); break;
                    case "CONLOAD": NodalLoads(block); reason = "Force/moment components mapped; load-group activation retained only."; break;
                    case "CONSTRAINT":
                        if (unresolvedAxes || stages)
                        {
                            reason = stages ? "Construction stages activate boundary groups; no permanent support is applied." : "Nodal local axes by angles prevent the mapping of these supports.";
                            Warn("CivilCommandPreserved", evidence.SourceRecord, "*" + command + ": " + reason);
                            if (unresolvedAxes) break;
                        }
                        Constraints(block); reason = reason ?? "Supports mapped in the node axes; boundary-group label retained only."; break;
                    case "SECTION": Sections(block); reason = "Sections mapped through the Civil NX reader; unsupported shapes and data are preserved."; break;
                    case "MATERIAL": Materials(block); reason = "Elastic data and code grades mapped; strengths come only from a recognised grade."; break;
                    case "THICKNESS": Thicknesses(block); reason = "Value thicknesses mapped; stiffened plates are preserved."; break;
                    case "SELFWEIGHT": SelfWeight(block); reason = "Gravity factors mapped; the load group is retained only."; break;
                    case "BEAMLOAD": BeamLoads(block); reason = "Element beam loads mapped; line, eccentric and additional loads are preserved."; break;
                    case "PRESSURE": Pressures(block); reason = "Plate face and edge pressures mapped; projected and non-plate pressures are preserved."; break;
                    case "LOADCOMB": Combinations(block); reason = "Combination definitions mapped with their factors."; break;
                    case "FRAME-RLS": Releases(block); reason = "Beam end releases mapped through the Civil NX reader."; break;
                    case "OFFSET": Offsets(block); reason = "Beam end offsets mapped through the Civil NX reader."; break;
                    case "LOCALAXIS": case "SKEW": LocalAxes(block); reason = "Node local axes by vectors or points mapped; by angles preserved."; break;
                    case "SPRING": Springs(block); reason = "Linear point springs mapped; damping and nonlinear springs preserved."; break;
                    case "ELASTICLINK": ElasticLinks(block); reason = "Uncoupled linear and rigid elastic links mapped; other link laws preserved."; break;
                    case "RIGIDLINK": RigidLinks(block); reason = "Rigid links mapped in global axes."; break;
                    case "STAGE": Stages(block); reason = "Construction schedule mapped as metadata; group operations are not permanent assignments."; break;
                    case "STORY": Stories(block); reason = "Story data preserved; rigid floor diaphragms are not represented."; break;
                    case "ENDDATA": HeaderOnly(block); break;
                    default:
                        reason = "This command has no semantic mapper in the current " + program + " text reader.";
                        Warn("CivilCommandPreserved", evidence.SourceRecord, "*" + command + ": " + reason);
                        break;
                }
                if (reason != null) evidence.UnsupportedReason = reason;
            }
            if (nodes.Count == 0) throw new FormatException(program + " model contains no *NODE records.");
            Tables.Table<TextSourceData>(CivilNxTables.TextConventionsTable)["1"] = new TextSourceData { Program = program, Version = Version, Lenient = true };
            foreach (var selected in selectedCases)
                if (!caseNames.Contains(selected.Item1)) throw new ModelFileReadException(selected.Item2, "Undeclared static case " + selected.Item1);
        }

        // ---- Units, nodes, elements, groups, cases ----

        private void Unit(CivilTextBlock block)
        {
            HeaderOnly(block); SingleLine(block);
            var f = Fields(block, block.Lines[0], 2, 4);
            if (f.Length != 2 && f.Length != 4) throw block.Lines[0].Error("UNIT", "Expected FORCE,LENGTH or FORCE,LENGTH,HEAT,TEMPERATURE.");
            force = MidasConventions.ForceFactor(f[0]) ?? throw block.Lines[0].Error("UNIT", "Unsupported force unit " + f[0]);
            length = MidasConventions.LengthFactor(f[1]) ?? throw block.Lines[0].Error("UNIT", "Unsupported length unit " + f[1]);
            unitLabel = f[0] + "," + f[1] + "; coordinates GCS; canonical N/mm; beta degrees";
        }

        private void Node(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block);
            var rows = Tables.Table<CivilNxModelProfile.NodeData>("NODE"); Tables.TableRecord("NODE", "*NODE");
            foreach (var line in block.Lines)
            {
                token.ThrowIfCancellationRequested(); var f = Fields(block, line, 4, 4); var id = Id(f[0], line, block.Command);
                var p = (Scaled(f[1], length, line, block.Command), Scaled(f[2], length, line, block.Command), Scaled(f[3], length, line, block.Command));
                if (nodes.TryGetValue(id, out var old))
                {
                    if (old.X != p.Item1 || old.Y != p.Item2 || old.Z != p.Item3) throw line.Error(block.Command, "Conflicting node ID " + id);
                    Duplicates.Add(new NodeRecord { Id = id, GlobalPosition = new GPC.Geometry.Point3d(p.Item1, p.Item2, p.Item3), Record = line.Record(block.Command) });
                    continue;
                }
                nodes.Add(id, p); rows.Add(id, new CivilNxModelProfile.NodeData { X = p.Item1, Y = p.Item2, Z = p.Item3 });
                Tables.Record("NODE", id, line.Record(block.Command));
            }
        }

        private void Element(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block);
            var rows = Tables.Table<CivilNxModelProfile.ElementData>("ELEM"); Tables.TableRecord("ELEM", "*ELEMENT");
            foreach (var line in block.Lines)
            {
                token.ThrowIfCancellationRequested(); var f = Fields(block, line, 8, 14); var id = Id(f[0], line, block.Command);
                if (rows.ContainsKey(id)) throw line.Error(block.Command, "Duplicate element ID " + id);
                var type = f[1].ToUpperInvariant();
                int material = int.Parse(Id(f[2], line, block.Command), CultureInfo.InvariantCulture), property = int.Parse(Id(f[3], line, block.Command), CultureInfo.InvariantCulture);
                var e = new CivilNxModelProfile.ElementData { Type = type, Material = material, Section = property };
                if (type == "BEAM" || type == "TRUSS")
                {
                    // iEL, TYPE, iMAT, iPRO, iN1, iN2, ANGLE, iSUB[, EXVAL]; or ..., iN2, REF, X, Y, Z, iSUB[, EXVAL] with a reference point.
                    bool reference = f[6].Equals("REF", StringComparison.OrdinalIgnoreCase);
                    if ((!reference && f.Length != 8 && f.Length != 9) || (reference && f.Length != 11 && f.Length != 12))
                        throw line.Error(block.Command, "Unsupported " + type.ToLowerInvariant() + " record layout.");
                    int sub = reference ? 10 : 7;
                    if (Number(f[sub], line, block.Command) != 0 || (f.Length > sub + 1 && f[sub + 1].Length != 0 && Number(f[sub + 1], line, block.Command) != 0))
                        throw line.Error(block.Command, "Unsupported beam subtype/EXVAL.");
                    e.SubType = 0;
                    e.Nodes = new[] { IntId(f[4], line, block.Command), IntId(f[5], line, block.Command) };
                    if (reference)
                    {
                        for (int i = 7; i <= 9; i++) Scaled(f[i], length, line, block.Command);
                        ReferenceBeams.Add(id); e.Angle = 0;
                    }
                    else e.Angle = Number(f[6], line, block.Command);
                }
                else if (type == "TENSTR" || type == "COMPTR" || type == "CABLE")
                {
                    // iEL, TYPE, iMAT, iPRO, iN1, iN2, ANGLE, iSUB, EXVAL, EXVAL2, bLMT (Comp/Tens Truss).
                    if (f.Length < 8 || f.Length > 11) throw line.Error(block.Command, "Unsupported " + type + " record layout.");
                    e.Nodes = new[] { IntId(f[4], line, block.Command), IntId(f[5], line, block.Command) };
                    e.Angle = Number(f[6], line, block.Command); e.SubType = (int)Number(f[7], line, block.Command);
                    if (f.Skip(8).Take(2).Any(v => v.Length != 0 && Number(v, line, block.Command) != 0))
                        Warn("CivilAxialLimitPreserved", "*ELEMENT", "Allowable forces, gaps or pretensions (EXVAL) of tension/compression-only members are preserved only.");
                }
                else if (type == "PLATE")
                {
                    // iEL, PLATE, iMAT, iPRO, iN1, iN2, iN3, iN4 (0 for triangles), iSUB, ANGLE.
                    if (f.Length != 9 && f.Length != 10) throw line.Error(block.Command, "Unsupported plate record layout.");
                    if (f[8] != "1" && f[8] != "2" && f[8] != "3" && f[8] != "4") throw line.Error(block.Command, "Only thick/thin (1-4) plate subtypes are supported.");
                    var ids = new List<int> { IntId(f[4], line, block.Command), IntId(f[5], line, block.Command), IntId(f[6], line, block.Command) };
                    if (f[7] != "0") ids.Add(IntId(f[7], line, block.Command));
                    e.Nodes = ids.Concat(Enumerable.Repeat(0, 8 - ids.Count)).ToArray(); e.SubType = int.Parse(f[8], CultureInfo.InvariantCulture);
                    e.Angle = f.Length == 10 && f[9].Length != 0 ? Number(f[9], line, block.Command) : 0;
                }
                else
                {
                    // Walls, plane stress/strain, axisymmetric and solid elements: connectivity checked, element preserved.
                    int count = type == "SOLID" ? 8 : 4;
                    e.Nodes = f.Skip(4).Take(count).Where(v => v.Length != 0 && v != "0").Select(v => IntId(v, line, block.Command)).ToArray();
                    if (f.Length > 4 + count && int.TryParse(f[4 + count], NumberStyles.Integer, CultureInfo.InvariantCulture, out var sub)) e.SubType = sub;
                    if (type == "WALL")
                        Warn("GenWallPreserved", "*ELEMENT", "Wall elements (and their wall IDs and marks) are not represented by the GPC model: left out, records preserved.");
                }
                // The API pads NODE to eight entries with zeros.
                if (e.Nodes.Length < 8) e.Nodes = e.Nodes.Concat(Enumerable.Repeat(0, 8 - e.Nodes.Length)).ToArray();
                rows.Add(id, e); Tables.Record("ELEM", id, line.Record(block.Command)); ElementLines[id] = line.Text;
            }
        }

        private void Group(CivilTextBlock block)
        {
            HeaderOnly(block);
            var rows = Tables.Table<CivilNxModelProfile.GroupData>("GRUP"); Tables.TableRecord("GRUP", "*GROUP");
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 3, 4); var key = Key(rows);
                rows.Add(key, new CivilNxModelProfile.GroupData { Name = Required(f[0], line, block.Command),
                    Nodes = Expand(f[1], line, block.Command).Select(Int).ToArray(), Elements = Expand(f[2], line, block.Command).Select(Int).ToArray() });
                Tables.Record("GRUP", key, line.Record(block.Command));
            }
        }

        private void Cases(CivilTextBlock block)
        {
            HeaderOnly(block);
            var rows = Tables.Table<CivilNxModelProfile.CaseData>("STLD"); Tables.TableRecord("STLD", "*STLDCASE");
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 3, 3); var name = Required(f[0], line, block.Command);
                if (!caseNames.Add(name)) throw line.Error(block.Command, "Duplicate static case " + name);
                var key = Key(rows);
                rows.Add(key, new CivilNxModelProfile.CaseData { Number = rows.Count + 1, Name = name, Type = f[1], Description = f[2] }); Tables.Record("STLD", key, line.Record(block.Command));
            }
        }

        // ---- Loads ----

        private string Case(CivilTextBlock block)
        {
            if (activeCase == null) throw block.Header.Error(block.Command, "Missing *USE-STLD case selection.");
            return activeCase;
        }

        private void NodalLoads(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block); var loadCase = Case(block);
            var rows = Tables.Table<CivilNxModelProfile.ItemList<CivilNxModelProfile.NodalLoadItem>>("CNLD"); Tables.TableRecord("CNLD", "*CONLOAD");
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 7, 9); var v = new double[6];
                // NX 9.5.5 adds STRTYPENAME after GROUP. A nonempty scope needs its own mapping.
                if (f.Length == 9 && f[8].Length != 0) throw line.Error(block.Command, "Unsupported nodal load structural-type scope.");
                for (int i = 0; i < 6; i++) v[i] = Scaled(f[i + 1], i < 3 ? force : force * length, line, block.Command);
                foreach (var node in Expand(f[0], line, block.Command))
                    Item(rows, "CNLD", node, line, block, id => new CivilNxModelProfile.NodalLoadItem { Id = id, Case = loadCase, Group = f.Length > 7 ? f[7] : "", FX = v[0], FY = v[1], FZ = v[2], MX = v[3], MY = v[4], MZ = v[5] });
            }
        }

        private void SelfWeight(CivilTextBlock block)
        {
            HeaderOnly(block); var loadCase = Case(block);
            var rows = Tables.Table<CivilNxModelProfile.SelfWeightData>("BODF"); Tables.TableRecord("BODF", "*SELFWEIGHT");
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 3, 4); var key = Key(rows);
                rows.Add(key, new CivilNxModelProfile.SelfWeightData { Case = loadCase, Group = f.Length > 3 ? f[3] : "",
                    Factors = new[] { Number(f[0], line, block.Command), Number(f[1], line, block.Command), Number(f[2], line, block.Command) } });
                Tables.Record("BODF", key, line.Record(block.Command));
            }
        }

        /// <summary>ELEM_LIST, CMD, TYPE, DIR, bPROJ, bECCEN, ECCDIR, I-END, J-END, bJ-END, D1, P1, D2, P2, D3, P3, D4, P4, GROUP, bADDITIONAL, ... Distances
        /// are fractions of the element length; P in force, force/length, force x length or force x length/length for CON/UNI LOAD/MOMENT.</summary>
        private void BeamLoads(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block); var loadCase = Case(block);
            var rows = Tables.Table<CivilNxModelProfile.ItemList<CivilNxModelProfile.BeamLoadItem>>("BMLD"); Tables.TableRecord("BMLD", "*BEAMLOAD");
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 5, 30);
                var item = new CivilNxModelProfile.BeamLoadItem { Case = loadCase, Command = f[1].ToUpperInvariant(), Type = f[2].ToUpperInvariant(), Direction = f[3].ToUpperInvariant() };
                bool direction = item.Direction.Length == 2 && "LG".IndexOf(item.Direction[0]) >= 0 && "XYZ".IndexOf(item.Direction[1]) >= 0;
                if (!direction || f.Length < 18) item.Command = item.Command + "/" + item.Direction; // Other layouts (vector directions) are preserved by the reader.
                else
                {
                    item.Projected = Flag(f[4], line, block.Command); item.Eccentric = Flag(f[5], line, block.Command);
                    if (item.Eccentric != true)
                    {
                        if (f.Length < 23) throw line.Error(block.Command, "Unexpected beam load layout.");
                        double scale = item.Type == "CONLOAD" ? force : item.Type == "UNILOAD" ? force / length : item.Type == "CONMOMENT" ? force * length : force;
                        item.Distances = new[] { 10, 12, 14, 16 }.Select(i => Number(f[i], line, block.Command)).ToArray();
                        item.Values = new[] { 11, 13, 15, 17 }.Select(i => Scaled(f[i], scale, line, block.Command)).ToArray();
                        item.Additional = f.Length > 19 && f[19].Length != 0 && Flag(f[19], line, block.Command);
                        item.Group = f[18];
                    }
                }
                foreach (var element in Expand(f[0], line, block.Command))
                    Item(rows, "BMLD", element, line, block, id => new CivilNxModelProfile.BeamLoadItem { Id = id, Case = item.Case, Group = item.Group, Command = item.Command, Type = item.Type,
                        Direction = item.Direction, Projected = item.Projected, Eccentric = item.Eccentric, Additional = item.Additional, Distances = item.Distances, Values = item.Values });
            }
        }

        /// <summary>FACE: ELEM_LIST, CMD, ETYP, FACE, DIR, VX, VY, VZ, bPROJ, PU, P1, P2, P3, P4, GROUP, PSLTKEY; EDGE: ELEM_LIST, CMD, ETYP, EDGE, iEDGE, DIR, VX, VY, VZ, PU, P1, P2, GROUP.
        /// CMD HYDRO stores the hydrostatic pressure of every node in P1..P4, read like a varying pressure.</summary>
        private void Pressures(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block); var loadCase = Case(block);
            var rows = Tables.Table<CivilNxModelProfile.ItemList<CivilNxModelProfile.PressureItem>>("PRES"); Tables.TableRecord("PRES", "*PRESSURE");
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 5, 20);
                var item = new CivilNxModelProfile.PressureItem { Case = loadCase, Command = f[1].ToUpperInvariant(), ElementType = f[2].ToUpperInvariant(), FaceEdge = f[3].ToUpperInvariant() };
                if (item.ElementType == "PLATE" && item.FaceEdge == "FACE" && f.Length >= 15)
                {
                    item.Direction = f[4].ToUpperInvariant(); item.Vectors = new[] { 5, 6, 7 }.Select(i => Number(f[i], line, block.Command)).ToArray();
                    item.Projected = Flag(f[8], line, block.Command);
                    item.Forces = new[] { 9, 10, 11, 12, 13 }.Select(i => Scaled(f[i], force / (length * length), line, block.Command)).ToArray();
                    item.Group = f[14];
                }
                else if (item.ElementType == "PLATE" && item.FaceEdge == "EDGE" && f.Length >= 12)
                {
                    item.EdgeFace = IntId(f[4], line, block.Command); item.Direction = f[5].ToUpperInvariant();
                    item.Vectors = new[] { 6, 7, 8 }.Select(i => Number(f[i], line, block.Command)).ToArray(); item.Projected = false;
                    item.EdgeLoads = new[] { 9, 10, 11 }.Select(i => Scaled(f[i], force / length, line, block.Command)).ToArray();
                    item.Group = f.Length > 12 ? f[12] : "";
                }
                else item.ElementType = item.ElementType + "/" + item.FaceEdge; // Plane, solid and unknown layouts are preserved by the reader.
                foreach (var element in Expand(f[0], line, block.Command))
                    Item(rows, "PRES", element, line, block, id => new CivilNxModelProfile.PressureItem { Id = id, Case = item.Case, Group = item.Group, Command = item.Command,
                        ElementType = item.ElementType, FaceEdge = item.FaceEdge,
                        Direction = item.Direction, Vectors = item.Vectors, Projected = item.Projected, EdgeFace = item.EdgeFace, Forces = item.Forces, EdgeLoads = item.EdgeLoads });
            }
        }

        /// <summary>Line 1 NAME=NAME, KIND, ACTIVE, bES, iTYPE, DESC, iSERV-TYPE, nLCOMTYPE, nSEISTYPE[, LcomFactor]; next lines ANAL, LCNAME, FACTOR repeated.</summary>
        private void Combinations(CivilTextBlock block)
        {
            HeaderOnly(block);
            foreach (var lines in Records(block, f => f[0].StartsWith("NAME=", StringComparison.OrdinalIgnoreCase)))
            {
                var head = lines[0]; var f = Fields(block, head, 5, 12);
                var name = Required(f[0].Substring(5).Trim(), head, block.Command);
                if (!CombinationTables.TryGetValue(f[1].ToUpperInvariant(), out var table)) throw head.Error(block.Command, "Unknown combination kind " + f[1]);
                if (f.Length > 9 && f[9].Length != 0 && Number(f[9], head, block.Command) != 1)
                    Warn("CivilCombinationFactorPreserved", head.Record(block.Command), "The combination factor of the line (LcomFactor) is preserved only; the term factors are imported unchanged.");
                var terms = new List<CivilNxModelProfile.CombinationTermData>();
                foreach (var line in lines.Skip(1))
                {
                    var t = MidasCivilTextSyntax.Fields(line.Text, line.Record(block.Command)).ToList();
                    while (t.Count > 0 && t[t.Count - 1].Length == 0) t.RemoveAt(t.Count - 1);
                    if (t.Count == 0 || t.Count % 3 != 0) throw line.Error(block.Command, "Expected ANAL, LCNAME, FACTOR triples.");
                    for (int i = 0; i < t.Count; i += 3)
                        terms.Add(new CivilNxModelProfile.CombinationTermData { Analysis = t[i].ToUpperInvariant(), Case = Required(t[i + 1], line, block.Command), Factor = Number(t[i + 2], line, block.Command) });
                }
                var rows = Tables.Table<CivilNxModelProfile.CombinationData>(table); Tables.TableRecord(table, "*LOADCOMB");
                if (rows.Values.Any(c => c.Name == name)) throw head.Error(block.Command, "Duplicate combination " + name);
                var key = Key(rows);
                rows.Add(key, new CivilNxModelProfile.CombinationData { Name = name, Active = f[2], Type = (int)Number(f[4], head, block.Command), Terms = terms.ToArray() });
                Tables.Record(table, key, head.Record(block.Command));
            }
        }

        // ---- Properties ----

        /// <summary>iMAT, TYPE, MNAME, SPHEAT, HEATCO, PLAST, TUNIT, bMASS, DAMPRATIO, [DATA1]; SRC with two data blocks.
        /// [DATA1] 1, STANDARD, CODE, DB, USEELAST, ELAST or 2, ELAST, POISN, THERMAL, DEN (weight density), MASS or 3 (orthotropic).</summary>
        private void Materials(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block);
            var rows = Tables.Table<CivilNxModelProfile.MaterialData>("MATL"); Tables.TableRecord("MATL", "*MATERIAL");
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 10, 40); var id = Id(f[0], line, block.Command);
                if (rows.ContainsKey(id)) throw line.Error(block.Command, "Duplicate material ID " + id);
                double thermal = f[6].Equals("F", StringComparison.OrdinalIgnoreCase) ? 1.8 : 1;
                var data = new CivilNxModelProfile.MaterialData { Type = f[1].ToUpperInvariant(), Name = Required(f[2], line, block.Command),
                    SpecificHeat = Optional(f[3], line, block.Command), HeatConduction = Optional(f[4], line, block.Command),
                    MassDensity = IsFlag(f[7]) ? Flag(f[7], line, block.Command) : (bool?)null, Damping = Optional(f[8], line, block.Command) };
                var p = new CivilNxModelProfile.MaterialParameters();
                if (f[9] == "1" && f.Length >= 15)
                {
                    p.Type = 1; p.Standard = f[10]; p.Code = f[11]; p.Grade = f[12]; p.Elastic = Scaled(f[14], force / (length * length), line, block.Command);
                    p.UseElastic = IsFlag(f[13]) ? Flag(f[13], line, block.Command) : (bool?)null;
                }
                else if (f[9] == "2" && f.Length >= 14)
                {
                    p.Type = 2; p.Elastic = Scaled(f[10], force / (length * length), line, block.Command); p.Poisson = Number(f[11], line, block.Command);
                    p.Thermal = Number(f[12], line, block.Command) * thermal; p.WeightDensity = Scaled(f[13], force / (length * length * length), line, block.Command);
                }
                else
                {
                    p.Type = (int)Number(f[9], line, block.Command);
                    Warn("CivilMaterialDataPreserved", line.Record(block.Command), "Orthotropic or unknown material data are preserved only; no elastic value is mapped.");
                }
                data.Parameters = new[] { p };
                rows.Add(id, data); Tables.Record("MATL", id, line.Record(block.Command));
            }
        }

        /// <summary>iTHK, VALUE, NAME, bSAME, THIK-IN, THIK-OUT, bOFFSET, OFFTYPE, VALUE; stiffened plates on three lines.
        /// OFFTYPE 0 is an offset ratio of the thickness, 1 a length, as OFFSET 1 and 2 of the Civil NX database.</summary>
        private void Thicknesses(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block);
            var rows = Tables.Table<CivilNxModelProfile.ThicknessData>("THIK"); Tables.TableRecord("THIK", "*THICKNESS");
            foreach (var lines in Records(block, f => f.Length > 1 && IsId(f[0]) && (f[1].Equals("VALUE", StringComparison.OrdinalIgnoreCase) || f[1].Equals("STIFFENED", StringComparison.OrdinalIgnoreCase))))
            {
                var head = lines[0]; var f = MidasCivilTextSyntax.Fields(head.Text, head.Record(block.Command)); var id = Id(f[0], head, block.Command);
                if (rows.ContainsKey(id)) throw head.Error(block.Command, "Duplicate thickness ID " + id);
                var type = f[1].ToUpperInvariant();
                if (type == "VALUE" && (f.Length != 9 || lines.Count != 1))
                {
                    Warn("CivilThicknessLayoutPreserved", head.Record(block.Command), "Thickness record layout differs from the exported iTHK, VALUE, NAME, bSAME, THIK-IN, THIK-OUT, bOFFSET, OFFTYPE, VALUE: preserved only.");
                    continue;
                }
                var data = new CivilNxModelProfile.ThicknessData { Name = type == "VALUE" ? f[2] : null, Type = type };
                if (type == "VALUE")
                {
                    data.Separate = !Flag(f[3], head, block.Command); data.In = Scaled(f[4], length, head, block.Command); data.Out = Scaled(f[5], length, head, block.Command);
                    bool offset = Flag(f[6], head, block.Command); var kind = f[7];
                    if (offset && kind != "0" && kind != "1") throw head.Error(block.Command, "Unknown thickness offset type " + kind);
                    data.Offset = !offset ? 0 : kind == "0" ? 1 : 2;
                    data.OffsetValue = offset && kind == "1" ? Scaled(f[8], length, head, block.Command) : Number(f[8], head, block.Command);
                }
                rows.Add(id, data); Tables.Record("THIK", id, head.Record(block.Command));
            }
        }

        /// <summary>iSEC, TYPE, SNAME, [OFFSET] (OFFSET, iCENT, iREF, iHORZ, HUSER, iVERT, VUSER), bSD, bWE, SHAPE, data. One record per section,
        /// on one (DB/USER) or more lines (VALUE, COMPOSITE, TAPERED, SRC, PSC); a record starts with an ID and a section type.</summary>
        private void Sections(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block);
            var rows = Tables.Table<CivilNxModelProfile.SectionData>("SECT"); Tables.TableRecord("SECT", "*SECTION");
            foreach (var lines in Records(block, f => f.Length >= 12 && IsId(f[0]) && SectionTypes.Contains(f[1].ToUpperInvariant())))
            {
                var head = lines[0]; var record = head.Record(block.Command);
                var f = MidasCivilTextSyntax.Fields(head.Text, record); var id = Id(f[0], head, block.Command);
                if (rows.ContainsKey(id)) throw head.Error(block.Command, "Duplicate section ID " + id);
                var type = f[1].ToUpperInvariant();
                var before = new CivilNxModelProfile.SectionBefore { OffsetPoint = f[3].ToUpperInvariant(), OffsetCenter = (int)Number(f[4], head, block.Command),
                    HorizontalOption = (int)Number(f[6], head, block.Command), UserYI = Scaled(f[7], length, head, block.Command),
                    VerticalOption = (int)Number(f[8], head, block.Command), UserZI = Scaled(f[9], length, head, block.Command),
                    UserOffsetReference = (int)Number(f[5], head, block.Command),
                    ShearDeformation = IsFlag(f[10]) ? Flag(f[10], head, block.Command) : (bool?)null, Warping = IsFlag(f[11]) ? Flag(f[11], head, block.Command) : (bool?)null };
                var data = new CivilNxModelProfile.SectionData { SectionType = type, Name = Required(f[2], head, block.Command), Before = before };
                if (type == "DBUSER")
                {
                    if (lines.Count != 1 || f.Length < 14) { Warn("CivilSectionLayoutPreserved", record, "DB/User section layout not recognised: preserved only."); continue; }
                    before.Shape = f[12].ToUpperInvariant(); before.DataType = (int)Number(f[13], head, block.Command);
                    if (before.DataType == 1 && f.Length >= 16) before.First = new CivilNxModelProfile.SectionData1 { Database = f[14], SectionName = f[15] };
                    else if (before.DataType == 2)
                    {
                        bool pair = before.Shape == "2L" || before.Shape == "2C";
                        if (f.Length < (pair ? 19 : 15) || f.Length > 24 || !IsFlag(f[10]) || !IsFlag(f[11]))
                        {
                            Warn(pair ? "CivilPairUnresolved" : "CivilSectionLayoutPreserved", record, "Only the native-tested explicit User dimension layout is mapped.");
                            continue;
                        }
                        var size = f.Skip(14).Select(v => Scaled(v, length, head, block.Command)).ToArray();
                        if (pair)
                            try { MidasIdenticalPairs.Definition(before.Shape, size, 1); }
                            catch (ArgumentException ex) { throw head.Error(block.Command, ex.Message); }
                        before.First = new CivilNxModelProfile.SectionData1 { Size = size };
                    }
                    else { Warn("CivilSectionLayoutPreserved", record, "DB/User data type " + f[13] + " is preserved only."); continue; }
                }
                else if (type == "VALUE")
                {
                    // Line 2: AREA, ASy, ASz, Ixx, Iyy, Izz; lines 3-4 stress points and shear factors, preserved.
                    if (lines.Count != 4) throw head.Error(block.Command, "VALUE sections require four lines.");
                    var k = Fields(block, lines[1], 6, 6).Select(v => Number(v, lines[1], block.Command)).ToArray();
                    before.Shape = f[12].ToUpperInvariant();
                    before.First = new CivilNxModelProfile.SectionData1 { Stiffness = new CivilNxModelProfile.SectionStiffness { Area = k[0] * length * length,
                        Ixx = k[3] * Math.Pow(length, 4), Iyy = k[4] * Math.Pow(length, 4), Izz = k[5] * Math.Pow(length, 4) } };
                }
                else if (type == "COMPOSITE")
                {
                    // Line 1 ..., bHUMBLY, SHAPE; I: line 2 Hw, tw, B1, tf1, B2, tf2; stiffener lines; last line SW, GN, CTC, Bc, Tc, Hh, ...
                    before.Shape = f.Length > 13 ? f[13].ToUpperInvariant() : null;
                    if (before.Shape == "I" && lines.Count >= 3)
                    {
                        var size = Fields(block, lines[1], 6, 6).Select(v => Scaled(v, length, lines[1], block.Command)).ToArray();
                        var slab = Fields(block, lines[lines.Count - 1], 6, 20);
                        bool stiffened = lines.Skip(2).Take(lines.Count - 3).Any(l => MidasCivilTextSyntax.Fields(l.Text, l.Record(block.Command)).Any(v => v.Length != 0 && v != "0"));
                        before.First = new CivilNxModelProfile.SectionData1 { Size = size, Stiffener = new CivilNxModelProfile.SectionStiffener { Counts = new[] { stiffened ? 1 : 0 } } };
                        data.After = new CivilNxModelProfile.SectionAfter { Slab = new[] { Scaled(slab[3], length, lines[lines.Count - 1], block.Command),
                            Scaled(slab[4], length, lines[lines.Count - 1], block.Command), Scaled(slab[5], length, lines[lines.Count - 1], block.Command) } };
                    }
                }
                else before.Shape = f[12].ToUpperInvariant();
                rows.Add(id, data); Tables.Record("SECT", id, record);
            }
        }

        // ---- Boundaries ----

        private void Constraints(CivilTextBlock block)
        {
            HeaderOnly(block);
            var rows = Tables.Table<CivilNxModelProfile.ItemList<CivilNxModelProfile.SupportItem>>("CONS"); Tables.TableRecord("CONS", "*CONSTRAINT");
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 2, 3);
                if ((f[1].Length != 6 && f[1].Length != 7) || f[1].Any(c => c != '0' && c != '1')) throw line.Error(block.Command, "Expected six binary DOF flags DX,DY,DZ,RX,RY,RZ (and RW).");
                foreach (var node in Expand(f[0], line, block.Command))
                    Item(rows, "CONS", node, line, block, id => new CivilNxModelProfile.SupportItem { Id = id, Group = f.Length > 2 ? f[2] : "", Constraint = f[1] });
            }
        }

        /// <summary>NODE_LIST, iMETHOD, ANGLE-X, ANGLE-Y, ANGLE-Z (1); P0X..P2Z (2); V1X..V2Z (3).</summary>
        private void LocalAxes(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block);
            var rows = Tables.Table<CivilNxModelProfile.SkewData>("SKEW"); Tables.TableRecord("SKEW", "*" + block.Command);
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 5, 11); var method = (int)Number(f[1], line, block.Command);
                var v = f.Skip(2).Select(x => Number(x, line, block.Command)).ToArray();
                var data = new CivilNxModelProfile.SkewData { Method = method };
                if (method == 2 && v.Length == 9)
                { data.P0X = v[0]; data.P0Y = v[1]; data.P0Z = v[2]; data.P1X = v[3]; data.P1Y = v[4]; data.P1Z = v[5]; data.P2X = v[6]; data.P2Y = v[7]; data.P2Z = v[8]; }
                else if (method == 3 && v.Length == 6)
                { data.V1X = v[0]; data.V1Y = v[1]; data.V1Z = v[2]; data.V2X = v[3]; data.V2Y = v[4]; data.V2Z = v[5]; }
                else if (method != 1 || v.Length != 3) throw line.Error(block.Command, "Unknown node local axis layout.");
                foreach (var node in Expand(f[0], line, block.Command))
                {
                    if (rows.ContainsKey(node)) throw line.Error(block.Command, "Duplicate local axes of node " + node);
                    rows.Add(node, data); Tables.Record("SKEW", node, line.Record(block.Command));
                }
            }
        }

        /// <summary>ELEM_LIST, bVALUE, FLAG-i, Fxi, Fyi, Fzi, Mxi, Myi, Mzi (line 1); FLAG-j, Fxj, ..., Mzj, GROUP (line 2). Values are fixity ratios, or
        /// stiffnesses (force/length, force x length/rad) with bVALUE.</summary>
        private void Releases(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block);
            var rows = Tables.Table<CivilNxModelProfile.ItemList<CivilNxModelProfile.ReleaseData>>("FRLS"); Tables.TableRecord("FRLS", "*FRAME-RLS");
            for (int i = 0; i < block.Lines.Count; i += 2)
            {
                var first = block.Lines[i];
                if (i + 1 >= block.Lines.Count) throw first.Error(block.Command, "Release record without its second (J end) line.");
                var second = block.Lines[i + 1];
                var a = Fields(block, first, 9, 10); var b = Fields(block, second, 8, 9);
                if (!IsFlag(a[1])) throw first.Error(block.Command, "Expected ELEM_LIST, bVALUE, FLAG-i, values.");
                bool absolute = Flag(a[1], first, block.Command); int count = a.Length - 3;
                double[] Values(string[] g, int start, CivilTextLine line) => g.Skip(start).Take(count).Select((v, k) => absolute
                    ? Scaled(v, k < 3 ? force / length : force * length, line, block.Command) : Number(v, line, block.Command)).ToArray();
                var release = new CivilNxModelProfile.ReleaseData { Absolute = absolute, FlagI = a[2], ValuesI = Values(a, 3, first), FlagJ = b[0], ValuesJ = Values(b, 1, second),
                    Group = b.Length > count + 1 ? b[count + 1] : "" };
                foreach (var element in Expand(a[0], first, block.Command)) Item(rows, "FRLS", element, first, block, id => release);
            }
        }

        /// <summary>ELEM_LIST, GLOBAL, RGDXi, RGDYi, RGDZi, RGDXj, RGDYj, RGDZj, GROUP; ELEM_LIST, ELEMENT, RGDyi, RGDzi, RGDyj, RGDzj, GROUP.</summary>
        private void Offsets(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block);
            var rows = Tables.Table<CivilNxModelProfile.ItemList<CivilNxModelProfile.OffsetItem>>("OFFS"); Tables.TableRecord("OFFS", "*OFFSET");
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 6, 9); var type = f[1].ToUpperInvariant(); double L(int i) => Scaled(f[i], length, line, block.Command);
                CivilNxModelProfile.OffsetItem item;
                if (type == "GLOBAL" && f.Length >= 8) item = new CivilNxModelProfile.OffsetItem { Type = type, Xi = L(2), Yi = L(3), Zi = L(4), Xj = L(5), Yj = L(6), Zj = L(7) };
                else if (type == "ELEMENT") item = new CivilNxModelProfile.OffsetItem { Type = type, Yi = L(2), Zi = L(3), Yj = L(4), Zj = L(5) };
                else throw line.Error(block.Command, "Unknown beam end offset type " + f[1]);
                foreach (var element in Expand(f[0], line, block.Command)) Item(rows, "OFFS", element, line, block, id => item);
            }
        }

        /// <summary>LINEAR: NODE_LIST, LINEAR, F_SDx..F_SRz (fixed), SDx..SRz, DAMPING, Cx..CRz, GROUP, [EFFAREA, Kx, Ky, Kz]; other laws preserved.</summary>
        private void Springs(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block);
            var rows = Tables.Table<CivilNxModelProfile.ItemList<CivilNxModelProfile.SpringItem>>("SPRING"); Tables.TableRecord("SPRING", "*SPRING");
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 3, 40); var type = f[1].ToUpperInvariant();
                var item = new CivilNxModelProfile.SpringItem { Type = type, Group = type == "LINEAR" && f.Length > 21 ? f[21] : null };
                if (type == "LINEAR")
                {
                    if (f.Length < 22) throw line.Error(block.Command, "Unexpected linear spring layout.");
                    item.Fixed = Enumerable.Range(2, 6).Select(i => Flag(f[i], line, block.Command)).ToArray();
                    item.Stiffness = Enumerable.Range(8, 6).Select(i => Scaled(f[i], i < 11 ? force / length : force * length, line, block.Command)).ToArray();
                    item.Damping = Flag(f[14], line, block.Command) && Enumerable.Range(15, 6).Any(i => Number(f[i], line, block.Command) != 0);
                    item.Extra = f.Skip(22).Where(v => v.Length != 0).Select(v => Number(v, line, block.Command)).ToArray();
                }
                foreach (var node in Expand(f[0], line, block.Command)) Item(rows, "SPRING", node, line, block, id => item);
            }
        }

        /// <summary>GEN: iNO, iNODE1, iNODE2, GEN, ANGLE, R_SDx..R_SRz (rigid), SDx..SRz, bSHEAR, DRy, DRz, GROUP; RIGID/SADDLE: ..., ANGLE, bSHEAR, DRy, DRz, GROUP.</summary>
        private void ElasticLinks(CivilTextBlock block)
        {
            HeaderOnly(block); RequireUnits(block);
            var rows = Tables.Table<CivilNxModelProfile.ElasticData>("ELNK"); Tables.TableRecord("ELNK", "*ELASTICLINK");
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 5, 30); var id = Id(f[0], line, block.Command);
                if (rows.ContainsKey(id)) throw line.Error(block.Command, "Duplicate elastic link " + id);
                var data = new CivilNxModelProfile.ElasticData { Nodes = new[] { IntId(f[1], line, block.Command), IntId(f[2], line, block.Command) }, Kind = f[3].ToUpperInvariant(),
                    Angle = Number(f[4], line, block.Command) };
                if (data.Kind == "GEN")
                {
                    if (f.Length < 18) throw line.Error(block.Command, "Unexpected general link layout.");
                    data.Rigid = Enumerable.Range(5, 6).Select(i => Flag(f[i], line, block.Command)).ToArray();
                    data.Stiffness = Enumerable.Range(11, 6).Select(i => Scaled(f[i], i < 14 ? force / length : force * length, line, block.Command)).ToArray();
                    data.Shear = Flag(f[17], line, block.Command);
                }
                else if ((data.Kind == "RIGID" || data.Kind == "SADDLE") && f.Length > 5) data.Shear = Flag(f[5], line, block.Command);
                rows.Add(id, data); Tables.Record("ELNK", id, line.Record(block.Command));
            }
        }

        /// <summary>M-NODE, DOF, S-NODE LIST, GROUP.</summary>
        private void RigidLinks(CivilTextBlock block)
        {
            HeaderOnly(block);
            var rows = Tables.Table<CivilNxModelProfile.ItemList<CivilNxModelProfile.RigidData>>("RIGD"); Tables.TableRecord("RIGD", "*RIGIDLINK");
            foreach (var line in block.Lines)
            {
                var f = Fields(block, line, 3, 4); var master = Id(f[0], line, block.Command);
                if (f[1].Length != 6 || f[1].Any(c => c != '0' && c != '1')) throw line.Error(block.Command, "Expected six binary DOF flags.");
                var slaves = Expand(f[2], line, block.Command).Select(Int).ToArray(); int dofs = int.Parse(f[1], CultureInfo.InvariantCulture);
                Item(rows, "RIGD", master, line, block, id => new CivilNxModelProfile.RigidData { Id = id, Dofs = dofs, Nodes = slaves });
            }
        }

        /// <summary>NAME=NAME, DURATION, bSAVESTAGE, bSAVESTEP, bINCRESTEP, INCRESTEP; then STEP=, AELEM=, DELEM=, ABNDR=, DBNDR=, ALOAD=, DLOAD= lines.</summary>
        private void Stages(CivilTextBlock block)
        {
            HeaderOnly(block);
            var rows = Tables.Table<CivilNxModelProfile.StageData>("STAG"); Tables.TableRecord("STAG", "*STAGE");
            foreach (var lines in Records(block, f => f[0].StartsWith("NAME=", StringComparison.OrdinalIgnoreCase)))
            {
                var head = lines[0]; var f = Fields(block, head, 1, 6);
                var stage = new CivilNxModelProfile.StageData { Name = Required(f[0].Substring(5).Trim(), head, block.Command) };
                if (f.Length >= 2 && f[1].Length != 0) stage.Duration = Number(f[1], head, block.Command);
                if (f.Length >= 3) stage.SaveResults = Flag(f[2], head, block.Command);
                if (f.Length >= 4) stage.SaveSteps = Flag(f[3], head, block.Command);
                if (f.Length >= 5) stage.SaveLoadSteps = Flag(f[4], head, block.Command);
                if (f.Length >= 6) stage.LoadSteps = (int)Number(f[5], head, block.Command);
                // KEY=values lines; MIDAS wraps a long list on the following lines without a KEY (and without a backslash).
                var entries = new List<Tuple<string, List<string>, CivilTextLine>>();
                foreach (var line in lines.Skip(1))
                {
                    var values = MidasCivilTextSyntax.Fields(line.Text, line.Record(block.Command)).ToList();
                    while (values.Count > 0 && values[values.Count - 1].Length == 0) values.RemoveAt(values.Count - 1);
                    int equal = values.Count == 0 ? -1 : values[0].IndexOf('=');
                    if (equal > 0)
                    {
                        var name = values[0].Substring(0, equal).Trim().ToUpperInvariant(); values[0] = values[0].Substring(equal + 1).Trim();
                        entries.Add(Tuple.Create(name, values, line));
                    }
                    else if (entries.Count != 0 && equal < 0) entries[entries.Count - 1].Item2.AddRange(values);
                    else throw line.Error(block.Command, "Expected KEY=values.");
                }
                foreach (var entry in entries)
                {
                    var key = entry.Item1; var g = entry.Item2; var line = entry.Item3;
                    if (g.Count == 1 && g[0].Length == 0) continue;
                    List<Tuple<string, string>> Pairs()
                    {
                        if (g.Count % 2 != 0) throw line.Error(block.Command, "Expected name, value pairs.");
                        return Enumerable.Range(0, g.Count / 2).Select(i => Tuple.Create(Required(g[2 * i], line, block.Command), g[2 * i + 1])).ToList();
                    }
                    switch (key)
                    {
                        case "STEP": stage.AdditionalDays = g.Select(v => Number(v, line, block.Command)).ToArray(); break;
                        case "AELEM": stage.ActivateElements = Pairs().Select(p => new CivilNxModelProfile.ElementActivation { Group = p.Item1, Age = Number(p.Item2, line, block.Command) }).ToArray(); break;
                        case "DELEM": stage.DeactivateElements = Pairs().Select(p => new CivilNxModelProfile.ElementDeactivation { Group = p.Item1, Redistribution = Number(p.Item2, line, block.Command) }).ToArray(); break;
                        case "ABNDR": stage.ActivateBoundaries = Pairs().Select(p => new CivilNxModelProfile.BoundaryActivation { Group = p.Item1, Position = p.Item2.ToUpperInvariant() }).ToArray(); break;
                        case "DBNDR": stage.DeactivateBoundaries = g.Select(v => Required(v, line, block.Command)).ToArray(); break;
                        case "ALOAD": stage.ActivateLoads = Pairs().Select(p => new CivilNxModelProfile.LoadActivation { Group = p.Item1, Day = p.Item2.ToUpperInvariant() }).ToArray(); break;
                        case "DLOAD": stage.DeactivateLoads = Pairs().Select(p => new CivilNxModelProfile.LoadActivation { Group = p.Item1, Day = p.Item2.ToUpperInvariant() }).ToArray(); break;
                        default: Warn("CivilStageDataPreserved", line.Record(block.Command), "Stage data " + key + " is preserved only."); break;
                    }
                }
                var id = (rows.Count + 1).ToString(CultureInfo.InvariantCulture);
                rows.Add(id, stage); Tables.Record("STAG", id, head.Record(block.Command));
            }
        }

        /// <summary>MIDAS Gen stories: NAME=NAME, LEVEL, bFLDIAP, ... A rigid floor diaphragm couples the in-plane displacements of the floor nodes.</summary>
        private void Stories(CivilTextBlock block)
        {
            HeaderOnly(block);
            foreach (var lines in Records(block, f => f[0].StartsWith("NAME=", StringComparison.OrdinalIgnoreCase)))
            {
                var f = Fields(block, lines[0], 3, 40);
                if (Flag(f[2], lines[0], block.Command))
                    Warn("GenFloorDiaphragmPreserved", lines[0].Record(block.Command), "Story " + f[0].Substring(5).Trim() + " (level " + f[1]
                        + "): the rigid floor diaphragm is not represented; the in-plane coupling of its nodes is missing from the GPC model.");
            }
        }

        // ---- Helpers ----

        /// <summary>Row key 1, 2, ... of a table without source IDs, as the API numbers STLD, GRUP, BODF and LCOM rows.</summary>
        private static string Key<T>(Dictionary<string, T> rows) => (rows.Count + 1).ToString(CultureInfo.InvariantCulture);

        private static double? Optional(string text, CivilTextLine line, string command) => text.Length == 0 ? (double?)null : Number(text, line, command);

        private void Item<T>(Dictionary<string, CivilNxModelProfile.ItemList<T>> rows, string table, string key, CivilTextLine line, CivilTextBlock block, Func<int, T> create)
        {
            if (!rows.TryGetValue(key, out var list)) rows.Add(key, list = new CivilNxModelProfile.ItemList<T> { Items = new T[0] });
            int id = list.Items.Length + 1; // Item IDs 1, 2, ... of each row, as the API.
            list.Items = list.Items.Concat(new[] { create(id) }).ToArray();
            Tables.Record(table, key, line.Record(block.Command)); Tables.Record(table, key + "/" + id.ToString(CultureInfo.InvariantCulture), line.Record(block.Command));
        }

        /// <summary>Lines grouped into records: a record starts at a line for which <paramref name="starts"/> holds.</summary>
        private static List<List<CivilTextLine>> Records(CivilTextBlock block, Func<string[], bool> starts)
        {
            var result = new List<List<CivilTextLine>>();
            foreach (var line in block.Lines)
            {
                var f = MidasCivilTextSyntax.Fields(line.Text, line.Record(block.Command));
                if (starts(f)) result.Add(new List<CivilTextLine> { line });
                else if (result.Count == 0) throw line.Error(block.Command, "Continuation line before the first record.");
                else result[result.Count - 1].Add(line);
            }
            return result;
        }

        private void Warn(string code, string record, string message) => Diagnostics.Add(new ModelDiagnostic { Code = code, Record = record, Severity = DiagnosticSeverity.Warning, Message = message });
        private static bool IsId(string text) => int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var v) && v > 0;
        private static bool IsFlag(string text) => text.Equals("YES", StringComparison.OrdinalIgnoreCase) || text.Equals("NO", StringComparison.OrdinalIgnoreCase);
        private static bool Flag(string text, CivilTextLine line, string command)
        {
            if (text.Equals("YES", StringComparison.OrdinalIgnoreCase)) return true;
            if (text.Equals("NO", StringComparison.OrdinalIgnoreCase)) return false;
            throw line.Error(command, "Expected YES or NO: " + text);
        }
        private static int Int(string id) => int.Parse(id, CultureInfo.InvariantCulture);
        private static int IntId(string text, CivilTextLine line, string command) => Int(Id(text, line, command));
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
        private void RequireUnits(CivilTextBlock block)
        { if (length == 0) throw block.Header.Error(block.Command, "Explicit *UNIT required before dimensional data."); }
        private static string Required(string value, CivilTextLine line, string command)
        { if (string.IsNullOrWhiteSpace(value)) throw line.Error(command, "Missing required value."); return value; }
        internal static string Id(string text, CivilTextLine line, string command)
        {
            if (!int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value) || value <= 0)
                throw line.Error(command, "Expected a positive integer ID: " + text);
            return value.ToString(CultureInfo.InvariantCulture);
        }
        internal static double Number(string text, CivilTextLine line, string command)
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
        private string[] Expand(string list, CivilTextLine line, string command)
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
    }
}
