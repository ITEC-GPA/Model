using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using GPC.Geometry;
using GPC.Model.Analysis;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;
using GPC.Model.Core.Identity;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.CivilNx
{
    /// <summary>MIDAS Civil NX model profile: nodes, beams/trusses, plates, element axes, groups, static cases, materials, sections,
    /// thicknesses, beam end offsets, supports, nodal/beam/pressure loads. Database values are in the model UNIT; every
    /// response is preserved. Releases, springs, links, stages and combinations are preserved only.
    /// Conventions checked on a real Civil NX model through the API (October 2026): plate axes are the ECS of the Analysis
    /// Manual rotated by ELEM ANGLE about z; empty databases answer {"message":""}.</summary>
    public sealed class CivilNxModelProfile : ICivilNxGeometryProfile
    {
        public const string Program = "MIDAS Civil NX";
        public static IReadOnlyList<string> RequiredTables { get; } = new[] { "UNIT", "NODE", "ELEM" };
        public static IReadOnlyList<string> OptionalTables { get; } = new[]
            { "MATL", "SECT", "THIK", "GRUP", "SKEW", "CONS", "OFFS", "FRLS", "STLD", "CNLD", "BMLD", "PRES", "BODF", "NBOF",
              "LCOM-GEN", "LCOM-CONC", "LCOM-STEEL", "LCOM-SRC", "LCOM-STLCOMP", "LCOM-SEISMIC" };
        /// <summary>Combination databases and the ANAL label of their references in other combinations (e.g. CBC: a LCOM-CONC combination).</summary>
        private static readonly IReadOnlyDictionary<string, string> CombinationTables = new Dictionary<string, string>(StringComparer.Ordinal)
            { ["CB"] = "LCOM-GEN", ["CBC"] = "LCOM-CONC", ["CBS"] = "LCOM-STEEL", ["CBR"] = "LCOM-SRC", ["CBSC"] = "LCOM-STLCOMP", ["CBSM"] = "LCOM-SEISMIC" };
        /// <summary>Optional post/TABLE response with computed section properties, used for sections without a usable outline.</summary>
        public const string SectionTable = "SECTIONALL";
        /// <summary>Optional post/TABLE response with the material data used by the analysis (weight density also for code-database grades).</summary>
        public const string MaterialTable = "MATERIAL";

        public string Id => "civil-nx/model/v1";
        public IReadOnlyDictionary<string, CapabilityStatus> Capabilities { get; } = new ReadOnlyDictionary<string, CapabilityStatus>(new Dictionary<string, CapabilityStatus>
        {
            ["Nodes, beams, trusses, plates"] = CapabilityStatus.ImplementedSyntheticTests,
            ["Beam beta axes and plate axes with ANGLE"] = CapabilityStatus.ImplementedSyntheticTests,
            ["Materials (code grades), DB/user sections, thickness"] = CapabilityStatus.ImplementedSyntheticTests,
            ["Section offset points, beam end offsets"] = CapabilityStatus.InterpretedWithoutRealFileTest,
            ["Groups, static cases, supports, nodal/beam/uniform pressure loads"] = CapabilityStatus.ImplementedSyntheticTests,
            ["Plate edge loads"] = CapabilityStatus.InterpretedWithoutRealFileTest,
            ["Non-uniform pressures, releases, springs, links, stages, combinations, self weight"] = CapabilityStatus.PreservedOnly,
        });

        public ImportBatch Read(CivilNxSnapshot snapshot, AnalysisSource identity, CancellationToken cancellationToken)
        {
            if (identity.Program != Program || string.IsNullOrWhiteSpace(identity.ModelRevision)) throw new ArgumentException("ExplicitCivilNxModelIdentityRequired");
            foreach (var key in RequiredTables) if (!snapshot.Responses.ContainsKey(key)) throw new InvalidDataException("MissingCivilNxDatabase: " + key);
            return new Reader(snapshot, identity, cancellationToken).Read();
        }

        private sealed class Reader
        {
            private readonly CivilNxSnapshot snapshot;
            private readonly CancellationToken token;
            private readonly ImportBatch batch;
            private double force, length, thermal = 1;
            private readonly Dictionary<string, Point3d> points = new Dictionary<string, Point3d>(StringComparer.Ordinal);
            private readonly Dictionary<string, CoordinateSystem> nodeAxes = new Dictionary<string, CoordinateSystem>(StringComparer.Ordinal);
            private readonly HashSet<string> unresolvedNodeAxes = new HashSet<string>(StringComparer.Ordinal);
            private readonly Dictionary<string, BeamRecord> beams = new Dictionary<string, BeamRecord>(StringComparer.Ordinal);
            private readonly Dictionary<string, ShellRecord> shells = new Dictionary<string, ShellRecord>(StringComparer.Ordinal);
            private readonly Dictionary<string, int> warnings = new Dictionary<string, int>(StringComparer.Ordinal);

            public Reader(CivilNxSnapshot snapshot, AnalysisSource identity, CancellationToken token)
            {
                this.snapshot = snapshot; this.token = token;
                batch = new ImportBatch { Program = identity.Program, SolverVersion = identity.SolverVersion, ModelRevision = identity.ModelRevision, AnalysisId = identity.AnalysisId };
            }

            public ImportBatch Read()
            {
                Units(); Skews(); Nodes();
                var thicknesses = Thicknesses(); var sections = Sections(); Materials();
                Elements(sections, thicknesses); Offsets(); Groups(); Cases(); Supports(); NodalLoads(); BeamLoads(); Pressures(); SelfWeight(); Combinations();
                foreach (var table in new[] { "FRLS", "NBOF" })
                    if (Has(table)) Warn("CivilNx" + table + "Preserved", "db/" + table, table == "FRLS" ? "Beam end releases are preserved, not mapped."
                        : "Nodal body forces (masses times factors, e.g. seismic inertia) are preserved, not converted to loads.");
                foreach (var missing in snapshot.Unavailable) Warn("CivilNxTableUnavailable", missing, "The API did not return this database; its data is not in the model.");
                foreach (var w in warnings) batch.Diagnostics.Add(new ModelDiagnostic { Code = w.Key.Split('\u001f')[0], Severity = DiagnosticSeverity.Warning,
                    Record = w.Key.Split('\u001f')[1], Message = w.Key.Split('\u001f')[2] + (w.Value > 1 ? " (" + w.Value.ToString(CultureInfo.InvariantCulture) + " records)" : "") });
                return batch;
            }

            private bool Has(string table) => snapshot.Responses.TryGetValue(table, out var response) && !CivilNxJson.IsEmptyDatabase(response.Json);
            private void Warn(string code, string record, string message)
            {
                var key = code + "\u001f" + record + "\u001f" + message;
                warnings[key] = warnings.TryGetValue(key, out var count) ? count + 1 : 1;
            }

            /// <summary>The records under the table's own key; none for an absent table or the API's empty answer {"message":""}.</summary>
            private Dictionary<string, T> Rows<T>(string name)
            {
                if (!snapshot.Responses.TryGetValue(name, out var response) || CivilNxJson.IsEmptyDatabase(response.Json)) return new Dictionary<string, T>();
                var root = CivilNxJson.Read<Dictionary<string, Dictionary<string, T>>>(response.Json);
                if (root == null || !root.TryGetValue(name, out var table)) throw new InvalidDataException("UnknownCivilNxSchema: " + name);
                return table ?? new Dictionary<string, T>();
            }

            private void Units()
            {
                var unit = Rows<UnitData>("UNIT");
                if (!unit.TryGetValue("1", out var u) || u == null) throw new NotSupportedException("UnknownCivilNxUnitSchema");
                force = MidasConventions.ForceFactor(u.Force) ?? throw new NotSupportedException("UnknownCivilNxForceUnit: " + u.Force);
                length = MidasConventions.LengthFactor(u.Distance) ?? throw new NotSupportedException("UnknownCivilNxLengthUnit: " + u.Distance);
                if (string.Equals(u.Temperature, "F", StringComparison.OrdinalIgnoreCase)) thermal = 1.8;
            }

            private void Skews()
            {
                foreach (var pair in Rows<SkewData>("SKEW"))
                {
                    var s = pair.Value; Vector3d v1 = null, v2 = null;
                    if (s?.Method == 3) { v1 = new Vector3d(s.V1X, s.V1Y, s.V1Z); v2 = new Vector3d(s.V2X, s.V2Y, s.V2Z); }
                    else if (s?.Method == 2) { v1 = new Vector3d(s.P1X - s.P0X, s.P1Y - s.P0Y, s.P1Z - s.P0Z); v2 = new Vector3d(s.P2X - s.P0X, s.P2Y - s.P0Y, s.P2Z - s.P0Z); }
                    CoordinateSystem axes = null;
                    if (v1 != null && Axes.Length(v1) > 1e-12)
                    {
                        var x = v1 / Axes.Length(v1); var z = x.CrossProduct(v2); var zl = Axes.Length(z);
                        if (zl > 1e-9) { z = z / zl; axes = new CoordinateSystem(Point3d.Origin, x, z.CrossProduct(x), z); }
                    }
                    if (axes == null)
                    {
                        unresolvedNodeAxes.Add(pair.Key);
                        Warn("CivilNxNodeAxesUnresolved", "SKEW/" + pair.Key, "Node local axes by angles or line vector are not mapped; supports on these nodes are preserved only.");
                    }
                    else nodeAxes.Add(pair.Key, axes);
                }
            }

            private void Nodes()
            {
                foreach (var pair in Rows<NodeData>("NODE"))
                {
                    token.ThrowIfCancellationRequested(); var n = pair.Value;
                    if (n?.X == null || n.Y == null || n.Z == null) throw new InvalidDataException("MissingCivilNxCoordinate: " + pair.Key);
                    var p = new Point3d(L(n.X.Value), L(n.Y.Value), L(n.Z.Value));
                    points.Add(pair.Key, p);
                    nodeAxes.TryGetValue(pair.Key, out var axes);
                    batch.Nodes.Add(new NodeRecord { Id = pair.Key, GlobalPosition = p, Record = "NODE/" + pair.Key,
                        CoordinateSystem = axes == null ? null : Frames.At(axes, p) });
                }
                if (points.Count == 0) throw new InvalidDataException("CivilNxModelWithoutNodes");
            }

            /// <summary>Rows of the MATERIAL table by material ID: weight density (force/length³), Poisson's ratio and modulus in the table units.</summary>
            private Dictionary<string, Func<string, double?>> AnalysisMaterials(out double tableForce, out double tableLength)
            {
                var result = new Dictionary<string, Func<string, double?>>(StringComparer.Ordinal); tableForce = force; tableLength = length;
                if (!snapshot.Responses.TryGetValue(MaterialTable, out var response)) return result;
                var tables = CivilNxJson.Read<Dictionary<string, CivilNxResultTableData>>(response.Json);
                var table = tables?.Values.FirstOrDefault(t => t?.Headers != null);
                if (table == null) return result;
                tableForce = MidasConventions.ForceFactor(table.ForceUnit) ?? throw new NotSupportedException("UnknownCivilNxForceUnit: " + table.ForceUnit);
                tableLength = MidasConventions.LengthFactor(table.LengthUnit) ?? throw new NotSupportedException("UnknownCivilNxLengthUnit: " + table.LengthUnit);
                for (int r = 0; r < table.Rows.Length; r++)
                {
                    int row = r;
                    result[table.Get(r, "ID")] = column => double.TryParse(table.Get(row, column), NumberStyles.Float, CultureInfo.InvariantCulture, out var v) ? v : (double?)null;
                }
                return result;
            }

            private void Materials()
            {
                var analysis = AnalysisMaterials(out var tableForce, out var tableLength);
                foreach (var pair in Rows<MaterialData>("MATL"))
                {
                    var m = pair.Value ?? throw new InvalidDataException("InvalidCivilNxMaterial: " + pair.Key);
                    var type = (m.Type ?? "").ToUpperInvariant();
                    var record = new MaterialRecord { Id = pair.Key, Name = m.Name, Record = "MATL/" + pair.Key,
                        Kind = type == "STEEL" ? MaterialKind.Steel : type == "CONC" ? MaterialKind.Concrete : type == "USER" ? MaterialKind.Unknown : MaterialKind.Other };
                    var p = m.Parameters?.FirstOrDefault();
                    if (record.Kind != MaterialKind.Other && p != null)
                    {
                        if (p.Type == 1) { record.Standard = p.Standard; record.Grade = string.IsNullOrWhiteSpace(p.Grade) ? null : p.Grade; }
                        if (p.Elastic.HasValue) record.ElasticModulus = p.Elastic.Value * force / (length * length);
                        if (p.Type == 2)
                        {
                            record.Poisson = p.Poisson; record.ThermalExpansion = p.Thermal * thermal;
                            if (p.WeightDensity.HasValue) record.Density = p.WeightDensity.Value * force / (length * length * length) / 9806.65;
                        }
                        if (analysis.TryGetValue(pair.Key, out var value))
                        {
                            // The weight density actually used by the analysis, as a mass density for the standard gravity.
                            var weight = value("Density"); var poisson = value("Poisson");
                            if (weight.HasValue) record.Density = weight.Value * tableForce / (tableLength * tableLength * tableLength) / 9806.65;
                            if (poisson.HasValue) record.Poisson = poisson;
                        }
                    }
                    if (record.Kind == MaterialKind.Other) Warn("CivilNxMaterialPreserved", record.Record, "Material type " + m.Type + " (" + m.Name + ") is not mapped.");
                    batch.Materials.Add(record);
                }
            }

            private HashSet<string> Thicknesses()
            {
                var supported = new HashSet<string>(StringComparer.Ordinal);
                foreach (var pair in Rows<ThicknessData>("THIK"))
                {
                    var t = pair.Value; var record = "THIK/" + pair.Key;
                    if (t == null || !string.Equals(t.Type, "VALUE", StringComparison.OrdinalIgnoreCase) || !t.In.HasValue)
                    { Warn("CivilNxThicknessPreserved", record, "Stiffened or incomplete thickness is not mapped; plates using it keep no property."); continue; }
                    double inPlane = L(t.In.Value); double? outOfPlane = t.Separate == true && t.Out.HasValue && t.Out.Value != t.In.Value ? L(t.Out.Value) : (double?)null;
                    double? offset = t.Offset == 1 ? t.OffsetValue * inPlane : t.Offset == 2 ? L(t.OffsetValue) : (double?)null;
                    batch.Thicknesses.Add(new ThicknessRecord { Id = pair.Key, Name = t.Name, InPlane = inPlane, OutOfPlane = outOfPlane, Offset = offset, Record = record });
                    supported.Add(pair.Key);
                }
                return supported;
            }

            private HashSet<string> Sections()
            {
                var values = SectionValues(); var defined = new HashSet<string>(StringComparer.Ordinal);
                foreach (var pair in Rows<SectionData>("SECT"))
                {
                    var s = pair.Value; var b = s?.Before; var record = "SECT/" + pair.Key;
                    var section = new SectionRecord { Id = pair.Key, Name = s?.Name, Record = record };
                    var type = (s?.SectionType ?? "").ToUpperInvariant();
                    if (b != null && type == "DBUSER")
                    {
                        if (b.DataType == 1) section.CatalogDesignation = b.First?.SectionName;
                        else if (b.DataType == 2) Shape(section, b.Shape, b.First?.Size);
                    }
                    else if (b != null && type == "VALUE" && b.First?.Stiffness?.Area > 0)
                    {
                        var k = b.First.Stiffness;
                        section.Values = new SectionValues { Area = A(k.Area.Value), I11 = I(k.Iyy ?? 0), I22 = I(k.Izz ?? 0), Torsion = I(k.Ixx ?? 0), Warping = 0 };
                    }
                    if (section.CatalogDesignation == null && section.Shape == SectionShapeKind.Unknown && section.Values == null && values.TryGetValue(pair.Key, out var computed) && type != "TAPERED")
                        section.Values = computed;
                    if (section.CatalogDesignation != null && values.TryGetValue(pair.Key, out var fallback)) section.Values = fallback;
                    if (type == "TAPERED") Warn("CivilNxTaperedSectionPreserved", record, "Tapered sections are not mapped; beams using them keep no property.");
                    else if (section.CatalogDesignation == null && section.Shape == SectionShapeKind.Unknown && section.Values == null)
                        Warn("CivilNxSectionPreserved", record, "Section type " + s?.SectionType + "/" + b?.Shape + " is not mapped; beams using it keep no property.");
                    section.Reference = Reference(b, record);
                    batch.Sections.Add(section); defined.Add(pair.Key);
                }
                return defined;
            }

            private Dictionary<string, SectionValues> SectionValues()
            {
                var result = new Dictionary<string, SectionValues>(StringComparer.Ordinal);
                if (!snapshot.Responses.TryGetValue(SectionTable, out var response)) return result;
                var tables = CivilNxJson.Read<Dictionary<string, CivilNxResultTableData>>(response.Json);
                var table = tables?.Values.FirstOrDefault(t => t?.Headers != null);
                if (table == null) return result;
                double l = MidasConventions.LengthFactor(table.LengthUnit) ?? length;
                for (int r = 0; r < table.Rows.Length; r++)
                {
                    double V(string column) => double.Parse(table.Get(r, column), NumberStyles.Float, CultureInfo.InvariantCulture);
                    result[table.Get(r, "ID")] = new SectionValues { Area = V("Area") * l * l, I11 = V("Iyy") * Math.Pow(l, 4), I22 = V("Izz") * Math.Pow(l, 4), Torsion = V("Ixx") * Math.Pow(l, 4) };
                }
                return result;
            }

            /// <summary>vSIZE of the DB/User page: H H,B1,tw,tf1,B2,tf2,r1; C H,B1,tw,tf1,B2,tf2; L/T H,B,tw,tf; B H,B,tw,tf1,C,tf2; P D,tw; SB H,B; SR D.</summary>
            private void Shape(SectionRecord section, string shape, double[] size)
            {
                var d = (size ?? new double[0]).Select(L).ToArray();
                double At(int i) => i < d.Length ? d[i] : 0;
                switch ((shape ?? "").ToUpperInvariant())
                {
                    case "H": section.Shape = SectionShapeKind.I; section.Dimensions = new[] { At(0), At(1), At(2), At(3), At(4), At(5), At(6) }; break;
                    case "C": section.Shape = SectionShapeKind.Channel; section.Dimensions = new[] { At(0), At(1), At(2), At(3), At(4), At(5) }; break;
                    case "L": section.Shape = SectionShapeKind.Angle; section.Dimensions = new[] { At(0), At(1), At(2), At(3) }; break;
                    case "T": section.Shape = SectionShapeKind.Tee; section.Dimensions = new[] { At(0), At(1), At(2), At(3) }; break;
                    case "P": section.Shape = SectionShapeKind.Pipe; section.Dimensions = new[] { At(0), At(1) }; break;
                    case "SB": section.Shape = SectionShapeKind.SolidRectangle; section.Dimensions = new[] { At(0), At(1) }; break;
                    case "SR": section.Shape = SectionShapeKind.SolidCircle; section.Dimensions = new[] { At(0) }; break;
                    case "B":
                        // C is the web spacing: only webs at the outer edges (C = B - tw) match a rectangular hollow section.
                        if (Math.Abs(At(4) - (At(1) - At(2))) <= 1e-6 * Math.Max(1, At(1)))
                        { section.Shape = SectionShapeKind.Box; section.Dimensions = new[] { At(0), At(1), At(2), At(3), At(5) }; }
                        break;
                }
            }

            private SectionReference Reference(SectionBefore b, string record)
            {
                if (b == null) return SectionReference.Centroid;
                var point = (b.OffsetPoint ?? "CC").ToUpperInvariant();
                if (point.Length != 2 || "LCR".IndexOf(point[0]) < 0 || "TCB".IndexOf(point[1]) < 0) throw new InvalidDataException("UnknownCivilNxOffsetPoint: " + point);
                bool user = (b.HorizontalOption == 1 && point[0] != 'C') || (b.VerticalOption == 1 && point[1] != 'C');
                if (user && ((b.UserYI ?? 0) != 0 || (b.UserZI ?? 0) != 0))
                {
                    Warn("CivilNxUserSectionOffsetUnresolved", record, "User-defined section offset distances are not mapped; the centroid position of these beams stays unresolved.");
                    return null;
                }
                var center = b.OffsetCenter == 1;
                return new SectionReference
                {
                    Horizontal = point[0] == 'L' ? SectionHorizontalReference.MinimumV1 : point[0] == 'R' ? SectionHorizontalReference.MaximumV1
                        : center ? SectionHorizontalReference.Center : SectionHorizontalReference.Centroid,
                    Vertical = point[1] == 'T' ? SectionVerticalReference.MaximumV2 : point[1] == 'B' ? SectionVerticalReference.MinimumV2
                        : center ? SectionVerticalReference.Center : SectionVerticalReference.Centroid
                };
            }

            private void Elements(HashSet<string> sections, HashSet<string> thicknesses)
            {
                foreach (var pair in Rows<ElementData>("ELEM"))
                {
                    token.ThrowIfCancellationRequested(); var e = pair.Value; var record = "ELEM/" + pair.Key;
                    if (e?.Nodes == null) throw new InvalidDataException("MissingCivilNxConnectivity: " + pair.Key);
                    var ids = e.Nodes.Where(n => n != 0).Select(n => n.ToString(CultureInfo.InvariantCulture)).ToArray();
                    if (ids.Any(id => !points.ContainsKey(id))) throw new InvalidDataException("MissingCivilNxNode: " + record);
                    var type = (e.Type ?? "").ToUpperInvariant();
                    string material = e.Material > 0 ? e.Material.Value.ToString(CultureInfo.InvariantCulture) : null;
                    string property = e.Section > 0 ? e.Section.Value.ToString(CultureInfo.InvariantCulture) : null;
                    if ((type == "BEAM" || type == "TRUSS" || (type == "TENSTR" && e.SubType == 1)) && ids.Length == 2)
                    {
                        CoordinateSystem axes;
                        try { axes = MidasConventions.BeamAxes(points[ids[0]], points[ids[1]], e.Angle ?? 0); }
                        catch (ArgumentException ex) { throw new ModelFileReadException(record, ex.Message); }
                        if (property != null && !sections.Contains(property)) throw new InvalidDataException("MissingCivilNxSection: " + record);
                        var beam = new BeamRecord { Id = pair.Key, I = ids[0], J = ids[1], CoordinateSystem = axes, MaterialId = material, SectionId = property, Record = record,
                            Formulation = type == "BEAM" ? BeamFormulation.StraightTwoNode : type == "TRUSS" ? BeamFormulation.Truss : BeamFormulation.TensionOnly };
                        batch.Beams.Add(beam); beams.Add(pair.Key, beam);
                    }
                    else if (type == "PLATE" && (ids.Length == 3 || ids.Length == 4))
                    {
                        var axes = MidasConventions.PlateAxes(ids.Select(id => points[id]).ToArray());
                        double angle = (e.Angle ?? 0) * Math.PI / 180;
                        if (angle != 0)
                        {
                            var x = axes.V1 * Math.Cos(angle) + axes.V2 * Math.Sin(angle);
                            axes = new CoordinateSystem(axes.Origin, x, axes.V3.CrossProduct(x), new Vector3d(axes.V3.X, axes.V3.Y, axes.V3.Z));
                        }
                        Axes.Validate(axes);
                        bool known = property != null && thicknesses.Contains(property);
                        var shell = new ShellRecord { Id = pair.Key, Nodes = ids, CoordinateSystem = axes, Record = record,
                            MaterialId = known ? material : null, ThicknessId = known ? property : null };
                        batch.Shells.Add(shell); shells.Add(pair.Key, shell);
                    }
                    else throw new NotSupportedException("UnsupportedCivilNxElementOrConnectivity: " + pair.Key + " (" + e.Type + ", " + ids.Length + " nodes)");
                }
            }

            private void Offsets()
            {
                foreach (var pair in Rows<ItemList<OffsetItem>>("OFFS"))
                {
                    var record = "OFFS/" + pair.Key; var items = pair.Value?.Items;
                    if (!beams.TryGetValue(pair.Key, out var beam)) throw new InvalidDataException("CivilNxOffsetWithoutBeam: " + record);
                    if (items == null || items.Length != 1) { Warn("CivilNxOffsetPreserved", record, "Several or no offset items on one beam are not mapped."); continue; }
                    var o = items[0];
                    if (string.Equals(o.Type, "GLOBAL", StringComparison.OrdinalIgnoreCase))
                    {
                        beam.OffsetI = new Vector3d(L(o.Xi), L(o.Yi), L(o.Zi)); beam.OffsetJ = new Vector3d(L(o.Xj), L(o.Yj), L(o.Zj)); beam.OffsetAxes = CoordinateSystem.Global;
                    }
                    else if (string.Equals(o.Type, "ELEMENT", StringComparison.OrdinalIgnoreCase) && o.Yi == o.Zi && o.Yj == o.Zj)
                    { beam.RigidLengthI = L(o.Yi); beam.RigidLengthJ = L(o.Yj); }
                    else Warn("CivilNxOffsetPreserved", record, "Element-type end offsets with different lengths in the two bending planes are not mapped.");
                }
            }

            private void Groups()
            {
                foreach (var pair in Rows<GroupData>("GRUP"))
                {
                    var g = pair.Value; var record = "GRUP/" + pair.Key;
                    if (string.IsNullOrWhiteSpace(g?.Name)) throw new InvalidDataException("InvalidCivilNxGroup: " + record);
                    var group = new GroupRecord { Name = g.Name, Record = record };
                    foreach (var n in g.Nodes ?? new int[0])
                    {
                        var id = n.ToString(CultureInfo.InvariantCulture);
                        if (!points.ContainsKey(id)) throw new InvalidDataException("MissingCivilNxGroupNode: " + record);
                        group.Members.Add(new SourceIdentity(Program, batch.ModelRevision, EntityFamily.Node, id));
                    }
                    foreach (var el in g.Elements ?? new int[0])
                    {
                        var id = el.ToString(CultureInfo.InvariantCulture);
                        var family = beams.ContainsKey(id) ? EntityFamily.Beam : shells.ContainsKey(id) ? EntityFamily.Shell : throw new InvalidDataException("MissingCivilNxGroupElement: " + record);
                        group.Members.Add(new SourceIdentity(Program, batch.ModelRevision, family, id));
                    }
                    batch.Groups.Add(group);
                }
            }

            private void Cases()
            {
                foreach (var pair in Rows<CaseData>("STLD"))
                {
                    if (string.IsNullOrWhiteSpace(pair.Value?.Name)) throw new InvalidDataException("InvalidCivilNxCase: STLD/" + pair.Key);
                    batch.LoadCases.Add(new LoadCaseRecord { Name = pair.Value.Name, Record = "STLD/" + pair.Key + " type " + pair.Value.Type });
                }
            }

            private void Supports()
            {
                foreach (var pair in Rows<ItemList<SupportItem>>("CONS"))
                {
                    var record = "CONS/" + pair.Key;
                    if (!points.ContainsKey(pair.Key)) throw new InvalidDataException("CivilNxSupportWithoutNode: " + record);
                    if (unresolvedNodeAxes.Contains(pair.Key)) continue;
                    var fixedDofs = new bool[6]; bool warping = false;
                    foreach (var item in pair.Value?.Items ?? new SupportItem[0])
                    {
                        var flags = item.Constraint ?? "";
                        if ((flags.Length != 6 && flags.Length != 7) || flags.Any(c => c != '0' && c != '1')) throw new InvalidDataException("InvalidCivilNxConstraint: " + record);
                        for (int i = 0; i < 6; i++) fixedDofs[i] |= flags[i] == '1';
                        warping |= flags.Length == 7 && flags[6] == '1';
                    }
                    if (warping) Warn("CivilNxWarpingRestraintIgnored", "CONS", "Warping (RW) restraints are not represented by the six-DOF support.");
                    nodeAxes.TryGetValue(pair.Key, out var axes);
                    batch.NodeRestrains.Add(new NodeRestrainRecord { NodeId = pair.Key, FixedDofs = fixedDofs, Record = record,
                        CoordinateSystem = axes == null ? CoordinateSystem.Global : Frames.At(axes, points[pair.Key]) });
                }
            }

            private void NodalLoads()
            {
                foreach (var pair in Rows<ItemList<NodalLoadItem>>("CNLD"))
                {
                    var record = "CNLD/" + pair.Key;
                    if (!points.ContainsKey(pair.Key)) throw new InvalidDataException("CivilNxLoadWithoutNode: " + record);
                    if (nodeAxes.ContainsKey(pair.Key) || unresolvedNodeAxes.Contains(pair.Key))
                        Warn("CivilNxNodalLoadOnSkewNode", "CNLD", "Nodal loads on nodes with local axes are taken in GCS, as stored by the API.");
                    foreach (var l in pair.Value?.Items ?? new NodalLoadItem[0])
                        batch.NodeLoads.Add(new NodeLoadRecord { NodeId = pair.Key, Case = l.Case, CoordinateSystem = CoordinateSystem.Global, Record = record + "/" + l.Id,
                            Components = new[] { F(l.FX), F(l.FY), F(l.FZ), M(l.MX), M(l.MY), M(l.MZ) } });
                }
            }

            private void BeamLoads()
            {
                foreach (var pair in Rows<ItemList<BeamLoadItem>>("BMLD"))
                {
                    if (!beams.TryGetValue(pair.Key, out var beam)) throw new InvalidDataException("CivilNxBeamLoadWithoutBeam: BMLD/" + pair.Key);
                    foreach (var l in pair.Value?.Items ?? new BeamLoadItem[0])
                    {
                        var record = "BMLD/" + pair.Key + "/" + l.Id; var type = (l.Type ?? "").ToUpperInvariant(); var direction = (l.Direction ?? "").ToUpperInvariant();
                        if (!string.Equals(l.Command, "BEAM", StringComparison.OrdinalIgnoreCase) || l.Eccentric == true || l.Additional == true
                            || (type != "CONLOAD" && type != "CONMOMENT" && type != "UNILOAD" && type != "UNIMOMENT") || direction.Length != 2 || "LG".IndexOf(direction[0]) < 0 || "XYZ".IndexOf(direction[1]) < 0)
                        { Warn("CivilNxBeamLoadPreserved", "BMLD", "Line/typical, pressure or eccentric beam loads are preserved, not mapped."); continue; }
                        bool local = direction[0] == 'L'; bool moment = type.EndsWith("MOMENT", StringComparison.Ordinal);
                        if (l.Projected == true && local) throw new InvalidDataException("CivilNxProjectedLocalBeamLoad: " + record);
                        // GPC components in the beam axes (V1,V2,V3) = MIDAS (y,z,x).
                        int axis = local ? (direction[1] == 'X' ? 2 : direction[1] == 'Y' ? 0 : 1) : direction[1] - 'X';
                        int index = (moment ? 3 : 0) + axis;
                        var system = local ? beam.CoordinateSystem : CoordinateSystem.Global;
                        var d = l.Distances ?? new double[0]; var p = l.Values ?? new double[0];
                        if (d.Length != p.Length || d.Length == 0 || d.Any(x => x < 0 || x > 1)) throw new InvalidDataException("CivilNxBeamLoadDistances: " + record + " (ratios 0..1 expected)");
                        double scale = type == "CONLOAD" ? force : type == "UNILOAD" ? force / length : type == "CONMOMENT" ? force * length : force;
                        Vector3d normal = l.Projected == true ? new Vector3d(direction[1] == 'X' ? 1 : 0, direction[1] == 'Y' ? 1 : 0, direction[1] == 'Z' ? 1 : 0) : null;
                        if (type.StartsWith("CON", StringComparison.Ordinal))
                        {
                            for (int k = 0; k < d.Length; k++)
                                if (p[k] != 0) batch.BeamLoads.Add(new BeamLoadRecord { BeamId = pair.Key, Case = l.Case, Start = d[k], End = d[k], Values = Vector(index, p[k] * scale),
                                    CoordinateSystem = system, Record = record + "/" + (k + 1).ToString(CultureInfo.InvariantCulture) });
                            continue;
                        }
                        int count = 1;
                        while (count < d.Length && d[count] > d[count - 1]) count++;
                        if (count < 2) throw new InvalidDataException("CivilNxBeamLoadDistances: " + record);
                        for (int k = 1; k < count; k++)
                            if (p[k - 1] != 0 || p[k] != 0)
                                batch.BeamLoads.Add(new BeamLoadRecord { BeamId = pair.Key, Case = l.Case, Start = d[k - 1], End = d[k], StartValues = Vector(index, p[k - 1] * scale),
                                    EndValues = Vector(index, p[k] * scale), CoordinateSystem = system, ProjectionPlaneNormal = normal, Record = record + "/segment " + k.ToString(CultureInfo.InvariantCulture) });
                    }
                }
            }

            private void Pressures()
            {
                foreach (var pair in Rows<ItemList<PressureItem>>("PRES"))
                {
                    if (!shells.TryGetValue(pair.Key, out var shell)) throw new InvalidDataException("CivilNxPressureWithoutPlate: PRES/" + pair.Key);
                    foreach (var l in pair.Value?.Items ?? new PressureItem[0])
                    {
                        var record = "PRES/" + pair.Key + "/" + l.Id; var direction = (l.Direction ?? "").ToUpperInvariant();
                        bool edge = string.Equals(l.FaceEdge, "EDGE", StringComparison.OrdinalIgnoreCase);
                        if (!string.Equals(l.ElementType, "PLATE", StringComparison.OrdinalIgnoreCase) || (!edge && !string.Equals(l.FaceEdge, "FACE", StringComparison.OrdinalIgnoreCase))
                            || l.Projected == true)
                        { Warn("CivilNxPressurePreserved", "PRES", "Projected or non-plate pressures are preserved, not mapped."); continue; }
                        var values = edge ? l.EdgeLoads : l.Forces;
                        double? uniform = Uniform(values, edge ? 3 : 5);
                        double[] nodal = null;
                        if (!uniform.HasValue)
                        {
                            // FORCES [0,P1..P4]: one value per plate node, in connectivity order (P4 unused on triangles).
                            if (edge || values == null || values.Length != 5 || values[0] != 0 || (shell.Nodes.Length == 3 && values[4] != 0))
                            { Warn("CivilNxVaryingPressurePreserved", "PRES", "Varying edge loads or unknown pressure layouts are preserved, not mapped."); continue; }
                            nodal = values.Skip(1).Take(shell.Nodes.Length).Select(p => p * force / (length * length)).ToArray();
                        }
                        Vector3d vector; CoordinateSystem system = CoordinateSystem.Global;
                        if (direction == "VECTOR")
                        {
                            var v = l.Vectors; if (v == null || v.Length != 3) throw new InvalidDataException("CivilNxPressureVector: " + record);
                            vector = new Vector3d(v[0], v[1], v[2]); var n = Axes.Length(vector); if (n <= 1e-12) throw new InvalidDataException("CivilNxPressureVector: " + record);
                            vector = vector / n;
                        }
                        else if (direction.Length == 2 && "LG".IndexOf(direction[0]) >= 0 && "XYZ".IndexOf(direction[1]) >= 0)
                        {
                            vector = new Vector3d(direction[1] == 'X' ? 1 : 0, direction[1] == 'Y' ? 1 : 0, direction[1] == 'Z' ? 1 : 0);
                            if (direction[0] == 'L') system = shell.CoordinateSystem;
                        }
                        else { Warn("CivilNxPressurePreserved", "PRES", "Pressure direction " + direction + " is not mapped."); continue; }
                        if (nodal != null)
                        {
                            var global = system == CoordinateSystem.Global ? vector : system.V1 * vector.X + system.V2 * vector.Y + system.V3 * vector.Z;
                            batch.ShellLoads.Add(new ShellLoadRecord { ShellId = pair.Key, Case = l.Case, NodalPressures = nodal, Record = record,
                                Normal = direction == "LZ", Direction = direction == "LZ" ? null : new Vector3d(global.X, global.Y, global.Z) });
                        }
                        else if (edge)
                        {
                            int index = (l.EdgeFace ?? 0) - 1;
                            if (index < 0 || index >= shell.Nodes.Length) throw new InvalidDataException("CivilNxPressureEdge: " + record);
                            double q = uniform.Value * force / length;
                            batch.ShellLoads.Add(new ShellLoadRecord { ShellId = pair.Key, Case = l.Case, Edge = index, CoordinateSystem = system, Record = record,
                                Components = new[] { vector.X * q, vector.Y * q, vector.Z * q } });
                        }
                        else
                        {
                            double q = uniform.Value * force / (length * length);
                            if (direction == "LZ") batch.ShellLoads.Add(new ShellLoadRecord { ShellId = pair.Key, Case = l.Case, Normal = true, Pressure = q, Record = record });
                            else batch.ShellLoads.Add(new ShellLoadRecord { ShellId = pair.Key, Case = l.Case, CoordinateSystem = system, Record = record,
                                Components = new[] { vector.X * q, vector.Y * q, vector.Z * q } });
                        }
                    }
                }
            }

            /// <summary>LCOM-*: iTYPE Add 0, Envelope 1, ABS 2, SRSS 3; terms ST (static case) or CB* (combination of the matching database).</summary>
            private void Combinations()
            {
                var staticCases = new HashSet<string>(batch.LoadCases.Select(c => c.Name), StringComparer.Ordinal);
                foreach (var table in CombinationTables.Values)
                    foreach (var pair in Rows<CombinationData>(table))
                    {
                        var c = pair.Value; var record = table + "/" + pair.Key;
                        if (string.IsNullOrWhiteSpace(c?.Name) || c.Terms == null) throw new InvalidDataException("InvalidCivilNxCombination: " + record);
                        if (c.Type < 0 || c.Type > 3) throw new InvalidDataException("UnknownCivilNxCombinationType: " + record);
                        var definition = new CombinationRecord { Id = table + "/" + c.Name, Name = c.Name, Kind = (CombinationKind)c.Type, Status = c.Active, Source = table, Record = record };
                        foreach (var t in c.Terms)
                        {
                            var analysis = t?.Analysis ?? "";
                            bool isCase = analysis == "ST", isCombination = CombinationTables.TryGetValue(analysis, out var referenced);
                            if (isCase && !staticCases.Contains(t.Case)) throw new InvalidDataException("CivilNxCombinationCaseMissing: " + record + " " + t.Case);
                            definition.Terms.Add(new CombinationTermRecord { Name = isCombination ? referenced + "/" + t.Case : t.Case, IsStaticCase = isCase, IsCombination = isCombination,
                                Analysis = analysis, Factor = t.Factor });
                        }
                        batch.Combinations.Add(definition);
                    }
                var ids = new HashSet<string>(batch.Combinations.Select(d => d.Id), StringComparer.Ordinal);
                foreach (var d in batch.Combinations)
                    foreach (var t in d.Terms.Where(t => t.IsCombination))
                        if (!ids.Contains(t.Name)) throw new InvalidDataException("CivilNxCombinationReferenceMissing: " + d.Record + " " + t.Name);
            }

            /// <summary>BODF: FV multiplies the gravity along GCS X, Y, Z for the whole model; a self weight restricted to a group is preserved only.</summary>
            private void SelfWeight()
            {
                foreach (var pair in Rows<SelfWeightData>("BODF"))
                {
                    var s = pair.Value; var record = "BODF/" + pair.Key;
                    if (s?.Factors == null || s.Factors.Length != 3 || string.IsNullOrWhiteSpace(s.Case)) throw new InvalidDataException("InvalidCivilNxSelfWeight: " + record);
                    if (!string.IsNullOrEmpty(s.Group)) { Warn("CivilNxGroupSelfWeightPreserved", record, "Self weight restricted to a load group is preserved, not mapped."); continue; }
                    batch.Gravity.Add(new GravityRecord { Case = s.Case, Factors = new Vector3d(s.Factors[0], s.Factors[1], s.Factors[2]), Record = record });
                }
            }

            /// <summary>FORCES [PU,0,0,0,0] or [0,P1..P4]; EDGE_LOADS [EPU,0,0] or [0,EP1,EP2]. Linear values all equal are uniform.</summary>
            private static double? Uniform(double[] values, int size)
            {
                if (values == null || values.Length != size) return null;
                if (values.Skip(1).All(v => v == 0)) return values[0];
                if (values[0] != 0) return null;
                var linear = values.Skip(1).ToArray();
                return linear.All(v => v == linear[0]) ? linear[0] : (double?)null;
            }

            private static double[] Vector(int index, double value) { var v = new double[6]; v[index] = value; return v; }
            private double L(double value) => value * length;
            private double A(double value) => value * length * length;
            private double I(double value) => value * Math.Pow(length, 4);
            private double F(double value) => value * force;
            private double M(double value) => value * force * length;
        }

        [DataContract] private sealed class UnitData
        { [DataMember(Name = "FORCE")] public string Force { get; set; } [DataMember(Name = "DIST")] public string Distance { get; set; } [DataMember(Name = "TEMPER")] public string Temperature { get; set; } }
        [DataContract] private sealed class NodeData
        { [DataMember] public double? X { get; set; } [DataMember] public double? Y { get; set; } [DataMember] public double? Z { get; set; } }
        [DataContract] private sealed class SkewData
        {
            [DataMember(Name = "iMETHOD")] public int? Method { get; set; }
            [DataMember] public double V1X { get; set; } [DataMember] public double V1Y { get; set; } [DataMember] public double V1Z { get; set; }
            [DataMember] public double V2X { get; set; } [DataMember] public double V2Y { get; set; } [DataMember] public double V2Z { get; set; }
            [DataMember] public double P0X { get; set; } [DataMember] public double P0Y { get; set; } [DataMember] public double P0Z { get; set; }
            [DataMember] public double P1X { get; set; } [DataMember] public double P1Y { get; set; } [DataMember] public double P1Z { get; set; }
            [DataMember] public double P2X { get; set; } [DataMember] public double P2Y { get; set; } [DataMember] public double P2Z { get; set; }
        }
        [DataContract] private sealed class ElementData
        {
            [DataMember(Name = "TYPE")] public string Type { get; set; } [DataMember(Name = "MATL")] public int? Material { get; set; }
            [DataMember(Name = "SECT")] public int? Section { get; set; } [DataMember(Name = "NODE")] public int[] Nodes { get; set; }
            [DataMember(Name = "ANGLE")] public double? Angle { get; set; } [DataMember(Name = "STYPE")] public int? SubType { get; set; }
        }
        [DataContract] private sealed class MaterialData
        {
            [DataMember(Name = "TYPE")] public string Type { get; set; } [DataMember(Name = "NAME")] public string Name { get; set; }
            [DataMember(Name = "PARAM")] public MaterialParameters[] Parameters { get; set; }
        }
        [DataContract] private sealed class MaterialParameters
        {
            [DataMember(Name = "P_TYPE")] public int Type { get; set; } [DataMember(Name = "STANDARD")] public string Standard { get; set; }
            [DataMember(Name = "DB")] public string Grade { get; set; } [DataMember(Name = "ELAST")] public double? Elastic { get; set; }
            [DataMember(Name = "POISN")] public double? Poisson { get; set; } [DataMember(Name = "THERMAL")] public double? Thermal { get; set; }
            [DataMember(Name = "DEN")] public double? WeightDensity { get; set; }
        }
        [DataContract] private sealed class SectionData
        {
            [DataMember(Name = "SECTTYPE")] public string SectionType { get; set; } [DataMember(Name = "SECT_NAME")] public string Name { get; set; }
            [DataMember(Name = "SECT_BEFORE")] public SectionBefore Before { get; set; }
        }
        [DataContract] private sealed class SectionBefore
        {
            [DataMember(Name = "SHAPE")] public string Shape { get; set; } [DataMember(Name = "DATATYPE")] public int? DataType { get; set; }
            [DataMember(Name = "OFFSET_PT")] public string OffsetPoint { get; set; } [DataMember(Name = "OFFSET_CENTER")] public int? OffsetCenter { get; set; }
            [DataMember(Name = "HORZ_OFFSET_OPT")] public int? HorizontalOption { get; set; } [DataMember(Name = "USERDEF_OFFSET_YI")] public double? UserYI { get; set; }
            [DataMember(Name = "VERT_OFFSET_OPT")] public int? VerticalOption { get; set; } [DataMember(Name = "USERDEF_OFFSET_ZI")] public double? UserZI { get; set; }
            [DataMember(Name = "SECT_I")] public SectionData1 First { get; set; }
        }
        [DataContract] private sealed class SectionData1
        {
            [DataMember(Name = "DB_NAME")] public string Database { get; set; } [DataMember(Name = "SECT_NAME")] public string SectionName { get; set; }
            [DataMember(Name = "vSIZE")] public double[] Size { get; set; } [DataMember(Name = "STIFF")] public SectionStiffness Stiffness { get; set; }
        }
        [DataContract] private sealed class SectionStiffness
        {
            [DataMember(Name = "AREA")] public double? Area { get; set; } [DataMember(Name = "RXX")] public double? Ixx { get; set; }
            [DataMember(Name = "RYY")] public double? Iyy { get; set; } [DataMember(Name = "RZZ")] public double? Izz { get; set; }
        }
        [DataContract] private sealed class ThicknessData
        {
            [DataMember(Name = "NAME")] public string Name { get; set; } [DataMember(Name = "TYPE")] public string Type { get; set; }
            [DataMember(Name = "bINOUT")] public bool? Separate { get; set; } [DataMember(Name = "T_IN")] public double? In { get; set; }
            [DataMember(Name = "T_OUT")] public double? Out { get; set; } [DataMember(Name = "OFFSET")] public int? Offset { get; set; }
            [DataMember(Name = "O_VALUE")] public double OffsetValue { get; set; }
        }
        [DataContract] private sealed class GroupData
        {
            [DataMember(Name = "NAME")] public string Name { get; set; } [DataMember(Name = "N_LIST")] public int[] Nodes { get; set; }
            [DataMember(Name = "E_LIST")] public int[] Elements { get; set; }
        }
        [DataContract] private sealed class CaseData
        { [DataMember(Name = "NAME")] public string Name { get; set; } [DataMember(Name = "TYPE")] public string Type { get; set; } }
        [DataContract] private sealed class ItemList<T> { [DataMember(Name = "ITEMS")] public T[] Items { get; set; } }
        [DataContract] private sealed class CombinationData
        {
            [DataMember(Name = "NAME")] public string Name { get; set; } [DataMember(Name = "ACTIVE")] public string Active { get; set; }
            [DataMember(Name = "iTYPE")] public int Type { get; set; } [DataMember(Name = "vCOMB")] public CombinationTermData[] Terms { get; set; }
        }
        [DataContract] private sealed class CombinationTermData
        {
            [DataMember(Name = "ANAL")] public string Analysis { get; set; } [DataMember(Name = "LCNAME")] public string Case { get; set; }
            [DataMember(Name = "FACTOR")] public double Factor { get; set; }
        }
        [DataContract] private sealed class SelfWeightData
        {
            [DataMember(Name = "LCNAME")] public string Case { get; set; } [DataMember(Name = "GROUP_NAME")] public string Group { get; set; }
            [DataMember(Name = "FV")] public double[] Factors { get; set; }
        }
        [DataContract] private sealed class SupportItem { [DataMember(Name = "CONSTRAINT")] public string Constraint { get; set; } }
        [DataContract] private sealed class OffsetItem
        {
            [DataMember(Name = "TYPE")] public string Type { get; set; }
            [DataMember(Name = "RGDXi")] public double Xi { get; set; } [DataMember(Name = "RGDYi")] public double Yi { get; set; } [DataMember(Name = "RGDZi")] public double Zi { get; set; }
            [DataMember(Name = "RGDXj")] public double Xj { get; set; } [DataMember(Name = "RGDYj")] public double Yj { get; set; } [DataMember(Name = "RGDZj")] public double Zj { get; set; }
        }
        [DataContract] private sealed class NodalLoadItem
        {
            [DataMember(Name = "ID")] public int Id { get; set; } [DataMember(Name = "LCNAME")] public string Case { get; set; }
            [DataMember] public double FX { get; set; } [DataMember] public double FY { get; set; } [DataMember] public double FZ { get; set; }
            [DataMember] public double MX { get; set; } [DataMember] public double MY { get; set; } [DataMember] public double MZ { get; set; }
        }
        [DataContract] private sealed class BeamLoadItem
        {
            [DataMember(Name = "ID")] public int Id { get; set; } [DataMember(Name = "LCNAME")] public string Case { get; set; }
            [DataMember(Name = "CMD")] public string Command { get; set; } [DataMember(Name = "TYPE")] public string Type { get; set; }
            [DataMember(Name = "DIRECTION")] public string Direction { get; set; } [DataMember(Name = "USE_PROJECTION")] public bool? Projected { get; set; }
            [DataMember(Name = "USE_ECCEN")] public bool? Eccentric { get; set; } [DataMember(Name = "USE_ADDITIONAL")] public bool? Additional { get; set; }
            [DataMember(Name = "D")] public double[] Distances { get; set; } [DataMember(Name = "P")] public double[] Values { get; set; }
        }
        [DataContract] private sealed class PressureItem
        {
            [DataMember(Name = "ID")] public int Id { get; set; } [DataMember(Name = "LCNAME")] public string Case { get; set; }
            [DataMember(Name = "ELEM_TYPE")] public string ElementType { get; set; } [DataMember(Name = "FACE_EDGE_TYPE")] public string FaceEdge { get; set; }
            [DataMember(Name = "DIRECTION")] public string Direction { get; set; } [DataMember(Name = "VECTORS")] public double[] Vectors { get; set; }
            [DataMember(Name = "OPT_PROJECTION")] public bool? Projected { get; set; } [DataMember(Name = "EDGE_FACE")] public int? EdgeFace { get; set; }
            [DataMember(Name = "FORCES")] public double[] Forces { get; set; } [DataMember(Name = "EDGE_LOADS")] public double[] EdgeLoads { get; set; }
        }
    }
}
