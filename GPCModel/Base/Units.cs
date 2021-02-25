using GPC.Utilities.Units;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model
{
    public static class Units    
    {
        public static UnitsSystem Knm = new UnitsSystem(UnitsConvert.LengthUnits.m, UnitsConvert.ForceUnits.kN, 
            UnitsConvert.MassUnits.ton, UnitsConvert.PressureUnits.kPa, UnitsConvert.TemperatureUnits.C);

        public static UnitsSystem SI = new UnitsSystem(UnitsConvert.LengthUnits.m, UnitsConvert.ForceUnits.N, 
            UnitsConvert.MassUnits.kg, UnitsConvert.PressureUnits.Pa, UnitsConvert.TemperatureUnits.C);

        public static UnitsSystem Nmm = new UnitsSystem(UnitsConvert.LengthUnits.mm, UnitsConvert.ForceUnits.N, 
            UnitsConvert.MassUnits.ton, UnitsConvert.PressureUnits.MPa, UnitsConvert.TemperatureUnits.C);

        public static UnitsSystem IPS = new UnitsSystem(UnitsConvert.LengthUnits.inch, UnitsConvert.ForceUnits.lbf, 
            UnitsConvert.MassUnits.lb, UnitsConvert.PressureUnits.psi, UnitsConvert.TemperatureUnits.C);

        /// <summary>
        /// Convert lengths from the given sistem units to Nmm standard
        /// </summary>
        /// <param name="length">The length to convert</param>
        /// <param name="units">The source measure units</param>
        /// <returns>The converted length</returns>
        public static double ConvertLengthToNmm(this double length, UnitsSystem units)
        {
            return UnitsConvert.Convert(length, units.LengthUnits, Nmm.LengthUnits, 1);
        }

        /// <summary>
        /// Convert forces from the given sistem units to Nmm standard
        /// </summary>
        /// <param name="force">The force to convert</param>
        /// <param name="units">The source measure units</param>
        /// <returns>The converted force</returns>
        public static double ConvertForceToNmm(this double force, UnitsSystem units)
        {
            return UnitsConvert.Convert(force, units.ForceUnits, Nmm.ForceUnits, 1);
        }

        /// <summary>
        /// Convert lengths from Nmm standard to the given sistem units
        /// </summary>
        /// <param name="length">The length to convert</param>
        /// <param name="units">The destination measure units</param>
        /// <returns>The converted length</returns>
        public static double ConvertLengthFromNmm(this double length, UnitsSystem units)
        {
            return UnitsConvert.Convert(length, Nmm.LengthUnits, units.LengthUnits, 1);
        }

        /// <summary>
        /// Convert forces from Nmm standard to the given sistem units
        /// </summary>
        /// <param name="force">The force to convert</param>
        /// <param name="units">The destination measure units</param>
        /// <returns>The converted force</returns>
        public static double ConvertForceFromNmm(this double force, UnitsSystem units)
        {
            return UnitsConvert.Convert(force, Nmm.ForceUnits, units.ForceUnits, 1);
        }
    }
}
