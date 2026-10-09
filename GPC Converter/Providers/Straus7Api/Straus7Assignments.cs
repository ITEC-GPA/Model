using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core.Coordinates;
using GPC.Model.Core.Diagnostics;

namespace GPC.Converter.Straus7
{
    /// <summary>Straus7 R3 properties, element attributes, restraints and loads as normalized records. Conventions checked against the
    /// native solver on a generated model (October 2026, R31 / API 3.1.5): standard section D along principal axis 2, B along axis 1,
    /// I section B1/T1 bottom flange, B2/T2 top flange, T3 web; distributed loads with relative positions a (from End 1) and b (from End 2);
    /// gravity along the positive selected axis times the value; normal pressure along +z = pressure on the -z face minus pressure on the +z face.</summary>
    internal sealed class Straus7Assignments
    {
        private readonly ImportBatch batch;
        private readonly IStraus7AssignmentReadApi api;
        private readonly double length, force, stress, mass, thermal;
        private readonly Dictionary<int, string> caseNames;
        private readonly Dictionary<string, int> warnings = new Dictionary<string, int>(StringComparer.Ordinal);
        private readonly Dictionary<int, double> sectionAngle = new Dictionary<int, double>();
        private readonly HashSet<string> materials = new HashSet<string>(StringComparer.Ordinal);

        public Straus7Assignments(ImportBatch batch, int[] units, IStraus7AssignmentReadApi api, Dictionary<int, string> caseNames)
        {
            this.batch = batch; this.api = api; this.caseNames = caseNames;
            double[] lengths = { 1000, 10, 1, 304.8, 25.4 }, forces = { 1, 1000, 1000000, 9.80665, 4.4482216152605, 9806.65, 4448.2216152605 };
            double[] stresses = { 1e-6, 1e-3, 1, 0.0980665, 0.00689475729316836, 6.89475729316836, 4.78802589803358e-5 };
            double[] masses = { 1e-3, 1, 1e-6, 0.45359237e-3, 14.5939029372064e-3 };
            if (units[2] < 0 || units[2] >= stresses.Length || units[3] < 0 || units[3] >= masses.Length) throw new NotSupportedException("Unknown Straus7 stress/mass unit code.");
            length = lengths[units[0]]; force = forces[units[1]]; stress = stresses[units[2]]; mass = masses[units[3]];
            thermal = units[4] == 1 || units[4] == 3 ? 1.8 : 1;
        }

        public bool Available => api != null;

        private void Warn(string code, string record, string message)
        {
            var key = code + "\u001f" + record + "\u001f" + message;
            warnings[key] = warnings.TryGetValue(key, out var count) ? count + 1 : 1;
        }

        public void Finish()
        {
            foreach (var w in warnings)
            {
                var parts = w.Key.Split('\u001f');
                batch.Diagnostics.Add(new ModelDiagnostic { Code = parts[0], Severity = DiagnosticSeverity.Warning, Record = parts[1],
                    Message = parts[2] + (w.Value > 1 ? " (" + w.Value.ToString(CultureInfo.InvariantCulture) + " records)" : "") });
            }
        }

        private string Material(string id, string name, double[] elastic, int modulus, int poisson, int density, int alpha, string record)
        {
            if (materials.Add(id))
                batch.Materials.Add(new MaterialRecord { Id = id, Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim(), Kind = MaterialKind.Unknown, Record = record,
                    ElasticModulus = elastic[modulus] * stress, Poisson = elastic[poisson], Density = elastic[density] * mass / (length * length * length),
                    ThermalExpansion = elastic[alpha] * thermal });
            return id;
        }

