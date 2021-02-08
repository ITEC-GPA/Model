using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.LoadCases;
using GPC.Geometry;

namespace GPC.Model.Results
{
    public class ResultPlateStress : Result
    {
        #region Variables

        /// <summary>
        /// Local Stresses
        /// </summary>
        protected double _sxx;
        protected double _syy;
        protected double _szz;
        protected double _sxy;
        protected double _sxz;
        protected double _syz;
        /// <summary>
        /// Global Stresses
        /// </summary>
        protected double _sXX;
        protected double _sYY;
        protected double _sZZ;
        protected double _sXY;
        protected double _sXZ;
        protected double _sYZ;
        /// <summary>
        /// Principal Stresses
        /// </summary>
        protected double _s11;
        protected double _s22;
        /// <summary>
        /// Combined Stresses
        /// </summary>
        protected double _sVM;
        protected double _sTR;

        #endregion


        #region Properties

        protected double Sxx => _sxx;
        protected double Syy => _syy;
        protected double Szz => _szz;
        protected double Sxy => _sxy;
        protected double Sxz => _sxz;
        protected double Syz => _syz;

        #endregion


        #region Public Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementID">Id of the element where these result are referred to</param>
        /// <param name="elementLabel">Label of the element where these result are referred t</param>
        /// <param name="caseId">Represenet the id of the loadcase / loadCombination where these result are referred to</param>
        /// <param name="cSys">Coordinate system where these result are provided</param>
        /// <param name="cSys"></param>
        /// <param name="sxx"></param>
        /// <param name="syy"></param>
        /// <param name="sxy"></param>
        public ResultPlateStress(int elementID, string elementLabel, int resultPointId, int caseId, CoordinateSystem cSys, double sxx, double syy, double sxy) 
            : base(elementID, elementLabel, caseId, cSys)
        {
            _sxx = sxx;
            _sxx = syy;
            _szz = 0;
            _sxy = sxy;
            _sxz = 0;
            _syz = 0;
        }


        #endregion


        #region Public Methods Specific

        /// <summary>
        /// Return the Principal stresses of the point
        /// </summary>
        /// <param name="S11">Principal Stress S11</param>
        /// <param name="S22">Principal Stress S22</param>
        public void GetPrincipalStress(out double S11, out double S22)
        {
            // double phi = 0.5 * Math.Atan( Math.Abs( (2*_sxy) / (_sxx + _syy )));         // The angle, Φ, is the angle in radians between the maximum normal stress and the local x-axis.
            S11 = ((_sxx + _syy) / 2) + Math.Sqrt((Math.Pow((_sxx - _syy), 2) / 4) + Math.Pow(_sxy, 2));
            S22 = ((_sxx + _syy) / 2) - Math.Sqrt((Math.Pow((_sxx - _syy), 2) / 4) + Math.Pow(_sxy, 2));
        }

        /// <summary>
        /// Return the VonMises Stress of the point
        /// </summary>
        /// <param name="Svm"></param>
        public void GetVMStress(out double Svm)
        {
            GetPrincipalStress(out _s11, out _s22);
            Svm = Math.Sqrt(Math.Pow((_s11), 2) + (Math.Pow((_s22), 2) - (_s22 * _s11)));
        }

        /// <summary>
        /// Return the stress of the point in global coordinate
        /// </summary>
        /// <returns>Array of stress</returns>
        public double[] GetGlobalStress()
        {
            Vector3d SigmaResult = new Vector3d(_sxx, _syy, 0 );
            Vector3d GlobalSigmaResult = _cSys.ToGlobal(SigmaResult);

            Vector3d TauResult = new Vector3d(0, 0, _sxy);
            Vector3d GlobalTauResult = _cSys.ToGlobal(TauResult);

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
    }
}
