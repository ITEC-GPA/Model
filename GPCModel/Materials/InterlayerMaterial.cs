using GPC.Utilities.Maths;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Utilities.Attributes;
using System.ComponentModel;
using GPC.Utilities.Converters;

namespace GPC.Model.Materials
{
    [Serializable]
    [UI(Description = "Interlayer", Group = "Materials", Kind = "Material")]
    public sealed class InterlayerMaterial : Material, IEquatable<InterlayerMaterial>
    {
        #region PUBLIC ENUMS

        [Serializable]
        [TypeConverter(typeof(EnumDescriptionTypeConverter))]
        public enum InterlayerType
        {
            [Description("AcusticPVB / Family0 prEN")] AcusticPVB = 0,
            [Description("NormalPVB / Family1 prEN")] NormalPVB = 1,
            [Description("SentryGlass / Family2 prEN")] SentryGlass = 2
        } 

        #endregion

        #region VARIABLES

        private List<LoadDurationShearModules> _shearModulus;
        private InterlayerType _type;

        #endregion

        #region PROPERTIES

        public InterlayerType Type => _type;

        #endregion

        #region CONSTRUCTOR

        public InterlayerMaterial(string name, double density, double alfaThermalExpansion, InterlayerType type, Guid guid)
            : base(name, 0, 0, density, alfaThermalExpansion, guid)
        {
            _shearModulus = new List<LoadDurationShearModules>();
            _type = type;
        }

        public InterlayerMaterial(string name, double density, double alfaThermalExpansion, InterlayerType type)
            : this(name, density, alfaThermalExpansion, type, Guid.NewGuid())
        {
        }

        public InterlayerMaterial(SerializationInfo info, StreamingContext context)
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
        public override double GetShearModule()
        {
            throw new NotImplementedException($"Do not use this method. Use: GetShearModule(double, double)");
        }


        public double GetShearModule(double loadDuration, double temperature)
        {
            return this[loadDuration, temperature];
        }

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
        /// Sort the load duration shear modulus list in place.
        /// </summary>
        public void Sort()
        {
            _shearModulus.Sort();
        }

        /// <summary>
        /// return the load durations list
        /// </summary>
        public List<double> GetLoadDurations()
        {
            return _shearModulus.Select(i => i.LoadDuration).ToList();
        }

        /// <summary>
        /// return the temperature list
        /// </summary>
        public List<double> GetTemperatures()
        {
            return _shearModulus.SelectMany(i => i.TemperatureShearModules.Select(j => j.Temperature)).Distinct().ToList();
        }


