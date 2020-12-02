using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public class ResultPlateForces : Result, ISerializable, IEquatable<ResultPlateForces>
    {
        #region Variables

        /// Local Forces
        protected double _fxx;
        protected double _fyy;
        protected double _fzz;
        protected double _fxy;
        protected double _fxz;
        protected double _fyz;

        /// Local Moments
        protected double _mxx;
        protected double _myy;
        protected double _mzz;
        protected double _mxy;
        protected double _mxz;
        protected double _myz;

        /// Global Forces
        protected double _fXX;
        protected double _fYY;
        protected double _fZZ;
        protected double _fXY;
        protected double _fXZ;
        protected double _fYZ;

        /// Global Moments
        protected double _mXX;
        protected double _mYY;
        protected double _mZZ;
        protected double _mXY;
        protected double _mXZ;
        protected double _mYZ;

        /// Principal forces
        protected double _f11;
        protected double _f22;

        /// Principal moments
        protected double _m11;
        protected double _m22;

        /// Combined forces
        protected double _fVM;
        protected double _fTR;

        /// Combined moments
        protected double _mVM;
        protected double _mTR;

        #endregion

        #region Properties

        /// Local Forces
        protected double Fxx => _fxx;
        protected double Fyy => _fyy;
        protected double Fzz => _fzz;
        protected double Fxy => _fxy;
        protected double Fxz => _fxz;
        protected double Fyz => _fyz;

        /// Local Moments      
        protected double Mxx => _mxx;
        protected double Myy => _myy;
        protected double Mzz => _mzz;
        protected double Mxy => _mxy;
        protected double Mxz => _mxz;
        protected double Myz => _myz; 

        #endregion


        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementID"></param>
        /// <param name="elementLabel"></param>
        /// <param name="loadCase"></param>
        /// <param name="coordinateSystem">Coordinate system where the forces are provided</param>
        /// <param name="fxx"></param>
        /// <param name="fyy"></param>
        /// <param name="fxy"></param>
        /// <param name="fxz"></param>
        /// <param name="fyz"></param>
        /// <param name="mxx"></param>
        /// <param name="myy"></param>
        /// <param name="mxy"></param>
        public ResultPlateForces(int elementID, string elementLabel, LoadCase loadCase, CoordinateSystem coordinateSystem, 
                                 double fxx, double fyy, double fxy, double fxz, double fyz, double mxx, double myy, double mxy)
                                 : base(elementID, elementLabel, loadCase, coordinateSystem)
        {
                _fxx = fxx;
                _fyy = fyy;
                _fzz = 0;
                _fxy = fxy;
                _fxz = fxz;
                _fyz = fyz;
                _mxx = mxx;
                _myy = myy;
                _mzz = 0;
                _mxy = mxy;
                _mxz = 0;
                _myz = 0;
        }

        #region Public methods
        public double[] GetPrincipalForces()
        {
            throw new NotImplementedException();
        }

        public double[] GetPrincipalMoments()
        {
            throw new NotImplementedException();
        }

        public double[] GetGlobalForces()
        {
            throw new NotImplementedException();
        }

        public double[] GetGlobalMoments()
        {
            throw new NotImplementedException();
        }

        public double[] GetVonMisesForces()
        {
            throw new NotImplementedException();
        }

        public double[] GetVonMisesMoments()
        {
            throw new NotImplementedException();
        }

        public double[] GetTrescaForces()
        {
            throw new NotImplementedException();
        }

        public double[] GetTrescaMoments()
        {
            throw new NotImplementedException();
        }

        public double[] GetCoordinateSystemForces(CoordinateSystem ucs)
        {
            throw new NotImplementedException();
        }
        public double[] GetCoordinateSystemMoments(CoordinateSystem ucs)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region Interface implementation

        public override bool Equals(ResultPlateForces other)
        {
            return !(other is null) &&
                    _fxx == other._fxx &&
                    _fyy == other._fyy &&
                    _fzz == other._fzz &&
                    _fxy == other._fxy &&
                    _fxz == other._fxz &&
                    _fyz == other._fyz &&
                    _mxx == other._mxx &&
                    _myy == other._myy &&
                    _mzz == other._mzz &&
                    _mxy == other._mxy &&
                    _mxz == other._mxz &&
                    _myz == other._myz;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        } 

        #endregion
    }
}