        /// <summary>Material and section of a beam property (btBeam); null ids when the property cannot be represented.</summary>
        public (string Material, string Section) Beam(Straus7Property p)
        {
            if (p.Material == null || p.Material.Length < 5 || p.Geometry == null) return (null, null);
            var record = "Property/Beam/" + p.Number.ToString(CultureInfo.InvariantCulture);
            var material = Material("Beam/" + p.Number.ToString(CultureInfo.InvariantCulture), p.MaterialName, p.Material, 0, 2, 3, 4, record);
            var section = new SectionRecord { Id = "Beam/" + p.Number.ToString(CultureInfo.InvariantCulture), Name = string.IsNullOrWhiteSpace(p.Name) ? null : p.Name.Trim(), Record = record,
                Reference = SectionReference.Centroid };
            var s = p.SectionProperties;
            if (s != null && s.Length >= 11 && s[0] > 0 && s[1] > 0 && s[2] > 0)
            {
                section.Values = new SectionValues { Area = s[0] * length * length, I11 = s[1] * Math.Pow(length, 4), I22 = s[2] * Math.Pow(length, 4), Torsion = s[3] * Math.Pow(length, 4) };
                sectionAngle[p.Number] = s[10];
            }
            if (p.MirrorType != 0) Warn("Straus7MirroredSectionByValues", record, "Mirrored sections are not interpreted; only their computed properties are known.");
            else Interpret(p, section);
            batch.Sections.Add(section);
            return (material, section.Id);
        }

        /// <summary>Candidate shapes from the standard/BGL dimensions; one is accepted only when its area, inertias and centroid reproduce the
        /// properties computed by Straus7, otherwise the section keeps the Straus7 values.</summary>
        private void Interpret(Straus7Property p, SectionRecord section)
        {
            var d = p.Geometry.Select(v => v * length).ToArray(); var candidates = new List<(SectionShapeKind, double[])>();
            switch (p.SectionType)
            {
                case 1: candidates.Add((SectionShapeKind.SolidCircle, new[] { d[0] })); break;
                case 2: candidates.Add((SectionShapeKind.Pipe, new[] { d[0], d[3] })); break;
                case 3: candidates.Add((SectionShapeKind.SolidRectangle, new[] { d[1], d[0] })); break;
                case 4:
                    candidates.Add((SectionShapeKind.Box, new[] { d[1], d[0], d[3], d[4], d[4] }));
                    candidates.Add((SectionShapeKind.Box, new[] { d[1], d[0], d[4], d[3], d[3] })); break;
                case 7: candidates.Add((SectionShapeKind.I, new[] { d[2], d[1], d[5], d[4], d[0], d[3] })); break;
                case 17:
                    var b = (p.BglDimensions ?? new double[0]).Select(v => v * length).ToArray();
                    if (p.BglShape == 2 && b.Length >= 8) candidates.Add((SectionShapeKind.I, new[] { b[0], b[2], b[3], b[5], b[1], b[4], b[6] }));
                    break;
            }
            var s = p.SectionProperties;
            foreach (var (kind, dimensions) in candidates)
            {
                if (s == null || s.Length < 11 || Math.Abs(s[10]) > 1e-9) break;
                try
                {
                    var shape = PropertyMapper.Parametric(new SectionRecord { Id = section.Id, Shape = kind, Dimensions = dimensions }, section.Name ?? section.Id);
                    shape.SetMechanicalProperties();
                    var points = shape.GetSectionPoints(); double minX = points.Min(q => q.X), minY = points.Min(q => q.Y);
                    bool Close(double a, double e) => Math.Abs(a - e) <= 5e-3 * Math.Max(Math.Abs(e), 1e-9);
                    if (Close(shape.Area, s[0] * length * length) && Close(shape.Jxx, s[1] * Math.Pow(length, 4)) && Close(shape.Jyy, s[2] * Math.Pow(length, 4))
                        && Math.Abs(shape.Centroid.X - minX - s[8] * length) <= 1e-3 * Math.Max(1, shape.Width) && Math.Abs(shape.Centroid.Y - minY - s[9] * length) <= 1e-3 * Math.Max(1, shape.Height))
                    { section.Shape = kind; section.Dimensions = dimensions; return; }
                }
                catch (ArgumentException) { }
            }
            Warn("Straus7SectionByValues", section.Record, "Section type " + p.SectionType.ToString(CultureInfo.InvariantCulture)
                + " has no verified outline; only the area and inertias computed by Straus7 are known.");
        }

