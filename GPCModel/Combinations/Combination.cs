using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GPC.Model.Combinations
{
    [Serializable]
    public abstract class Combination : ModelObject, ILoadCase
    {
        protected List<LoadCaseCoefficient> _coefficients;

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

        protected Combination(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _coefficients = (List<LoadCaseCoefficient>)info.GetValue("Coefficients", typeof(List<LoadCaseCoefficient>));
        }

        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Coefficients", _coefficients);
        }

        public abstract bool IsUltimate();

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
        /// 
        /// </summary>
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
            List<double> coefficients = new List<double>() ;
            loadCases = new List<LoadCase>();

            foreach (var coeff in _coefficients)
            {
                loadCases.Add(coeff.LoadCase);
                coefficients.Add(coeff.Coefficient);
            }

            return coefficients;
        }



        #endregion

        #region INDEXER

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

        #endregion

        #region Nested class

        protected class LoadCaseCoefficient : IComparable<LoadCaseCoefficient>
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

        }

        #endregion
    }
}