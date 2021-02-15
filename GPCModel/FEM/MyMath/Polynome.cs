using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FEM
{
    public class Polynome {
        IEnumerable<Polynome1D> _polynomes;

        public IEnumerable<double> Coefficients
        {
            get
            {
                List<double> coefficients = new List<double>();
                for (int i = 0; i < _polynomes.Count(); i++)
                {
                    int start = 0;
                    if (i > 0)
                    {
                        start = 1; //not consider the constant value of i-th polinome with i >0s
                    }
                    for (int j = start; j < _polynomes.ElementAt(i).Coefficient.Count(); j++) { 
                        coefficients.Add(_polynomes.ElementAt(i).Coefficient.ElementAt(j));
                    }
                }
                return coefficients;
            }

            set
            {
                int index = 0;
                for (int i = 0; i < _polynomes.Count(); i++)
                {
                    List<double> coeff = new List<double>();
                    for (int j = 0; j < _polynomes.ElementAt(i).Coefficient.Count(); j++)
                    {
                        if (i > 0 && j == 0)
                        {
                            coeff.Add(0); //constant coefficient for i-th polinome with i >0
                        }
                        else
                        {
                            coeff.Add(value.ElementAt(index));
                            index++;
                        }
                    }
                    _polynomes.ElementAt(i).Coefficient = coeff;
                }
            }
        }

        public Polynome(IEnumerable<Polynome1D> polynomes)
        {
            _polynomes = polynomes;
        }

        public int Dimension()
        {
            HashSet<string> variables = new HashSet<string>();
            for (int i = 0; i < _polynomes.Count(); i++)
            {
                variables.Add(_polynomes.ElementAt(i).Variable);
            }
            return variables.Count();
        }

        public double Evaluate(Dictionary<string, double> input)
        {
            double result = 0;
            for (int i = 0; i < _polynomes.Count(); i++)
            {
                double inputVal = input[_polynomes.ElementAt(i).Variable];
                result = result + _polynomes.ElementAt(i).Evaluate(inputVal);
            }
            return result;
        }

        public double EvaluateDifferential(string variable, double input)
        {
            double result = 0;
            for (int i = 0; i < _polynomes.Count(); i++)
            {
                if (_polynomes.ElementAt(i).Variable == variable)
                {
                    result = result + _polynomes.ElementAt(i).EvaluateDifferential(input);
                }
            }
            return result;
        }

        public override string ToString()
        {
            string s = "";
            foreach (Polynome1D p in _polynomes)
            {
                s = s + p.ToString();
            }
            return s;
        }
    }
    public class Polynome1D
    {
        IEnumerable<double> _coefficients;

        public string Variable { get; set; }

        public IEnumerable<double> Coefficient
        {
            get => _coefficients;
            set
            {
                _coefficients = value;
            }
        }

        public Polynome1D(IEnumerable<double> coefficient, string variable)
        {
            _coefficients = new List<double>(coefficient.Count());
            _coefficients = coefficient;
            Variable = variable;
        }

        public Polynome1D(int order, string variable)
        {
            _coefficients = new List<double>(order+1);
            List<double> _coeff = new List<double>(order + 1);
            for (int i = 0; i <= order; i++) {
                _coeff.Add(0);
            }
            _coefficients = _coeff;
            Variable = variable;
        }
        
        public double Evaluate(double input)
        {
            double result = 0;
            int order = 0;
            for (int i = 0; i < _coefficients.Count(); i++)
            {
                result = result + _coefficients.ElementAt(i) * Math.Pow(input, order);
                order++;
            }
            return result;
        }

        public double EvaluateDifferential(double input)
        {
            double result = 0;
            int order = 0;
            for (int i = 0; i < _coefficients.Count(); i++)
            {
                if (order == 0)
                {
                    result = result + 0;
                } else
                {
                    result = result + _coefficients.ElementAt(i) * order * Math.Pow(input, order-1);
                }
                order++;
            }
            return result;
        }

        public override string ToString()
        {
            string s = "";
            int order = 0;
            for (int i = 0; i < _coefficients.Count(); i++)
            {
                if (order == 0)
                {
                    if (_coefficients.ElementAt(0) < 0)
                    {
                        s = s + _coefficients.ElementAt(i) + " ";
                    } else
                    {
                        s = s + "+" + _coefficients.ElementAt(i) + " ";
                    }
                }
                else
                {
                    if (_coefficients.ElementAt(i) < 0)
                    {
                        s = s + _coefficients.ElementAt(i) + " " + Variable + "^" + order + " ";
                    } else
                    {
                        s = s + "+" + _coefficients.ElementAt(i) + " " + Variable + "^" + order + " ";
                    }

                }
                order++;
            }
            return s;
        }
    }
}
