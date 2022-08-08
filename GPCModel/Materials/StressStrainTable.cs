using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
    //struct perchè è pensata per essere un value type

    /// <remarks>Sign convention: Stress and strain negative if compression</remarks>
    [Serializable]
    public struct StressStrainTable
    {
        #region Variables

        private double[] _stresses;
        private double[] _strains;

        #endregion

        #region Properties

        public double[] Stresses => (double[])_stresses.Clone(); // ritoriamo il clone in quanto serve che i valori siano blindati 

        public double[] Strains => (double[])_strains.Clone();

        #endregion

        #region Constructor

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
            if (stresses != null && strains != null)
                if (stresses.Length != strains.Length && stresses != null && strains != null)
                    throw new ArgumentException();

            if (stresses == null)
                stresses = new double[0];
            if (strains == null)
                strains = new double[0];

            _stresses = stresses;
            _strains = strains;

            if (_stresses.Length == 0 || _strains.Length == 0)
                Add(0, 0);

            if (_stresses[0] != 0 || _strains[0] != 0)
                Insert(0, 0, 0);
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Append new values to the array
        /// </summary>
        /// <param name="stress">The stress value to add</param>
        /// <param name="strain">The strain value to add</param>
        public void Add(double stress, double strain)
        {
            double[] newStesses = new double[_stresses.Length + 1];
            for (int i = 0; i < _stresses.Length; i++)
                newStesses[i] = _stresses[i];
            newStesses[_stresses.Length] = stress;
            _stresses = newStesses;

            double[] newStrains = new double[_strains.Length + 1];
            for (int i = 0; i < _strains.Length; i++)
                newStrains[i] = _strains[i];
            newStrains[_strains.Length] = strain;
            _strains = newStrains;
        }

        /// <summary>
        /// Insert new values at the given position
        /// </summary>
        /// <param name="pos">The position where to add the vew values</param>
        /// <param name="stress">The stress value</param>
        /// <param name="strain">The strain value</param>
        public void Insert(int pos, double stress, double strain)
        {
            double[] newStesses = new double[_stresses.Length + 1];
            for (int i = 0; i < newStesses.Length; i++)
            {
                if (i < pos)
                    newStesses[i] = _stresses[i];
                else if (i == pos)
                    newStesses[i] = stress;
                else
                    newStesses[i] = _stresses[i - 1];
            }
            _stresses = newStesses;

            double[] newStrains = new double[_strains.Length + 1];
            for (int i = 0; i < newStrains.Length; i++)
            {
                if (i < pos)
                    newStrains[i] = _strains[i];
                else if (i == pos)
                    newStrains[i] = strain;
                else
                    newStrains[i] = _strains[i - 1];
            }
            _strains = newStrains;
        }

        /// <summary>
        /// Remove the values at given position
        /// </summary>
        /// <param name="pos"></param>
        public void Remove(int pos)
        {
            double[] newStesses = new double[_stresses.Length - 1];
            for (int i = 0; i < _stresses.Length; i++)
            {
                if (i < pos)
                    newStesses[i] = _stresses[i];
                else if (i > pos)
                    newStesses[i - 1] = _stresses[i];
            }
            _stresses = newStesses;

            double[] newStrains = new double[_strains.Length - 1];
            for (int i = 0; i < _strains.Length; i++)
            {
                if (i < pos)
                    newStrains[i] = _strains[i];
                else if (i > pos)
                    newStrains[i - 1] = _strains[i];
            }
            _strains = newStrains;
        }

        /// <summary>
        /// Set the value <paramref name="stress"/> at the position <paramref name="pos"/>
        /// </summary>
        /// <param name="stress"></param>
        /// <param name="pos"></param>
        public void SetStress(double stress, int pos)
        {
            _stresses[pos] = stress;
        }

        /// <summary>
        /// Set the value <paramref name="strain"/> at the position <paramref name="pos"/>
        /// </summary>
        /// <param name="strain"></param>
        /// <param name="pos"></param>
        public void SetStrain(double strain, int pos)
        {
            _strains[pos] = strain;
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
                    return _stresses[i];

                if (Math.Abs(_strains[i]) > Math.Abs(strain) && i > 0)
                    return Utilities.Maths.Interpolation.GetLinearInterpolation(_strains[i], _strains[i - 1], _stresses[i], _stresses[i - 1], strain);
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

            int index = Array.IndexOf(_stresses, max);
            strain = _strains[index];
            return max;
        }

        /// <param name="strain">Strain associated to maximum stress</param>
        /// <returns>Maximum stress</returns>
        public double GetMinimumStress(out double strain)
        {
            double min = _stresses.Min();

            int index = Array.IndexOf(_stresses, min);
            strain = _strains[index];

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

        public bool IsHardening()
		{
            int sign = Math.Sign(_stresses[1] - _stresses[0]);

            for (int i = 1; i < _stresses.Length - 1; i++)
			{
                double diff = _stresses[i + 1] - _stresses[i];
                int signBuffer = Math.Sign(diff);

                if (signBuffer != sign && signBuffer != 0)
                    return false;
			}

            return true;
		}

		#endregion

		#region Equals - hashcode - operators

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

        #endregion
    }
}
