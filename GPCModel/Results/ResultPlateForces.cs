using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;
using GPC.Model.FEM.FiniteElements;

namespace GPC.Model.Results
{
    /// <summary>
    /// This class needs to be revised and updated according to <see cref="ResultPlateStress"/>
    /// </summary>
    [Serializable]
    public sealed class ResultPlateForces : Result, ISerializable, IEquatable<ResultPlateForces>
    {
        #region Variables

        /// Local Forces
        private readonly double _fxx;
        private readonly double _fyy;
        private readonly double _fzz;
        private readonly double _fxy;
        private readonly double _fxz;
        private readonly double _fyz;

        /// Local Moments
        private readonly double _mxx;
        private readonly double _myy;
        private readonly double _mzz;
        private readonly double _mxy;
        private readonly double _mxz;
        private readonly double _myz;

        /// Global Forces
        
        // TODO aggiungere proprietà se servono 
#pragma warning disable CS0169 
        private double _fXX;
        private double _fYY;
        private double _fZZ;
        private double _fXY;
        private double _fXZ;
        private double _fYZ;

        /// Global Moments
        private double _mXX;
        private double _mYY;
        private double _mZZ;
        private double _mXY;
        private double _mXZ;
        private double _mYZ;

        /// Principal forces
        private double _f11;
        private double _f22;

        /// Principal moments
        private double _m11;
        private double _m22;

        /// Combined forces
        private double _fVM;
        private double _fTR;

        /// Combined moments
        private double _mVM;
        private double _mTR;
#pragma warning restore CS0169 

        #endregion


        #region Properties

        /// Local Forces
        public double Fxx => _fxx;
        public double Fyy => _fyy;
        public double Fzz => _fzz;
        public double Fxy => _fxy;
        public double Fxz => _fxz;
        public double Fyz => _fyz;

        /// Local Moments      
        public double Mxx => _mxx;
        public double Myy => _myy;
        public double Mzz => _mzz;
        public double Mxy => _mxy;
        public double Mxz => _mxz;
        public double Myz => _myz;

        public new Plate Element => (Plate)_element;
        #endregion


        #region Public Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element">Element where these result are referred </param>
        /// <param name="Case">The case where these results are reffered </param>
        /// <param name="coordinateSystem">Coordinate system where these result are provided</param>
        /// <param name="resultPoint">Stress point where these results are provided</param>
        /// <param name="fxx"></param>
        /// <param name="fyy"></param>
        /// <param name="fxy"></param>
        /// <param name="fxz"></param>
        /// <param name="fyz"></param>
        /// <param name="mxx"></param>
        /// <param name="myy"></param>
        /// <param name="mxy"></param>
        public ResultPlateForces(Plate element, ILoadCase Case, ResultStressPoint resultPoint, CoordinateSystem coordinateSystem, 
                                 double fxx, double fyy, double fxy, double fxz, double fyz, double mxx, double myy, double mxy)
                                 : base(element, Case, resultPoint, coordinateSystem)
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

        #endregion


        #region Public methods - Get forces
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


        #region Equals, hashcode, operators

        public bool Equals(ResultPlateForces other)
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
