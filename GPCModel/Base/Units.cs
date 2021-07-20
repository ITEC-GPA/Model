using GPC.Utilities.Converters;
using GPC.Utilities.Units;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model
{
    [Serializable]
    public static class Units    
    {
        [Description("kNm")]
        public static UnitsSystem Knm = new UnitsSystem(UnitsConvert.LengthUnits.m, UnitsConvert.ForceUnits.kN, 
            UnitsConvert.MassUnits.ton, UnitsConvert.PressureUnits.kPa, UnitsConvert.TemperatureUnits.C);

        public static UnitsSystem SI = new UnitsSystem(UnitsConvert.LengthUnits.m, UnitsConvert.ForceUnits.N, 
            UnitsConvert.MassUnits.kg, UnitsConvert.PressureUnits.Pa, UnitsConvert.TemperatureUnits.C);

        public static UnitsSystem Nmm = new UnitsSystem(UnitsConvert.LengthUnits.mm, UnitsConvert.ForceUnits.N, 
            UnitsConvert.MassUnits.ton, UnitsConvert.PressureUnits.MPa, UnitsConvert.TemperatureUnits.C);

        public static UnitsSystem IPS = new UnitsSystem(UnitsConvert.LengthUnits.inch, UnitsConvert.ForceUnits.lbf, 
            UnitsConvert.MassUnits.lb, UnitsConvert.PressureUnits.psi, UnitsConvert.TemperatureUnits.F);

        public static readonly UnitsSystem DefaultUnits = Nmm;

        /// <summary>
        /// Convert lengths from the given units to the default units
        /// </summary>
        /// <param name="length">The length to convert</param>
        /// <param name="units">The source measure units</param>
        /// <returns>The converted length</returns>
        public static double ConvertLengthToDefault(this double length, UnitsSystem units)
        {
            return UnitsConvert.Convert(length, units.LengthUnits, DefaultUnits.LengthUnits, 1);
        }

        /// <summary>
        /// Convert areas from the given units to the default units
        /// </summary>
        /// <param name="area">The area to convert</param>
        /// <param name="units">The source measure units</param>
        /// <returns>The converted area</returns>
        public static double ConvertAreaToDefault(this double area, UnitsSystem units)
        {
            return UnitsConvert.Convert(area, units.LengthUnits, DefaultUnits.LengthUnits, 2);
        }

        /// <summary>
        /// Convert section modulus from the given units to the default units
        /// </summary>
        /// <param name="modulus">The section modulus to convert</param>
        /// <param name="units">The source measure units</param>
        /// <returns>The converted section modulus</returns>
        public static double ConvertSectionModulusToDefault(this double modulus, UnitsSystem units)
        {
            return UnitsConvert.Convert(modulus, units.LengthUnits, DefaultUnits.LengthUnits, 3);
        }

        /// <summary>
        /// Convert moment of inertia from the given units to the default units
        /// </summary>
        /// <param name="modulus">The moment of inertia to convert</param>
        /// <param name="units">The source measure units</param>
        /// <returns>The converted moment of inertia</returns>
        public static double ConvertMomentOfIntertiaToDefault(this double modulus, UnitsSystem units)
        {
            return UnitsConvert.Convert(modulus, units.LengthUnits, DefaultUnits.LengthUnits, 4);
        }

        /// <summary>
        /// Convert forces from the given units to the default units
        /// </summary>
        /// <param name="force">The force to convert</param>
        /// <param name="units">The source measure units</param>
        /// <returns>The converted force</returns>
        public static double ConvertForceToDefault(this double force, UnitsSystem units)
        {
            return UnitsConvert.Convert(force, units.ForceUnits, DefaultUnits.ForceUnits, 1);
        }

        /// <summary>
        /// Convert moment from the given units to the default units
        /// </summary>
        /// <param name="moment">The moment to convert</param>
        /// <param name="units">The source measure units</param>
        /// <returns>The converted moment</returns>
        public static double ConverMomentToDefault(this double moment, UnitsSystem units)
        {
            return UnitsConvert.Convert(moment, units.ForceUnits, DefaultUnits.ForceUnits, 1, units.LengthUnits, DefaultUnits.LengthUnits, 1);
        }

        /// <summary>
        /// Convert masses from the given units to the default units
        /// </summary>
        /// <param name="mass">The mass to convert</param>
        /// <param name="units">The source measure units</param>
        /// <returns>The converted mass</returns>
        public static double ConvertMassToDefault(this double mass, UnitsSystem units)
        {
            return UnitsConvert.Convert(mass, units.MassUnits, DefaultUnits.MassUnits, 1);
        }

        /// <summary>
        /// Convert pressures from the given units to the default units
        /// </summary>
        /// <param name="pressure">The pressure to convert</param>
        /// <param name="units">The source measure units</param>
        /// <returns>The converted pressure</returns>
        public static double ConvertPressureToDefault(this double pressure, UnitsSystem units)
        {
            return UnitsConvert.Convert(pressure, units.PressureUnits, DefaultUnits.PressureUnits, 1);
        }

        /// <summary>
        /// Convert temperatures from the given units to the default units
        /// </summary>
        /// <param name="temperature">The temperature to convert</param>
        /// <param name="units">The source measure units</param>
        /// <returns>The converted temperature</returns>
        public static double ConvertTemperatureToDefault(this double temperature, UnitsSystem units)
        {
            return UnitsConvert.Convert(temperature, units.TemperatureUnits, DefaultUnits.TemperatureUnits);
        }

        /// <summary>
        /// Convert density from the given units to the default units
        /// </summary>
        /// <param name="density">The temperature to convert</param>
        /// <param name="units">The source measure units</param>
        /// <returns>The converted temperature</returns>
        public static double ConvertDensityToDefault(this double density, UnitsSystem units)
        {
            return UnitsConvert.Convert(density, units.MassUnits, DefaultUnits.MassUnits, 1, units.LengthUnits, DefaultUnits.LengthUnits, -3);
        }

        /// <summary>
        /// Convert lengths from the default units to the given ones
        /// </summary>
        /// <param name="length">The length to convert</param>
        /// <param name="units">The destination measure units</param>
        /// <returns>The converted length</returns>
        public static double ConvertLengthFromDefault(this double length, UnitsSystem units)
        {
            return UnitsConvert.Convert(length, DefaultUnits.LengthUnits, units.LengthUnits, 1);
        }

        /// <summary>
        /// Convert areas from the default units to the given ones
        /// </summary>
        /// <param name="area">The area to convert</param>
        /// <param name="units">The destination measure units</param>
        /// <returns>The converted area</returns>
        public static double ConvertAreaFromDefault(this double area, UnitsSystem units)
        {
            return UnitsConvert.Convert(area, DefaultUnits.LengthUnits, units.LengthUnits, 2);
        }

        /// <summary>
        /// Convert section modulus from the default units to the given ones
        /// </summary>
        /// <param name="modulus">The section modulus to convert</param>
        /// <param name="units">The destination measure units</param>
        /// <returns>The converted section modulus</returns>
        public static double ConvertSectionModulusFromDefault(this double modulus, UnitsSystem units)
        {
            return UnitsConvert.Convert(modulus, DefaultUnits.LengthUnits, units.LengthUnits, 3);
        }

        /// <summary>
        /// Convert moment of intertia from the default units to the given ones
        /// </summary>
        /// <param name="inertia">The moment of intertia to convert</param>
        /// <param name="units">The destination measure units</param>
        /// <returns>The converted moment of intertia</returns>
        public static double ConvertMomentOfInertiaFromDefault(this double inertia, UnitsSystem units)
        {
            return UnitsConvert.Convert(inertia, DefaultUnits.LengthUnits, units.LengthUnits, 4);
        }

        /// <summary>
        /// Convert forces from the default units to the given ones
        /// </summary>
        /// <param name="force">The force to convert</param>
        /// <param name="units">The destination measure units</param>
        /// <returns>The converted force</returns>
        public static double ConvertForceFromDefault(this double force, UnitsSystem units)
        {
            return UnitsConvert.Convert(force, DefaultUnits.ForceUnits, units.ForceUnits, 1);
        }

        /// <summary>
        /// Convert moment from the default units to the given ones
        /// </summary>
        /// <param name="moment">The moment to convert</param>
        /// <param name="units">The destination measure units</param>
        /// <returns>The converted moment</returns>
        public static double ConverMomentFromDefault(this double moment, UnitsSystem units)
        {
            return UnitsConvert.Convert(moment, DefaultUnits.ForceUnits, units.ForceUnits, 1, DefaultUnits.LengthUnits, units.LengthUnits, 1);
        }

        /// <summary>
        /// Convert masses from the default units to the given ones
        /// </summary>
        /// <param name="mass">The mass to convert</param>
        /// <param name="units">The destination measure units</param>
        /// <returns>The converted mass</returns>
        public static double ConvertMassFromDefault(this double mass, UnitsSystem units)
        {
            return UnitsConvert.Convert(mass, DefaultUnits.MassUnits, units.MassUnits, 1);
        }

        /// <summary>
        /// Convert pressure from the default units to the given ones
        /// </summary>
        /// <param name="pressure">The pressure to convert</param>
        /// <param name="units">The destination measure units</param>
        /// <returns>The converted pressure</returns>
        public static double ConvertPressureFromDefault(this double pressure, UnitsSystem units)
        {
            return UnitsConvert.Convert(pressure, DefaultUnits.PressureUnits, units.PressureUnits, 1);
        }

        /// <summary>
        /// Convert temperature from the default units to the given ones
        /// </summary>
        /// <param name="temperature">The temperature to convert</param>
        /// <param name="units">The destination measure units</param>
        /// <returns>The converted temperature</returns>
        public static double ConvertTemperatureFromDefault(this double temperature, UnitsSystem units)
        {
            return UnitsConvert.Convert(temperature, DefaultUnits.TemperatureUnits, units.TemperatureUnits);
        }


        /// <summary>
        /// Convert density from the default units to the given ones
        /// </summary>
        /// <param name="density">The density to convert</param>
        /// <param name="units">The destination measure units</param>
        /// <returns>The converted temperature</returns>
        public static double ConvertDensityFromDefault(this double density, UnitsSystem units)
        {
            return UnitsConvert.Convert(density, DefaultUnits.MassUnits, units.MassUnits, 1, DefaultUnits.LengthUnits, units.LengthUnits, -3);
        }
    }
}
