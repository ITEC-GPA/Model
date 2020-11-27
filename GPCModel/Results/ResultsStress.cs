using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Results
{
    public class ResultsStress : Results
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
        #endregion

        #region Public Constructors
        public ResultsStress(double sxx, double syy, double szz, double sxy, double sxz, double syz,
            double sXX, double sYY, double sZZ, double sXY, double sXZ, double sYZ)
        {
            _sxx = sxx;
            _syy = syy;
            _szz = szz;
            _sxy = sxy;
            _sxz = sxz;
            _syz = syz;

            _sXX = sXX;
            _sYY = sYY;
            _sZZ = sZZ;
            _sXY = sXY;
            _sXZ = sXZ;
            _sYZ = sYZ;
        }
        public ResultsStress(double sxx, double syy, double szz, double sxy, double sxz, double syz)
        {
            _sxx = sxx;
            _syy = syy;
            _szz = szz;
            _sxy = sxy;
            _sxz = sxz;
            _syz = syz;
        }
        public ResultsStress(double sxx, double syy, double sxy)
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
        public double GetPrincipalStress()
        {
            double result = 0;
            return result;
        }
        public double GetVMStress()
        {
            double result = 0;
            return result;
        }
        public double[] GetGlobalStress()
        {
            double[] result = new double[6];
            return result;
        }
        #endregion
    }
}
