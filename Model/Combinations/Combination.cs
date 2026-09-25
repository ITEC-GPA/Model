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

    /// <summary>
    /// A combination of load cases: a list of load cases with their coefficients (see <see cref="LoadCaseCoefficient"/>), sorted, and the
    /// options of the standard used to create it. Two combinations are equal if they have the same name, options and coefficients (in any order)
    /// </summary>
    /// <remarks>This is a mutable object</remarks>
    [Serializable]
    [DebuggerDisplay("{" + nameof(GetDebuggerDisplay) + "(),nq}")]
    public class Combination : ModelObject, ILoadCase, ICloneable, ISerializable
    {
        /// <summary>
        /// The load cases with their coefficients (sorted, see <see cref="LoadCaseCoefficient"/>)
        /// </summary>
        protected List<LoadCaseCoefficient> _coefficients;
        /// <summary>
        /// The options of the standard used to create the combination (can be null)
        /// </summary>
        protected Standards.Standard.CombinationsOptions _options;

        /// <summary>
        /// The number of load cases
        /// </summary>
        public int LoadCaseCount => _coefficients.Count;

        #region PUBLIC CONSTRUCTOR

        /// <summary>
        /// Creates an empty combination
        /// </summary>
        /// <param name="name">The name (not empty)</param>
        /// <param name="options">The options of the standard (can be null)</param>
        /// <param name="guid">The Guid</param>
        /// <exception cref="ArgumentException">If the name is null, empty or white space</exception>
        public Combination(string name, Standards.Standard.CombinationsOptions options, Guid guid)
            : base(guid, name)
        {
            if (string.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Combination name cannot be empty");

            _name = name;
            _coefficients = new List<LoadCaseCoefficient>();
            _options = options;
        }

        /// <summary>
        /// Creates an empty combination without options
        /// </summary>
        /// <param name="name">The name (not empty)</param>
        /// <param name="guid">The Guid</param>
        /// <exception cref="ArgumentException">If the name is null, empty or white space</exception>
        public Combination(string name, Guid guid)
            : this(name, null, guid)
        {

        }

        /// <summary>
        /// Creates an empty combination with a new Guid
        /// </summary>
        /// <param name="name">The name (not empty)</param>
        /// <param name="options">The options of the standard (can be null)</param>
        /// <exception cref="ArgumentException">If the name is null, empty or white space</exception>
        public Combination(string name, Standards.Standard.CombinationsOptions options)
            : this(name, options, Guid.NewGuid())
        {
        }

        /// <summary>
        /// Creates an empty combination without options, with a new Guid
        /// </summary>
        /// <param name="name">The name (not empty)</param>
        /// <exception cref="ArgumentException">If the name is null, empty or white space</exception>
        public Combination(string name)
            : this(name, Guid.NewGuid())
        {
        }

        /// <summary>
        /// Creates a copy of a combination: same name, options and Guid, a new list with the same coefficients (the load cases are shared)
        /// </summary>
        /// <param name="combination">The combination to copy</param>
        public Combination(Combination combination)
            : this(combination._name, combination._options, combination.Guid)
        {
            _coefficients = combination._coefficients.ToList(); //Shallow copy, i puntatori dei loadcase non cambiano
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="ModelObject"/>, the coefficients and the options
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected Combination(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _coefficients = (List<LoadCaseCoefficient>)info.GetValue("Coefficients", typeof(List<LoadCaseCoefficient>));
            _options = (Standards.Standard.CombinationsOptions)info.GetValue("Options", typeof(Standards.Standard.CombinationsOptions));
        }

        #endregion

        #region Abstract methods
        

        /// <summary>
        /// Duplicate the object, overriding the name with a new one (the Guid is the same)
        /// </summary>
        /// <param name="nameOverride">Name overriding</param>
        /// <returns>The copy with the new name</returns>
        public Combination Duplicate(string nameOverride)
        {
            var c = new Combination(this);
            c._name = nameOverride;

            return c;
        }

        /// <summary>
        /// Create a new empty <see cref="Combination"/> object. I.e. with the same properties (name, options, Guid) except the <see cref="LoadCaseCoefficient"/> List that will be empty
        /// </summary>
        /// <returns>The empty copy</returns>
        public object CloneEmpty()
        {
            return new Combination(this._name, this._options, this._guid) ;
        }
        

        /// <summary>
        /// Creates a copy of the combination (see <see cref="Combination(Combination)"/>)
        /// </summary>
        /// <returns>The copy</returns>
        public object Clone()
        {
            return new Combination(this);
        }

        #endregion Abstract methods

        #region PUBLIC METHODS

        #region Adder

        /// <summary>
        /// Adds load cases with their coefficients (see <see cref="this[LoadCaseBase]"/>: the coefficient of a load case already present is added
        /// to the existing one)
        /// </summary>
        /// <param name="loadCases">The load cases</param>
        /// <param name="coefficients">The coefficients, in the same order</param>
        /// <remarks>The <paramref name="loadCases"/> will be added only if the coefficient is not zero</remarks>
        /// <exception cref="ArgumentException"> If <paramref name="loadCases"/> Count != <paramref name="coefficients"/> Count</exception>
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

        /// <summary>
        /// Adds load cases with their coefficients (see <see cref="this[LoadCaseBase]"/>)
        /// </summary>
        /// <param name="loadCaseCoefficients">The pairs load case - coefficient</param>
        /// <exception cref="ArgumentNullException"> If a <see cref="LoadCaseBase"/> is null </exception>
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
        /// Adds a load case with its coefficient (see <see cref="this[LoadCaseBase]"/>: if the load case is already present, the coefficient is added)
        /// </summary>
        /// <param name="loadcase">The load case</param>
        /// <param name="coefficient">The coefficient</param>
        /// <remarks>The <paramref name="loadcase"/> will be added only if the coefficient is not zero</remarks>
        public virtual void AddLoadCaseCoefficient(LoadCaseBase loadcase, double coefficient)
        {
            this[loadcase] = coefficient;
        }

        #endregion Adder

        #region Indexer

        /// <summary>
        /// The coefficient of a load case. The setter ADDS the value to the coefficient of the load case (if it is already present), then
        /// sorts the coefficients; a zero value is ignored
        /// </summary>
        /// <param name="loadcase">The load case</param>
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

        /// <summary>
        /// The load cases of the combination
        /// </summary>
        /// <returns>A new list with the load cases, in the order of the coefficients</returns>
        public virtual List<LoadCaseBase> GetLoadCases()
        {
            return _coefficients.Select(i => i.LoadCase).ToList();
        }

        /// <summary>
        /// The coefficient of a load case (see <see cref="this[LoadCaseBase]"/>)
        /// </summary>
        /// <param name="loadcase">The load case</param>
        /// <returns>The coefficient associated to the <paramref name="loadcase"/>
        /// <para>If the <paramref name="loadcase"/> is not found, then return 0</para>
        /// </returns>
        public virtual double GetLoadCaseCoefficient(LoadCaseBase loadcase)
        {
            return this[loadcase];
        }

        /// <summary>
        /// The coefficients and the load cases of the combination
        /// </summary>
        /// <param name="loadCases">The load cases, in the order of the coefficients</param>
        /// <returns>The coefficients</returns>
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

        /// <summary>
        /// The pairs load case - coefficient
        /// </summary>
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

        /// <summary>
        /// The tuples load case - coefficient
        /// </summary>
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

        /// <summary>
        /// The tuples load case - coefficient of some load cases
        /// </summary>
        /// <param name="loadCases">The load cases to return</param>
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
        /// Tell if the combination contains a load case
        /// </summary>
        /// <param name="loadCase">The load case</param>
        /// <returns><see langword="True"/> if <paramref name="loadCase"/> is contained in this combination</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadCase"/> is null</exception>
        public virtual bool ContainsLoadCase(LoadCaseBase loadCase)
        {
            if (loadCase is null)
                throw new ArgumentNullException();

            return _coefficients.Select(i => i.LoadCase).Contains(loadCase);
        }

        /// <summary>
        /// Tell if the combination contains some load cases
        /// </summary>
        /// <param name="loadCases">The load cases</param>
        /// <returns><see langword="True"/> if all the elements of <paramref name="loadCases"/> are contained in this combination (true for an empty list)</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadCases"/> is null</exception>
        public virtual bool ContainsLoadCases(IEnumerable<LoadCaseBase> loadCases)
        {
            if (loadCases is null)
                throw new ArgumentNullException();

            if (loadCases.Count() > 0)
                return _coefficients.Select(i => i.LoadCase).Intersect(loadCases).Count().Equals(loadCases.Count());
            else
                return true;
        }

        /// <summary>
        /// Tell if the combination contains some pairs load case - coefficient (exact coefficients)
        /// </summary>
        /// <param name="loadCasesCoefficients">The pairs</param>
        /// <returns><see langword="True"/> if all KeyValuePairs are contained in this combination (true for an empty list)</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadCasesCoefficients"/> is null</exception>
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

        /// <summary>
        /// Tell if the combination contains some tuples load case - coefficient (exact coefficients)
        /// </summary>
        /// <param name="loadCasesCoefficients">The tuples</param>
        /// <returns><see langword="True"/> if all Tuple are contained in this combination (true for an empty list)</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="loadCasesCoefficients"/> is null</exception>
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

        /// <summary>
        /// Removes load cases from the combination (all their coefficients: the coefficients of the tuples are not used)
        /// </summary>
        /// <param name="loadCaseCoefficients">The tuples with the load cases to remove</param>
        /// <exception cref="ArgumentNullException"> If a <see cref="LoadCaseBase"/> is null </exception>
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

        /// <summary>
        /// Serializes the data of <see cref="ModelObject"/>, the coefficients and the options
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Coefficients", _coefficients);
            info.AddValue("Options", _options);
        }

        /// <summary>
        /// The combination as text: "coefficient*load case" joined by " + "
        /// </summary>
        /// <returns>The text of the coefficients</returns>
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

        /// <summary>
        /// The text shown by the debugger
        /// </summary>
        /// <returns>The name and the coefficients</returns>
        private string GetDebuggerDisplay()
        {
            return $"{Name}: {ToString()}";
        }


        /// <summary>
        /// Equality of name, options and coefficients (in any order)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal combination</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            if (obj is Combination objCasted)
            {
                bool equalsOption;
                if (_options is null)
                    equalsOption = objCasted._options == null;
                else
                    equalsOption = _options.Equals(objCasted._options);

                return !(objCasted is null) && _coefficients.ScrambledEquals(objCasted._coefficients) && equalsOption && base.Equals(objCasted);
            }
            return false;
        }

        /// <summary>
        /// The hash code of name, coefficients (independent of their order) and options
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = -23;
                hashCode += base.GetHashCode();

                foreach (var element in _coefficients)
                {
                    hashCode += -17 * EqualityComparer<LoadCaseCoefficient>.Default.GetHashCode(element);
                }

                if (_options != null)
                    hashCode += -17 * _options.GetHashCode();

                return hashCode; 
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null combinations are equal
        /// </summary>
        /// <param name="obj1">The first combination</param>
        /// <param name="obj2">The second combination</param>
        /// <returns>True if the combinations are equal</returns>
        public static bool operator ==(Combination obj1, Combination obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first combination</param>
        /// <param name="obj2">The second combination</param>
        /// <returns>True if the combinations are different</returns>
        public static bool operator !=(Combination obj1, Combination obj2)
        {
            return !(obj1 == obj2);
        }

        
        #endregion Equals - HashCode - Operators - Serialization - ToString

        #region Nested class

        /// <summary>
        /// A load case with its coefficient in a combination. The coefficients are sorted: self weight first, then superimposed dead loads, then
        /// the others by decreasing coefficient (see the remarks of the comparison)
        /// </summary>
        [Serializable]
        public sealed class LoadCaseCoefficient : IComparable<LoadCaseCoefficient>, IEquatable<LoadCaseCoefficient>, ISerializable
        {
            /// <summary>
            /// The load case
            /// </summary>
            private readonly LoadCaseBase _loadcase;
            /// <summary>
            /// The coefficient
            /// </summary>
            private readonly double _coefficient;

            /// <summary>
            /// The load case
            /// </summary>
            public LoadCaseBase LoadCase => _loadcase;
            /// <summary>
            /// The coefficient
            /// </summary>
            public double Coefficient => _coefficient;

            /// <summary>
            /// Creates a pair load case - coefficient
            /// </summary>
            /// <param name="coefficient">The coefficient</param>
            /// <param name="loadCase">The load case</param>
            public LoadCaseCoefficient(double coefficient, LoadCaseBase loadCase)
            {
                _loadcase = loadCase;
                _coefficient = coefficient;
            }


            /// <summary>
            /// Deserialization constructor: reads the load case and the coefficient
            /// </summary>
            /// <param name="info">The serialization data</param>
            /// <param name="context">The serialization context</param>
            public LoadCaseCoefficient(SerializationInfo info, StreamingContext context)
            {
                _loadcase = (LoadCaseBase)info.GetValue("Loadcase", typeof(LoadCaseBase));
                _coefficient = (double)info.GetValue("Coefficient", typeof(double));
            }


            /// <summary>
            /// Serializes the load case and the coefficient
            /// </summary>
            /// <param name="info">The serialization data</param>
            /// <param name="context">The serialization context</param>
            public void GetObjectData(SerializationInfo info, StreamingContext context)
            {
                info.AddValue("Loadcase", _loadcase);
                info.AddValue("Coefficient", _coefficient);
            }


            /// <summary>
            /// The pair as text: "coefficient*name of the load case"
            /// </summary>
            /// <returns>The text</returns>
            public override string ToString() => $"{String.Format("{0:0.0##}", Coefficient)}*{LoadCase.Name}";

            /// <summary>
            /// The order of the coefficients: self weight, superimposed dead loads, then decreasing coefficient
            /// </summary>
            /// <param name="other">The other pair</param>
            /// <returns>-1 if this pair comes first, 1 if it comes after, 0 if they have the same position</returns>
            /// <remarks>Most of the conditions compare the type of this load case with itself instead of the type of <paramref name="other"/>: a self
            /// weight is "equal" to every load case, so the order is not consistent (see the list of the defects found)</remarks>
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

            /// <summary>
            /// Equality with another object (see <see cref="Equals(LoadCaseCoefficient)"/>)
            /// </summary>
            /// <param name="obj">The object to compare</param>
            /// <returns>True if <paramref name="obj"/> is an equal pair</returns>
            public override bool Equals(object obj)
            {
                return Equals(obj as LoadCaseCoefficient);
            }

            /// <summary>
            /// Equality of the load case and of the exact coefficient
            /// </summary>
            /// <param name="other">The pair to compare</param>
            /// <returns>True if the pairs are equal</returns>
            public bool Equals(LoadCaseCoefficient other)
            {
                return other != null && _loadcase.Equals(other._loadcase) && _coefficient.Equals(other._coefficient);
            }

            /// <summary>
            /// The hash code of the load case and of the coefficient
            /// </summary>
            /// <returns>The hash code</returns>
            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = -23;
                    hashCode = hashCode * -17 + _loadcase.GetHashCode();
                    hashCode = hashCode * -17 + _coefficient.GetHashCode();
                    return hashCode; 
                }
            }


            /// <summary>
            /// Equality operator (see <see cref="Equals(LoadCaseCoefficient)"/>); two null pairs are equal
            /// </summary>
            /// <param name="obj1">The first pair</param>
            /// <param name="obj2">The second pair</param>
            /// <returns>True if the pairs are equal</returns>
            public static bool operator ==(LoadCaseCoefficient obj1, LoadCaseCoefficient obj2)
            {
                if (obj1 is null)
                {
                    return obj2 is null;
                }

                if (ReferenceEquals(obj1, obj2))
                    return true;

                return obj1.Equals(obj2);
            }

            /// <summary>
            /// Inequality operator (see <see cref="Equals(LoadCaseCoefficient)"/>)
            /// </summary>
            /// <param name="obj1">The first pair</param>
            /// <param name="obj2">The second pair</param>
            /// <returns>True if the pairs are different</returns>
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
            /// <summary>
            /// Equality of the coefficients, in any order (name and options are not compared)
            /// </summary>
            /// <param name="x">The first combination</param>
            /// <param name="y">The second combination</param>
            /// <returns>True if the combinations have the same coefficients, or if both <paramref name="x"/> and <paramref name="y"/> are null</returns>
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

            /// <summary>
            /// The hash code of the coefficients, independent of their order
            /// </summary>
            /// <param name="obj">The combination</param>
            /// <returns>The hash code</returns>
            /// <remarks> Only <see cref="Combination._coefficients"/> are used as equality parameters </remarks>
            int IEqualityComparer<Combination>.GetHashCode(Combination obj)
            {
                var hashCode = -23;
                foreach (var element in obj._coefficients)
                {
                    hashCode += EqualityComparer<LoadCaseCoefficient>.Default.GetHashCode(element);
                }
                return hashCode;
            }
        }

        #endregion
    }
}