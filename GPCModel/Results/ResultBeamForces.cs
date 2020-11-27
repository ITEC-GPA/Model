using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Results
{
    public class ResultBeamForces : Result
    {
        #region Variables
        protected double _N;
        protected double _V1;
        protected double _V2;
        protected double _T;
        protected double _M1;
        protected double _M2;
        #endregion

        #region Properties
        public double N => _N;
        public double V1 => _V1;
        public double V2 => _V2;
        public double T => _T;
        public double M1 => _M1;
        public double M2 => _M2;
        #endregion

        #region Public Constructors
        /// <summary>
        /// 
        /// </summary>
        /// <param name="elementID"></param>
        /// <param name="elementLabel"></param>
        /// <param name="loadCase"></param>
        /// <param name="N"> axial force </param>
        /// <param name="V1"> shear along principal axis 1 </param>
        /// <param name="V2"> shear along principal axis 2</param>
        /// <param name="T"> torque moment </param>
        /// <param name="M1"> Bending moment around axis 1 (in plane 2, right hand rule) </param>
        /// <param name="M2"> Bending moment around axis 2 (in plane 1, right hand rule) </param>
        public ResultBeamForces(int elementID, string elementLabel, LoadCase loadCase, 
            double N, double V1, double V2, double T, double M1, double M2)
            : base(elementID, elementLabel, loadCase, null)
        {
            _N = N;
            _V1 = V1;
            _V2 = V2;
            _T = T;
            _M1 = M1;
            _M2 = M2;
        }
        #endregion

        #region Public Methods
        #endregion
    }
}
