using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class ResultBeamForces : ResultType, IEquatable<ResultBeamForces>, ISerializable, IBeamResult
    {

        #region Variables

        private readonly double _N;
        private readonly double _V1;
        private readonly double _V2;
        private readonly double _T;
        private readonly double _M1;
        private readonly double _M2;

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


        /// <param name="N"> axial force </param>
        /// <param name="V1"> shear along principal axis 1</param>
        /// <param name="V2"> shear along principal axis 2</param>
        /// <param name="T"> torque moment </param>
        /// <param name="M1"> Bending moment around axis 1 (in plane 2, right hand rule) </param>
        /// <param name="M2"> Bending moment around axis 2 (in plane 1, right hand rule) </param>
        public ResultBeamForces(double N, double V1, double V2, double T, double M1, double M2)
            : base(null)
        {
            _N = N;
            _V1 = V1;
            _V2 = V2;
            _T = T;
            _M1 = M1;
            _M2 = M2;
        }

        #endregion 


        #region Public Methods - getforces

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

        public bool Equals(ResultBeamForces other)
        {
            throw new NotImplementedException();
        }

        #endregion

    }
}