using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Bolt
{
    [Serializable]
    public class BoltSection : SectionCircular, ISerializable
    {
        /// <summary>
        /// Coarse pitch.
        /// ISO 261 & 262:1998(E) & ISO 724:2009(E).
        /// With a nominal diameter of 20 mm --> use the value of the previous row with 20<24, so {18m, 2.5} --> pitch=2.5 mm.
        /// </summary>
        public static readonly Dictionary<decimal, double> PitchPerNominalDiameter = new Dictionary<decimal, double>()
        {
            {1.6m, 0.35},
            {2m, 0.4},
            {2.5m, 0.45},
            {3m, 0.5},
            {3.5m, 0.6},
            {4m, 0.7},
            {5m, 0.8},
            {6m, 1.0},
            {8m, 1.25},
            {10m, 1.5},
            {12m, 1.75},
            {14m, 2.0},
            {18m, 2.5},
            {24m, 3.0},
            {30m, 3.5},
            {36m, 4.0},
            {42m, 4.5},
            {48m, 5.0},
            {56m, 5.5},
            {64m, 6.0}
        };

        /// <summary>
        /// d_w from ISO 4014 & 4017:2022.
        /// </summary>
        public static readonly Dictionary<double, double> DwPerNominalDiameter = new Dictionary<double, double>()
        {
            {1.6, 2.54},
            {2.0, 3.34},
            {2.5, 4.34},
            {3.0, 4.84},
            {3.5, 5.34},
            {4.0, 6.2},
            {5.0, 7.2},
            {6.0, 8.88},
            {7.0, 9.63},
            {8.0, 11.63},
            {10.0, 14.63},
            {12.0, 16.63},
            {14.0, 19.64},
            {16.0, 22.0},
            {18.0, 24.85},
            {20.0, 27.7},
            {22.0, 31.35},
            {24.0, 33.25},
            {27.0, 38.0},
            {30.0, 42.75},
            {33.0, 46.55},
            {36.0, 51.11},
            {39.0, 55.86},
            {42.0, 59.95},
            {45.0, 64.7},
            {48.0, 69.45},
            {52.0, 74.2},
            {56.0, 78.66},
            {60.0, 83.41},
            {64.0, 88.16}
        };

        #region Fields

        protected SteelMaterial _boltMaterial;

        #endregion

        #region Public Property

        public SteelMaterial BoltMaterial
        {
            get => _boltMaterial;
            set
            {
                if (value is SteelMaterial)
                    _boltMaterial = value;
            }
        }

        #endregion

        #region Public Constructors

        public BoltSection(double diameter, SteelMaterial material, string name = "") : base(diameter, name)
        {
            _boltMaterial = material;
        }

        public BoltSection(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("BoltSectionVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            if (version > 1)
                _boltMaterial = (SteelMaterial)info.GetValue("BoltMaterial", typeof(SteelMaterial));
            else
                // Before version 2 this was a GPCCheckers.Core.Mvvm.Models.SteelMaterialModel class.
                // Change of name in TypenameConverterBinder.
                _boltMaterial = (SteelMaterial)info.GetValue("Material", typeof(SteelMaterial));
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("BoltSectionVersion", version);

            info.AddValue("BoltMaterial", _boltMaterial);
        }

        protected decimal CalculateDiameterDecimal()
        {
            var DiaDec = Convert.ToDecimal(Diameter);
            return Math.Round(DiaDec, 2);
        }

        /// <summary>
        /// Calculate dw using PitchPerNominalDiameter.
        /// ISO 261 & 262:1998(E) & ISO 724:2009(E).
        /// Used to calculate the net resistant area.
        /// </summary>
        /// <returns></returns>
        public double CalculatePitchFromNominalDiameter()
        {
            var DiaDec = CalculateDiameterDecimal();

            // Try first with exact value.
            if (PitchPerNominalDiameter.TryGetValue(DiaDec, out double pitch))
                return pitch;

            // Value is intermediate, take the row preceding the first key with greater value.
            decimal precision = 0.000001m;

            for (int i = 1; i < PitchPerNominalDiameter.Count; i++)
                if (DiaDec < PitchPerNominalDiameter.ElementAt(i).Key - precision)
                    return PitchPerNominalDiameter.ElementAt(i - 1).Value;

            return PitchPerNominalDiameter.Last().Value;
        }

        /// <summary>
        /// BoltSection net area.
        /// EN ISO 898-1:2013.
        /// </summary>
        /// <returns>Net area.</returns>
        public double CalculateAreaEff()
        {
            // 0.86602540378443865d --> cos(30°)
            double H = 0.86602540378443865d * CalculatePitchFromNominalDiameter();
            double d1 = Diameter - 1.25 * H;
            double d2 = Diameter - 0.75 * H;
            double d3 = d1 - H / 6.0;
            return Math.Pow((d2 + d3) * 0.25, 2) * Math.PI;
        }

        /// <summary>
        /// Uses a linear interpolation on DwPerNominalDiameter.
        /// </summary>
        /// <returns></returns>
        public double CalculateDwFromNominalDiameter()
        {
            // Try first with exact value.
            if (DwPerNominalDiameter.TryGetValue(Diameter, out double dw))
                return dw;

            // Interpolates by value smaller than the first one available.
            if (Diameter < DwPerNominalDiameter.First().Key)
                return DwPerNominalDiameter.First().Value * Diameter / DwPerNominalDiameter.First().Key;

            // Interpolates for intermediate values.
            for (int i = 1; i < DwPerNominalDiameter.Count; i++)
            {
                var DwNext = DwPerNominalDiameter.ElementAt(i);
                if (Diameter <= DwNext.Key)
                {
                    var DwPrev = DwPerNominalDiameter.ElementAt(i - 1);
                    return DwPrev.Value + (Diameter - DwPrev.Key) * (DwNext.Value - DwPrev.Value) / (DwNext.Key - DwPrev.Key);
                }
            }

            // Use approximation for Diameter > DwPerNominalDiameter.Last().Key=64.
            return -4.88 + 1.71 * Diameter - 0.00386 * Diameter * Diameter;
        }

        /// <summary>
        /// Calculate gross o net area.
        /// </summary>
        /// <param name="ThreadedArea">True to calculate threaded or net area.</param>
        /// <returns>Resistant area.</returns>
        public double CalculateResistantArea(in bool ThreadedArea)
        {
            return ThreadedArea ? CalculateAreaEff() : Area;
        }

        /// <summary>
        /// Calculation of the average diameter of the bolt head.
        /// This is an approximation, derived the coefficient 1.61 as the average of various bolt heads obtained from the standards.
        /// Called d_M in Eurocode EN 1993-1-8: the mean of the across points and across flats dimensions of the bolt head or the nut,
        /// whichever is  smaller.
        /// If this value is not provided by the user then this value can be used as the default.
        /// Info in "\\studio\Software_Development\01 Theory\08 BoltSection\Net area bolt.docx".
        /// </summary>
        /// <returns>Average diameter of the bolt head.</returns>
        public double CalculateMeanDiameterBoltHead() => 1.61 * Diameter;

        /// <summary>
        /// Calculate fourth of d_w.
        /// Used in Eurocode EN 1993-1-8, called e_w.
        /// </summary>
        /// <returns>Fourth of d_w.</returns>
        public double CalculateFourthOfDw() => CalculateDwFromNominalDiameter() * 0.25;

        public bool Equals(BoltSection other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return base.Equals(other);
        }

        public override bool Equals(object obj) => Equals(obj as BoltSection);

        public override int GetHashCode()
        {
            return base.GetHashCode() * 103;
        }

        #endregion

        #region Operators overrides

        public static bool operator ==(BoltSection left, BoltSection right) => left.Equals(right);

        public static bool operator !=(BoltSection left, BoltSection right) => !(left == right);

        #endregion

    }
}