        /// <summary>Material and thickness of a plate property (ptPlateShell, isotropic).</summary>
        public (string Material, string Thickness) Plate(Straus7Property p)
        {
            var record = "Property/Plate/" + p.Number.ToString(CultureInfo.InvariantCulture);
            if (p.MaterialType != 1 || p.Material == null || p.Material.Length < 4 || p.Geometry == null || p.Geometry.Length < 2)
            { Warn("Straus7PlateMaterialPreserved", record, "Only isotropic plate materials are mapped; plates of this property keep no property."); return (null, null); }
            var material = Material("Plate/" + p.Number.ToString(CultureInfo.InvariantCulture), p.MaterialName, p.Material, 0, 1, 2, 3, record);
            return (material, Thickness("Plate/" + p.Number.ToString(CultureInfo.InvariantCulture), p.Name, p.Geometry, record));
        }

        private string Thickness(string id, string name, double[] values, string record)
        {
            double membrane = values[0] * length, bending = values[1] * length;
            batch.Thicknesses.Add(new ThicknessRecord { Id = id, Name = string.IsNullOrWhiteSpace(name) ? null : name.Trim(), InPlane = membrane,
                OutOfPlane = Math.Abs(bending - membrane) <= 1e-9 * Math.Max(1, membrane) ? (double?)null : bending, Record = record });
            return id;
        }

        private Straus7Attribute[] Attributes(Straus7Entity entity, int number, int attribute) => api.ReadAttributes(entity, number, attribute) ?? new Straus7Attribute[0];

        private string Case(int number, string record)
        {
            if (!caseNames.TryGetValue(number, out var name)) throw new InvalidDataException("Load attribute of an unknown load case " + number + ": " + record);
            return name;
        }

