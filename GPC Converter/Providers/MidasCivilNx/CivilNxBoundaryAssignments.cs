using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Attributes;
using GPC.Model.Core.Coordinates;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.CivilNx
{
    public sealed partial class CivilNxModelProfile
    {
        private sealed partial class Reader
        {
            private void BoundaryAssignments()
            {
                if (Has("STAG"))
                {
                    foreach (var table in new[] { "FRLS", "RIGD", "ELNK" })
                        if (Has(table)) Warn("CivilNxStagedBoundaryPreserved", table, "Construction-stage activation is not resolved; the original boundary table is preserved only.");
                    return;
                }
                Releases(); RigidLinks(); ElasticLinks();
            }

            private void Releases()
            {
                foreach (var pair in Rows<ItemList<ReleaseData>>("FRLS"))
                {
                    var record = "FRLS/" + pair.Key;
                    if (!beams.TryGetValue(pair.Key, out var beam)) throw new InvalidDataException("ReleaseWithoutBeam: " + record);
                    if (beam.Formulation != BeamFormulation.StraightTwoNode)
                    { Warn("CivilNxNonBeamReleasePreserved", record, "End releases on an axial-only or other non-beam formulation are preserved without assigning flexural DOFs."); continue; }
                    var items = pair.Value?.Items;
                    if (items == null || items.Length != 1)
                    { Warn("CivilNxReleaseScopePreserved", record, "Multiple or missing release assignments require explicit boundary-group selection."); continue; }
                    var item = items[0];
                    var i = Connections(item?.FlagI, item?.ValuesI, item?.Absolute ?? false, record);
                    var j = Connections(item?.FlagJ, item?.ValuesJ, item?.Absolute ?? false, record);
                    if (i == null || j == null) { Warn("CivilNxWarpingReleasePreserved", record, "The seventh release DOF is outside the six-DOF contract; the entire assignment is preserved only."); continue; }
                    var a = beam.CoordinateSystem;
                    var release = new BeamReleasesAttribute { Name = item.Group ?? "", SourceRecord = record,
                        CoordinateSystem = new CoordinateSystem(a.Origin, new Vector3d(a.V3.X, a.V3.Y, a.V3.Z), new Vector3d(a.V1.X, a.V1.Y, a.V1.Z), new Vector3d(a.V2.X, a.V2.Y, a.V2.Z)) };
                    for (int k = 0; k < 6; k++) { release.I[k] = i[k]; release.J[k] = j[k]; }
                    batch.BeamReleases.Add(new BeamReleaseRecord { BeamId = pair.Key, Release = release, Record = record });
                }
            }

            private BeamDofConnection[] Connections(string flags, double[] values, bool absolute, string record)
            {
                if (flags == null || (flags.Length != 6 && flags.Length != 7) || flags.Any(c => c != '0' && c != '1') ||
                    values != null && (values.Length != 6 && values.Length != 7 || values.Any(v => double.IsNaN(v) || double.IsInfinity(v) || v < 0)))
                    throw new InvalidDataException("InvalidCivilNxRelease: " + record);
                if (flags.Length == 7 && flags[6] != '0' || values?.Length == 7 && values[6] != 0) return null;
                var result = new BeamDofConnection[6];
                for (int k = 0; k < 6; k++)
                {
                    double value = values == null ? 0 : values[k];
                    if (!absolute && value > 1) throw new InvalidDataException("InvalidRelativeFixity: " + record);
                    result[k] = flags[k] == '0' ? new BeamDofConnection(BeamConnectionKind.Continuous)
                        : value == 0 ? new BeamDofConnection(BeamConnectionKind.Released)
                        : new BeamDofConnection(absolute ? BeamConnectionKind.AbsoluteStiffness : BeamConnectionKind.RelativeFixity, value * (absolute ? k < 3 ? force / length : force * length : 1));
                }
                return result;
            }

            private void RigidLinks()
            {
                foreach (var pair in Rows<ItemList<RigidData>>("RIGD"))
                {
                    string record = "RIGD/" + pair.Key;
                    if (!points.ContainsKey(pair.Key) || pair.Value?.Items == null) throw new InvalidDataException("InvalidRigidLink: " + record);
                    foreach (var item in pair.Value.Items)
                    {
                        string mask = item.Dofs.ToString("D6", System.Globalization.CultureInfo.InvariantCulture);
                        if (mask.Length != 6 || mask.Any(c => c != '0' && c != '1') || item.Nodes == null || item.Nodes.Length == 0 || item.Nodes.Distinct().Count() != item.Nodes.Length)
                            throw new InvalidDataException("InvalidRigidLinkDofs: " + record);
                        foreach (int node in item.Nodes)
                        {
                            string second = node.ToString(System.Globalization.CultureInfo.InvariantCulture);
                            if (!points.ContainsKey(second) || second == pair.Key) throw new InvalidDataException("InvalidRigidLinkNode: " + record);
                            if (nodeAxes.ContainsKey(pair.Key) || nodeAxes.ContainsKey(second) || unresolvedNodeAxes.Contains(pair.Key) || unresolvedNodeAxes.Contains(second))
                            { Warn("CivilNxSkewRigidLinkPreserved", record, "Rigid links involving local nodal axes require a dedicated equation transform."); continue; }
                            if (mask.Contains('1')) batch.NodeLinks.Add(new NodeLinkRecord { I = pair.Key, J = second, RigidDofs = mask.Select(c => c == '1').ToArray(), Record = record + "/" + item.Id });
                        }
                    }
                }
            }

            private void ElasticLinks()
            {
                foreach (var pair in Rows<ElasticData>("ELNK"))
                {
                    string record = "ELNK/" + pair.Key; var item = pair.Value;
                    if (item?.Nodes == null || item.Nodes.Length != 2 || item.Nodes[0] == item.Nodes[1]) throw new InvalidDataException("InvalidElasticLinkNodes: " + record);
                    string first = item.Nodes[0].ToString(), second = item.Nodes[1].ToString();
                    if (!points.ContainsKey(first) || !points.ContainsKey(second)) throw new InvalidDataException("DanglingElasticLink: " + record);
                    if (item.Kind == "RIGID")
                    {
                        if (nodeAxes.ContainsKey(first) || nodeAxes.ContainsKey(second) || unresolvedNodeAxes.Contains(first) || unresolvedNodeAxes.Contains(second))
                        { Warn("CivilNxSkewRigidLinkPreserved", record, "Rigid links involving local nodal axes require a dedicated equation transform."); continue; }
                        batch.NodeLinks.Add(new NodeLinkRecord { I = first, J = second, RigidDofs = Enumerable.Repeat(true, 6).ToArray(), Record = record }); continue;
                    }
                    if (item.Kind != "GEN" || item.Shear || item.Rigid?.Any(v => v) == true)
                    { Warn("CivilNxElasticLinkLawPreserved", record, "Nonlinear, shear-coupled or partly rigid link law is preserved without an equivalent linear spring."); continue; }
                    if (item.Stiffness == null || item.Stiffness.Length != 6 || item.Stiffness.Any(v => v < 0 || double.IsNaN(v) || double.IsInfinity(v)) || item.Rigid != null && item.Rigid.Length != 6)
                        throw new InvalidDataException("InvalidElasticLinkStiffness: " + record);
                    CoordinateSystem axes;
                    try { axes = MidasConventions.BeamAxes(points[first], points[second], item.Angle); }
                    catch (ArgumentException) { Warn("CivilNxElasticLinkAxesPreserved", record, "Coincident or near-vertical nodes require explicit link axes."); continue; }
                    var midas = new CoordinateSystem(axes.Origin, new Vector3d(axes.V3.X, axes.V3.Y, axes.V3.Z), new Vector3d(axes.V1.X, axes.V1.Y, axes.V1.Z), new Vector3d(axes.V2.X, axes.V2.Y, axes.V2.Z));
                    var matrix = new double[36];
                    for (int k = 0; k < 6; k++) matrix[k * 6 + k] = item.Stiffness[k] * (k < 3 ? force / length : force * length);
                    batch.NodeLinks.Add(new NodeLinkRecord { I = first, J = second, Spring = new SpringMatrix(matrix, midas) { SourceRecord = record }, Record = record });
                }
            }
        }
        [DataContract] private sealed class ReleaseData
        {
            [DataMember(Name = "GROUP_NAME")] public string Group { get; set; }
            [DataMember(Name = "bVALUE")] public bool Absolute { get; set; }
            [DataMember(Name = "FLAG_I")] public string FlagI { get; set; }
            [DataMember(Name = "FLAG_J")] public string FlagJ { get; set; }
            [DataMember(Name = "VALUE_I")] public double[] ValuesI { get; set; }
            [DataMember(Name = "VALUE_J")] public double[] ValuesJ { get; set; }
        }
        [DataContract] private sealed class RigidData
        {
            [DataMember(Name = "ID")] public int Id { get; set; }
            [DataMember(Name = "DOF")] public int Dofs { get; set; }
            [DataMember(Name = "S_NODE")] public int[] Nodes { get; set; }
        }
        [DataContract] private sealed class ElasticData
        {
            [DataMember(Name = "NODE")] public int[] Nodes { get; set; }
            [DataMember(Name = "LINK")] public string Kind { get; set; }
            [DataMember(Name = "ANGLE")] public double Angle { get; set; }
            [DataMember(Name = "R_S")] public bool[] Rigid { get; set; }
            [DataMember(Name = "SDR")] public double[] Stiffness { get; set; }
            [DataMember(Name = "bSHEAR")] public bool Shear { get; set; }
        }
    }
}
