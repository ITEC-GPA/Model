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
    public class ResultPlateStrain : Result, ISerializable, IEquatable<ResultPlateStrain>
    {
        #region Variables

        /// <summary>
        /// Local Strain
        /// </summary>
        protected double _epsilonxx;
        protected double _epsilonyy;
        protected double _epsilonzz;
        protected double _epsilonxy;
        protected double _epsilonyz;
        protected double _epsilonzx;

        #endregion


        #region Properties

        protected double Epsilonxx => _epsilonxx;
        protected double Epsilonyy => _epsilonyy;
        protected double Epsilonzz => _epsilonzz;
        protected double Epsilonxy => _epsilonxy;
        protected double Epsilonyz => _epsilonyz;
        protected double Epsilonzx => _epsilonzx;

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
        public ResultPlateStrain(int elementID, string elementLabel, int resultPointId, int caseId, CoordinateSystem cSys, double epsilonxx, double epsilonyy, double epsilonzz, double epsilonxy, double epsilonyz, double epsilonzx)
            : base(elementID, elementLabel, caseId, cSys)
        {
            _epsilonxx = epsilonxx;
            _epsilonyy = epsilonyy;
            _epsilonzz = epsilonzz;
            _epsilonxy = epsilonxy;
            _epsilonyz = epsilonyz;
            _epsilonzx = epsilonzx;
        }


        #endregion

        #region Public Methods Specific

        public void GetStrain()
        {
            throw new NotImplementedException();
        }



        #endregion


        #region Interface implementation

        public bool Equals(ResultPlateStrain other)
        {
            return !(other is null) &&
                    _epsilonxx == other._epsilonxx &&
                    _epsilonyy == other._epsilonyy &&
                    _epsilonzz == other._epsilonzz &&
                    _epsilonxy == other._epsilonxy &&
                    _epsilonyz == other._epsilonyz &&
                    _epsilonzx == other._epsilonzx;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
