using System;
using System.Collections.Generic;
using System.Linq;

namespace GPC.Model.Geotechnics
{
    /// <summary>A soil layer between two elevations (mm, z upwards): <see cref="Top"/> &gt; <see cref="Bottom"/>.</summary>
    [Serializable]
    public sealed class SoilLayer : IEquatable<SoilLayer>
    {
        private readonly Soil _soil;
        private readonly double _top, _bottom;
        public Soil Soil => _soil;
        public double Top => _top;
        public double Bottom => _bottom;
        public double Thickness => _top - _bottom;
        public SoilLayer(Soil soil, double top, double bottom)
        {
            _soil = soil ?? throw new ArgumentNullException(nameof(soil));
            if (!Soil.Finite(top) || !Soil.Finite(bottom) || top <= bottom) throw new ArgumentOutOfRangeException(nameof(top), "Top > bottom required.");
            _top = top; _bottom = bottom;
        }
        public bool Equals(SoilLayer o) => o != null && Equals(_soil, o._soil) && _top == o._top && _bottom == o._bottom;
        public override bool Equals(object obj) => Equals(obj as SoilLayer);
        public override int GetHashCode() => unchecked(_top.GetHashCode() * 31 + _bottom.GetHashCode());
    }

    /// <summary>
    /// Stratigraphy of a vertical: contiguous layers from the ground surface (top of the first layer) downwards and an optional
    /// hydrostatic water table. Elevations mm (z upwards), stresses MPa. The profile states data; design methods live in Checker.
    /// </summary>
    [Serializable]
    public sealed class SoilProfile : IEquatable<SoilProfile>
    {
        private readonly string _name, _source;
        private readonly SoilLayer[] _layers;
        private readonly double? _groundwater;
        private readonly double _waterUnitWeight;

        public string Name => _name;
        public IReadOnlyList<SoilLayer> Layers => Array.AsReadOnly(_layers);
        /// <summary>Elevation of the hydrostatic water table, mm; null when no water table is present in the profile.</summary>
        public double? GroundwaterElevation => _groundwater;
        /// <summary>γw, N/mm³.</summary>
        public double WaterUnitWeight => _waterUnitWeight;
        public string Source => _source;
        public double GroundSurface => _layers[0].Top;
        public double Base => _layers[_layers.Length - 1].Bottom;

        public SoilProfile(string name, IEnumerable<SoilLayer> layers, string source, double? groundwaterElevation = null, double waterUnitWeight = SoilUnits.WaterUnitWeight)
        {
            if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("A name is required.", nameof(name));
            if (string.IsNullOrWhiteSpace(source)) throw new ArgumentException("The provenance of the stratigraphy is required.", nameof(source));
            _layers = (layers ?? throw new ArgumentNullException(nameof(layers))).ToArray();
            if (_layers.Length == 0 || _layers.Any(l => l == null)) throw new ArgumentException("At least one layer is required.", nameof(layers));
            for (int i = 1; i < _layers.Length; i++)
                if (Math.Abs(_layers[i].Top - _layers[i - 1].Bottom) > 1e-9) throw new ArgumentException("Layers must be contiguous and ordered from the top.", nameof(layers));
            if (groundwaterElevation.HasValue && !Soil.Finite(groundwaterElevation.Value)) throw new ArgumentOutOfRangeException(nameof(groundwaterElevation));
            if (!Soil.Positive(waterUnitWeight)) throw new ArgumentOutOfRangeException(nameof(waterUnitWeight));
            _name = name; _source = source; _groundwater = groundwaterElevation; _waterUnitWeight = waterUnitWeight;
        }

        /// <summary>The layer containing z (at an interface, the lower layer; at the base, the last one).</summary>
        public SoilLayer LayerAt(double z)
        {
            Inside(z);
            return _layers.FirstOrDefault(l => z <= l.Top && z > l.Bottom) ?? _layers[_layers.Length - 1];
        }

        /// <summary>Total vertical stress σv at z, MPa: γ above and γsat below the water table; water above the ground surface is not included.</summary>
        public double VerticalTotalStress(double z)
        {
            Inside(z);
            double stress = 0;
            foreach (var layer in _layers)
            {
                double top = layer.Top, bottom = Math.Max(layer.Bottom, z);
                if (bottom >= top) break;
                double water = _groundwater ?? double.NegativeInfinity;
                double dry = Math.Max(0, top - Math.Max(bottom, Math.Min(top, water))), wet = (top - bottom) - dry;
                stress += layer.Soil.UnitWeight * dry + layer.Soil.SaturatedUnitWeight * wet;
            }
            return stress;
        }

        /// <summary>Hydrostatic pore pressure u at z, MPa (0 above the water table).</summary>
        public double PorePressure(double z) { Inside(z); return _groundwater.HasValue && z < _groundwater.Value ? _waterUnitWeight * (_groundwater.Value - z) : 0; }

        /// <summary>Effective vertical stress σ'v = σv − u at z, MPa.</summary>
        public double VerticalEffectiveStress(double z) => VerticalTotalStress(z) - PorePressure(z);

        private void Inside(double z)
        {
            if (!Soil.Finite(z) || z > GroundSurface + 1e-9 || z < Base - 1e-9) throw new ArgumentOutOfRangeException(nameof(z), "Elevation outside the profile.");
        }

        public bool Equals(SoilProfile o) => o != null && _name == o._name && _source == o._source && _groundwater == o._groundwater
            && _waterUnitWeight == o._waterUnitWeight && _layers.SequenceEqual(o._layers);
        public override bool Equals(object obj) => Equals(obj as SoilProfile);
        public override int GetHashCode() => unchecked((_name?.GetHashCode() ?? 0) * 31 + _layers.Length);
    }
}
