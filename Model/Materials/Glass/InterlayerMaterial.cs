using GPC.Utilities.Attributes;
using GPC.Utilities.Maths;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
    /// <summary>
    /// The interlayer of laminated glass: the shear modulus depends on the load duration and on the temperature (a table for each load duration)
    /// </summary>
    [Serializable]
    [UI(Description = "Interlayer", Group = "Materials", Kind = "Material")]
    public sealed class InterlayerMaterial : Material, IEquatable<InterlayerMaterial>
    {
        #region PUBLIC ENUMS

        /// <summary>
        /// The families of interlayer (prEN 16613)
        /// </summary>
        [Serializable]
        public enum InterlayerType
        {
            /// <summary>Acoustic PVB (family 0)</summary>
            [Description("AcusticPVB / Family0 prEN")] AcusticPVB = 0,
            /// <summary>Normal PVB (family 1)</summary>
            [Description("NormalPVB / Family1 prEN")] NormalPVB = 1,
            /// <summary>SentryGlas ionoplast (family 2)</summary>
            [Description("SentryGlass / Family2 prEN")] SentryGlass = 2
        }

        #endregion

        #region VARIABLES

        /// <summary>
        /// The tables of the shear modulus, one for each load duration
        /// </summary>
        private List<LoadDurationShearModules> _shearModulus;
        /// <summary>
        /// The family of interlayer
        /// </summary>
        private InterlayerType _type;

        #endregion

        #region PROPERTIES

        /// <summary>
        /// The family of interlayer
        /// </summary>
        public InterlayerType Type { get => _type; set => _type = value; }

        #endregion

        #region CONSTRUCTOR

        /// <summary>
        /// Creates an interlayer without shear modulus tables (elastic modulus and Poisson's ratio zero)
        /// </summary>
        /// <param name="name">The name</param>
        /// <param name="density">The density</param>
        /// <param name="alfaThermalExpansion">The coefficient of thermal expansion</param>
        /// <param name="type">The family of interlayer</param>
        public InterlayerMaterial(string name, double density, double alfaThermalExpansion, InterlayerType type)
            : base(name, 0, 0, density, alfaThermalExpansion)
        {
            _shearModulus = new List<LoadDurationShearModules>();
            _type = type;
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="Material"/>, the tables and the family (the nested table classes are not
        /// [Serializable]: see the list of the defects found)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private InterlayerMaterial(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _shearModulus = (List<LoadDurationShearModules>)info.GetValue("ShearModulus", typeof(List<LoadDurationShearModules>));
            _type = (InterlayerType)info.GetValue("InterlayerType", typeof(InterlayerType));
        }

        #endregion

        #region PUBLIC METHODS

        /// <summary>
        /// Do not use this method. Use: <see cref="GetShearModule(double, double)"/>
        /// </summary>
        /// <returns>Nothing</returns>
        /// <exception cref="NotImplementedException">Always</exception>
        public override double GetShearModule()
        {
            throw new NotImplementedException($"Do not use this method. Use: GetShearModule(double, double)");
        }

        /// <summary>
        /// The shear modulus for a load duration and a temperature: the value of the table of the duration; between two durations the values of the two
        /// tables at the temperature are interpolated, but with the temperature as abscissa instead of the duration (see the list of the defects found)
        /// </summary>
        /// <param name="loadDuration">The load duration</param>
        /// <param name="temperature">The temperature</param>
        /// <returns>The shear modulus</returns>
        /// <exception cref="IndexOutOfRangeException">If the duration or the temperature is out of the tables</exception>
        /// <exception cref="KeyNotFoundException">If the value is not found</exception>
        public double GetShearModule(double loadDuration, double temperature)
        {
            for (int i = 0; i < _shearModulus.Count; i++)
            {
                if (_shearModulus[i].LoadDuration == loadDuration)
                    return _shearModulus[i][temperature];

                if (i == 0)
                {
                    if (loadDuration < _shearModulus[i].LoadDuration)
                    {
                        throw new IndexOutOfRangeException("Requested load duration is lower than minimum load duration available");
                    }

                    if (_shearModulus[i].LoadDuration < loadDuration && _shearModulus[i + 1].LoadDuration > loadDuration)
                    {
                        return Interpolation.GetLinearInterpolation(_shearModulus[i].LoadDuration, _shearModulus[i + 1].LoadDuration,
                                                                    _shearModulus[i][temperature], _shearModulus[i + 1][temperature],
                                                                        temperature);
                    }
                }
                else if (i < _shearModulus.Count - 1)
                {
                    if (_shearModulus[i].LoadDuration < loadDuration && _shearModulus[i + 1].LoadDuration > loadDuration)
                    {
                        return Interpolation.GetLinearInterpolation(_shearModulus[i].LoadDuration, _shearModulus[i + 1].LoadDuration,
                                                                    _shearModulus[i][temperature], _shearModulus[i + 1][temperature],
                                                                        temperature);
                    }
                }
                else if (i == _shearModulus.Count - 1) // Caso di temperature == all'ultimo valore già coperto all'inizio.
                {
                    if (loadDuration > _shearModulus[i].LoadDuration)
                    {
                        throw new IndexOutOfRangeException("Requested load duration is greater than maximum load duration available");
                    }
                }
            }
            throw new KeyNotFoundException();
        }

        /// <summary>
        /// Adds the table of a load duration (sorted by temperature)
        /// </summary>
        /// <param name="loadDuration">The load duration</param>
        /// <param name="temperature">The temperatures</param>
        /// <param name="shearModules">The shear moduli at the temperatures</param>
        /// <exception cref="ArgumentException">If the arrays have different lengths</exception>
        public void AddShearModule(double loadDuration, double[] temperature, double[] shearModules)
        {
            if (temperature.Length != shearModules.Length)
                throw new ArgumentException("Temperature and shearModules lenghts are different");

            var ld = new LoadDurationShearModules(loadDuration);
            ld.AddTemperatures(temperature, shearModules);
            ld.Sort();

            _shearModulus.Add(ld);
        }

        /// <summary>
        /// Replaces all the tables (the list instance is kept)
        /// </summary>
        /// <param name="loadDurationShearModules">The tables, sorted by load duration</param>
        public void AddLoadDurationShearModules(List<LoadDurationShearModules> loadDurationShearModules)
        {
            _shearModulus = loadDurationShearModules;
        }

        /// <summary>
        /// Sort the load duration shear modulus list in place (by load duration).
        /// </summary>
        public void Sort()
        {
            _shearModulus.Sort();
        }

        /// <summary>
        /// The load durations of the tables
        /// </summary>
        /// <returns>A new list with the durations</returns>
        public List<double> GetLoadDurations()
        {
            return _shearModulus.Select(i => i.LoadDuration).ToList();
        }

        /// <summary>
        /// The temperatures of all the tables, without repetitions
        /// </summary>
        /// <returns>A new list with the temperatures</returns>
        public List<double> GetTemperatures()
        {
            return _shearModulus.SelectMany(i => i.TemperatureShearModules.Select(j => j.Temperature)).Distinct().ToList();
        }

        #region Equals - Hashcode - Operators

        /// <summary>
        /// Serializes the data of <see cref="Material"/>, the tables and the family
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ShearModulus", _shearModulus);
            info.AddValue("InterlayerType", _type);
        }

        /// <summary>
        /// Equality of the data of <see cref="Material"/>, the family and the tables (in the same order)
        /// </summary>
        /// <param name="other">The interlayer to compare</param>
        /// <returns>True if the materials are equal</returns>
        public bool Equals(InterlayerMaterial other)
        {
            if (ReferenceEquals(this, other))
                return true;
            if (other is null || !base.Equals(other) || !other._type.Equals(_type) || other._shearModulus.Count != _shearModulus.Count)
                return false;

            bool isEqual = true;
            for (int i = 0; i < other._shearModulus.Count; i++)
            {
                if (!other._shearModulus[i].Equals(_shearModulus[i]))
                {
                    isEqual = false;
                    break;
                }
            }
            return isEqual;
        }

        /// <summary>
        /// Equality with another object (see <see cref="Equals(InterlayerMaterial)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal interlayer</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return Equals(obj as InterlayerMaterial);
        }

        /// <summary>
        /// The hash code of the data of <see cref="Material"/>, of the list of the tables (as instance) and of the family
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<List<LoadDurationShearModules>>.Default.GetHashCode(_shearModulus);
            hashCode = hashCode * -17 + _type.GetHashCode();
            return hashCode;
        }

        /// <summary>
        /// Equality operator; two null materials are equal
        /// </summary>
        /// <param name="obj1">The first value</param>
        /// <param name="obj2">The second value</param>
        /// <returns>True if they are equal</returns>
        public static bool operator ==(InterlayerMaterial obj1, InterlayerMaterial obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator
        /// </summary>
        /// <param name="obj1">The first value</param>
        /// <param name="obj2">The second value</param>
        /// <returns>True if they are different</returns>
        public static bool operator !=(InterlayerMaterial obj1, InterlayerMaterial obj2)
        {
            return !(obj1 == obj2);
        }
        #endregion

        #endregion

        #region NESTED CLASS

        /// <summary>
        /// The table of the shear modulus of a load duration: shear modulus as function of the temperature
        /// </summary>
        public sealed class LoadDurationShearModules : IComparable<LoadDurationShearModules>, IEquatable<LoadDurationShearModules>
        {
            #region VARIABLES

            /// <summary>
            /// The load duration
            /// </summary>
            private double _loadDuration;
            /// <summary>
            /// The shear moduli at the temperatures
            /// </summary>
            private List<TemperatureShearModule> _temperatureShearModules;

            #endregion

            #region PROPERTIES

            /// <summary>
            /// The load duration
            /// </summary>
            public double LoadDuration { get => _loadDuration; set => _loadDuration = value; }
            /// <summary>
            /// The shear moduli at the temperatures (the list of the table)
            /// </summary>
            public List<TemperatureShearModule> TemperatureShearModules { get => _temperatureShearModules; set => _temperatureShearModules = value; }

            #endregion

            #region CONSTRUCTOR

            /// <summary>
            /// Creates a table (the list instance is kept)
            /// </summary>
            /// <param name="loadDuration">The load duration</param>
            /// <param name="temperatureShearModulus">The shear moduli at the temperatures</param>
            public LoadDurationShearModules(double loadDuration, List<TemperatureShearModule> temperatureShearModulus)
            {
                _loadDuration = loadDuration;
                _temperatureShearModules = temperatureShearModulus;
            }

            /// <summary>
            /// Creates an empty table
            /// </summary>
            /// <param name="loadDuration">The load duration</param>
            public LoadDurationShearModules(double loadDuration)
                : this(loadDuration, null)
            {
                _temperatureShearModules = new List<TemperatureShearModule>();
            }

            #endregion

            #region PUBLIC METHODS

            /// <summary>
            /// Adds a temperature with its shear modulus
            /// </summary>
            /// <param name="temperatureShearModule">The pair temperature - shear modulus</param>
            public void AddTemperatures(TemperatureShearModule temperatureShearModule)
            {
                _temperatureShearModules.Add(temperatureShearModule);
            }

            /// <summary>
            /// Adds temperatures with their shear moduli
            /// </summary>
            /// <param name="temperature">The temperatures</param>
            /// <param name="shearModules">The shear moduli</param>
            /// <exception cref="ArgumentException">If the arrays have different lengths</exception>
            public void AddTemperatures(double[] temperature, double[] shearModules)
            {
                if (temperature.Length != shearModules.Length)
                    throw new ArgumentException("Temperature and shearModules lenghts are different");

                for (int i = 0; i < temperature.Length; i++)
                {
                    _temperatureShearModules.Add(new TemperatureShearModule(temperature[i], shearModules[i]));
                }
            }

            /// <summary>
            /// The shear modulus at a temperature (see <see cref="GetShearModulus"/>)
            /// </summary>
            /// <param name="temperature">The temperature</param>
            /// <returns>The shear modulus</returns>
            public double GetShearModule(double temperature)
            {
                return this[temperature];
            }

            /// <summary>
            /// Sort the Temperature shearModulus list in place (by temperature).
            /// </summary>
            public void Sort()
            {
                _temperatureShearModules.Sort();
            }

            /// <summary>
            /// The shear modulus at a temperature: the value of the table or the linear interpolation between the two temperatures around it
            /// </summary>
            /// <param name="temperature">The temperature</param>
            /// <returns>The shear modulus</returns>
            /// <exception cref="IndexOutOfRangeException">If the temperature is out of the table</exception>
            /// <exception cref="KeyNotFoundException">If the value is not found</exception>
            public double GetShearModulus(double temperature)
            {
                for (int i = 0; i < _temperatureShearModules.Count; i++)
                {
                    if (_temperatureShearModules[i].Temperature == temperature)
                        return _temperatureShearModules[i].ShearModule;

                    if (i == 0)
                    {
                        if (temperature < _temperatureShearModules[i].Temperature)
                        {
                            throw new IndexOutOfRangeException("Requested temperature lower than minimum temperature available");
                        }

                        if (_temperatureShearModules[i].Temperature <= temperature && _temperatureShearModules[i + 1].Temperature >= temperature)
                        {
                            return Interpolation.GetLinearInterpolation(_temperatureShearModules[i].Temperature, _temperatureShearModules[i + 1].Temperature,
                                                                         _temperatureShearModules[i].ShearModule, _temperatureShearModules[i + 1].ShearModule,
                                                                          temperature);
                        }
                    }
                    else if (i < _temperatureShearModules.Count - 1)
                    {
                        if (_temperatureShearModules[i].Temperature <= temperature && _temperatureShearModules[i + 1].Temperature >= temperature)
                        {
                            return Interpolation.GetLinearInterpolation(_temperatureShearModules[i].Temperature, _temperatureShearModules[i + 1].Temperature,
                                                                         _temperatureShearModules[i].ShearModule, _temperatureShearModules[i + 1].ShearModule,
                                                                          temperature);
                        }
                    }
                    else if (i == _temperatureShearModules.Count - 1) // Caso di temperature == all'ultimo valore già coperto all'inizio.
                    {
                        if (temperature > _temperatureShearModules[i].Temperature)
                        {
                            throw new IndexOutOfRangeException("Requested temperature greater than maximum temperature available");
                        }
                    }
                }
                throw new KeyNotFoundException();
            }

            #endregion

            #region INDEXER

            /// <summary>
            /// The shear modulus at a temperature (see <see cref="GetShearModulus"/>)
            /// </summary>
            /// <param name="temperature">The temperature</param>
            /// <returns>The shear modulus</returns>
            public double this[double temperature] { get => GetShearModulus(temperature); }

            #endregion

            #region INTERFACE IMPLEMENTATION

            /// <summary>
            /// The order of the tables: by load duration
            /// </summary>
            /// <param name="other">The other table</param>
            /// <returns>-1, 0 or 1</returns>
            int IComparable<LoadDurationShearModules>.CompareTo(LoadDurationShearModules other)
            {
                if (_loadDuration > other._loadDuration)
                    return 1;
                if (_loadDuration < other._loadDuration)
                    return -1;
                else
                    return 0;
            }
            /// <summary>
            /// Equality of the load duration and of the values (in the same order)
            /// </summary>
            /// <param name="other">The table to compare</param>
            /// <returns>True if the tables are equal</returns>
            public bool Equals(LoadDurationShearModules other)
            {
                if (ReferenceEquals(this, other))
                    return true;

                if (other is null || !other._loadDuration.Equals(_loadDuration) ||
                    other._temperatureShearModules.Count != _temperatureShearModules.Count)
                    return false;

                bool isEqual = true;
                for (int i = 0; i < other._temperatureShearModules.Count; i++)
                {
                    if (!other._temperatureShearModules[i].Equals(_temperatureShearModules[i]))
                    {
                        isEqual = false;
                        break;
                    }

                }
                return isEqual;
            }

            /// <summary>
            /// Reference equality (it calls object.Equals, not <see cref="Equals(LoadDurationShearModules)"/>)
            /// </summary>
            /// <param name="obj">The object to compare</param>
            /// <returns>True if <paramref name="obj"/> is the same instance</returns>
            public override bool Equals(object obj)
            {
                return base.Equals(obj as LoadDurationShearModules);
            }

            /// <summary>
            /// The hash code of the load duration and of the list of the values (as instance)
            /// </summary>
            /// <returns>The hash code</returns>
            public override int GetHashCode()
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + _loadDuration.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<List<TemperatureShearModule>>.Default.GetHashCode(_temperatureShearModules);
                return hashCode;
            }

            /// <summary>
            /// Equality operator; two null tables are equal
            /// </summary>
            /// <param name="obj1">The first value</param>
            /// <param name="obj2">The second value</param>
            /// <returns>True if they are equal</returns>
            public static bool operator ==(LoadDurationShearModules obj1, LoadDurationShearModules obj2)
            {
                if (ReferenceEquals(obj1, obj2))
                    return true;

                if (obj1 is null || obj2 is null)
                    return false;

                return obj1.Equals(obj2);
            }

            /// <summary>
            /// Inequality operator
            /// </summary>
            /// <param name="obj1">The first value</param>
            /// <param name="obj2">The second value</param>
            /// <returns>True if they are different</returns>
            public static bool operator !=(LoadDurationShearModules obj1, LoadDurationShearModules obj2)
            {
                return !(obj1 == obj2);
            }

            #endregion
        }

        /// <summary>
        /// A pair temperature - shear modulus
        /// </summary>
        public sealed class TemperatureShearModule : IComparable<TemperatureShearModule>, IEquatable<TemperatureShearModule>
        {
            #region VARIABLES

            /// <summary>
            /// The temperature
            /// </summary>
            private double _temperature;
            /// <summary>
            /// The shear modulus
            /// </summary>
            private double _shearModule;

            #endregion

            #region PROPERTIES

            /// <summary>
            /// The temperature
            /// </summary>
            public double Temperature { get => _temperature; set => _temperature = value; }
            /// <summary>
            /// The shear modulus
            /// </summary>
            public double ShearModule { get => _shearModule; set => _shearModule = value; }

            #endregion

            #region CONSTRUCTOR

            /// <summary>
            /// Creates a pair
            /// </summary>
            /// <param name="temperature">The temperature</param>
            /// <param name="shearModule">The shear modulus</param>
            public TemperatureShearModule(double temperature, double shearModule)
            {
                _temperature = temperature;
                _shearModule = shearModule;
            }

            #endregion

            #region INTERFACE IMPLEMENTATION

            /// <summary>
            /// The order of the pairs: by temperature
            /// </summary>
            /// <param name="other">The other pair</param>
            /// <returns>-1, 0 or 1</returns>
            int IComparable<TemperatureShearModule>.CompareTo(TemperatureShearModule other)
            {
                if (_temperature > other._temperature)
                    return 1;
                if (_temperature < other._temperature)
                    return -1;
                else
                    return 0;
            }

            /// <summary>
            /// Equality of temperature and shear modulus (exact)
            /// </summary>
            /// <param name="other">The pair to compare</param>
            /// <returns>True if the pairs are equal</returns>
            public bool Equals(TemperatureShearModule other)
            {
                if (ReferenceEquals(this, other))
                    return true;

                return !(other is null) &&
                    other._temperature.Equals(_temperature) &&
                    other._shearModule.Equals(_shearModule);
            }

            /// <summary>
            /// Reference equality (it calls object.Equals, not <see cref="Equals(TemperatureShearModule)"/>)
            /// </summary>
            /// <param name="obj">The object to compare</param>
            /// <returns>True if <paramref name="obj"/> is the same instance</returns>
            public override bool Equals(object obj)
            {
                return base.Equals(obj as TemperatureShearModule);
            }

            /// <summary>
            /// The hash code of temperature and shear modulus
            /// </summary>
            /// <returns>The hash code</returns>
            public override int GetHashCode()
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + _temperature.GetHashCode();
                hashCode = hashCode * -17 + _shearModule.GetHashCode();
                return hashCode;
            }

            /// <summary>
            /// Equality operator; two null pairs are equal
            /// </summary>
            /// <param name="obj1">The first value</param>
            /// <param name="obj2">The second value</param>
            /// <returns>True if they are equal</returns>
            public static bool operator ==(TemperatureShearModule obj1, TemperatureShearModule obj2)
            {
                if (ReferenceEquals(obj1, obj2))
                    return true;

                if (obj1 is null || obj2 is null)
                    return false;

                return obj1.Equals(obj2);
            }
            /// <summary>
            /// Inequality operator
            /// </summary>
            /// <param name="obj1">The first value</param>
            /// <param name="obj2">The second value</param>
            /// <returns>True if they are different</returns>
            public static bool operator !=(TemperatureShearModule obj1, TemperatureShearModule obj2)
            {
                return !(obj1 == obj2);
            }

            #endregion
        }

        #endregion
    }
}