        public void BeamElement(int number, BeamRecord beam, int property)
        {
            if (api == null) return;
            var record = beam.Record; Vector3d eccentricity = null;
            if (Attributes(Straus7Entity.Beam, number, Straus7Attributes.BeamOffset).Length != 0)
            {
                var o = api.ReadBeamOffset(number); Straus7ApiConverter.Finite(o, 2, record + " offset");
                beam.CentroidOffset = new Vector2d(o[0] * length, o[1] * length); // Principal 1-2 axes are the beam V1, V2.
                // The solver applies the beam loads on the offset (centroidal) axis.
                if (o[0] != 0 || o[1] != 0) eccentricity = new Vector3d(o[0] * length, o[1] * length, 0);
            }
            int first = batch.BeamLoads.Count;
            foreach (var (ord, name) in new[] { (Straus7Attributes.BeamTaper, "taper"), (Straus7Attributes.BeamSectionFactor, "section factors") })
                if (Attributes(Straus7Entity.Beam, number, ord).Length != 0) Warn("Straus7BeamAttributePreserved", "Beam " + name, "Beam " + name + " are not mapped; the section is taken as prismatic and unfactored.");
            sectionAngle.TryGetValue(property, out var angle);
            foreach (var frame in new[] { (Straus7Attributes.BeamDLL, Straus7LoadFrame.Principal), (Straus7Attributes.BeamDLG, Straus7LoadFrame.Global), (Straus7Attributes.BeamDML, Straus7LoadFrame.PrincipalMoment) })
                foreach (var a in Attributes(Straus7Entity.Beam, number, frame.Item1))
                {
                    var load = api.ReadBeamDistributedLoad(number, frame.Item2, a.Axis, a.Case, a.Id);
                    var loadRecord = record + "/distributed/" + frame.Item2 + "/" + a.Axis + "/case " + a.Case + "/id " + a.Id;
                    int direction = a.Axis;
                    if (frame.Item2 != Straus7LoadFrame.Global && (direction == 4 || direction == 5))
                    {
                        // Local x/y coincide with principal 1/2 only for a zero principal angle.
                        if (Math.Abs(angle) > 1e-12) { Warn("Straus7LocalBeamLoadPreserved", "Beam local loads", "Distributed loads in local section axes of rotated principal axes are not mapped."); continue; }
                        direction -= 3;
                    }
                    if (direction < 1 || direction > 3) throw new InvalidDataException("Unknown distributed load direction " + a.Axis + ": " + loadRecord);
                    if (frame.Item2 == Straus7LoadFrame.Global && load.Project != 0 && load.Project != 1) throw new InvalidDataException("Unknown projection flag: " + loadRecord);
                    double scale = frame.Item2 == Straus7LoadFrame.PrincipalMoment ? force : force / length;
                    int index = (frame.Item2 == Straus7LoadFrame.PrincipalMoment ? 3 : 0) + direction - 1;
                    var system = frame.Item2 == Straus7LoadFrame.Global ? CoordinateSystem.Global : beam.CoordinateSystem;
                    Vector3d normal = frame.Item2 == Straus7LoadFrame.Global && load.Project == 1 ? new Vector3d(direction == 1 ? 1 : 0, direction == 2 ? 1 : 0, direction == 3 ? 1 : 0) : null;
                    var points = Distribution(load, loadRecord);
                    for (int k = 1; k < points.Count; k++)
                        if (points[k].Station > points[k - 1].Station && (points[k - 1].Value != 0 || points[k].Value != 0))
                            batch.BeamLoads.Add(new BeamLoadRecord { BeamId = beam.Id, Case = Case(a.Case, loadRecord), Start = points[k - 1].Station, End = points[k].Station,
                                StartValues = Vector(index, points[k - 1].Value * scale), EndValues = Vector(index, points[k].Value * scale), CoordinateSystem = system,
                                ProjectionPlaneNormal = normal, Record = loadRecord + "/segment " + k.ToString(CultureInfo.InvariantCulture) });
                }
            foreach (var point in new[] { (Straus7Attributes.BeamCFL, false, false), (Straus7Attributes.BeamCFG, false, true), (Straus7Attributes.BeamCML, true, false), (Straus7Attributes.BeamCMG, true, true) })
                foreach (var a in Attributes(Straus7Entity.Beam, number, point.Item1))
                {
                    var v = api.ReadBeamPointLoad(number, a.Case, a.Id, point.Item2, point.Item3); var loadRecord = record + "/point/" + point.Item1 + "/case " + a.Case + "/id " + a.Id;
                    Straus7ApiConverter.Finite(v, 4, loadRecord);
                    if (v[3] < 0 || v[3] > 1) throw new InvalidDataException("Point load position outside the beam: " + loadRecord);
                    double scale = point.Item2 ? force * length : force; var values = new double[6];
                    for (int c = 0; c < 3; c++) values[(point.Item2 ? 3 : 0) + c] = v[c] * scale;
                    batch.BeamLoads.Add(new BeamLoadRecord { BeamId = beam.Id, Case = Case(a.Case, loadRecord), Start = v[3], End = v[3], Values = values,
                        CoordinateSystem = point.Item3 ? CoordinateSystem.Global : beam.CoordinateSystem, Record = loadRecord });
                }
            if (eccentricity != null)
                for (int k = first; k < batch.BeamLoads.Count; k++) { batch.BeamLoads[k].Eccentricity = eccentricity; batch.BeamLoads[k].EccentricityAxes = beam.CoordinateSystem; }
        }

        private sealed class LoadPoint { public double Station; public double Value; }

        /// <summary>Doubles PA, PB, P1, P2, a, b: dlConstant PA on [a, 1-b]; dlLinear PA at a to PB at 1-b; dlTriangular P1 at 0, PA at a, P2 at 1;
        /// dlThreePoint0 P1 at 0, PA at a, PB at 1-b; dlThreePoint1 PA at a, PB at 1-b, P2 at 1; dlTrapezoidal P1 at 0, PA at a, PB at 1-b, P2 at 1.</summary>
        private static List<LoadPoint> Distribution(Straus7DistributedLoad load, string record)
        {
            var v = load.Values; Straus7ApiConverter.Finite(v, 6, record);
            double pa = v[0], pb = v[1], p1 = v[2], p2 = v[3], a = v[4], end = 1 - v[5];
            if (a < 0 || end > 1 || a > end) throw new InvalidDataException("Distributed load positions outside the beam: " + record);
            var points = new List<LoadPoint>();
            void Add(double station, double value) => points.Add(new LoadPoint { Station = station, Value = value });
            switch (load.Type)
            {
                case 0: Add(a, pa); Add(end, pa); break;
                case 1: Add(a, pa); Add(end, pb); break;
                case 2: Add(0, p1); Add(a, pa); Add(1, p2); break;
                case 3: Add(0, p1); Add(a, pa); Add(end, pb); break;
                case 4: Add(a, pa); Add(end, pb); Add(1, p2); break;
                case 5: Add(0, p1); Add(a, pa); Add(end, pb); Add(1, p2); break;
                default: throw new NotSupportedException("Unknown distributed load type " + load.Type + ": " + record);
            }
            return points;
        }

