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
    public class Combination : ModelObject, ILoadCase, ICloneable
    {
        protected List<LoadCaseCoefficient> _coefficients;

        protected int LoadCaseCount => _coefficients.Count;

        #region PUBLIC CONSTRUCTOR

        public Combination(string name, Guid guid)
            : base(guid, name)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Combination name cannot be empty");

            _name = name;
            _coefficients = new List<LoadCaseCoefficient>();
        }

        public Combination(string name)
            : this(name, Guid.NewGuid())
        {
        }

        public Combination(Combination combination)
            : this(combination._name, combination.Guid)
        {
            _coefficients = combination._coefficients.ToList(); //Shallow copy, i puntatori dei loadcase non cambiano
        }

        public Combination(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _coefficients = (List<LoadCaseCoefficient>)info.GetValue("Coefficients", typeof(List<LoadCaseCoefficient>));
        }

        #endregion

        #region Abstract methods
        /*
        public abstract bool IsUltimate();

        public abstract object Clone();

        /// <summary>
        /// Duplicate the object, overriding the name with a new one
        /// </summary>
        /// <param name="nameOverride">Name overriding</param>
        public abstract Combination Duplicate(string nameOverride);

        /// <summary>
        /// Create a new empty <see cref="Combination"/> object. I.e. with the same properties except the <see cref="Combination.LoadCaseCoefficient"/> List that will be empty
        /// </summary>
        public abstract object CloneEmpty();
        */

        public object Clone()
        {
            return null;
        }

        #endregion Abstract methods

        #region PUBLIC METHODS

        #region Adder

        /// <param name="loadCases"></param>
        /// <param name="coefficients"></param>
        /// <remarks>The <paramref name="loadCases"/> will be added only if the coefficient is not zero</remarks>
        /// <exception cref="ArgumentException"> If <paramref name="loadCases"/> Count != <paramref name="coefficients"/> </exception>
        public virtual void AddLoadCaseCoefficients(IEnumerable<LoadCaseBase> loadCases, IEnumerable<double> coefficients)
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

        /// <exception cref="ArgumentNullException"> If <see cref="LoadCaseBase"/> is null </exception>
        public virtual void AddLoadCaseCoefficients(IEnumerable<(LoadCaseBase, double)> loadCaseCoefficients)
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
        public virtual void AddLoadCaseCoefficient(LoadCaseBase loadcase, double coefficient)
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
        public double this[LoadCaseBase loadcase]
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
        public virtual List<LoadCaseBase> GetLoadCases()
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
        public virtual double GetLoadCaseCoefficient(LoadCaseBase loadcase)
        {
            return this[loadcase];
        }

        public virtual List<double> GetLoadCaseCoefficients(out List<LoadCaseBase> loadCases)
        {
            List<double> coefficients = new List<double>();
            loadCases = new List<LoadCaseBase>();

            foreach (var coeff in _coefficients)
            {
                loadCases.Add(coeff.LoadCase);
                coefficients.Add(coeff.Coefficient);
            }

            return coefficients;
        }

        /// <returns>An array of all the pairs <see cref="LoadCaseBase"/>-LoadCaseCoefficient of this combination</returns>
        public virtual KeyValuePair<LoadCaseBase, double>[] GetLoadCaseCoefficientsPair()
        {
            KeyValuePair<LoadCaseBase, double>[] pairs = new KeyValuePair<LoadCaseBase, double>[_coefficients.Count];

            for (var i = 0; i < _coefficients.Count; i++)
            {
                pairs[i] = new KeyValuePair<LoadCaseBase, double>(_coefficients[i].LoadCase, _coefficients[i].Coefficient);
            }

            return pairs;
        }

        /// <returns>An array of all the tuples <see cref="LoadCaseBase"/>-LoadCaseCoefficient of this combination</returns>
        public virtual (LoadCaseBase loadcase, double coefficient)[] GetLoadCaseCoefficientsTuple()
        {
            (LoadCaseBase loadcase, double coefficient)[] pairs = new (LoadCaseBase loadcase, double coefficient)[_coefficients.Count];

            for (var i = 0; i < _coefficients.Count; i++)
            {
                pairs[i] = (_coefficients[i].LoadCase, _coefficients[i].Coefficient);
            }

            return pairs;
        }

        /// <returns>An array of tuples <see cref="LoadCaseBase"/>-LoadCaseCoefficient. Where the <see cref="LoadCaseBase"/> are only the ones contained in <paramref name="loadCases"/></returns>
        public virtual (LoadCaseBase loadcase, double coefficient)[] GetLoadCaseCoefficientsTuple(IEnumerable<LoadCaseBase> loadCases)
        {
            var pairs = new List<(LoadCaseBase loadcase, double coefficient)>();

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
        public virtual bool ContainsLoadCases(IEnumerable<LoadCaseBase> loadCases)
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
        public virtual bool ContainsLoadCaseCoefficients(IEnumerable<KeyValuePair<LoadCaseBase, double>> loadCasesCoefficients)
        {
            if (loadCasesCoefficients is null)
                throw new ArgumentNullException();

            if (loadCasesCoefficients.Count() > 0)
            {
                return _coefficients.Select(i => new KeyValuePair<LoadCaseBase, double>(i.LoadCase, i.Coefficient)).Except(loadCasesCoefficients).Count() == _coefficients.Count() - loadCasesCoefficients.Count();
            }
            else
                return true;
        }

        /// <param name="loadCasesCoefficients"></param>
        /// <returns><see langword="True"/> if all Tuple are contained in this combination</returns>
        /// <exception cref="ArgumentNullException"></exception>
        public virtual bool ContainsLoadCaseCoefficients(IEnumerable<(LoadCaseBase loadcase, double coefficient)> loadCasesCoefficients)
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

        /// <exception cref="ArgumentNullException"> If <see cref="LoadCaseBase"/> is null </exception>
        public virtual void RemoveLoadCaseCoefficients(IEnumerable<(LoadCaseBase, double)> loadCaseCoefficients)
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

        private string GetDebuggerDisplay()
        {
            return $"{Name}: {ToString()}";
        }


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

        #region Nested class

        public sealed class LoadCaseCoefficient : IComparable<LoadCaseCoefficient>, IEquatable<LoadCaseCoefficient>
        {
            private LoadCaseBase _loadcase;
            private double _coefficient;

            public LoadCaseBase LoadCase => _loadcase;
            public double Coefficient => _coefficient;

            public LoadCaseCoefficient(double coefficient, LoadCaseBase loadCase)
            {
                _loadcase = loadCase;
                _coefficient = coefficient;
            }

            public override string ToString() => $"{String.Format("{0:0.0##}", Coefficient)}*{LoadCase.Name}";

            int IComparable<LoadCaseCoefficient>.CompareTo(LoadCaseCoefficient other)
            {
                if (_loadcase is LoadCase thisLoadCase && other._loadcase is LoadCase otherLoadCase)
                {
                    if (thisLoadCase.LoadCaseType == LoadCases.LoadCase.LoadCaseTypes.SelfWeight && thisLoadCase.LoadCaseType == LoadCases.LoadCase.LoadCaseTypes.SelfWeight)
                        return 0;
                    else if (thisLoadCase.LoadCaseType == LoadCases.LoadCase.LoadCaseTypes.SelfWeight && thisLoadCase.LoadCaseType != LoadCases.LoadCase.LoadCaseTypes.SelfWeight)
                        return -1;
                    else if (thisLoadCase.LoadCaseType != LoadCases.LoadCase.LoadCaseTypes.SelfWeight && thisLoadCase.LoadCaseType == LoadCases.LoadCase.LoadCaseTypes.SelfWeight)
                        return 1;
                    else if (thisLoadCase.LoadCaseType == LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad && thisLoadCase.LoadCaseType == LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
                        return 0;
                    else if (thisLoadCase.LoadCaseType == LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad
                            && (otherLoadCase.LoadCaseType != LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad || thisLoadCase.LoadCaseType != LoadCases.LoadCase.LoadCaseTypes.SelfWeight))
                        return -1;
                    else if ((thisLoadCase.LoadCaseType != LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad || thisLoadCase.LoadCaseType != LoadCases.LoadCase.LoadCaseTypes.SelfWeight)
                            && thisLoadCase.LoadCaseType == LoadCases.LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
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
                       EqualityComparer<LoadCaseBase>.Default.Equals(_loadcase, other._loadcase) &&
                       _coefficient == other._coefficient;
            }

            public override int GetHashCode()
            {
                var hashCode = -23;
                hashCode = hashCode * -17 + EqualityComparer<LoadCaseBase>.Default.GetHashCode(_loadcase);
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