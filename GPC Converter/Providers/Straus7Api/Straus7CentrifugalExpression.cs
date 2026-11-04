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
    /// in this dialect as "Straus7 API" preserved data, referenced by the law through its record and content SHA-256.
    /// <para>Offset (user decision 6 of 4/11; measured on R31, review L10): R31 applies the offset expression to every force of a vehicle that
    /// applies the centrifugal force (flag set, path axes), in the force unit of the expression per point force and per mm of a distributed force
    /// (along the path, in the length of the model whatever the length unit of the expression: 0.05 with m and kN gives 5000 N on 100 mm); the
    /// factor multiplies the vertical component of each of those forces. GPC's <see cref="MovingCentrifugalLaw.OffsetForce"/> is the radial force on
    /// the whole vehicle (N). The writer therefore divides it by the <see cref="OffsetMeasure">measure</see> of the vehicles (their flagged point
    /// forces plus the length of their flagged distributed forces, in mm), writing "offset/measure"; the reader multiplies back, exactly when it reads
    /// that text. A law whose offset cannot be the same N per vehicle (vehicles of different measures, a flagged distributed force of variable
    /// loaded length) stays in the R3 dialect as adapter data.</para></summary>
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

        /// <summary>Native expressions of a GPC law for vehicles of one point force: the offset is written as it is, per force (R31). For a
        /// template use <see cref="FromDomain(MovingCentrifugalLaw, global::GPC.Model.Models.Model, MovingLoadTemplate)"/>, which converts the
        /// offset of the vehicle to the forces of its vehicles.</summary>
        public static Straus7CentrifugalExpression FromDomain(MovingCentrifugalLaw law, global::GPC.Model.Models.Model model) => FromDomain(law, model, null);

        /// <summary>Native expressions of a GPC law. N-mm laws give the offset in N and the factor as a constant or V^2/(g*R) in mm and N;
        /// an adapter-defined law is read back verbatim from its preserved record, after checking the content hash. The offset of the vehicle
        /// (N) is divided by the <see cref="OffsetMeasure"/> of the vehicles of <paramref name="template"/> and written as "offset/measure"
        /// (as it is when the measure is 1), because R31 applies it to every flagged force: N per point force, N/mm of distributed force.</summary>
        /// <exception cref="NotSupportedException">StrausCentrifugalOffsetNotPerVehicle: a nonzero offset and vehicles of different measures, or
        /// a flagged distributed force of variable loaded length (R31 has one offset per template).</exception>
        public static Straus7CentrifugalExpression FromDomain(MovingCentrifugalLaw law, global::GPC.Model.Models.Model model, MovingLoadTemplate template)
        {
            if (law == null) throw new ArgumentNullException(nameof(law)); law.Validate();
            Straus7CentrifugalExpression result;
            if (law.FactorKind == MovingCentrifugalFactorKind.AdapterDefined)
            {
                var record = model?.PreservedSourceData.Where(p => p != null && p.Kind == "Straus7 API" && p.SourceRecord == law.AdapterRecord).ToArray() ?? new PreservedAssignment[0];
                if (record.Length != 1 || Hash(record[0].RawData) != law.AdapterContentHash) throw new ArgumentException("MissingOrChangedStrausCentrifugalExpression: " + law.AdapterRecord);
                result = Parse(record[0].RawData);
            }
            else
            {
                string offset = law.OffsetForce.ToString("R", CultureInfo.InvariantCulture);
                if (law.OffsetForce != 0 && template != null)
                {
                    double measure = OffsetMeasure(template, out string reason) ?? throw new NotSupportedException("StrausCentrifugalOffsetNotPerVehicle: " + template.Name + ": " + reason
                        + "; R31 applies one offset per point force (N) and per mm of distributed force, so " + law.OffsetForce.ToString("R", CultureInfo.InvariantCulture) + " N per vehicle has no R31 expression.");
                    if (measure != 1) offset += "/" + measure.ToString("R", CultureInfo.InvariantCulture);
                }
                result = new Straus7CentrifugalExpression {
                    OffsetExpression = offset,
                    FactorExpression = law.FactorKind == MovingCentrifugalFactorKind.SpeedSquaredOverGravityRadius ? SpeedLaws[0] : law.Factor.ToString("R", CultureInfo.InvariantCulture),
                    LengthUnit = "mm", ForceUnit = "N" };
            }
            result.LimitFactor = law.LimitFactor; result.MinimumFactor = law.MinimumFactor ?? 0; result.MaximumFactor = law.MaximumFactor ?? 0;
            return result;
        }

        /// <summary>What R31 multiplies a per-force offset by on one vehicle (review L10, native): the number of its point forces that apply the
        /// centrifugal force (flag set, path axes: R31 gives none to a force in global axes) plus the length along the path of its distributed
        /// forces that apply it, in mm: R31 applies the offset per unit length of the model, whatever the length unit of the expression (native,
        /// review L10), and GPC templates are in mm. Vehicles without such forces carry no centrifugal force and do not count;
        /// with none at all the measure is 1. Null, with the <paramref name="reason"/>, when the vehicles that count have different measures or
        /// a flagged distributed force has a loaded length that depends on its position (leading, trailing, full length).</summary>
        public static double? OffsetMeasure(MovingLoadTemplate template, out string reason)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            double? measure = null; string first = null; reason = null;
            foreach (var vehicle in template.Vehicles ?? new MovingVehicle[0])
            {
                if (vehicle == null) continue;
                double value = (vehicle.PointForces ?? new MovingPointForce[0]).Count(f => f != null && f.ApplyCentrifugal == true && f.Axes == MovingLoadAxes.Path);
                foreach (var force in (vehicle.DistributedForces ?? new MovingDistributedForce[0]).Where(f => f != null && f.ApplyCentrifugal == true && f.Axes == MovingLoadAxes.Path))
                {
                    if (force.Mobility != MovingDistributionMobility.Grouped && force.Mobility != MovingDistributionMobility.Floating)
                    { reason = "vehicle " + vehicle.Name + " has a distributed force with the centrifugal flag and a " + force.Mobility + " loaded length"; return null; }
                    value += force.X2 - force.X1;
                }
                if (value == 0) continue;
                if (measure == null) { measure = value; first = vehicle.Name; }
                else if (Math.Abs(value - measure.Value) > 1e-12 * Math.Max(value, measure.Value))
                {
                    reason = "vehicles " + first + " and " + vehicle.Name + " have centrifugal measures " + measure.Value.ToString("R", CultureInfo.InvariantCulture) + " and "
                        + value.ToString("R", CultureInfo.InvariantCulture) + " (point forces plus distributed length in mm)";
                    return null;
                }
            }
            return measure ?? 1;
        }

        /// <summary>The GPC law of an expression read without its template. The offset of R31 is per force: without the vehicles it cannot be
        /// expressed as N per vehicle, so a nonzero offset keeps the law in the R3 dialect (adapter-defined, written back verbatim). Use
        /// <see cref="ToDomain(MovingLoadTemplate, out PreservedAssignment)"/> for a template.</summary>
        public MovingCentrifugalLaw ToDomain(out PreservedAssignment record) => ToDomain(null, out record);

        /// <summary>The GPC law of the expression of <paramref name="template"/>. Constants are converted from the declared force unit and
        /// V^2/(g*R) is dimensionless in any consistent unit set. The offset per force becomes the offset of the vehicle: the text "a/b" that the
        /// writer gives with b the <see cref="OffsetMeasure"/> of the vehicles gives a exactly, another constant is multiplied by the measure. Any
        /// other expression, or a nonzero offset that is not the same N per vehicle, gives an adapter-defined law and the <paramref name="record"/>
        /// to store with the model.</summary>
        public MovingCentrifugalLaw ToDomain(MovingLoadTemplate template, out PreservedAssignment record)
        {
            int length = LengthUnits.ToList().IndexOf(LengthUnit), force = ForceUnits.ToList().IndexOf(ForceUnit);
            if (length < 0 || force < 0) throw new NotSupportedException("UnsupportedCentrifugalExpressionUnits: " + LengthUnit + "/" + ForceUnit);
            string factor = (FactorExpression ?? "").Replace(" ", ""); record = null;
            var law = new MovingCentrifugalLaw { LimitFactor = LimitFactor, MinimumFactor = MinimumFactor, MaximumFactor = MaximumFactor };
            if (VehicleOffset(template, out double offset) && (SpeedLaws.Contains(factor) || TryConstant(factor, out _)))
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

        /// <summary>The offset of one vehicle, in the declared force unit: zero for an empty or zero offset; with a template, the text "a/b" with
        /// b its <see cref="OffsetMeasure"/> gives a (the writer's text, exactly), another constant or quotient of constants is multiplied by the
        /// measure. False for any other text, for a nonzero offset without a template, or when the vehicles have no common measure.</summary>
        private bool VehicleOffset(MovingLoadTemplate template, out double offset)
        {
            offset = 0; string text = (OffsetExpression ?? "").Replace(" ", ""); double divisor = 1;
            if (!TryConstant(text, out double numerator))
            {
                int slash = text.IndexOf('/');
                if (slash <= 0 || slash != text.LastIndexOf('/') || slash == text.Length - 1 || !TryConstant(text.Substring(0, slash), out numerator)
                    || !TryConstant(text.Substring(slash + 1), out divisor) || divisor == 0) return false;
            }
            if (numerator == 0) return true;
            double? measure = template == null ? null : OffsetMeasure(template, out _);
            if (measure == null) return false;
            offset = divisor == measure.Value ? numerator : numerator / divisor * measure.Value;
            return true;
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
