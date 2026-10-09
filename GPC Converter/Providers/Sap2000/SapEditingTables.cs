using System;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Text;
using GPC.Geometry;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter
{
    /// <summary>Reader for the legacy editing-array layout inspected in FemToRhino/CSI/SapInteractiveDatabaseModel.cs.
    /// This is an array adapter, not a claimed parser for arbitrary SAP exports.</summary>
    public static class SapEditingTables
    {
        public static ImportBatch Read(string[] joints9, string[] frames5, string[] areas6, string modelRevision,
            string solverVersion, double lengthToMm, bool globalCartesianCoordinatesConfirmed, CultureInfo numericCulture)
        {
            if (!globalCartesianCoordinatesConfirmed) throw new NotSupportedException("The source coordinate basis must be confirmed before mapping.");
            if (double.IsNaN(lengthToMm) || double.IsInfinity(lengthToMm) || lengthToMm <= 0) throw new ArgumentOutOfRangeException(nameof(lengthToMm));
            if (numericCulture == null) throw new ArgumentNullException(nameof(numericCulture));
            Check(joints9, 9); Check(frames5, 5); Check(areas6, 6);
            var batch = new ImportBatch { Program = "SAP2000", ModelRevision = modelRevision, SolverVersion = solverVersion };
            // Preserve every source field, including columns whose semantics are not yet mapped.
            // This length-prefixed encoding describes the supplied strings, not a SAP file format.
            using (var raw = new MemoryStream())
            {
                using (var writer = new BinaryWriter(raw, Encoding.UTF8, true))
                {
                    foreach (var table in new[] { joints9, frames5, areas6 })
                    {
                        writer.Write(table.Length);
                        foreach (var field in table) { writer.Write(field != null); if (field != null) writer.Write(field); }
                    }
                }
                raw.Position = 0; batch.SourceHash = SourceEvidence.Sha256(raw);
                batch.Uninterpreted.Add(new PreservedAssignment
                {
                    Kind = "SAP legacy editing arrays; length-prefixed UTF8 strings v1",
                    RawData = Convert.ToBase64String(raw.ToArray()),
                    SourceRecord = batch.SourceHash,
                    UnitsAndAxes = "Input length factor to mm=" + lengthToMm.ToString("R", CultureInfo.InvariantCulture) + "; global Cartesian confirmed; culture=" + numericCulture.Name,
                    UnsupportedReason = "Only node positions and frame/area connectivity are mapped; all original fields retained."
                });
            }
            for (int r = 0; r < joints9.Length; r += 9)
            {
                double x = Number(joints9[r + 3], numericCulture), y = Number(joints9[r + 4], numericCulture), z = Number(joints9[r + 6], numericCulture);
                batch.Nodes.Add(new NodeRecord { Id = joints9[r], GlobalPosition = new Point3d(x * lengthToMm, y * lengthToMm, z * lengthToMm), Record = "Joint Coordinates:" + (r / 9 + 1) });
            }
            for (int r = 0; r < frames5.Length; r += 5) batch.Beams.Add(new BeamRecord { Id = frames5[r], I = frames5[r + 1], J = frames5[r + 2], Record = "Connectivity - Frame:" + (r / 5 + 1) });
            for (int r = 0; r < areas6.Length; r += 6)
            {
                var nodes = areas6.Skip(r + 1).Take(4).ToArray();
                if (string.IsNullOrWhiteSpace(nodes[3])) nodes = nodes.Take(3).ToArray();
                if (nodes.Any(string.IsNullOrWhiteSpace)) throw new FormatException("Incomplete Connectivity - Area row " + (r / 6 + 1));
                batch.Shells.Add(new ShellRecord { Id = areas6[r], Nodes = nodes, Record = "Connectivity - Area:" + (r / 6 + 1) });
            }
            return batch;
        }
        private static void Check(string[] rows, int width) { if (rows == null || rows.Length % width != 0) throw new FormatException("Incomplete SAP editing table; expected row width " + width); }
        private static double Number(string value, CultureInfo culture)
        {
            if (!double.TryParse(value, NumberStyles.Float, culture, out double result) || double.IsNaN(result) || double.IsInfinity(result)) throw new FormatException("Invalid numeric source field: " + value);
            return result;
        }
    }
}