        public void PlateElement(int number, ShellRecord shell, Straus7Property property)
        {
            if (api == null) return;
            var record = shell.Record;
            if (Attributes(Straus7Entity.Plate, number, Straus7Attributes.PlateThickness).Length != 0 && shell.MaterialId != null)
            {
                var t = api.ReadPlateThickness(number); Straus7ApiConverter.Finite(t, 2, record + " thickness");
                var label = (property?.Name?.Trim() ?? "Plate") + " t=" + (t[0] * length).ToString("G6", CultureInfo.InvariantCulture);
                shell.ThicknessId = Thickness("PlateElement/" + number.ToString(CultureInfo.InvariantCulture), label, t, record);
            }
            if (Attributes(Straus7Entity.Plate, number, Straus7Attributes.PlateOffset).Length != 0)
            {
                var o = api.ReadPlateOffset(number); Straus7ApiConverter.Finite(o, 1, record + " offset"); shell.Offset = o[0] * length;
            }
            foreach (var (ord, name) in new[] { (Straus7Attributes.PlateFaceShear, "face shear"), (Straus7Attributes.PlateEdgeNormalPressure, "edge normal pressure"),
                (Straus7Attributes.PlateEdgeGlobalPressure, "edge global pressure"), (Straus7Attributes.PlatePointForce, "point force"), (Straus7Attributes.PlatePointMoment, "point moment"),
                (Straus7Attributes.PlateSectionFactor, "section factors") })
                if (Attributes(Straus7Entity.Plate, number, ord).Length != 0) Warn("Straus7PlateAttributePreserved", "Plate " + name, "Plate " + name + " is not mapped.");
            foreach (var a in Attributes(Straus7Entity.Plate, number, Straus7Attributes.PlateFacePressure))
            {
                var p = api.ReadPlateNormalPressure(number, a.Case); var loadRecord = record + "/normal pressure/case " + a.Case; Straus7ApiConverter.Finite(p, 2, loadRecord);
                // Positive pressure acts from its face into the plate: the -z face pushes along +z, the +z face along -z.
                batch.ShellLoads.Add(new ShellLoadRecord { ShellId = shell.Id, Case = Case(a.Case, loadRecord), Normal = true, Pressure = (p[0] - p[1]) * stress, Record = loadRecord });
            }
            foreach (var a in Attributes(Straus7Entity.Plate, number, Straus7Attributes.PlateGlobalPressure))
            {
                var loadRecord = record + "/global pressure/surface " + a.Local + "/case " + a.Case;
                var p = api.ReadPlateGlobalPressure(number, a.Local, a.Case, out var project); Straus7ApiConverter.Finite(p, 3, loadRecord);
                if (project != 0) { Warn("Straus7ProjectedPressurePreserved", "Plate global pressure", "Projected global pressures are not mapped."); continue; }
                batch.ShellLoads.Add(new ShellLoadRecord { ShellId = shell.Id, Case = Case(a.Case, loadRecord), Components = p.Select(x => x * stress).ToArray(),
                    CoordinateSystem = CoordinateSystem.Global, Record = loadRecord });
            }
        }

