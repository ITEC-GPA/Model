using GPC.Model.LoadCases;
using System;

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
        /// <param name="elementID">Id of the element where these result are referred to</param>
        /// <param name="elementLabel">Label of the element where these result are referred t</param>
        /// <param name="caseId">Represenet the id of the loadcase / loadCombination where these result are referred to</param>
        /// <param name="N"> axial force </param>
        /// <param name="V1"> shear along principal axis 1 </param>
        /// <param name="V2"> shear along principal axis 2</param>
        /// <param name="T"> torque moment </param>
        /// <param name="M1"> Bending moment around axis 1 (in plane 2, right hand rule) </param>
        /// <param name="M2"> Bending moment around axis 2 (in plane 1, right hand rule) </param>
        public ResultBeamForces(int elementID, string elementLabel, int caseId, double N, double V1, double V2, double T, double M1, double M2)
            : base(elementID, elementLabel, caseId, null)
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

        /// <summary>
        /// Return the combined bending moment between M1 and M2
        /// </summary>
        /// <returns>The combined bending moment</returns>
        public double GetCombinedBendingMoment()
        {
            return Math.Sqrt(Math.Pow(M1, 2) + Math.Pow(M2, 2));
        }

        /// <summary>
        /// Return the combined bending moment between V1 and V2
        /// </summary>
        /// <returns>he combined shear force</returns>
        public double GetCombinedShearForce()
        {
            return Math.Sqrt(Math.Pow(V1, 2) + Math.Pow(V2, 2));
        }

        #endregion 
    }
}