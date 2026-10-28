using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Core.Coordinates;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.Straus7
{
    /// <summary>Straus7 element families other than beams and plates, beam end releases, node springs, links and LSA combinations.
    /// Releases are in the principal axes 1, 2, 3 of the beam (GPC V1, V2, V3); node springs and restraints of the single freedom case.</summary>
    internal sealed partial class Straus7Assignments
    {
        private readonly IStraus7ExtendedReadApi extended;
        private static readonly string[] BeamFamilies = { "null", "spring-damper", "cable", "truss", "cut-off bar", "point contact", "beam", "user-defined", "pipe", "connection" };
        private static readonly string[] LinkFamilies = { "", "master-slave", "sector symmetry", "coupling", "pinned", "rigid", "shrink", "two-point", "attachment",
            "interpolated multi-point", "reaction multi-point", "rigid multi-point", "pinned multi-point", "master-slave multi-point", "user-defined multi-point" };

        /// <summary>True when the binding reads releases, node springs, links and combinations.</summary>
        public bool Extended => extended != null;

        private static string Family(string[] names, int code) => code >= 0 && code < names.Length ? names[code] : "type " + code.ToString(CultureInfo.InvariantCulture);

        public void SkippedBeam(int number, int formulation) => Warn("Straus7BeamFamilyPreserved", "Beam " + Family(BeamFamilies, formulation),
            "Beam elements of type " + Family(BeamFamilies, formulation) + " are not represented by the GPC model: left out with their loads, records preserved.");

        /// <summary>Axes of a truss, cable or spring whose binding gives none: V3 from I to J, V1 horizontal (global Y for a vertical member).</summary>
        public static CoordinateSystem AxialAxes(Point3d i, Point3d j)
        {
            Vector3d z = j - i; double l = Axes.Length(z);
            if (!(l > 1e-9)) throw new InvalidDataException("Degenerate two-node element.");
            z = z / l;
            var x = Math.Abs(z.Z) > 1 - 1e-12 ? new Vector3d(0, 1, 0) : new Vector3d(-z.Y, z.X, 0);
            x = x / Axes.Length(x);
            return new CoordinateSystem(new Point3d(i.X, i.Y, i.Z), x, z.CrossProduct(x), z);
        }

        /// <summary>A spring-damper element (btSpring) is a linear spring between its nodes: axial stiffness along V3, lateral along V1 and V2,
        /// torsional about V3 (St7GetSpringDamperData). Dampings and masses are preserved only.</summary>
        public void SpringElement(int number, string i, string j, CoordinateSystem axes, Straus7Property property, string record)
        {
            if (api != null && new[] { Straus7Attributes.BeamDLL, Straus7Attributes.BeamDLG, Straus7Attributes.BeamDML, Straus7Attributes.BeamCFL, Straus7Attributes.BeamCFG,
                Straus7Attributes.BeamCML, Straus7Attributes.BeamCMG }.Any(a => Attributes(Straus7Entity.Beam, number, a).Length != 0))
                Warn("Straus7SpringLoadsPreserved", "Beam spring-damper", "Loads on spring-damper elements are preserved only: a spring is a node link of the GPC model.");
            var d = property?.SpringData;
            if (d == null || d.Length < 3) { Warn("Straus7SpringPreserved", "Beam spring-damper", "Spring-damper elements without stiffness data are preserved only."); return; }
            Straus7ApiConverter.Finite(d, d.Length, record + " spring data");
            if (d.Skip(3).Any(v => v != 0)) Warn("Straus7SpringDampingPreserved", "Beam spring-damper", "Dampings and masses of spring-damper elements are preserved only.");
            var k = new double[36];
            k[0] = k[7] = d[1] * force / length; k[14] = d[0] * force / length; k[35] = d[2] * force * length;
            batch.NodeLinks.Add(new NodeLinkRecord { I = i, J = j, Spring = new SpringMatrix(k, Frames.At(axes, axes.Origin)) { SourceRecord = record }, Record = record });
        }

        /// <summary>St7GetBeamTRelease3/RRelease3 at End 1 (I) and End 2 (J): fixed, released or partial (stiffness) for translations along and
        /// rotations about the principal axes 1, 2, 3 = GPC V1, V2, V3.</summary>
        private void Releases(int number, BeamRecord beam)
        {
            // One attribute instance per released end; its local number is the end (1 = I, 2 = J). Ends without it are fixed.
            var translated = new HashSet<int>(Attributes(Straus7Entity.Beam, number, Straus7Attributes.BeamTEndRelease).Select(x => x.Local));
            var rotated = new HashSet<int>(Attributes(Straus7Entity.Beam, number, Straus7Attributes.BeamREndRelease).Select(x => x.Local));
            if (translated.Count + rotated.Count == 0) return;
            if (translated.Concat(rotated).Any(e => e != 1 && e != 2)) throw new InvalidDataException("Unknown beam release end: " + beam.Record);
            if (extended == null) { Warn("Straus7ReleasesPreserved", "Beam end releases", "The API binding does not read beam end releases: preserved in the source file."); return; }
            if (beam.Formulation != BeamFormulation.StraightTwoNode)
            { Warn("Straus7AxialReleasePreserved", "Beam end releases", "End releases of trusses and cables are preserved only."); return; }
            var a = beam.CoordinateSystem;
            var release = new BeamReleasesAttribute { Name = "", SourceRecord = beam.Record + "/releases",
                CoordinateSystem = new CoordinateSystem(a.Origin, new Vector3d(a.V1.X, a.V1.Y, a.V1.Z), new Vector3d(a.V2.X, a.V2.Y, a.V2.Z), new Vector3d(a.V3.X, a.V3.Y, a.V3.Z)) };
            bool any = false;
            foreach (var end in new[] { 1, 2 })
            {
                var fixedEnd = new Straus7BeamRelease { Status = new[] { 1, 1, 1 }, Values = new double[3] };
                var t = translated.Contains(end) ? extended.ReadBeamRelease(number, end, false) : fixedEnd;
                var r = rotated.Contains(end) ? extended.ReadBeamRelease(number, end, true) : fixedEnd;
                var target = end == 1 ? release.I : release.J;
                for (int k = 0; k < 6; k++)
                {
                    var data = k < 3 ? t : r; int dof = k % 3;
                    if (data?.Status == null || data.Status.Length != 3 || data.Values == null || data.Values.Length != 3) throw new InvalidDataException("Invalid beam release: " + beam.Record);
                    int status = data.Status[dof]; double value = data.Values[dof];
                    if (status == 1) target[k] = new BeamDofConnection(BeamConnectionKind.Continuous);
                    else if (status == 0) { target[k] = new BeamDofConnection(BeamConnectionKind.Released); any = true; }
                    else if (status == 2)
                    {
                        if (!Straus7ApiConverter.Finite(value) || value < 0) throw new InvalidDataException("Invalid partial release stiffness: " + beam.Record);
                        target[k] = new BeamDofConnection(BeamConnectionKind.AbsoluteStiffness, value * (k < 3 ? force / length : force * length)); any = true;
                    }
                    else throw new InvalidDataException("Unknown release status " + status + ": " + beam.Record);
                }
            }
            if (any) batch.BeamReleases.Add(new BeamReleaseRecord { BeamId = beam.Id, Release = release, Record = release.SourceRecord });
        }

        /// <summary>Node translational/rotational stiffnesses of the single freedom case, as a ground spring in their UCS at the node.</summary>
        private void NodeSprings(int node, Point3d point, int freedomCases, string record)
        {
            var translation = Attributes(Straus7Entity.Node, node, Straus7Attributes.KTranslation);
            var rotation = Attributes(Straus7Entity.Node, node, Straus7Attributes.KRotation);
            if (translation.Length + rotation.Length == 0) return;
            if (extended == null) { Warn("Straus7NodeSpringsPreserved", "Node springs", "The API binding does not read node springs: preserved in the source file."); return; }
            if (freedomCases != 1) { Warn("Straus7FreedomCasesPreserved", "Freedom cases", "Restraints and node springs of models with several freedom cases are not mapped."); return; }
            var parts = new List<(Straus7NodeStiffness Data, bool Rotational)>();
            if (translation.Length != 0) parts.Add((extended.ReadNodeStiffness(node, 1, false), false));
            if (rotation.Length != 0) parts.Add((extended.ReadNodeStiffness(node, 1, true), true));
            foreach (var group in parts.GroupBy(p => p.Data?.Ucs ?? 0))
            {
                var axes = group.Key == 1 ? CoordinateSystem.Global : Ucs(group.Key);
                if (axes == null) { Warn("Straus7SpringUcsPreserved", "Node spring UCS", "Node springs in non-Cartesian coordinate systems are not mapped."); continue; }
                var k = new double[36];
                foreach (var (data, rotational) in group)
                {
                    Straus7ApiConverter.Finite(data.Values, 3, record + " stiffness");
                    if (data.Values.Any(v => v < 0)) throw new InvalidDataException("Negative node stiffness: " + record);
                    for (int c = 0; c < 3; c++) k[(rotational ? 3 + c : c) * 7] = data.Values[c] * (rotational ? force * length : force / length);
                }
                if (k.All(v => v == 0)) continue;
                batch.NodeSprings.Add(new NodeSpringRecord { NodeId = Straus7ApiConverter.Id(node), Spring = new SpringMatrix(k, Frames.At(axes, point)) { SourceRecord = record + "/stiffness" },
                    Record = record + "/stiffness" });
            }
        }

        /// <summary>Rigid links (XYZ, or a plane of the global axes with the nodes in that plane), master-slave links of coincident nodes and
        /// pinned links of coincident nodes become rigid node links; the other link families are preserved.</summary>
        public void Links(int count, IReadOnlyDictionary<int, Point3d> points)
        {
            if (count == 0) return;
            if (extended == null)
            { Warn("Straus7LinksPreserved", "Links", count.ToString(CultureInfo.InvariantCulture) + " links are preserved: the API binding does not read them."); return; }
            for (int l = 1; l <= count; l++)
            {
                var link = extended.ReadLink(l); var record = "Link/" + l.ToString(CultureInfo.InvariantCulture);
                if (link == null || link.Number != l || link.Nodes == null) throw new InvalidDataException("Invalid link: " + record);
                var evidence = Straus7ApiConverter.Evidence(record, link, "Native link; rigid/master-slave/pinned links are also node links of the model."); batch.Uninterpreted.Add(evidence);
                if (link.Nodes.Any(n => !points.ContainsKey(n))) throw new InvalidDataException("Link node missing: " + record);
                bool[] dofs = null; string reason = null;
                bool coincident = link.Nodes.Length == 2 && Axes.Length(points[link.Nodes[1]] - points[link.Nodes[0]]) <= 1e-6;
                if (link.Nodes.Length != 2 || link.Nodes[0] == link.Nodes[1]) reason = "multi-node or degenerate";
                else if (link.Type == 5)
                {
                    if (link.Plane == 0) dofs = Enumerable.Repeat(true, 6).ToArray();
                    else if (link.Plane >= 1 && link.Plane <= 3 && (link.Ucs == 1 || IsGlobal(Ucs(link.Ucs))))
                    {
                        // In-plane rigid body of the XY, YZ or ZX plane; the lever arm must lie in that plane.
                        int normal = link.Plane == 1 ? 2 : link.Plane == 2 ? 0 : 1;
                        Vector3d arm = points[link.Nodes[1]] - points[link.Nodes[0]];
                        double off = normal == 0 ? arm.X : normal == 1 ? arm.Y : arm.Z;
                        if (Math.Abs(off) <= 1e-6 * Math.Max(1, Axes.Length(arm)))
                        {
                            dofs = new bool[6];
                            for (int k = 0; k < 3; k++) if (k != normal) dofs[k] = true;
                            dofs[3 + normal] = true;
                        }
                        else reason = "planar rigid link whose nodes are not in its plane";
                    }
                    else reason = "planar rigid link in a rotated UCS";
                }
                else if (link.Type == 1)
                {
                    if (!coincident) reason = "master-slave link of distinct nodes (equal displacements without lever arm)";
                    else if (link.Dofs == null || link.Dofs.Length != 6) throw new InvalidDataException("Invalid master-slave DOFs: " + record);
                    else if (link.Dofs.All(d => d != 0) || link.Ucs == 1 || IsGlobal(Ucs(link.Ucs))) dofs = link.Dofs.Select(d => d != 0).ToArray();
                    else reason = "master-slave link with some DOFs in a rotated UCS";
                }
                else if (link.Type == 4)
                {
                    if (coincident) dofs = new[] { true, true, true, false, false, false };
                    else reason = "pinned link of distinct nodes";
                }
                else reason = Family(LinkFamilies, link.Type) + " link";
                if (dofs != null && dofs.Any(d => d))
                {
                    evidence.UnsupportedReason = "Mapped as a rigid node link.";
                    batch.NodeLinks.Add(new NodeLinkRecord { I = Straus7ApiConverter.Id(link.Nodes[0]), J = Straus7ApiConverter.Id(link.Nodes[1]), RigidDofs = dofs, Record = record });
                }
                else if (reason != null) Warn("Straus7LinkPreserved", "Link " + reason, "Links (" + reason + ") are preserved without a rigid equivalent.");
            }
        }

        private static bool IsGlobal(CoordinateSystem axes) => axes != null && Math.Abs(axes.V1.X - 1) <= 1e-12 && Math.Abs(axes.V2.Y - 1) <= 1e-12 && Math.Abs(axes.V3.Z - 1) <= 1e-12;

        /// <summary>LSA combinations as linear combinations of the load cases (factors of freedom case 1); combinations of LSA combinations
        /// are preserved only.</summary>
        public void Combinations(int cases)
        {
            if (api == null || extended == null) return;
            int count = extended.CombinationCount, freedom = api.FreedomCaseCount;
            if (count < 0 || count > 100000) throw new InvalidDataException("Invalid LSA combination count.");
            for (int c = 1; c <= count; c++)
            {
                var data = extended.ReadCombination(c, cases, freedom); var record = "LSACombination/" + c.ToString(CultureInfo.InvariantCulture);
                if (data == null || data.Number != c || data.Factors == null || data.Factors.Length != cases * freedom) throw new InvalidDataException("Invalid LSA combination: " + record);
                Straus7ApiConverter.Finite(data.Factors, data.Factors.Length, record);
                batch.Uninterpreted.Add(Straus7ApiConverter.Evidence(record, data, "Native LSA combination factors by load case and freedom case."));
                if (Enumerable.Range(0, cases).Any(l => Enumerable.Range(1, freedom - 1).Any(f => data.Factors[l * freedom + f] != 0)))
                { Warn("Straus7CombinationFreedomCasesPreserved", "LSA combinations", "Combinations of several freedom cases are preserved only."); continue; }
                var definition = new CombinationRecord { Id = "LSA/" + c.ToString(CultureInfo.InvariantCulture), Name = string.IsNullOrWhiteSpace(data.Name) ? record : data.Name.Trim(),
                    Kind = CombinationKind.Linear, Status = data.Enabled ? "ACTIVE" : "INACTIVE", Source = "LSA", Record = record };
                for (int l = 1; l <= cases; l++)
                {
                    double factor = data.Factors[(l - 1) * freedom];
                    if (factor != 0) definition.Terms.Add(new CombinationTermRecord { Name = Case(l, record), IsStaticCase = true, Analysis = "LSA", Factor = factor });
                }
                if (definition.Terms.Count == 0) { Warn("Straus7EmptyCombinationPreserved", "LSA combinations", "LSA combinations without factors are preserved only."); continue; }
                batch.Combinations.Add(definition);
            }
            if (extended.CombinedCombinationCount > 0)
                Warn("Straus7CombinedCombinationsPreserved", "Combined LSA combinations", "Combinations of LSA combinations are preserved in the source file, not mapped.");
        }
    }
}
