using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.LoadCases;
using GPC.Geometry;
using System.Runtime.Serialization;
using GPC.Model.Elements;
using GPC.Model.FEM;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class ResultNodeForce : Result, ISerializable, IEquatable<ResultNodeForce>
    {
        #region Variables

        private readonly double _f1;
        private readonly double _f2;
        private readonly double _f3;
        private readonly double _m1;
        private readonly double _m2;
        private readonly double _m3;

        #endregion


        #region Properties

        public double F1 => _f1;
        public double F2 => _f2;
        public double F3 => _f3;
        public double M1 => _m1;
        public double M2 => _m2;
        public double M3 => _m3;

        #endregion


        #region Public Constructors

        /// <summary>
        /// 
        /// </summary>
        /// <param name="node">Node where these result are referred </param>
        /// <param name="Case">The case where these results are reffered </param>
        /// <param name="coordinateSystem">Coordinate system where these result are provided</param>
        /// <param name="f1">Force along <see cref="CoordinateSystem.V1"/> direction </param>
        /// <param name="f2">Force along <see cref="CoordinateSystem.V2"/> direction </param>
        /// <param name="f3">Force along <see cref="CoordinateSystem.V3"/> direction </param>
        /// <param name="m1">Moment around <see cref="CoordinateSystem.V1"/> direction </param>
        /// <param name="m2">Moment around <see cref="CoordinateSystem.V2"/> direction </param>
        /// <param name="m3">Moment around <see cref="CoordinateSystem.V3"/> direction </param>
        public ResultNodeForce(Node node, ILoadCase Case, CoordinateSystem coordinateSystem, double f1, double f2, double f3, double m1, double m2, double m3)
                                : base(node, Case, null, coordinateSystem)
        {
            _f1 = f1;
            _f2 = f2;
            _f3 = f3;
            _m1 = m1;
            _m2 = m2;
            _m3 = m3;
        }

        public ResultNodeForce(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            throw new NotImplementedException();
        }

        #endregion


        #region Public Methods - Get forces

        /// <summary>
        /// </summary>
        /// <returns>The resultant force of f1, f2, f3 </returns>
        public double GetForceResult()
        {
            return Math.Sqrt(Math.Pow(_f1, 2) + Math.Pow(_f2, 2) + Math.Pow(_f3, 2));
        }

        /// <summary>
        /// Return an array with the 6 components of force in local coordinates
        /// </summary>
        /// <param name="cSys">The local coordinate system</param>
        /// <returns>The array</returns>
        public double[] GetLocalForces(CoordinateSystem cSys)
        {
            Point3d forceGlobal = new Point3d(_f1, _f2, _f3);                               // 
            Point3d momentglobal = new Point3d(_m1, _m2, _m3);                              // Crea un array di double in le 3 componenti di forza e di momento
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

        public Node GetNode()
        {
            return (Node)Element;
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

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as ResultNodeForce);
        }

        public bool Equals(ResultNodeForce other)
        {
            return !(other is null) &&
                    _f1 == other._f1 &&
                    _f2 == other._f2 &&
                    _f3 == other._f3 &&
                    _m1 == other._m1 &&
                    _m2 == other._m2 &&
                    _m3 == other._m3 && base.Equals(other);
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            throw new NotImplementedException();
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _f1.GetHashCode();
                hashCode = hashCode * -17 + _f2.GetHashCode();
                hashCode = hashCode * -17 + _f3.GetHashCode();
                hashCode = hashCode * -17 + _m1.GetHashCode();
                hashCode = hashCode * -17 + _m2.GetHashCode();
                hashCode = hashCode * -17 + _m3.GetHashCode();
                return hashCode; 
            }
        }

        public static bool operator ==(ResultNodeForce obj1, ResultNodeForce obj2)
        {
            if (ReferenceEquals(obj1, obj2))
                return true;

            if (obj1 is null || obj2 is null)
                return false;

            return obj1.Equals(obj2);
        }

        public static bool operator !=(ResultNodeForce obj1, ResultNodeForce obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion
    }
}
