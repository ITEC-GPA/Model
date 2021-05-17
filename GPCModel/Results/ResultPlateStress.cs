using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.LoadCases;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class ResultPlateStress : Result, IEquatable<ResultPlateStress>, ISerializable
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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element">Element where these result are referred </param>
        /// <param name="Case">The case where these results are reffered </param>
        /// <param name="coordinateSystem">Coordinate system where these result are provided</param>
        /// <param name="resultPoint">Stress point where these results are provided</param>
        /// <param name="sxx">Stress on <see cref="CoordinateSystem.V1"/> side of the plate along <see cref="CoordinateSystem.V1"/> direction</param>
        /// <param name="syy">Stress on <see cref="CoordinateSystem.V2"/> side of the plate along <see cref="CoordinateSystem.V2"/> direction</param>
        /// <param name="sxy">Stress on <see cref="CoordinateSystem.V1"/> side of the plate along <see cref="CoordinateSystem.V2"/> direction</param>
        /// <param name="sxz">Stress on <see cref="CoordinateSystem.V1"/> side of the plate along <see cref="CoordinateSystem.V3"/> direction</param>
        /// <param name="syz">Stress on <see cref="CoordinateSystem.V2"/> side of the plate along <see cref="CoordinateSystem.V3"/> direction</param>
        /// <remarks> _szz is set to zero by default </remarks>
        public ResultPlateStress(Plate element, ILoadCase Case, ResultStressPoint resultPoint, CoordinateSystem coordinateSystem, double sxx, double syy, double sxy, double sxz, double syz) 
            : base(element, Case, resultPoint, coordinateSystem)
        {
            _sxx = sxx;
            _syy = syy;
            _szz = 0;
            _sxy = sxy;
            _sxz = sxz;
            _syz = syz;
        }


        #endregion


        #region Public method - Stresses

        /// <summary>
        /// Calculate the Principal stresses
        /// <para>This method use an approximate solution.</para>
        /// <para>If <see cref="ResultPlateStress.Sxx"/>, <see cref="ResultPlateStress.Sxx"/>, <see cref="ResultPlateStress.Sxz"/> and <see cref="ResultPlateStress.Syz"/> are relavant then the 
        /// <seealso cref="CalculatePrincipalStressFullMethod"/> must be used</para>
        /// <para>If <see cref="ResultPlateStress.Sxz"/> and <see cref="ResultPlateStress.Syz"/> are 0. This method gives the exact solution</para>
        /// </summary>
        public void CalculatePrincipalStressSimplifiedMethod()
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
                svm = Math.Sqrt(Math.Pow(S11, 2.0) + Math.Pow(S22, 2.0) - (S22 * S11) );
            else
                svm = Math.Sqrt(0.5*(Math.Pow(S11 - S22, 2.0) + Math.Pow(S22 - S33, 2.0) + Math.Pow(S33 - S11, 2.0)));

            _vonMisesStressCalculated = true;
            return svm;
        }

        /// <summary>
        /// Return the stress of the point in global coordinate
        /// </summary>
        /// <returns>Array of stress</returns>
        public double[] GetGlobalStress()
        {
            Vector3d SigmaResult = new Vector3d(_sxx, _syy, 0 );
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



        #endregion

        #region Public method - Get attributes


        public Plate GetPlate()
        {
            return (Plate)Element;
        }


        public override int GetElementId()
        {
            return Element.Id;
        }


        public override int GetResultPointId()
        {
            return ResultPoint.Id;
        }


        #endregion



        #region Equals, hashcode, operators
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as ResultPlateStress);
        }

        public bool Equals(ResultPlateStress other)
        {
            if (other is null)
                return false;

            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && _sxx == other._sxx && _syy == other._syy
                                    && _szz == other._szz && _sxy == other._sxy
                                    && _sxz == other._sxz && _syz == other._syz && base.Equals(other);
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotSupportedException();
        }

        public static bool operator ==(ResultPlateStress obj1, ResultPlateStress obj2)
        {
            if (obj1 is null || obj2 is null)
                return false;

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultPlateStress obj1, ResultPlateStress obj2)
        {
            return !(obj1 == obj2);
        }


        #endregion


    }
}
