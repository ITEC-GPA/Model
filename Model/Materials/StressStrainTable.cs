using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
    /// <summary>
    /// A stress-strain curve as a table of points starting from (0, 0), with linear interpolation between them. It is a value type (struct), but
    /// the arrays are shared by the copies until a point is added, inserted or removed
    /// </summary>
    /// <remarks>Sign convention: Stress and strain negative if compression</remarks>
    [Serializable]
    public struct StressStrainTable
    {
        #region Variables

        /// <summary>
        /// The stresses of the points
        /// </summary>
        private double[] _stresses;
        /// <summary>
        /// The strains of the points
        /// </summary>
        private double[] _strains;

        #endregion

        #region Properties

        /// <summary>
        /// A copy of the stresses of the points (the table can not be changed through it)
        /// </summary>
        public double[] Stresses => (double[])_stresses.Clone();

        /// <summary>
        /// A copy of the strains of the points
        /// </summary>
        public double[] Strains => (double[])_strains.Clone();

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a table from stresses and strains (the arrays are kept, not copied). If stresses[0] or strains[0] are not zero, the point (0, 0) is
        /// inserted at the start; an empty table gets the point (0, 0)
        /// </summary>
        /// <param name="stresses">The stresses (null: empty)</param>
        /// <param name="strains">The strains (null: empty)</param>
        /// <exception cref="ArgumentException">If the arrays have different lengths</exception>
        /// <remarks>
        /// Strain value assumed to be ordered from smaller to greatest (in absolute value, from zero)
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
        /// <param name="pos">The position of the point to remove</param>
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
        /// <param name="stress">The new stress</param>
        /// <param name="pos">The position of the point</param>
        public void SetStress(double stress, int pos)
        {
            _stresses[pos] = stress;
        }

        /// <summary>
        /// Set the value <paramref name="strain"/> at the position <paramref name="pos"/>
        /// </summary>
        /// <param name="strain">The new strain</param>
        /// <param name="pos">The position of the point</param>
        public void SetStrain(double strain, int pos)
        {
            _strains[pos] = strain;
        }

        /// <summary>
        /// Get stress associated to <paramref name="strain"/>: linear interpolation between the points whose strains (in absolute value) contain it
        /// </summary>
        /// <param name="strain">The strain (with the sign of the table)</param>
        /// <returns>The stress; 0 if the strain is beyond the last point</returns>
        public double GetStress(double strain)
        {
            if (strain == _strains[0])
                return _stresses[0];

            for (int i = 1; i < _strains.Length; i++)
            {
                if (_strains[i] == strain)
                    return _stresses[i];

                if (Math.Abs(_strains[i]) > Math.Abs(strain) && i > 0)
                    return GPC.Utilities.Maths.Interpolation.GetLinearInterpolation(_strains[i], _strains[i - 1], _stresses[i], _stresses[i - 1], strain);
            }

            return 0;
        }

        /// <summary>
        /// Get strains associated to <paramref name="stress"/>: the strains of the points with that stress and of the segments that cross it
        /// (in absolute value)
        /// </summary>
        /// <param name="stress">The stress</param>
        /// <returns>The strains (more than one if the curve is not monotonic; empty if the stress is never reached)</returns>
        public double[] GetStrain(double stress)
        {
            List<double> strains = new List<double>();

            for (int i = 0; i < _stresses.Length; i++)
            {
                if (_stresses[i] == stress)
                    strains.Add(_strains[i]);

                if (i != _strains.Length - 1)
                {

                    if (Math.Abs(_stresses[i]) < Math.Abs(stress) && Math.Abs(_stresses[i + 1]) > Math.Abs(stress) ||
                        Math.Abs(_stresses[i + 1]) < Math.Abs(stress) && Math.Abs(_stresses[i]) > Math.Abs(stress))
                        strains.Add(GPC.Utilities.Maths.Interpolation.GetLinearInterpolation(_stresses[i], _stresses[i + 1], _strains[i], _strains[i + 1], stress));
                }
            }

            return strains.ToArray();
        }

        /// <summary>
        /// The largest stress of the table
        /// </summary>
        /// <returns>maximum stress</returns>
        public double GetMaximumStress()
        {
            return _stresses.Max();
        }

        /// <summary>
        /// The smallest stress of the table (the largest compression)
        /// </summary>
        /// <returns>minimum stress</returns>
        public double GetMinimumStress()
        {
            return _stresses.Min();
        }

        /// <summary>
        /// The strain of the last point
        /// </summary>
        /// <returns>The last strain</returns>
        public double GetLastStrain()
        {
            return _strains.Last();
        }

        /// <summary>
        /// The stress of the last point
        /// </summary>
        /// <returns>The last stress</returns>
        public double GetLastStress()
        {
            return _stresses.Last();
        }

        /// <summary>
        /// The largest stress of the table and its strain
        /// </summary>
        /// <param name="strain">Strain associated to maximum stress (the first one)</param>
        /// <returns>Maximum stress</returns>
        public double GetMaximumStress(out double strain)
        {
            double max = _stresses.Max();

            int index = Array.IndexOf(_stresses, max);
            strain = _strains[index];
            return max;
        }

        /// <summary>
        /// The smallest stress of the table and its strain
        /// </summary>
        /// <param name="strain">Strain associated to minimum stress (the first one)</param>
        /// <returns>Minimum stress</returns>
        public double GetMinimumStress(out double strain)
        {
            double min = _stresses.Min();

            int index = Array.IndexOf(_stresses, min);
            strain = _strains[index];

            return min;
        }

        /// <summary>
        /// The initial (secant) elastic modulus
        /// </summary>
        /// <returns>ratio between the stress and the strain of the first point with strain not zero (absolute value); 0 if there is none</returns>
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

        /// <summary>
        /// Tell if the curve is monotonic: the stress never changes direction (constant parts allowed)
        /// </summary>
        /// <returns>True if the stresses do not decrease after increasing (or the opposite)</returns>
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

		/// <summary>
		/// Equality of the points (exact values)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is a table with the same stresses and strains</returns>
		public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return (obj is StressStrainTable objCasted) && objCasted.Strains.SequenceEqual(_strains) &&
                                                           objCasted.Stresses.SequenceEqual(_stresses);
        }

        /// <summary>
        /// The hash code of the points (0 for the tables starting from (0, 0), the factors are multiplied)
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first table</param>
        /// <param name="right">The second table</param>
        /// <returns>True if the tables are equal</returns>
        public static bool operator ==(StressStrainTable left, StressStrainTable right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="left">The first table</param>
        /// <param name="right">The second table</param>
        /// <returns>True if the tables are different</returns>
        public static bool operator !=(StressStrainTable left, StressStrainTable right)
        {
            return !(left == right);
        }

        #endregion
    }
}
