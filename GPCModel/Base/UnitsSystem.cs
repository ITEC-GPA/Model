using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Utilities.Units;

namespace GPC.Model
{
    [Serializable]
    public class UnitsSystem
    {
        public UnitsConvert.LengthUnits LengthUnits { get; private set; }
        public UnitsConvert.ForceUnits ForceUnits { get; private set; }
        public UnitsConvert.MassUnits MassUnits { get; private set; }
        public UnitsConvert.PressureUnits PressureUnits { get; private set; }
        public UnitsConvert.TemperatureUnits TemperatureUnits { get; private set; }

        public UnitsSystem(UnitsConvert.LengthUnits lengthUnits, UnitsConvert.ForceUnits forceUnits, UnitsConvert.MassUnits massUnits,
            UnitsConvert.PressureUnits pressureUnits, UnitsConvert.TemperatureUnits temperatureUnits)
        {
            LengthUnits = lengthUnits;
            ForceUnits = forceUnits;
            MassUnits = massUnits;
            PressureUnits = pressureUnits;
            TemperatureUnits = temperatureUnits;
        }

        public override bool Equals(object obj)
        {
            return obj is UnitsSystem system &&
                   LengthUnits == system.LengthUnits &&
                   ForceUnits == system.ForceUnits &&
                   MassUnits == system.MassUnits &&
                   PressureUnits == system.PressureUnits &&
                   TemperatureUnits == system.TemperatureUnits;
        }

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

        public static bool operator ==(UnitsSystem us1, UnitsSystem us2)
        {
            if (ReferenceEquals(us1, us2))
                return true;
            return !(us1 is null) && us1.Equals(us2);
        }

        public static bool operator !=(UnitsSystem us1, UnitsSystem us2)
        {
            return !(us1 == us2);
        }
    }
}
