using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.LoadCases;
using GPC.Geometry;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    public class ResultNodeDisplacement : Result, ISerializable, IEquatable<ResultNodeDisplacement>
    {
        #region Variables

        /// <summary>
        /// Local Displacement
        /// </summary>
        protected double _dx;                   // the 6 Dof in local coordinates
        protected double _dy;                   // 3 translation and 3 rotation
        protected double _dz;                   // 
        protected double _rxy;                  // 
        protected double _ryz;                  // 
        protected double _rzx;                  // 
        /// <summary>
        /// Global Displacement
        /// </summary>
        protected double _dX;                   // the 6 Dof in global coordinates
        protected double _dY;                   // 3 translation and 3 rotation
        protected double _dZ;                   // 
        protected double _rXY;                  // 
        protected double _rYZ;                  // 
        protected double _rZX;                  // 
        /// <summary>
        /// Principal Displacement
        /// </summary>
        protected double _dip;                  // In Plane displacement
        protected double _dop;                  // Out of plane displacement
        /// <summary>
        /// Combined Displacement
        /// </summary>
        protected double _resultDisplacement;   // 
        protected double _resultRotation;       // 

        #endregion


        #region Properties

        protected double Dx => _dx;
        protected double Dy => _dy;
        protected double Dz => _dz;
        protected double Rxy => _rxy;
        protected double Ryz => _ryz;
        protected double Rzx => _rzx;

        #endregion


        #region Public Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementID">Id of the element where these result are referred to</param>
        /// <param name="elementLabel">Label of the element where these result are referred t</param>
        /// <param name="caseId">Represenet the id of the loadcase / loadCombination where these result are referred to</param>
        /// <param name="cSys">Coordinate system where these result are provided</param>
        /// <param name="dx"></param>
        /// <param name="dy"></param>
        /// <param name="dz"></param>
        /// <param name="rxy"></param>
        /// <param name="ryz"></param>
        /// <param name="rzx"></param>
        public ResultNodeDisplacement(int elementID, string elementLabel, int caseId, int resultPointId, CoordinateSystem cSys, double dx, double dy, double dz, double rxy, double ryz, double rzx) 
            : base(elementID, elementLabel, caseId, resultPointId, cSys)
        {
            _dx = dx;
            _dy = dy;
            _dz = dz;
            _rxy = rxy;
            _ryz = ryz;
            _rzx = rzx;
        }

        #endregion


        #region Public Methods Specific

        /// <summary>
        /// Return the Principal Displacement of the point
        /// </summary>
        /// <param name="dip">Displacement in plane</param>
        /// <param name="dop">Displacement out of plane</param>
        public void GetPrincipalDisplacement(out double dip, out double dop)
        {
            dip = Math.Sqrt(Math.Pow(_dx, 2) + Math.Pow(_dy, 2));
            dop = _dz;
        }

        /// <summary>
        /// Return the resulting displacement 
        /// </summary>
        /// <param name="Svm"></param>
        public void GetResultingDisplacement(out double resultDisplacement)
        {
            resultDisplacement = Math.Sqrt(Math.Pow(_dx, 2) + Math.Pow(_dy, 2) + Math.Pow(_dz, 2));
        }

        /// <summary>
        /// Return the resulting vector displacement 
        /// </summary>
        /// <param name="Svm"></param>
        public void GetResultingVectorDisplacement(out Vector3d resultDisplacement)
        {
            resultDisplacement = new Vector3d(_dx, _dy, _dz);
        }

        /// <summary>
        /// Return the resulting Rotation 
        /// </summary>
        /// <param name="Svm"></param>
        public void GetResultingRotation(out double resultRotation)
        {
            resultRotation = Math.Sqrt(Math.Pow(_rxy, 2) + Math.Pow(_ryz, 2) + Math.Pow(_rzx, 2));
        }

        /// <summary>
        /// Return the global stress of the point
        /// </summary>
        /// <returns>Array of stress in global coordinate</returns>
        public double[] GetGlobalStress()
        {
            Vector3d DisplResult = new Vector3d(_dx, _dy, _dz );
            Vector3d GlobalDisplResult = _coordinateSystem.ToGlobal(DisplResult);

            Vector3d RotResult = new Vector3d(_rxy, _ryz, _rzx);
            Vector3d GlobalRotResult = _coordinateSystem.ToGlobal(RotResult);

            double[] globalDispRot = new double[6];

            globalDispRot[0] = GlobalDisplResult.X;
            globalDispRot[1] = GlobalDisplResult.Y;
            globalDispRot[2] = GlobalDisplResult.Z;
            globalDispRot[3] = GlobalRotResult.X;
            globalDispRot[4] = GlobalRotResult.Y;
            globalDispRot[5] = GlobalRotResult.Z;

            return globalDispRot;
        }

        #endregion


        #region Interface implementation

        public bool Equals(ResultNodeDisplacement other)
        {
            return !(other is null) &&
                    _dx == other._dx &&
                    _dy == other._dy &&
                    _dz == other._dz &&
                    _rxy == other._rxy &&
                    _ryz == other._ryz &&
                    _rzx == other._rzx &&
                    _dX == other._dX &&
                    _dY == other._dY &&
                    _dZ == other._dZ &&
                    _rXY == other._rXY &&
                    _rYZ == other._rYZ &&
                    _rZX == other._rZX &&
                    _dip == other._dip && 
                    _dop == other._dop && 
                    _resultDisplacement == other._resultDisplacement &&
                    _resultRotation == other._resultRotation;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
