using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GPC.Model.Materials
{
    //struct perchè è pensata per essere un value type

    /// <remarks>Sign convention: Stress and strain negative if compression</remarks>
    public struct StressStrainTable
    {
        private readonly double[] _stresses;
        private readonly double[] _strains;

        public double[] Stresses => (double[])_stresses.Clone(); // ritoriamo il clone in quanto serve che i valori siano blindati 
        public double[] Strains => (double[])_strains.Clone();

        /// <summary>
        /// If stresses[0] or strains[0] are not zero this will be added automatically.
        /// </summary>
        /// <param name="stresses"></param>
        /// <param name="strains"></param>
        /// <remarks>
        /// Strain value assumed to be ordered from smaller to greatest
        /// <para>Sign convention: Stress and strain negative if compression</para>
        /// </remarks>
        public StressStrainTable(double[] stresses, double[] strains)
        {
            if (stresses.Length != strains.Length)
                throw new ArgumentException();

            if (stresses.Length < 2 || strains.Length < 2) // servono almeno due valori
                throw new ArgumentException();


            if (stresses[0] != 0)
            {
                var buffer = new List<double>() { 0 };
                buffer.AddRange(stresses);
                _stresses = buffer.ToArray();
            }

            if (strains[0] != 0)
            {
                var buffer = new List<double>() { 0 };
                buffer.AddRange(strains);
                _strains = buffer.ToArray();
            }

            _stresses = stresses;
            _strains = strains;
        }

        /// <summary>
        /// Get stress associated to <paramref name="strain"/>
        /// </summary>
        /// <param name="strain"></param>
        /// <returns></returns>
        public double GetStress(double strain)
        {
            if (strain == _strains[0])
                return _stresses[0];


            for (int i = 1; i < _strains.Length; i++)
            {
                if (_strains[i] == strain)
                    return _strains[i];

                if (Math.Abs(_strains[i]) > Math.Abs(strain) && i > 0)
                {
                    double deltaSigma = _stresses[i] - _stresses[i - 1];
                    double deltaStrain = _strains[i] - _strains[i - 1];

                    if (deltaStrain == 0)
                    {
                        return _stresses[i - 1];
                    }

                    if (deltaStrain < 0)
                        throw new ArgumentException();

                    return _stresses[i - 1] + deltaSigma / deltaStrain * Math.Abs((strain - _strains[i - 1]));
                }
            }

            return 0;
        }

        /// <returns>maximum stress</returns>
        public double GetMaximumStress()
        {
            return _stresses.Max();
        }

        /// <returns>maximum stress</returns>
        public double GetMinimumStress()
        {
            return _stresses.Min();
        }

        public double GetLastStrain()
        {
            return _strains.Last();
        }

        public double GetLastStress()
        {
            return _stresses.Last();
        }

        /// <param name="strain">Strain associated to maximum stress</param>
        /// <returns>Maximum stress</returns>
        public double GetMaximumStress(out double strain)
        {
            double max = _stresses.Max();

            strain = Array.IndexOf(_stresses, max);

            return max;
        }

        /// <param name="strain">Strain associated to maximum stress</param>
        /// <returns>Maximum stress</returns>
        public double GetMinimumStress(out double strain)
        {
            double min = _stresses.Min();

            strain = Array.IndexOf(_stresses, min);

            return min;
        }

        /// <returns>ratio between first not null stress and strain</returns>
        public double GetElasticModulus()
        {
            if (_strains[1] != 0)
                return Math.Abs(_stresses[1] / _strains[1]);


            if (_stresses.Length > 2)
            {
                for (int i = 2; i < _stresses.Length; i++)
                {
                    if (_strains[i] != 0)
                        return Math.Abs(_stresses[i] / _strains[i]);
                }
            }

            return 0.0;
        }



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is StressStrainTable objCasted) && objCasted.Strains.SequenceEqual(_strains) &&
                                                           objCasted.Stresses.SequenceEqual(_stresses);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;

                for (int i = 0; i < _stresses.Length; i++)
                {
                    hashCode = hashCode * -19 * _stresses[i].GetHashCode();
                    hashCode = hashCode * -19 * _strains[i].GetHashCode();
                }

                return hashCode;
            }
        }

        public static bool operator ==(StressStrainTable left, StressStrainTable right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(StressStrainTable left, StressStrainTable right)
        {
            return !(left == right);
        }
    }
}