        #region Equals - Hashcode - Operators

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ShearModulus", _shearModulus);
            info.AddValue("InterlayerType", _type);
        }

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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;
            return Equals(obj as InterlayerMaterial);
        }

        public override int GetHashCode()
        {
            int hashCode = -23;
            hashCode = hashCode * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<List<LoadDurationShearModules>>.Default.GetHashCode(_shearModulus);
            hashCode = hashCode * -17 + _type.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(InterlayerMaterial obj1, InterlayerMaterial obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(InterlayerMaterial obj1, InterlayerMaterial obj2)
        {
            return !(obj1 == obj2);
        }
        #endregion

        #endregion

        #region INDEXER


        /// <param name="loadDuration"></param>
        /// <param name="temperature"></param>
        /// <returns>The value of Shear modulus</returns>
        /// <exception cref="IndexOutOfRangeException"></exception>
        /// <exception cref="KeyNotFoundException"></exception>
        public double this[double loadDuration, double temperature]
        {
            get
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
        }

        #endregion

        #region NESTED CLASS

        private sealed class LoadDurationShearModules : IComparable<LoadDurationShearModules>, IEquatable<LoadDurationShearModules>
        {
            #region VARIABLES

            private double _loadDuration;
            private List<TemperatureShearModule> _temperatureShearModules;

            #endregion

            #region PROPERTIES

            public double LoadDuration => _loadDuration;
            public List<TemperatureShearModule> TemperatureShearModules => _temperatureShearModules;

            #endregion

            #region CONSTRUCTOR

            public LoadDurationShearModules(double loadDuration, List<TemperatureShearModule> temperatureShearModulus)
            {
                _loadDuration = loadDuration;
                _temperatureShearModules = temperatureShearModulus;
            }

            public LoadDurationShearModules(double loadDuration)
                : this(loadDuration, null)
            {
                _temperatureShearModules = new List<TemperatureShearModule>();
            }

            #endregion

            #region PUBLIC METHODS

            public void AddTemperatures(TemperatureShearModule temperatureShearModule)
            {
                _temperatureShearModules.Add(temperatureShearModule);
            }

            public void AddTemperatures(double[] temperature, double[] shearModules)
            {
                if (temperature.Length != shearModules.Length)
                    throw new ArgumentException("Temperature and shearModules lenghts are different");

                for (int i = 0; i < temperature.Length; i++)
                {
                    _temperatureShearModules.Add(new TemperatureShearModule(temperature[i], shearModules[i]));
                }
            }

            public double GetShearModule(double temperature)
            {
                return this[temperature];
            }

            /// <summary>
            /// Sort the Temperature shearModulus list in place.
            /// </summary>
            public void Sort()
            {
                _temperatureShearModules.Sort();
            }

            #endregion

            #region INDEXER

            public double this[double temperature]
            {
                get
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
            }

            #endregion

            #region INTERFACE IMPLEMENTATION

            int IComparable<LoadDurationShearModules>.CompareTo(LoadDurationShearModules other)
            {
                if (_loadDuration > other._loadDuration)
                    return 1;
                if (_loadDuration < other._loadDuration)
                    return -1;
                else
                    return 0;
            }
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

            public override bool Equals(object obj)
            {
                return base.Equals(obj as LoadDurationShearModules);
            }

            public override int GetHashCode()
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + _loadDuration.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<List<TemperatureShearModule>>.Default.GetHashCode(_temperatureShearModules);
                return hashCode;
            }

            public static bool operator ==(LoadDurationShearModules obj1, LoadDurationShearModules obj2)
            {
                if (ReferenceEquals(obj1, obj2))
                    return true;

                if (obj1 is null || obj2 is null)
                    return false;

                return obj1.Equals(obj2);
            }

            public static bool operator !=(LoadDurationShearModules obj1, LoadDurationShearModules obj2)
            {
                return !(obj1 == obj2);
            }

            #endregion
        }

        private sealed class TemperatureShearModule : IComparable<TemperatureShearModule>, IEquatable<TemperatureShearModule>
        {
            #region VARIABLES

            private double _temperature;
            private double _shearModule;

            #endregion

            #region PROPERTIES

            public double Temperature => _temperature;
            public double ShearModule => _shearModule;

            #endregion

            #region CONSTRUCTOR

            public TemperatureShearModule(double temperature, double shearModule)
            {
                _temperature = temperature;
                _shearModule = shearModule;
            }

            #endregion

            #region INTERFACE IMPLEMENTATION

            int IComparable<TemperatureShearModule>.CompareTo(TemperatureShearModule other)
            {
                if (_temperature > other._temperature)
                    return 1;
                if (_temperature < other._temperature)
                    return -1;
                else
                    return 0;
            }

            public bool Equals(TemperatureShearModule other)
            {
                if (ReferenceEquals(this, other))
                    return true;

                return !(other is null) && other._temperature.Equals(_temperature)
                                        && other._shearModule.Equals(_shearModule);
            }

            public override bool Equals(object obj)
            {
                return base.Equals(obj as TemperatureShearModule);
            }

            public override int GetHashCode()
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + _temperature.GetHashCode();
                hashCode = hashCode * -17 + _shearModule.GetHashCode();
                return hashCode;
            }

            public static bool operator ==(TemperatureShearModule obj1, TemperatureShearModule obj2)
            {
                if (ReferenceEquals(obj1, obj2))
                    return true;

                if (obj1 is null || obj2 is null)
                    return false;

                return obj1.Equals(obj2);
            }
            public static bool operator !=(TemperatureShearModule obj1, TemperatureShearModule obj2)
            {
                return !(obj1 == obj2);
            }

            #endregion
        }

        #endregion 
    }
}
