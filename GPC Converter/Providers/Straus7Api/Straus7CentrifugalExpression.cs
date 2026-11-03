using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using GPC.Model.Actions.Moving;
using GPC.Model.Structure.Assignments;

namespace GPC.Converter.Straus7
{
    /// <summary>Centrifugal law of a Straus7 R3 load-path template: expression text in the R3 dialect with its declared length and
    /// force units (St7Get/SetLoadPathTemplateCentrifugalData). The GPC model keeps only N-mm data (<see cref="MovingCentrifugalLaw"/>):
    /// constants and V²/(g R) become N-mm values; any other R3 expression (for example the radius-dependent library laws) stays
    /// in this dialect as "Straus7 API" preserved data, referenced by the law through its record and content SHA-256.</summary>
    public sealed class Straus7CentrifugalExpression
    {
        public const string Language = "Straus7-R3", RecordPrefix = "MovingCentrifugal/";
        /// <summary>R3 unit codes: index in these arrays.</summary>
        public static readonly IReadOnlyList<string> LengthUnits = new[] { "m", "cm", "mm", "ft", "in" }, ForceUnits = new[] { "N", "kN", "MN", "kgf", "lbf", "tf", "kip" };
        private static readonly double[] Newtons = { 1, 1e3, 1e6, 9.80665, 4.4482216152605, 9806.65, 4448.2216152605 };
        private static readonly string[] SpeedLaws = { "V^2/(g*R)", "V^2/(R*g)", "V*V/(g*R)", "V^2/g/R" };
        public string OffsetExpression { get; set; } = "";
        public string FactorExpression { get; set; } = "";
        public string LengthUnit { get; set; } = "mm";
        public string ForceUnit { get; set; } = "N";
        public bool LimitFactor { get; set; }
        public double MinimumFactor { get; set; }
        public double MaximumFactor { get; set; }
        public bool IsEmpty => string.IsNullOrWhiteSpace(OffsetExpression) && string.IsNullOrWhiteSpace(FactorExpression);

        /// <summary>Native expressions of a GPC law. N-mm laws give the offset in N and the factor as a constant or V^2/(g*R) in mm and N;
        /// an adapter-defined law is read back verbatim from its preserved record, after checking the content hash.</summary>
        public static Straus7CentrifugalExpression FromDomain(MovingCentrifugalLaw law, global::GPC.Model.Models.Model model)
        {
            if (law == null) throw new ArgumentNullException(nameof(law)); law.Validate();
            Straus7CentrifugalExpression result;
            if (law.FactorKind == MovingCentrifugalFactorKind.AdapterDefined)
            {
                var record = model?.PreservedSourceData.Where(p => p != null && p.Kind == "Straus7 API" && p.SourceRecord == law.AdapterRecord).ToArray() ?? new PreservedAssignment[0];
                if (record.Length != 1 || Hash(record[0].RawData) != law.AdapterContentHash) throw new ArgumentException("MissingOrChangedStrausCentrifugalExpression: " + law.AdapterRecord);
                result = Parse(record[0].RawData);
            }
            else result = new Straus7CentrifugalExpression {
                OffsetExpression = law.OffsetForce.ToString("R", CultureInfo.InvariantCulture),
                FactorExpression = law.FactorKind == MovingCentrifugalFactorKind.SpeedSquaredOverGravityRadius ? SpeedLaws[0] : law.Factor.ToString("R", CultureInfo.InvariantCulture),
                LengthUnit = "mm", ForceUnit = "N" };
            result.LimitFactor = law.LimitFactor; result.MinimumFactor = law.MinimumFactor ?? 0; result.MaximumFactor = law.MaximumFactor ?? 0;
            return result;
        }

        /// <summary>The GPC law. Constants are converted from the declared force unit and V^2/(g*R) is dimensionless in any consistent
        /// unit set; any other expression gives an adapter-defined law and the <paramref name="record"/> to store with the model.</summary>
        public MovingCentrifugalLaw ToDomain(out PreservedAssignment record)
        {
            int length = LengthUnits.ToList().IndexOf(LengthUnit), force = ForceUnits.ToList().IndexOf(ForceUnit);
            if (length < 0 || force < 0) throw new NotSupportedException("UnsupportedCentrifugalExpressionUnits: " + LengthUnit + "/" + ForceUnit);
            string factor = (FactorExpression ?? "").Replace(" ", ""); record = null;
            var law = new MovingCentrifugalLaw { LimitFactor = LimitFactor, MinimumFactor = MinimumFactor, MaximumFactor = MaximumFactor };
            if (TryConstant(OffsetExpression, out double offset) && (SpeedLaws.Contains(factor) || TryConstant(factor, out _)))
            {
                law.OffsetForce = offset * Newtons[force];
                if (SpeedLaws.Contains(factor)) law.FactorKind = MovingCentrifugalFactorKind.SpeedSquaredOverGravityRadius;
                else { TryConstant(factor, out double value); law.Factor = value; }
            }
            else
            {
                string content = Content(); string hash = Hash(content);
                record = new PreservedAssignment { Kind = "Straus7 API", SourceRecord = RecordPrefix + hash, RawData = content,
                    UnsupportedReason = "Straus7 R3 centrifugal expression with its units; not representable as N-mm data, written verbatim" };
                law.FactorKind = MovingCentrifugalFactorKind.AdapterDefined; law.AdapterRecord = record.SourceRecord; law.AdapterContentHash = hash;
            }
            law.Validate(); return law;
        }

        /// <summary>Canonical record content: dialect, units and the two expressions, one per line.</summary>
        private string Content() => string.Join("\n", Language, "length=" + LengthUnit, "force=" + ForceUnit, "offset=" + (OffsetExpression ?? ""), "factor=" + (FactorExpression ?? ""));
        private static Straus7CentrifugalExpression Parse(string content)
        {
            var lines = (content ?? "").Split('\n');
            if (lines.Length != 5 || lines[0] != Language || !lines[1].StartsWith("length=", StringComparison.Ordinal) || !lines[2].StartsWith("force=", StringComparison.Ordinal)
                || !lines[3].StartsWith("offset=", StringComparison.Ordinal) || !lines[4].StartsWith("factor=", StringComparison.Ordinal))
                throw new ArgumentException("InvalidStrausCentrifugalRecord");
            var result = new Straus7CentrifugalExpression { LengthUnit = lines[1].Substring(7), ForceUnit = lines[2].Substring(6), OffsetExpression = lines[3].Substring(7), FactorExpression = lines[4].Substring(7) };
            if (!LengthUnits.Contains(result.LengthUnit) || !ForceUnits.Contains(result.ForceUnit)) throw new ArgumentException("InvalidStrausCentrifugalRecordUnits");
            return result;
        }
        private static string Hash(string content)
        { using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(content ?? ""))).Replace("-", ""); }
        private static bool TryConstant(string text, out double value)
        {
            value = 0; if (string.IsNullOrWhiteSpace(text)) return true;
            return double.TryParse(text.Trim(), NumberStyles.Float, CultureInfo.InvariantCulture, out value) && !double.IsNaN(value) && !double.IsInfinity(value);
        }
    }
}
