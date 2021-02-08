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
    public class ResultNodeForce : Result, ISerializable, IEquatable<ResultNodeForce>
    {
        #region Variables

        // Global forces
        protected double _fx;
        protected double _fy;
        protected double _fz;

        // Global moments
        protected double _mx;
        protected double _my;
        protected double _mz;

        #endregion


        #region Properties

        // Global forces
        protected double Fx => _fx;
        protected double Fy => _fy;
        protected double Fz => _fz;

        // Global moments
        protected double Mx => _mx;
        protected double My => _my;
        protected double Mz => _mz;

        #endregion


        #region Public Constructors

        public ResultNodeForce(int elementID, string elementLabel, int caseID, double fx, double fy, double fz, 
                                double mx, double my, double mz)
                                : base(elementID, elementLabel, caseID, CoordinateSystem.Global)
        {
            _fx = fx;
            _fy = fy;
            _fz = fz;
            _mx = mx;
            _my = my;
            _mz = mz;
        }

        #endregion


        #region Public Methods Specific

        /// <summary>
        /// Return the resultant force of fx, fy, fz
        /// </summary>
        /// <param name="result">Resultant force</param>
        /// <returns>The resultant force </returns>
        public double GetForceResult(out double result)
        {
            result = Math.Sqrt(Math.Pow(_fz, 2) + Math.Pow(_fy, 2) + Math.Pow(_fz, 2));
            return result;
        }

        /// <summary>
        /// Return an array with the 6 components of force in local coordinates
        /// </summary>
        /// <param name="cSys">The local coordinate system</param>
        /// <returns>The array</returns>
        public double[] GetLocalForces(CoordinateSystem cSys)
        {
            Point3d forceGlobal = new Point3d(_fx, _fy, _fz);                               // 
            Point3d momentglobal = new Point3d(_mx, _my, _mz);                              // Crea un array di double in le 3 componenti di forza e di momento
                                                                                            // nelle 3 direzioni del sistema di coordinate locali.
            Point3d ForceLocal = cSys.ToLocal(forceGlobal);                                 // 
            Point3d MomentLocal = cSys.ToLocal(momentglobal);

            Point3d OriginGlobal = cSys.ToLocal(CoordinateSystem.Global.Origin);

            Vector3d forceLocal = new Vector3d(ForceLocal - OriginGlobal);
            Vector3d momentLocal = new Vector3d(MomentLocal - OriginGlobal);

            double[] LocalForces = new double[6];
            LocalForces[0] = forceLocal.X;
            LocalForces[1] = forceLocal.Y;
            LocalForces[2] = forceLocal.Z;
            LocalForces[3] = momentLocal.X;
            LocalForces[4] = momentLocal.Y;
            LocalForces[5] = momentLocal.Z;

            return LocalForces;
        }

        #endregion


        #region Interface implementation

        public bool Equals(ResultNodeForce other)
        {
            return !(other is null) &&
                    _fx == other._fx &&
                    _fy == other._fy &&
                    _fz == other._fz &&
                    _mx == other._mx &&
                    _my == other._my &&
                    _mz == other._mz;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
