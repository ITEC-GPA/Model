using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
    /// <summary>Geotechnical verification to which a combination and a resistance factor apply.</summary>
    public enum GeotechnicalCheck
    {
        ShallowFoundationBearing, ShallowFoundationSliding, RetainingWallBearing, RetainingWallSliding, RetainingWallOverturning,
        RetainingWallPassiveResistance, GlobalStability, SlopeStability, PileBase, PileShaftCompression, PileTotalCompression, PileShaftTension, PileTransverse
    }

    /// <summary>Design situation: persistent/transient (static) or seismic.</summary>
    public enum GeotechnicalSituation { PersistentTransient, Seismic }

    /// <summary>Partial factors on actions of a set (A1, A2, EQU). G = structural permanent, G2 = non-structural permanent, Q = variable.</summary>
    [Serializable]
    public sealed class GeotechnicalActionFactors : IEquatable<GeotechnicalActionFactors>
    {
        public string Name { get; }
        public double PermanentUnfavourable { get; }
        public double PermanentFavourable { get; }
        public double NonStructuralUnfavourable { get; }
        public double NonStructuralFavourable { get; }
        public double VariableUnfavourable { get; }
        public double VariableFavourable { get; }
        public GeotechnicalActionFactors(string name, double permanentUnfavourable, double permanentFavourable, double nonStructuralUnfavourable,
            double nonStructuralFavourable, double variableUnfavourable, double variableFavourable)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
            if (new[] { permanentUnfavourable, permanentFavourable, nonStructuralUnfavourable, nonStructuralFavourable, variableUnfavourable, variableFavourable }
                .Any(v => double.IsNaN(v) || double.IsInfinity(v) || v < 0)) throw new ArgumentOutOfRangeException(nameof(permanentUnfavourable));
            Name = name; PermanentUnfavourable = permanentUnfavourable; PermanentFavourable = permanentFavourable; NonStructuralUnfavourable = nonStructuralUnfavourable;
            NonStructuralFavourable = nonStructuralFavourable; VariableUnfavourable = variableUnfavourable; VariableFavourable = variableFavourable;
        }
        public bool Equals(GeotechnicalActionFactors o) => o != null && Name == o.Name && PermanentUnfavourable == o.PermanentUnfavourable && PermanentFavourable == o.PermanentFavourable
            && NonStructuralUnfavourable == o.NonStructuralUnfavourable && NonStructuralFavourable == o.NonStructuralFavourable
            && VariableUnfavourable == o.VariableUnfavourable && VariableFavourable == o.VariableFavourable;
        public override bool Equals(object obj) => Equals(obj as GeotechnicalActionFactors);
        public override int GetHashCode() => unchecked(Name.GetHashCode() * 31 + PermanentUnfavourable.GetHashCode());
    }

    /// <summary>Partial factors on soil parameters of a set (M1, M2): tan φ', c', cu, qu and unit weight.</summary>
    [Serializable]
    public sealed class GeotechnicalMaterialFactors : IEquatable<GeotechnicalMaterialFactors>
    {
        public string Name { get; }
        public double TanFrictionAngle { get; }
        public double EffectiveCohesion { get; }
        public double UndrainedShearStrength { get; }
        public double UnconfinedStrength { get; }
        public double UnitWeight { get; }
        public GeotechnicalMaterialFactors(string name, double tanFrictionAngle, double effectiveCohesion, double undrainedShearStrength, double unconfinedStrength, double unitWeight)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name required.", nameof(name));
            if (new[] { tanFrictionAngle, effectiveCohesion, undrainedShearStrength, unconfinedStrength, unitWeight }.Any(v => double.IsNaN(v) || double.IsInfinity(v) || v <= 0))
                throw new ArgumentOutOfRangeException(nameof(tanFrictionAngle));
            Name = name; TanFrictionAngle = tanFrictionAngle; EffectiveCohesion = effectiveCohesion; UndrainedShearStrength = undrainedShearStrength;
            UnconfinedStrength = unconfinedStrength; UnitWeight = unitWeight;
        }
        public bool Equals(GeotechnicalMaterialFactors o) => o != null && Name == o.Name && TanFrictionAngle == o.TanFrictionAngle && EffectiveCohesion == o.EffectiveCohesion
            && UndrainedShearStrength == o.UndrainedShearStrength && UnconfinedStrength == o.UnconfinedStrength && UnitWeight == o.UnitWeight;
        public override bool Equals(object obj) => Equals(obj as GeotechnicalMaterialFactors);
        public override int GetHashCode() => unchecked(Name.GetHashCode() * 31 + TanFrictionAngle.GetHashCode());
    }

    /// <summary>A combination of a design approach for one check: action set, material set, resistance set and γR.</summary>
    public sealed class GeotechnicalCombination
    {
        public string Name { get; }
        public GeotechnicalCheck Check { get; }
        public GeotechnicalSituation Situation { get; }
        public GeotechnicalActionFactors Actions { get; }
        public GeotechnicalMaterialFactors Materials { get; }
        public string ResistanceSet { get; }
        public double ResistanceFactor { get; }
        public string Reference { get; }
        internal GeotechnicalCombination(GeotechnicalCheck check, GeotechnicalSituation situation, GeotechnicalActionFactors actions,
            GeotechnicalMaterialFactors materials, string resistanceSet, double resistanceFactor, string reference)
        {
            Check = check; Situation = situation; Actions = actions; Materials = materials; ResistanceSet = resistanceSet;
            ResistanceFactor = resistanceFactor; Reference = reference; Name = actions.Name + "+" + materials.Name + "+" + resistanceSet;
        }
    }

    /// <summary>
    /// Geotechnical standard: action sets, material sets, resistance factors γR by check, situation and resistance set, design
    /// approaches and correlation factors of the piles. Every factor can be overridden explicitly (national annex, project choice);
    /// a check that the standard does not define returns no combination, never the combination of another check.
    /// </summary>
    [Serializable]
    public abstract class StandardGeotechnical : Standard, ISerializable
    {
        protected Dictionary<string, GeotechnicalActionFactors> _actions = new Dictionary<string, GeotechnicalActionFactors>(StringComparer.Ordinal);
        protected Dictionary<string, GeotechnicalMaterialFactors> _materials = new Dictionary<string, GeotechnicalMaterialFactors>(StringComparer.Ordinal);
        protected Dictionary<string, double> _resistance = new Dictionary<string, double>(StringComparer.Ordinal);

        public override StandardGroupType StandardGroup => StandardGroupType.European;
        /// <summary>Edition of the implemented text, for example "2018" or "2004/AC:2009".</summary>
        public abstract string Edition { get; }

        protected StandardGeotechnical(string name, string remarks) : base(name, remarks) { }
        protected StandardGeotechnical(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            int actions = info.GetInt32("ActionSets"), materials = info.GetInt32("MaterialSets"), resistance = info.GetInt32("ResistanceFactors");
            for (int i = 0; i < actions; i++) { var a = (GeotechnicalActionFactors)info.GetValue("ActionSet" + i, typeof(GeotechnicalActionFactors)); _actions[a.Name] = a; }
            for (int i = 0; i < materials; i++) { var m = (GeotechnicalMaterialFactors)info.GetValue("MaterialSet" + i, typeof(GeotechnicalMaterialFactors)); _materials[m.Name] = m; }
            for (int i = 0; i < resistance; i++) _resistance[info.GetString("ResistanceKey" + i)] = info.GetDouble("ResistanceValue" + i);
        }
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            var actions = _actions.Values.OrderBy(a => a.Name, StringComparer.Ordinal).ToArray();
            var materials = _materials.Values.OrderBy(m => m.Name, StringComparer.Ordinal).ToArray();
            var resistance = _resistance.OrderBy(p => p.Key, StringComparer.Ordinal).ToArray();
            info.AddValue("ActionSets", actions.Length); info.AddValue("MaterialSets", materials.Length); info.AddValue("ResistanceFactors", resistance.Length);
            for (int i = 0; i < actions.Length; i++) info.AddValue("ActionSet" + i, actions[i], typeof(GeotechnicalActionFactors));
            for (int i = 0; i < materials.Length; i++) info.AddValue("MaterialSet" + i, materials[i], typeof(GeotechnicalMaterialFactors));
            for (int i = 0; i < resistance.Length; i++) { info.AddValue("ResistanceKey" + i, resistance[i].Key); info.AddValue("ResistanceValue" + i, resistance[i].Value); }
        }

        public IReadOnlyCollection<GeotechnicalActionFactors> ActionSets => _actions.Values.ToArray();
        public IReadOnlyCollection<GeotechnicalMaterialFactors> MaterialSets => _materials.Values.ToArray();
        public GeotechnicalActionFactors ActionSet(string name) => _actions.TryGetValue(name, out var a) ? a : throw new KeyNotFoundException("Action set " + name);
        public GeotechnicalMaterialFactors MaterialSet(string name) => _materials.TryGetValue(name, out var m) ? m : throw new KeyNotFoundException("Material set " + name);
        /// <summary>Explicit override of an action set (same name replaces it).</summary>
        public void SetActionSet(GeotechnicalActionFactors set) { if (set == null) throw new ArgumentNullException(nameof(set)); _actions[set.Name] = set; }
        /// <summary>Explicit override of a material set (same name replaces it).</summary>
        public void SetMaterialSet(GeotechnicalMaterialFactors set) { if (set == null) throw new ArgumentNullException(nameof(set)); _materials[set.Name] = set; }

        private static string Key(GeotechnicalCheck check, GeotechnicalSituation situation, string set) => check + "/" + situation + "/" + set;
        /// <summary>γR of a check, situation and resistance set; null when the standard does not define it.</summary>
        public double? ResistanceFactor(GeotechnicalCheck check, GeotechnicalSituation situation, string set)
            => _resistance.TryGetValue(Key(check, situation, set), out var value) ? value : (double?)null;
        /// <summary>Explicit override (or definition) of γR.</summary>
        public void SetResistanceFactor(GeotechnicalCheck check, GeotechnicalSituation situation, string set, double value)
        {
            if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0) throw new ArgumentOutOfRangeException(nameof(value));
            _resistance[Key(check, situation, set)] = value;
        }

        /// <summary>Combinations required by the standard for the check; empty when the standard does not define it (not supported).</summary>
        public abstract IReadOnlyList<GeotechnicalCombination> Combinations(GeotechnicalCheck check, GeotechnicalSituation situation);

        /// <summary>Correlation factors (ξ on the mean, ξ on the minimum) of pile resistances from n investigated profiles; null if not defined.</summary>
        public abstract Tuple<double, double> PileCorrelationFactors(int investigatedProfiles);

        protected GeotechnicalCombination Combination(GeotechnicalCheck check, GeotechnicalSituation situation, string actions, string materials, string resistanceSet, string reference)
        {
            var factor = ResistanceFactor(check, situation, resistanceSet);
            if (!factor.HasValue) throw new InvalidOperationException("Missing γR for " + Key(check, situation, resistanceSet));
            return new GeotechnicalCombination(check, situation, ActionSet(actions), MaterialSet(materials), resistanceSet, factor.Value, reference);
        }
        protected void Define(GeotechnicalCheck check, GeotechnicalSituation situation, string set, double value) => _resistance[Key(check, situation, set)] = value;
        /// <summary>Tabulated factors: for a number of profiles between two columns the column with fewer profiles applies (larger ξ, safe side).</summary>
        protected static Tuple<double, double> Tabulated(int n, int[] profiles, double[] mean, double[] minimum)
        {
            if (n < 1) throw new ArgumentOutOfRangeException(nameof(n));
            int i = Array.FindLastIndex(profiles, p => p <= n);
            return Tuple.Create(mean[i], minimum[i]);
        }

        public override bool Equals(object obj) => obj is StandardGeotechnical s && s.GetType() == GetType() && base.Equals(obj)
            && _actions.Count == s._actions.Count && _actions.All(p => s._actions.TryGetValue(p.Key, out var a) && a.Equals(p.Value))
            && _materials.Count == s._materials.Count && _materials.All(p => s._materials.TryGetValue(p.Key, out var m) && m.Equals(p.Value))
            && _resistance.Count == s._resistance.Count && _resistance.All(p => s._resistance.TryGetValue(p.Key, out var r) && r == p.Value);
        public override int GetHashCode() => base.GetHashCode();
        protected static string F(double v) => v.ToString("R", CultureInfo.InvariantCulture);
    }
}
