using System;
using GPC.Utilities.Units;

namespace GPC.Model.Core.Units
{
    /// <summary>Explicit conversion to the domain basis N, mm, rad, s, tonne, °C.
    /// Temperature values and differences are separate. Time is seconds; gravity is an
    /// acceleration supplied by the caller, never an implicit conversion from mass to weight.</summary>
    public sealed class DomainUnits
    {
        private readonly UnitsSystem source;
        public double LengthToMm { get; }
        public double ForceToN { get; }
        public double MassToTonne { get; }
        public double AngleToRad { get; }
        public double TemperatureDifferenceToC { get; }
        public DomainUnits(UnitsSystem source, double angleToRad = 1)
        {
            this.source = source ?? throw new ArgumentNullException(nameof(source));
            LengthToMm = Positive(UnitsConvert.Convert(1, source.LengthUnits, UnitsConvert.LengthUnits.mm, 1), "lengthFactor");
            ForceToN = Positive(UnitsConvert.Convert(1, source.ForceUnits, UnitsConvert.ForceUnits.N, 1), "forceFactor");
            MassToTonne = Positive(UnitsConvert.Convert(1, source.MassUnits, UnitsConvert.MassUnits.ton, 1), "massFactor");
            AngleToRad = Positive(angleToRad, nameof(angleToRad));
            TemperatureDifferenceToC = Positive(UnitsConvert.ConvertDifference(1, source.TemperatureUnits, UnitsConvert.TemperatureUnits.C), "temperatureFactor");
        }
        private static double Positive(double value, string name) => NumericGuard.Finite(value, name) > 0 ? value : throw new ArgumentOutOfRangeException(name);
        private static double Scale(double value, double factor) => NumericGuard.Finite(NumericGuard.Finite(value, nameof(value)) * factor, "convertedValue");
        public double Length(double value) => Scale(value, LengthToMm);
        public double Force(double value) => Scale(value, ForceToN);
        public double Moment(double value) => Scale(value, ForceToN * LengthToMm);
        public double Angle(double value) => Scale(value, AngleToRad);
        public double Mass(double value) => Scale(value, MassToTonne);
        public double Area(double value) => Scale(value, LengthToMm * LengthToMm);
        public double Pressure(double value) => NumericGuard.Finite(UnitsConvert.Convert(NumericGuard.Finite(value, nameof(value)), source.PressureUnits, UnitsConvert.PressureUnits.MPa, 1), "pressure");
        /// <summary>Force/length² columns, independently of the explicitly named pressure unit.</summary>
        public double ForcePerArea(double value) => Scale(value, ForceToN / (LengthToMm * LengthToMm));
        public double ForcePerLength(double value) => Scale(value, ForceToN / LengthToMm);
        /// <summary>q = axial force / current length, N/mm.</summary>
        public double ForceDensity(double value) => ForcePerLength(value);
        /// <summary>Membrane extensional stiffness Et or tension per unit width, N/mm.</summary>
        public double MembraneStiffness(double value) => ForcePerLength(value);
        public double MassPerArea(double value) => Scale(value, MassToTonne / (LengthToMm * LengthToMm));
        public double MassDensity(double value) => Scale(value, MassToTonne / Math.Pow(LengthToMm, 3));
        public double Acceleration(double value) => Length(value);
        public double AbsoluteTemperature(double value) => NumericGuard.Finite(UnitsConvert.Convert(NumericGuard.Finite(value, nameof(value)), source.TemperatureUnits, UnitsConvert.TemperatureUnits.C), "temperature");
        public double TemperatureDifference(double value) => Scale(value, TemperatureDifferenceToC);
        public double TemperatureGradient(double value) => Scale(value, TemperatureDifferenceToC / LengthToMm);
        public double ThermalExpansion(double value) => Scale(value, 1 / TemperatureDifferenceToC);
        /// <summary>Rows: Fx,Fy,Fz,Mx,My,Mz; columns: ux,uy,uz,rx,ry,rz. Row-major.</summary>
        public double[] Spring(double[] coefficients) => new ResultUnits(ForceToN, LengthToMm, ForceToN * LengthToMm, AngleToRad).Spring(coefficients);
    }
}
