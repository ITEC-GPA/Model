using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.Serialization;
using GPC.Utilities.Maths;

namespace GPC.Model.Materials
{
    [Serializable]
    public class InterlayerMaterial : Material
    {
        #region VARIABLES
        private List<LoadDurationShearModules> _shearModulus; 
        #endregion

        #region CONSTRUCTOR

        public InterlayerMaterial(double density, double alfaThermalExpansion, Guid guid)
            : base(density, alfaThermalExpansion, guid)
        {

        }

        public InterlayerMaterial(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _shearModulus = (List<LoadDurationShearModules>)info.GetValue("ShearModulus", typeof(List<LoadDurationShearModules>));
        }

        #endregion CONSTRUCTOR

        #region PUBLIC METHODS
        public double GetShearModule(double loadDuration, double temperature)
        {
            return this[loadDuration, temperature];
        }

        public void AddShearModule(double loadDuration, List<double> temperature, List<double> shearModules)
        {
            if (temperature.Count != shearModules.Count)
                throw new ArgumentException("Temperature and shearModules lenghts are different");

            var ld = new LoadDurationShearModules(loadDuration);
            ld.AddTemperatures(temperature, shearModules);

            _shearModulus.Add(ld);
        }

        /// <summary>
        /// Sort the load duration shear modulus list in place.
        /// </summary>
        public void Sort()
        {
            _shearModulus.Sort();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ShearModulus", _shearModulus);
        }

        #endregion

        #region INDEXER
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
                    }
                    else if (i == _shearModulus.Count) // Caso di temperature == all'ultimo valore già coperto all'inizio.
                    {
                        if (loadDuration > _shearModulus[i].LoadDuration)
                        {
                            throw new IndexOutOfRangeException("Requested load duration is greater than maximum load duration available");
                        }
                    }

                    if (_shearModulus[i].LoadDuration <= loadDuration && _shearModulus[i + 1].LoadDuration >= loadDuration)
                    {
                        return Interpolation.GetLinearInterpolation(_shearModulus[i].LoadDuration, _shearModulus[i + 1].LoadDuration,
                                                                    _shearModulus[i][temperature], _shearModulus[i + 1][temperature],
                                                                      temperature);
                    }
                }
                throw new KeyNotFoundException();
            }
        } 
        #endregion

        #region NESTED CLASS
        private sealed class LoadDurationShearModules : IComparable<LoadDurationShearModules>
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

            }
            #endregion

            #region PUBLIC METHODS
            public void AddTemperatures(TemperatureShearModule temperatureShearModule)
            {
                _temperatureShearModules.Add(temperatureShearModule);
            }
            public void AddTemperatures(List<double> temperature, List<double> shearModules)
            {
                if (temperature.Count != shearModules.Count)
                    throw new ArgumentException("Temperature and shearModules lenghts are different");

                for (int i = 0; i < temperature.Count; i++)
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
                            if(temperature < _temperatureShearModules[i].Temperature)
                            {
                                throw new IndexOutOfRangeException("Requested temperature lower than minimum temperature available");
                            }
                        }
                        else if (i == _temperatureShearModules.Count) // Caso di temperature == all'ultimo valore già coperto all'inizio.
                        {
                            if (temperature > _temperatureShearModules[i].Temperature)
                            {
                                throw new IndexOutOfRangeException("Requested temperature greater than maximum temperature available");
                            }                           
                        }

                        if (_temperatureShearModules[i].Temperature <= temperature && _temperatureShearModules[i + 1].Temperature >= temperature)
                        {
                            return Interpolation.GetLinearInterpolation(_temperatureShearModules[i].Temperature, _temperatureShearModules[i + 1].Temperature,
                                                                         _temperatureShearModules[i].ShearModule, _temperatureShearModules[i + 1].ShearModule,
                                                                          temperature);
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
            #endregion
        }

        private sealed class TemperatureShearModule : IComparable<TemperatureShearModule>
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
            #endregion
        }
        #endregion
    }
}