        /// <summary>Restraints of a single freedom case (global or Cartesian UCS, no enforced values) and nodal forces/moments of every load case.</summary>
        public void Nodes(IReadOnlyDictionary<int, Point3d> points)
        {
            if (api == null) return;
            int freedomCases = api.FreedomCaseCount;
            foreach (var pair in points)
            {
                int node = pair.Key; var record = "Node/" + node.ToString(CultureInfo.InvariantCulture);
                var restraints = Attributes(Straus7Entity.Node, node, Straus7Attributes.Restraint);
                if (restraints.Length != 0)
                {
                    if (freedomCases != 1) Warn("Straus7FreedomCasesPreserved", "Freedom cases", "Restraints of models with several freedom cases are not mapped.");
                    else
                    {
                        var r = api.ReadRestraint(node, 1);
                        if (r?.Status == null || r.Status.Length != 6 || r.Values == null || r.Values.Length != 6) throw new InvalidDataException("Invalid restraint: " + record);
                        bool global = r.Ucs == 1; CoordinateSystem axes = global ? CoordinateSystem.Global : Ucs(r.Ucs);
                        if (axes == null) Warn("Straus7RestraintUcsPreserved", "Restraint UCS", "Restraints in non-Cartesian coordinate systems are not mapped.");
                        else if (Enumerable.Range(0, 6).Any(i => r.Status[i] != 0 && r.Values[i] != 0)) Warn("Straus7EnforcedDisplacementPreserved", "Enforced displacements", "Restraints with enforced displacements are not mapped.");
                        else batch.NodeRestrains.Add(new NodeRestrainRecord { NodeId = Straus7ApiConverter.Id(node), FixedDofs = r.Status.Select(s => s != 0).ToArray(),
                            CoordinateSystem = global ? axes : Frames.At(axes, pair.Value), Record = record + "/restraint" });
                    }
                }
                foreach (var moment in new[] { false, true })
                    foreach (var a in Attributes(Straus7Entity.Node, node, moment ? Straus7Attributes.Moment : Straus7Attributes.Force))
                    {
                        var v = api.ReadNodeLoad(node, a.Case, moment); var loadRecord = record + (moment ? "/moment" : "/force") + "/case " + a.Case;
                        Straus7ApiConverter.Finite(v, 3, loadRecord); double scale = moment ? force * length : force; var values = new double[6];
                        for (int c = 0; c < 3; c++) values[(moment ? 3 : 0) + c] = v[c] * scale;
                        batch.NodeLoads.Add(new NodeLoadRecord { NodeId = Straus7ApiConverter.Id(node), Case = Case(a.Case, loadRecord), Components = values,
                            CoordinateSystem = CoordinateSystem.Global, Record = loadRecord });
                    }
            }
        }

        private CoordinateSystem Ucs(int id)
        {
            var u = api.ReadUcs(id);
            if (u == null || u.Type != 0 || u.Data == null || u.Data.Length < 9) return null;
            var o = new Vector3d(u.Data[0], u.Data[1], u.Data[2]);
            var x = new Vector3d(u.Data[3], u.Data[4], u.Data[5]) - o; var inPlane = new Vector3d(u.Data[6], u.Data[7], u.Data[8]) - o;
            double xl = Axes.Length(x); if (!(xl > 0)) return null;
            x = x / xl; var z = x.CrossProduct(inPlane); double zl = Axes.Length(z); if (!(zl > 0)) return null;
            z = z / zl; return new CoordinateSystem(new Point3d(0, 0, 0), x, z.CrossProduct(x), z);
        }

        /// <summary>lcGravity cases: gravity along the positive selected global axis times the stored value; other inertia types are preserved.</summary>
        public void Gravity(int cases)
        {
            if (api == null) return;
            for (int i = 1; i <= cases; i++)
            {
                var data = api.ReadLoadCaseData(i); var record = "LoadCase/" + i.ToString(CultureInfo.InvariantCulture);
                if (data == null || data.Type == 0) continue;
                if (data.Type != 1) { Warn("Straus7InertiaCasePreserved", record, "Acceleration and seismic load case types are not converted to loads."); continue; }
                if (data.GravityDirection < 1 || data.GravityDirection > 3 || double.IsNaN(data.Gravity) || double.IsInfinity(data.Gravity))
                    throw new InvalidDataException("Invalid gravity: " + record);
                if (data.Gravity == 0) continue;
                double factor = data.Gravity * length / ModelGravityLoadAcceleration;
                batch.Gravity.Add(new GravityRecord { Case = Case(i, record), Record = record,
                    Factors = new Vector3d(data.GravityDirection == 1 ? factor : 0, data.GravityDirection == 2 ? factor : 0, data.GravityDirection == 3 ? factor : 0) });
            }
        }
        private const double ModelGravityLoadAcceleration = 9806.65;

        private static double[] Vector(int index, double value) { var v = new double[6]; v[index] = value; return v; }
    }
}
