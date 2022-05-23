using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.LoadCases;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class ResultStress : ResultType, IEquatable<ResultStress>, ISerializable, IBrickResult, IResult<ResultStress>
    {
        #region Variables

        /// <summary>
        /// Local Stresses
        /// </summary>
        private readonly double _sxx;
        private readonly double _syy;
        private readonly double _szz;
        private readonly double _sxy;
        private readonly double _sxz;
        private readonly double _syz;

        /// <summary>
        /// Principal Stresses
        /// </summary>

        // questa variabile serve per sapere se gli stress principali sono stati calcolati, in modo da evitare di calcolari due volte. 
        // Confrotando i valori non è giusto perchè potrebbero essere zero. Lo svantaggio è che non so se sono stati calcolati con il metodo preciso o approssimato.
        private bool _principalStressCalculated;

        private double _s11;
        private double _s22;
        private double _s33;

        private bool _vonMisesStressCalculated;
        private double _vM;

        #endregion
        
        #region Properties

        public double Sxx => _sxx;
        public double Syy => _syy;
        public double Szz => _szz;
        public double Sxy => _sxy;
        public double Sxz => _sxz;
        public double Syz => _syz;

        public double S11
        {
            get
            {
                if (!_principalStressCalculated)
                    CalculatePrincipalStressFullMethod();

                return _s11;
            }
        }

        public double S22
        {
            get
            {
                if (!_principalStressCalculated)
                    CalculatePrincipalStressFullMethod();

                return _s22;
            }
        }

        public double S33
        {
            get
            {
                if (!_principalStressCalculated)
                    CalculatePrincipalStressFullMethod();

                return _s33;
            }
        }

        public double SVM
        {
            get
            {
                if (!_vonMisesStressCalculated)
                    _vM = GetVMStress();

                return _vM;
            }
        }

        #endregion

        #region Public Constructors

        /// <param name="coordinateSystem"></param>
        /// <param name="sxx">Stress on <see cref="CoordinateSystem.V1"/> side of the plate along <see cref="CoordinateSystem.V1"/> direction</param>
        /// <param name="syy">Stress on <see cref="CoordinateSystem.V2"/> side of the plate along <see cref="CoordinateSystem.V2"/> direction</param>
        /// <param name="szz">Stress on <see cref="CoordinateSystem.V2"/> side of the plate along <see cref="CoordinateSystem.V3"/> direction</param>
        /// <param name="sxy">Stress on <see cref="CoordinateSystem.V1"/> side of the plate along <see cref="CoordinateSystem.V2"/> direction</param>
        /// <param name="sxz">Stress on <see cref="CoordinateSystem.V1"/> side of the plate along <see cref="CoordinateSystem.V3"/> direction</param>
        /// <param name="syz">Stress on <see cref="CoordinateSystem.V2"/> side of the plate along <see cref="CoordinateSystem.V3"/> direction</param>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <remarks> _szz is set to zero by default </remarks>
        public ResultStress(CoordinateSystem coordinateSystem, double sxx, double syy, double szz, double sxy, double sxz, double syz, string name = "", int id = ModelObjectId.IDUNASSIGNED)
            : base(coordinateSystem, name, id)
        {
            _sxx = sxx;
            _syy = syy;
            _szz = szz;
            _sxy = sxy;
            _sxz = sxz;
            _syz = syz;
        }

        private ResultStress(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _sxx = (double)info.GetValue("Sxx", typeof(double));
            _syy = (double)info.GetValue("Syy", typeof(double));
            _szz = (double)info.GetValue("Szz", typeof(double));
            _sxy = (double)info.GetValue("Sxy", typeof(double));
            _sxz = (double)info.GetValue("Sxz", typeof(double));
            _syz = (double)info.GetValue("Syz", typeof(double));
        }

		#endregion

		#region Public Methods  

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Sxx", _sxx);
            info.AddValue("Syy", _syy);
            info.AddValue("Szz", _szz);
            info.AddValue("Sxy", _sxy);
            info.AddValue("Sxz", _sxz);
            info.AddValue("Syz", _syz);
        }

		#endregion

		#region Public method - Stresses

		/// <summary>
		/// Calculate the Principal stresses
		/// <para>This method use an approximate solution.</para>
		/// <para>If <see cref="Sxx"/>, <see cref="Sxx"/>, <see cref="Sxz"/> and <see cref="Syz"/> are relavant then the 
		/// <seealso cref="CalculatePrincipalStressFullMethod"/> must be used</para>
		/// <para>If <see cref="Sxz"/> and <see cref="Syz"/> are 0. This method gives the exact solution</para>
		/// </summary>
		internal void CalculatePrincipalStressSimplifiedMethod()
        {
            _s11 = ((_sxx + _syy) / 2.0) + Math.Sqrt((Math.Pow((_sxx - _syy), 2.0) / 4.0) + Math.Pow(_sxy, 2.0));
            _s22 = ((_sxx + _syy) / 2.0) - Math.Sqrt((Math.Pow((_sxx - _syy), 2.0) / 4.0) + Math.Pow(_sxy, 2.0));
            _s33 = 0;

            _principalStressCalculated = true;
        }

        /// <summary>
        /// Calculated the principal stress by means of an enginevalue evaluation
        /// </summary>
        public void CalculatePrincipalStressFullMethod()
        {

            if (_sxz == 0 && _syz == 0 && _szz == 0)
            {
                CalculatePrincipalStressSimplifiedMethod();
                _s33 = 0;
            }
            else
            {
                Matrix<double> m = CreateMatrix.Dense<double>(3, 3);
                m[0, 0] = _sxx;
                m[1, 1] = _syy;
                m[2, 2] = _szz;

                m[0, 1] = _sxy;
                m[1, 0] = _sxy;

                m[0, 2] = _sxz;
                m[2, 0] = _sxz;

                m[1, 2] = _syz;
                m[2, 1] = _syz;

                MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> eigen = m.Evd();

                _s11 = eigen.EigenValues[2].Real;
                _s22 = eigen.EigenValues[1].Real;
                _s33 = eigen.EigenValues[0].Real;
            }

            _principalStressCalculated = true;
        }

        /// <summary>
        /// Return the VonMises Stress
        /// </summary>
        private double GetVMStress()
        {
            double svm;

            if (S33 == 0)
                svm = Math.Sqrt(Math.Pow(S11, 2.0) + Math.Pow(S22, 2.0) - (S22 * S11));
            else
                svm = Math.Sqrt(0.5 * (Math.Pow(S11 - S22, 2.0) + Math.Pow(S22 - S33, 2.0) + Math.Pow(S33 - S11, 2.0)));

            _vonMisesStressCalculated = true;
            return svm;
        }

        /// <summary>
        /// Return the stress of the point in global coordinate
        /// </summary>
        /// <returns>Array of stress</returns>
        public double[] GetGlobalStress()
        {
            Vector3d SigmaResult = new Vector3d(_sxx, _syy, 0);
            Vector3d GlobalSigmaResult = _coordinateSystem.ToGlobal(SigmaResult);

            Vector3d TauResult = new Vector3d(0, 0, _sxy);
            Vector3d GlobalTauResult = _coordinateSystem.ToGlobal(TauResult);

            double[] globalstress = new double[6];

            globalstress[0] = GlobalSigmaResult.X;
            globalstress[1] = GlobalSigmaResult.Y;
            globalstress[2] = GlobalSigmaResult.Z;
            globalstress[3] = GlobalTauResult.X;
            globalstress[4] = GlobalTauResult.Y;
            globalstress[5] = GlobalTauResult.Z;

            return globalstress;
        }

        /// <returns>Return the stress tensor</returns>
        public Matrix<double> GetTensor(bool toGlobal = false)
        {
            if (toGlobal)
            {
                Matrix<double> stress = Matrix<double>.Build.Sparse(3, 3);
                stress[0, 0] = _sxx;
                stress[0, 1] = _sxy;
                stress[0, 2] = _sxz;
                stress[1, 0] = _sxy;
                stress[1, 1] = _syy;
                stress[1, 2] = _syz;
                stress[2, 0] = _sxz;
                stress[2, 1] = _syz;
                stress[2, 2] = _szz;

                return _coordinateSystem.TrfMatrix.Resize(3, 3) * stress * _coordinateSystem.TrfMatrix.Resize(3, 3).Transpose();
            }
            else
            {
                Matrix<double> stress = Matrix<double>.Build.Sparse(3, 3);
                stress[0, 0] = _sxx;
                stress[0, 1] = _sxy;
                stress[0, 2] = _sxz;
                stress[1, 0] = _sxy;
                stress[1, 1] = _syy;
                stress[1, 2] = _syz;
                stress[2, 0] = _sxz;
                stress[2, 1] = _syz;
                stress[2, 2] = _szz;

                return stress;
            }
        }

        public ResultStress ToCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            var globalTensor = GetTensor(true);

            var rotatedTensor = coordinateSystem.TrfMatrix.Resize(3, 3).Transpose() * (globalTensor) * coordinateSystem.TrfMatrix.Resize(3, 3);

            return new ResultStress(coordinateSystem, rotatedTensor[0, 0], rotatedTensor[1, 1], rotatedTensor[2, 2], rotatedTensor[0, 1], rotatedTensor[0, 2], rotatedTensor[1, 2]);
        }

        #endregion
                
        #region Equals, hashcode, operators

        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as ResultStress);
        }

        public bool Equals(ResultStress other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && _sxx == other._sxx && _syy == other._syy
                                    && _szz == other._szz && _sxy == other._sxy
                                    && _sxz == other._sxz && _syz == other._syz
                                    && base.Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _sxx.GetHashCode();
                hashCode = hashCode * -17 + _syy.GetHashCode();
                hashCode = hashCode * -17 + _szz.GetHashCode();
                hashCode = hashCode * -17 + _sxy.GetHashCode();
                hashCode = hashCode * -17 + _sxz.GetHashCode();
                hashCode = hashCode * -17 + _syz.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(ResultStress obj1, ResultStress obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        // statico perchè è come se fosse un operatore
        /// <summary>
        /// Returns a <see cref="ResultStress"/> that represent the arithmetic mean between the <paramref name="values"/>
        /// </summary>
        public static ResultStress GetArithmeticMean(ResultStress[] values)
        {

            if (values.Select(i => i._coordinateSystem).Distinct().Count() > 0)
            {
                return new ResultStress(values[0]._coordinateSystem,
                                        Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.Sxx).ToArray()), // TODO: rimuovere toarray e metter ienumer
                                        Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.Syy).ToArray()),
                                        Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.Szz).ToArray()),
                                        Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.Sxy).ToArray()),
                                        Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.Sxz).ToArray()),
                                        Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.Syz).ToArray()),
                                        string.Join(" ", values.Select(i => i.Name).ToHashSet().ToArray())
                                        );
            }
            else
            {
                var rotated = new List<ResultStress>
                {
                    [0] = values[0]
                };

                rotated.AddRange(values.Skip(1).Select(i => i.ToCoordinateSystem(values[0]._coordinateSystem)));

                return new ResultStress(values[0]._coordinateSystem,
                                                Utilities.Maths.Averages.ArithmeticMean(rotated.Select(i => i.Sxx).ToArray()), // TODO: rimuovere toarray e metter ienumer
                                                Utilities.Maths.Averages.ArithmeticMean(rotated.Select(i => i.Syy).ToArray()),
                                                Utilities.Maths.Averages.ArithmeticMean(rotated.Select(i => i.Szz).ToArray()),
                                                Utilities.Maths.Averages.ArithmeticMean(rotated.Select(i => i.Sxy).ToArray()),
                                                Utilities.Maths.Averages.ArithmeticMean(rotated.Select(i => i.Sxz).ToArray()),
                                                Utilities.Maths.Averages.ArithmeticMean(rotated.Select(i => i.Syz).ToArray()),
                                                string.Join(" ", values.Select(i => i.Name).ToHashSet().ToArray())
                                        );
            }
        }


        public static bool operator !=(ResultStress obj1, ResultStress obj2)
        {
            return !(obj1 == obj2);
        }

        /// <returns>The sum of the two stress tensor written in the <paramref name="obj1"/> <see cref="CoordinateSystem"/></returns>
        public static ResultStress operator +(ResultStress obj1, ResultStress obj2)
        {
            if (obj1 is null || obj2 is null)
                throw new ArgumentNullException();

            if (obj1._coordinateSystem.Equals(obj2._coordinateSystem))
            {
                return new ResultStress(obj1._coordinateSystem, obj1._sxx + obj2._sxx,
                                                                obj1._syy + obj2._syy,
                                                                obj1._szz + obj2._szz,
                                                                obj1._sxy + obj2._sxy,
                                                                obj1._sxz + obj2._sxz,
                                                                obj1._syz + obj2._syz,
                                                                string.Join(" ", new string[] { obj1.Name, obj2.Name }.ToHashSet())
                                                                );
            }
            else
            {
                // prendo tensori rotati nel globale
                // li sommo
                // li ruoto nel sistema obj1

                var sumRotated = obj1._coordinateSystem.TrfMatrix.Resize(3, 3).Transpose() * (obj1.GetTensor(true) + obj2.GetTensor(true)) * obj1._coordinateSystem.TrfMatrix.Resize(3, 3);

                return new ResultStress(obj1._coordinateSystem,
                                        sumRotated[0, 0],
                                        sumRotated[1, 1],
                                        sumRotated[2, 2],
                                        sumRotated[0, 1],
                                        sumRotated[0, 2],
                                        sumRotated[1, 2],
                                        string.Join(" ", new string[] { obj1.Name, obj2.Name }.ToHashSet())
                                        );
            }
        }

        public static ResultStress operator -(ResultStress obj1, ResultStress obj2)
        {
            if (obj1 is null || obj2 is null)
                throw new ArgumentNullException();

            if (obj1._coordinateSystem.Equals(obj2._coordinateSystem))
            {
                return new ResultStress(obj1._coordinateSystem, obj1._sxx - obj2._sxx,
                                                                obj1._syy - obj2._syy,
                                                                obj1._szz - obj2._szz,
                                                                obj1._sxy - obj2._sxy,
                                                                obj1._sxz - obj2._sxz,
                                                                obj1._syz - obj2._syz,
                                                                string.Join(" ", new string[] { obj1.Name, obj2.Name }.ToHashSet())
                                                                );
            }
            else
            {
                // prendo tensori rotati nel globale
                // li sommo
                // li ruoto nel sistema obj1

                var sumRotated = obj1._coordinateSystem.TrfMatrix.Resize(3, 3).Transpose() * (obj1.GetTensor(true) - obj2.GetTensor(true)) * obj1._coordinateSystem.TrfMatrix.Resize(3, 3);

                return new ResultStress(obj1._coordinateSystem,
                                        sumRotated[0, 0],
                                        sumRotated[1, 1],
                                        sumRotated[2, 2],
                                        sumRotated[0, 1],
                                        sumRotated[0, 2],
                                        sumRotated[1, 2],
                                        string.Join(" ", new string[] { obj1.Name, obj2.Name }.ToHashSet())
                                        );
            }
        }


        /// <returns>This will produce the multipltication of <paramref name="obj1"/> Tensor in global coordinate by <paramref name="matrix"/>. M * T * M^t</returns>
        public static ResultStress operator *(ResultStress obj1, Matrix<double> matrix)
        {
            if (obj1 is null || matrix is null)
                throw new ArgumentNullException();

            if (matrix.Rank() != 3)
            {
                throw new NotSupportedException();
            }

            var rotated = matrix * obj1.GetTensor(true) * matrix.Transpose();

            return new ResultStress(obj1._coordinateSystem, rotated[0, 0], rotated[1, 1], rotated[2, 2], rotated[0, 1], rotated[0, 2], rotated[1, 2], obj1.Name);
        }

        #endregion
    }
}
