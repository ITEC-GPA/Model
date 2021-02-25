using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Utilities.Units;

namespace GPC.Model
{
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
    }
}
