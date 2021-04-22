using GPC.Model.LoadCases;
using GPC.Utilities.Extensions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GPC.Model.Combinations
{
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public abstract class Combination : ModelObject, ILoadCase, ICloneable
    {
        #region VARIABLES

        protected List<LoadCaseCoefficient> _coefficients;

        #endregion


        #region PUBLIC CONSTRUCTOR

        protected Combination(string name, Guid guid)
            : base(guid, name)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Combination name cannot be empty");

            this._name = name;
            this._coefficients = new List<LoadCaseCoefficient>();
        }

        protected Combination(string name)
            : this(name, Guid.NewGuid())
        {
        }

        protected Combination(Combination combination)
            : this(combination._name, combination.Guid)
        {
            _coefficients = combination._coefficients.ToList(); //Shallow copy, i puntatori dei loadcase non cambiano
        }

        protected Combination(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _coefficients = (List<LoadCaseCoefficient>)info.GetValue("Coefficients", typeof(List<LoadCaseCoefficient>));
        }

        #endregion

        #region Abstract methods

        public abstract bool IsUltimate();

        public abstract object Clone();

        /// <summary>
        /// Create a new empty <see cref="Combination"/> object. I.e. with the same properties except the <see cref="Combination.LoadCaseCoefficient"/> List that will be empty
        /// </summary>
        public abstract object CloneEmpty();

        #endregion Abstract methods

        #region PUBLIC METHODS

        #region Adder

        /// <param name="loadCases"></param>
        /// <param name="coefficients"></param>
        /// <remarks>The <paramref name="loadCases"/> will be added only if the coefficient is not zero</remarks>
        /// <exception cref="ArgumentException"> If <paramref name="loadCases"/> Count != <paramref name="coefficients"/> </exception>
        public virtual void AddLoadCaseCoefficients(IEnumerable<LoadCase> loadCases, IEnumerable<double> coefficients)
        {
            if (loadCases.Count() != coefficients.Count())
                throw new ArgumentException("loadcases and coefficients list lenght must be equal");
            else
            {
                using (var lc = loadCases.GetEnumerator())
                using (var cf = coefficients.GetEnumerator())
                    while (lc.MoveNext() && cf.MoveNext())
                    {
                        this[lc.Current] = cf.Current;
                    }
            }
        }

        /// <exception cref="ArgumentNullException"> If <see cref="LoadCase"/> is null </exception>
        public virtual void AddLoadCaseCoefficients(IEnumerable<(LoadCase, double)> loadCaseCoefficients)
        {
            foreach (var lcc in loadCaseCoefficients)
            {
                if (lcc.Item1 is null)
                    throw new ArgumentNullException();

                this[lcc.Item1] = lcc.Item2;
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="loadcase"></param>
        /// <param name="coefficient"></param>
        /// <remarks>The <paramref name="loadcase"/> will be added only if the coefficient is not zero</remarks>
        public virtual void AddLoadCaseCoefficient(LoadCase loadcase, double coefficient)
        {
            this[loadcase] = coefficient;
        }

        #endregion Adder

        #region Indexer

        /// <summary>
        ///
        /// </summary>
        /// <param name="loadcase"></param>
        /// <returns>The coefficient associated to the <paramref name="loadcase"/>
        /// <para>If the <paramref name="loadcase"/> is not found, then return 0</para></returns>
        /// <remarks>The <paramref name="loadcase"/> will be added only if the coefficient is not zero</remarks>
        public double this[LoadCase loadcase]
        {
            set
            {
                if (value != 0)
                {
                    var loadCaseCoefficient = _coefficients.Where(a => a.LoadCase == loadcase).ToList();

                    if (loadCaseCoefficient.Count() > 0) // Se loadcase è già presente in lista
                    {
                        LoadCaseCoefficient lcc = new LoadCaseCoefficient(value + loadCaseCoefficient.First().Coefficient, loadcase);

                        _coefficients.Remove(loadCaseCoefficient.First());

                        _coefficients.Add(lcc);
                    }
                    else
                    {
                        _coefficients.Add(new LoadCaseCoefficient(value, loadcase));
                    }
                    _coefficients.Sort();
                }
            }
            get
            {
                for (int i = 0; i < _coefficients.Count; i++)
                {
                    if (_coefficients[i].LoadCase == loadcase)
                    {
                        return _coefficients[i].Coefficient;
                    }
                }
                return 0;
            }
        }

        #endregion Indexer

        #region Getter

        /// <returns>The loadcases of this combination</returns>
        public virtual List<LoadCase> GetLoadCases()
        {
            return _coefficients.Select(i => i.LoadCase).ToList();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="loadcase"></param>
        /// <returns>The coefficient associated to the <paramref name="loadcase"/>
        /// <para>If the <paramref name="loadcase"/> is not found, then return 0</para>
        /// </returns>
        public virtual double GetLoadCaseCoefficient(LoadCase loadcase)
        {
            return this[loadcase];
        }

        public virtual List<double> GetLoadCaseCoefficients(out List<LoadCase> loadCases)
        {
            List<double> coefficients = new List<double>();
            loadCases = new List<LoadCase>();

            foreach (var coeff in _coefficients)
            {
                loadCases.Add(coeff.LoadCase);
                coefficients.Add(coeff.Coefficient);
            }

            return coefficients;
        }

        /// <returns>An array of all the pairs <see cref="LoadCase"/>-LoadCaseCoefficient of this combination</returns>
        public virtual KeyValuePair<LoadCase, double>[] GetLoadCaseCoefficientsPair()
        {
            KeyValuePair<LoadCase, double>[] pairs = new KeyValuePair<LoadCase, double>[_coefficients.Count];

            for (var i = 0; i < _coefficients.Count; i++)
            {
                pairs[i] = new KeyValuePair<LoadCase, double>(_coefficients[i].LoadCase, _coefficients[i].Coefficient);
            }

            return pairs;
        }

        /// <returns>An array of all the tuples <see cref="LoadCase"/>-LoadCaseCoefficient of this combination</returns>
        public virtual (LoadCase loadcase, double coefficient)[] GetLoadCaseCoefficientsTuple()
        {
            (LoadCase loadcase, double coefficient)[] pairs = new (LoadCase loadcase, double coefficient)[_coefficients.Count];

            for (var i = 0; i < _coefficients.Count; i++)
            {
                pairs[i] = (_coefficients[i].LoadCase, _coefficients[i].Coefficient);
            }

            return pairs;
        }

        /// <returns>An array of tuples <see cref="LoadCase"/>-LoadCaseCoefficient. Where the <see cref="LoadCase"/> are only the ones contained in <paramref name="loadCases"/></returns>
        public virtual (LoadCase loadcase, double coefficient)[] GetLoadCaseCoefficientsTuple(IEnumerable<LoadCase> loadCases)
        {
            var pairs = new List<(LoadCase loadcase, double coefficient)>();

            for (var i = 0; i < _coefficients.Count; i++)
            {
                if (loadCases.Contains(_coefficients[i].LoadCase))
                    pairs.Add((_coefficients[i].LoadCase, _coefficients[i].Coefficient));
            }

            return pairs.ToArray();
        }

        #endregion Getter

        #region Checks

        /// <summary>
        ///
        /// </summary>
        /// <param name="loadCases"></param>
        /// <returns><see langword="True"/> if all the elements of <paramref name="loadCases"/> are contained in this combination</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual bool ContainsLoadCases(IEnumerable<LoadCase> loadCases)
        {
            if (loadCases is null)
                throw new ArgumentNullException();

            if (loadCases.Count() > 0)
                return _coefficients.Select(i => i.LoadCase).Intersect(loadCases).Count().Equals(loadCases.Count());
            else
                return true;
        }

        /// <param name="loadCasesCoefficients"></param>
        /// <returns><see langword="True"/> if all KeyValuePairs are contained in this combination</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual bool ContainsLoadCaseCoefficients(IEnumerable<KeyValuePair<LoadCase, double>> loadCasesCoefficients)
        {
            if (loadCasesCoefficients is null)
                throw new ArgumentNullException();

            if (loadCasesCoefficients.Count() > 0)
            {
                return _coefficients.Select(i => new KeyValuePair<LoadCase, double>(i.LoadCase, i.Coefficient)).Except(loadCasesCoefficients).Count() == _coefficients.Count() - loadCasesCoefficients.Count();
            }
            else
                return true;
        }

        /// <param name="loadCasesCoefficients"></param>
        /// <returns><see langword="True"/> if all Tuple are contained in this combination</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual bool ContainsLoadCaseCoefficients(IEnumerable<(LoadCase loadcase, double coefficient)> loadCasesCoefficients)
        {
            if (loadCasesCoefficients is null)
                throw new ArgumentNullException();

            if (loadCasesCoefficients.Count() > 0)
            {
                return _coefficients.Select(i => (i.LoadCase, i.Coefficient)).Except(loadCasesCoefficients).Count() == _coefficients.Count() - loadCasesCoefficients.Count();
            }
            else
                return true;
        }

        #endregion Checks

        #region Edit

        /// <exception cref="ArgumentNullException"> If <see cref="LoadCase"/> is null </exception>
        public virtual void RemoveLoadCaseCoefficients(IEnumerable<(LoadCase, double)> loadCaseCoefficients)
        {
            foreach (var lcc in loadCaseCoefficients)
            {
                if (lcc.Item1 is null)
                    throw new ArgumentNullException();

                _coefficients.RemoveAll(i => i.LoadCase.Equals(lcc.Item1));
            }
        }

        #endregion Edit

        #endregion PUBLIC METHODS

        #region Equals - HashCode - Operators - Serialization - ToString

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Coefficients", _coefficients);
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < _coefficients.Count; i++)
            {
                sb.Append(_coefficients[i].ToString());
                if (i != _coefficients.Count - 1)
                {
                    sb.Append(" + ");
                }
            }
            return sb.ToString();
        }

        #endregion


        #region Equals - HashCode - Operators

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            Combination objCasted = obj as Combination;

            return !(objCasted is null) && _coefficients.ScrambledEquals(objCasted._coefficients) && base.Equals(objCasted);
        }

        public override int GetHashCode()
        {
            var hashCode = 23;
            hashCode = hashCode + base.GetHashCode();

            foreach (var element in _coefficients)
            {
                hashCode = hashCode + EqualityComparer<LoadCaseCoefficient>.Default.GetHashCode(element);
            }
            return hashCode;
        }

        public static bool operator ==(Combination obj1, Combination obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(Combination obj1, Combination obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Equals - HashCode - Operators - Serialization - ToString


        #region Nested protected class

        

        #endregion


        #region Nested class

        protected sealed class LoadCaseCoefficient : IComparable<LoadCaseCoefficient>, IEquatable<LoadCaseCoefficient>
        {
            private LoadCase _loadcase;
            private double _coefficient;

            public LoadCase LoadCase => _loadcase;
            public double Coefficient => _coefficient;

            public LoadCaseCoefficient(double coefficient, LoadCase loadCase)
            {
                this._loadcase = loadCase;
                this._coefficient = coefficient;
            }

            public override string ToString() => $"{String.Format("{0:0.0##}", Coefficient)}*{LoadCase.Name}";

            int IComparable<LoadCaseCoefficient>.CompareTo(LoadCaseCoefficient other)
            {
                if (_loadcase.GetLoadCaseType() != null && other.LoadCase.GetLoadCaseType() != null)
                {
                    if (_loadcase.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight && other.LoadCase.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight)
                        return 0;
                    else if (_loadcase.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight && other.LoadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SelfWeight)
                        return -1;
                    else if (_loadcase.GetLoadCaseType() != LoadCase.LoadCaseType.SelfWeight && other.LoadCase.GetLoadCaseType() == LoadCase.LoadCaseType.SelfWeight)
                        return 1;
                    else if (_loadcase.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad && other.LoadCase.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad)
                        return 0;
                    else if (_loadcase.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad
                            && (other.LoadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SuperImposedDeadLoad || other.LoadCase.GetLoadCaseType() != LoadCase.LoadCaseType.SelfWeight))
                        return -1;
                    else if ((_loadcase.GetLoadCaseType() != LoadCase.LoadCaseType.SuperImposedDeadLoad || _loadcase.GetLoadCaseType() != LoadCase.LoadCaseType.SelfWeight)
                            && other.LoadCase.GetLoadCaseType() == LoadCase.LoadCaseType.SuperImposedDeadLoad)
                        return 1;
                    else
                    {
                        if (_coefficient == other._coefficient)
                            return 0;
                        else
                            return _coefficient > other._coefficient ? -1 : 1;
                    }
                }
                else
                {
                    if (_coefficient == other._coefficient)
                        return 0;
                    else
                        return _coefficient > other._coefficient ? -1 : 1;
                }
            }

            public override bool Equals(object obj)
            {
                return Equals(obj as LoadCaseCoefficient);
            }

            public bool Equals(LoadCaseCoefficient other)
            {
                return other != null &&
                       EqualityComparer<LoadCase>.Default.Equals(_loadcase, other._loadcase) &&
                       _coefficient == other._coefficient;
            }

            public override int GetHashCode()
            {
                var hashCode = -23;
                hashCode = hashCode * -17 + EqualityComparer<LoadCase>.Default.GetHashCode(_loadcase);
                hashCode = hashCode * -17 + _coefficient.GetHashCode();
                return hashCode;
            }

            public static bool operator ==(LoadCaseCoefficient obj1, LoadCaseCoefficient obj2)
            {
                if (ReferenceEquals(obj1, obj2))
                    return true;

                if (obj1 is null || obj2 is null)
                    return false;

                return obj1.Equals(obj2);
            }

            public static bool operator !=(LoadCaseCoefficient obj1, LoadCaseCoefficient obj2)
            {
                return !(obj1 == obj2);
            }
        }

        private string GetDebuggerDisplay()
        {
            return $"{Name}: {ToString()}";
        }

        #endregion Nested protected class


        #region Equality comprarer

        /// <summary>
        /// Compare two <see cref="Combination"/> using only <see cref="Combination._coefficients"/> as equality parameters
        /// </summary>
        public class CombinationCoefficientEqualityComparer : IEqualityComparer<Combination>
        {
            /// <returns> <inheritdoc/>
            /// <para> true if both <paramref name="x"/> and <paramref name="y"/> are null </para>
            /// </returns>
            /// <remarks> Only <see cref="Combination._coefficients"/> are used as equality parameters</remarks>
            bool IEqualityComparer<Combination>.Equals(Combination x, Combination y)
            {
                if (ReferenceEquals(x, y))
                    return true;

                if (x == null && y == null)
                    return true;

                if (x == null || y == null)
                    return false;

                if (x._coefficients.ScrambledEquals(y._coefficients))
                    return true;

                return false;
            }

            /// <inheritdoc/>
            /// <remarks> Only <see cref="Combination._coefficients"/> are used as equality parameters </remarks>
            int IEqualityComparer<Combination>.GetHashCode(Combination obj)
            {
                var hashCode = 23;
                foreach (var element in obj._coefficients)
                {
                    hashCode = hashCode + EqualityComparer<LoadCaseCoefficient>.Default.GetHashCode(element);
                }
                return hashCode;
            }
        }

        #endregion
    }
}