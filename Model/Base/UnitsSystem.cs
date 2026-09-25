using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Utilities.Units;

namespace GPC.Model
{
    /// <summary>
    /// A system of units: length, force, mass, pressure and temperature
    /// </summary>
    [Serializable]
    public class UnitsSystem
    {
        /// <summary>
        /// The length units
        /// </summary>
        public UnitsConvert.LengthUnits LengthUnits { get; private set; }
        /// <summary>
        /// The force units
        /// </summary>
        public UnitsConvert.ForceUnits ForceUnits { get; private set; }
        /// <summary>
        /// The mass units
        /// </summary>
        public UnitsConvert.MassUnits MassUnits { get; private set; }
        /// <summary>
        /// The pressure units
        /// </summary>
        public UnitsConvert.PressureUnits PressureUnits { get; private set; }
        /// <summary>
        /// The temperature units
        /// </summary>
        public UnitsConvert.TemperatureUnits TemperatureUnits { get; private set; }

        /// <summary>
        /// Creates a system of units
        /// </summary>
        /// <param name="lengthUnits">The length units</param>
        /// <param name="forceUnits">The force units</param>
        /// <param name="massUnits">The mass units</param>
        /// <param name="pressureUnits">The pressure units</param>
        /// <param name="temperatureUnits">The temperature units</param>
        public UnitsSystem(UnitsConvert.LengthUnits lengthUnits, UnitsConvert.ForceUnits forceUnits, UnitsConvert.MassUnits massUnits,
            UnitsConvert.PressureUnits pressureUnits, UnitsConvert.TemperatureUnits temperatureUnits)
        {
            LengthUnits = lengthUnits;
            ForceUnits = forceUnits;
            MassUnits = massUnits;
            PressureUnits = pressureUnits;
            TemperatureUnits = temperatureUnits;
        }

        /// <summary>
        /// Equality of all the units
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is a system with the same units</returns>
        public override bool Equals(object obj)
        {
            return obj is UnitsSystem system &&
                   LengthUnits == system.LengthUnits &&
                   ForceUnits == system.ForceUnits &&
                   MassUnits == system.MassUnits &&
                   PressureUnits == system.PressureUnits &&
                   TemperatureUnits == system.TemperatureUnits;
        }

        /// <summary>
        /// The hash code of the units
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = -514217984;
            hashCode = hashCode * -1521134295 + LengthUnits.GetHashCode();
            hashCode = hashCode * -1521134295 + ForceUnits.GetHashCode();
            hashCode = hashCode * -1521134295 + MassUnits.GetHashCode();
            hashCode = hashCode * -1521134295 + PressureUnits.GetHashCode();
            hashCode = hashCode * -1521134295 + TemperatureUnits.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null systems are equal
        /// </summary>
        /// <param name="us1">The first system</param>
        /// <param name="us2">The second system</param>
        /// <returns>True if the systems have the same units</returns>
        public static bool operator ==(UnitsSystem us1, UnitsSystem us2)
        {
            if (ReferenceEquals(us1, us2))
                return true;
            return !(us1 is null) && us1.Equals(us2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="us1">The first system</param>
        /// <param name="us2">The second system</param>
        /// <returns>True if a unit is different</returns>
        public static bool operator !=(UnitsSystem us1, UnitsSystem us2)
        {
            return !(us1 == us2);
        }
    }
}
