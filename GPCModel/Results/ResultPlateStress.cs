using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.LoadCases;
using GPC.Geometry;
using MathNet.Numerics.LinearAlgebra;
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
        private double _sxx;
        private double _syy;
        private double _szz;
        private double _sxy;
        private double _sxz;
        private double _syz;

        ///// <summary>
        ///// Global Stresses
        ///// </summary>
        //private double _sXX;
        //private double _sYY;
        //private double _sZZ;
        //private double _sXY;
        //private double _sXZ;
        //private double _sYZ;

        ///// <summary>
        ///// Principal Stresses
        ///// </summary>
        //private double _s11;
        //private double _s22;
        //private double _s33;

        ///// <summary>
        ///// Combined Stresses
        ///// </summary>
        //private double _sVM;
        //private double _sTR;

        #endregion


        #region Properties

        public double Sxx => _sxx;
        public double Syy => _syy;
        public double Szz => _szz;
        public double Sxy => _sxy;
        public double Sxz => _sxz;
        public double Syz => _syz;

        #endregion


        #region Public Constructors
          
        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementID">Id of the element where these result are referred to</param>
        /// <param name="elementLabel">Label of the element where these result are referred t</param>
        /// <param name="caseId">Represenet the id of the loadcase / loadCombination where these result are referred to</param>
        /// <param name="coordinateSystem">Coordinate system where these result are provided</param>
        /// <param name="sxx">Stress on <see cref="CoordinateSystem.V1"/> side of the plate along <see cref="CoordinateSystem.V1"/> direction</param>
        /// <param name="syy">Stress on <see cref="CoordinateSystem.V2"/> side of the plate along <see cref="CoordinateSystem.V2"/> direction</param>
        /// <param name="sxy">Stress on <see cref="CoordinateSystem.V1"/> side of the plate along <see cref="CoordinateSystem.V2"/> direction</param>
        /// <param name="sxz">Stress on <see cref="CoordinateSystem.V1"/> side of the plate along <see cref="CoordinateSystem.V3"/> direction</param>
        /// <param name="syz">Stress on <see cref="CoordinateSystem.V2"/> side of the plate along <see cref="CoordinateSystem.V3"/> direction</param>
        public ResultPlateStress(int elementID, string elementLabel, int resultPointId, int caseId, CoordinateSystem coordinateSystem, double sxx, double syy, double sxy, double sxz, double syz) 
            : base(elementID, elementLabel, caseId, resultPointId, coordinateSystem)
        {
            _sxx = sxx;
            _syy = syy;
            _szz = 0;
            _sxy = sxy;
            _sxz = sxz;
            _syz = syz;
        }


        #endregion


        #region Public Methods Specific


        /// <summary>
        /// Return the Principal stresses of the point.  
        /// <para>This method use an approximate solution.</para>
        /// <para>If <see cref="ResultPlateStress.Sxx"/>, <see cref="ResultPlateStress.Sxx"/>, <see cref="ResultPlateStress.Sxz"/> and <see cref="ResultPlateStress.Syz"/> are relavant then the 
        /// <seealso cref="ResultPlateStress.GetPrincipalStress(out double, out double, out double)"/> must be used</para>
        /// 
        /// <para>If <see cref="ResultPlateStress.Sxz"/> and <see cref="ResultPlateStress.Syz"/> are 0. This method gives the exact solution</para>
        /// </summary>
        /// <param name="S11">Principal Stress S11</param>
        /// <param name="S22">Principal Stress S22</param>
        public void GetPrincipalStress(out double S11, out double S22)
        {
            // double phi = 0.5 * Math.Atan( Math.Abs( (2*_sxy) / (_sxx + _syy )));         // The angle, Φ, is the angle in radians between the maximum normal stress and the local x-axis.
            S11 = ((_sxx + _syy) / 2.0) + Math.Sqrt((Math.Pow((_sxx - _syy), 2.0) / 4.0) + Math.Pow(_sxy, 2.0));
            S22 = ((_sxx + _syy) / 2.0) - Math.Sqrt((Math.Pow((_sxx - _syy), 2.0) / 4.0) + Math.Pow(_sxy, 2.0));
        }

        /// <summary>
        /// Return the Principal stresses of the point by means of an enginevalue evaluation
        /// </summary>
        /// <param name="S11">Principal Stress S11</param>
        /// <param name="S22">Principal Stress S22</param>
        /// <param name="S33">Principal Stress S33</param>
        public void GetPrincipalStress(out double S11, out double S22, out double S33)
        {
            // double phi = 0.5 * Math.Atan( Math.Abs( (2*_sxy) / (_sxx + _syy )));         // The angle, Φ, is the angle in radians between the maximum normal stress and the local x-axis.
            if (_sxz == 0 && _sxy == 0 && _szz == 0)
            {
                GetPrincipalStress(out S11, out S22);
                S33 = 0;
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

                S11 = eigen.EigenValues[2].Real;
                S22 = eigen.EigenValues[1].Real;
                S33 = eigen.EigenValues[0].Real;
            }
        }


        /// <summary>
        /// Return the VonMises Stress
        /// </summary>
        /// <param name="Svm"></param>
        public void GetVMStress(out double Svm)
        {
            GetPrincipalStress(out double _s11, out double _s22, out double _s33);
            
            if (_s33 == 0)
                Svm = Math.Sqrt(Math.Pow((_s11), 2.0) + (Math.Pow((_s22), 2.0) - (_s22 * _s11)));
            else
                Svm = Math.Sqrt(0.5*( Math.Pow((_s11 - _s22), 2.0) + Math.Pow((_s22 - _s33), 2.0) + Math.Pow((_s33 - _s11), 2.0)));

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



        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return base.Equals(obj as ResultPlateStress);
        }

        public bool Equals(ResultPlateStress other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && _sxx == other._sxx 
                                    && _syy == other._syy
                                    && _szz == other._szz
                                    && _sxy == other._sxy
                                    && _sxz == other._sxz
                                    && _syz == other._syz && base.Equals(other);
        }

        public override int GetHashCode()
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

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            throw new NotSupportedException();
        }

        public static bool operator ==(ResultPlateStress obj1, ResultPlateStress obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultPlateStress obj1, ResultPlateStress obj2)
        {
            return !(obj1 == obj2);
        }




        #endregion
    }
}
