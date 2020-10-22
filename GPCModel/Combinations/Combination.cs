using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace GPC.Model.Combinations
{
    public abstract class Combination : ModelObject
    {
        private string _name;
        private List<LoadCaseCoefficient> _coefficients;

        public string Name => _name;
        
        #region COMBINATIONS

        protected Combination(string name, Guid guid)
            : base(guid)
        {
            if (String.IsNullOrEmpty(name) || string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Combination name cannot be empty");

            this._name = name;
            _coefficients = new List<LoadCaseCoefficient>();
        }

        protected Combination(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _name = info.GetString("Name");
        }

        #endregion COMBINATIONS

        #region PUBLIC METHODS

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Name", _name);
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

        public void AddLoadCaseCoefficients(IEnumerable<LoadCase> loadCases, IEnumerable<double> coefficients)
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

        #endregion PUBLIC METHODS

        #region INDEXER
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
                else
                    throw new ArgumentException("Coefficient can't be zero");
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
                throw new KeyNotFoundException();
            }
        }

        #endregion INDEXER

        #region NESTED STRUCT

        protected struct LoadCaseCoefficient : IComparable<LoadCaseCoefficient>
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


            #region INTERFACE IMPLEMENTATION

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

            #endregion INTERFACE IMPLEMENTATION
        }

        #endregion NESTED STRUCT
    }
}