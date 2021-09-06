using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    [Serializable]
    public sealed class ResultDisplacement : ResultType, IEquatable<ResultDisplacement>,
                                             ISerializable, INodeResult, IPlateResult, IBrickResult, IBeamResult
    {

        #region Variables

        private readonly double _d1;
        private readonly double _d2;
        private readonly double _d3;
        private readonly double _r1;
        private readonly double _r2;
        private readonly double _r3;

        #endregion

        #region Properties

        public double D1 => _d1;
        public double D2 => _d2;
        public double D3 => _d3;
        public double R1 => _r1;
        public double R2 => _r2;
        public double R3 => _r3;

        #endregion


        #region Public Constructors

        /// <param name="coordinateSystem">Coordinate system where these result are provided </param>
        /// <param name="d1">Displacement along <see cref="CoordinateSystem.V1"/> direction </param>
        /// <param name="d2">Displacement along <see cref="CoordinateSystem.V2"/> direction </param>
        /// <param name="d3">Displacement along <see cref="CoordinateSystem.V3"/> direction </param>
        /// <param name="r1">Rotation around <see cref="CoordinateSystem.V1"/> direction </param>
        /// <param name="r2">Rotation around <see cref="CoordinateSystem.V2"/> direction </param>
        /// <param name="r3">Rotation around <see cref="CoordinateSystem.V3"/> direction </param>
        public ResultDisplacement(CoordinateSystem coordinateSystem, double d1, double d2, double d3, double r1, double r2, double r3)
            : base(coordinateSystem)
        {
            _d1 = d1;
            _d2 = d2;
            _d3 = d3;
            _r1 = r1;
            _r2 = r2;
            _r3 = r3;
        }


        /// <param name="d1">Displacement along <see cref="CoordinateSystem.V1"/> direction </param>
        /// <param name="d2">Displacement along <see cref="CoordinateSystem.V2"/> direction </param>
        /// <param name="d3">Displacement along <see cref="CoordinateSystem.V3"/> direction </param>
        /// <param name="r1">Rotation around <see cref="CoordinateSystem.V1"/> direction </param>
        /// <param name="r2">Rotation around <see cref="CoordinateSystem.V2"/> direction </param>
        /// <param name="r3">Rotation around <see cref="CoordinateSystem.V3"/> direction </param>
        /// <remarks>Set the <see cref="CoordinateSystem"/> to <see cref="CoordinateSystem.Global"/></remarks>
        public ResultDisplacement(double d1, double d2, double d3, double r1, double r2, double r3)
            : base(CoordinateSystem.Global)
        {
            _d1 = d1;
            _d2 = d2;
            _d3 = d3;
            _r1 = r1;
            _r2 = r2;
            _r3 = r3;
        }


        public ResultDisplacement(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _d1 = info.GetDouble("D1");
            _d2 = info.GetDouble("D2");
            _d3 = info.GetDouble("D3");
            _r1 = info.GetDouble("R1");
            _r2 = info.GetDouble("R2");
            _r3 = info.GetDouble("R3");
        }


        #endregion


        #region Public Methods - Get displacement


        /// <summary>
        /// Return the resulting displacement 
        /// </summary>
        public double GetResultingDisplacement()
        {
            return Math.Sqrt(Math.Pow(_d1, 2) + Math.Pow(_d2, 2) + Math.Pow(_d3, 2));
        }

        /// <summary>
        /// Return the resulting vector displacement 
        /// </summary>
        public Vector3d GetResultingVectorDisplacement()
        {
            return new Vector3d(_d1, _d2, _d3);
        }

        /// <summary>
        /// Return the resulting Rotation 
        /// </summary>
        public double GetResultingRotation()
        {
            return Math.Sqrt(Math.Pow(_r1, 2) + Math.Pow(_r2, 2) + Math.Pow(_r3, 2));
        }

        /// <summary>
        /// Return the resulting vector rotation 
        /// </summary>
        public Vector3d GetResultingVectorRotation()
        {
            return new Vector3d(_r1, _r2, _r3);
        }

        /// <summary>
        /// Return the global displacements
        /// </summary>
        /// <returns>Array of displacements in global coordinate</returns>
        public double[] GetGlobalDisplacements()
        {
            Vector3d GlobalDisplResult = _coordinateSystem.ToGlobal(new Vector3d(_d1, _d2, _d3));

            Vector3d GlobalRotResult = _coordinateSystem.ToGlobal(new Vector3d(_r1, _r2, _r3));

            double[] globalDispRot = new double[6];

            globalDispRot[0] = GlobalDisplResult.X;
            globalDispRot[1] = GlobalDisplResult.Y;
            globalDispRot[2] = GlobalDisplResult.Z;
            globalDispRot[3] = GlobalRotResult.X;
            globalDispRot[4] = GlobalRotResult.Y;
            globalDispRot[5] = GlobalRotResult.Z;

            return globalDispRot;
        }


        /// <summary>
        /// Return the global displacements tuple
        /// </summary>
        /// <returns>Tuple of displacements in global coordinate</returns>
        public (Vector3d displacements, Vector3d rotations) GetGlobalDisplacementsTuple()
        {
            return (_coordinateSystem.ToGlobal(new Vector3d(_d1, _d2, _d3)), _coordinateSystem.ToGlobal(new Vector3d(_r1, _r2, _r3)));
        }


        /// <summary>
        /// Return the local displacements vector
        /// </summary>
        /// <returns>Array of displacements in local coordinate</returns>
        public double[] GetLocalDisplacements()
        {

            double[] localDisplacements = new double[6];

            localDisplacements[0] = _d1;
            localDisplacements[1] = _d2;
            localDisplacements[2] = _d3;
            localDisplacements[3] = _r1;
            localDisplacements[4] = _r2;
            localDisplacements[5] = _r3;

            return localDisplacements;
        }

        /// <summary>
        /// Return the local displacements tuple
        /// </summary>
        /// <returns>Tuple of displacements in local coordinate</returns>
        public (Vector3d displacements, Vector3d rotations) GetLocalDisplacementsTuple()
        {
            return (new Vector3d(_d1, _d2, _d3), new Vector3d(_r1, _r2, _r3));
        }

        #endregion


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("D1", _d1, typeof(double));
            info.AddValue("D2", _d2, typeof(double));
            info.AddValue("D3", _d3, typeof(double));
            info.AddValue("R1", _r1, typeof(double));
            info.AddValue("R2", _r2, typeof(double));
            info.AddValue("R3", _r3, typeof(double));
        }


        #region Equals, hashcode, operators


        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals((ResultDisplacement)obj);
        }


        public bool Equals(ResultDisplacement other)
        {
            return !(other is null) &&
                    _d1 == other._d1 && _d2 == other._d2 && _d3 == other._d3 &&
                    _r1 == other._r1 && _r2 == other._r2 && _r3 == other._r3 &&
                    base.Equals(other);
        }


        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _d1.GetHashCode();
                hashCode = hashCode * -17 + _d2.GetHashCode();
                hashCode = hashCode * -17 + _d3.GetHashCode();
                hashCode = hashCode * -17 + _r1.GetHashCode();
                hashCode = hashCode * -17 + _r2.GetHashCode();
                hashCode = hashCode * -17 + _r3.GetHashCode();
                return hashCode;
            }
        }

        public static ResultDisplacement GetArithmeticMean(ResultDisplacement[] values)
        {

            if (values.Select(i => i._coordinateSystem).Distinct().Count() > 0)
            {
                return new ResultDisplacement(values[0]._coordinateSystem, 
                                        Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.D1).ToArray()),
                                        Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.D2).ToArray()),
                                        Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.D3).ToArray()),
                                        Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.R1).ToArray()),
                                        Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.R2).ToArray()),
                                        Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.R3).ToArray())
                                        );
            }
            else
            {
                throw new NotImplementedException();

            }

        }


        public static bool operator ==(ResultDisplacement obj1, ResultDisplacement obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }


        public static bool operator !=(ResultDisplacement obj1, ResultDisplacement obj2)
        {
            return !(obj1 == obj2);
        }


        /// <returns>The sum of the displacements written in the <paramref name="obj1"/> <see cref="CoordinateSystem"/></returns>
        public static ResultDisplacement operator +(ResultDisplacement obj1, ResultDisplacement obj2)
        {
            if (obj1 is null || obj2 is null)
                throw new ArgumentNullException();

            if (obj1._coordinateSystem.Equals(obj2._coordinateSystem))
            {
                return new ResultDisplacement(obj1._coordinateSystem, obj1._d1 + obj2._d1,
                                                                      obj1._d2 + obj2._d2,
                                                                      obj1._d3 + obj2._d3,
                                                                      obj1._r1 + obj2._r1,
                                                                      obj1._r2 + obj2._r2,
                                                                      obj1._r3 + obj2._r3
                                                                      );
            }
            else
            {
                // prendo spostamenti 2 nel globale
                // li giro nel locale di obj1
                // sommo e ritonrno classe con obj1.coordinasystem

                (Vector3d displacements, Vector3d rotations) obj2GlobalDisp = obj2.GetGlobalDisplacementsTuple();

                var obj2GlobalDispToObj1 = obj1._coordinateSystem.ToLocal(obj2GlobalDisp.displacements); // spostamenti nel locale di obj1
                var obj2GlobalRotToObj2 = obj1._coordinateSystem.ToLocal(obj2GlobalDisp.rotations);   // rotazioni nel locale di obj1


                return new ResultDisplacement(obj1._coordinateSystem, obj1._d1 + obj2GlobalDispToObj1.X,
                                                                      obj1._d2 + obj2GlobalDispToObj1.Y,
                                                                      obj1._d3 + obj2GlobalDispToObj1.Z,
                                                                      obj1._r1 + obj2GlobalRotToObj2.X,
                                                                      obj1._r2 + obj2GlobalRotToObj2.Y,
                                                                      obj1._r3 + obj2GlobalRotToObj2.Z);
            }
        }


        /// <returns>The sum of the displacements written in the <paramref name="obj1"/> <see cref="CoordinateSystem"/></returns>
        public static ResultDisplacement operator -(ResultDisplacement obj1, ResultDisplacement obj2)
        {
            if (obj1 is null || obj2 is null)
                throw new ArgumentNullException();

            if (obj1._coordinateSystem.Equals(obj2._coordinateSystem))
            {
                return new ResultDisplacement(obj1._coordinateSystem, obj1._d1 - obj2._d1,
                                                                      obj1._d2 - obj2._d2,
                                                                      obj1._d3 - obj2._d3,
                                                                      obj1._r1 - obj2._r1,
                                                                      obj1._r2 - obj2._r2,
                                                                      obj1._r3 - obj2._r3
                                                                      );
            }
            else
            {
                // prendo spostamenti 2 nel globale
                // li giro nel locale di obj1
                // sommo e ritonrno classe con obj1.coordinasystem

                (Vector3d displacements, Vector3d rotations) obj2GlobalDisp = obj2.GetGlobalDisplacementsTuple();

                var obj2GlobalDispToObj1 = obj1._coordinateSystem.ToLocal(obj2GlobalDisp.displacements); // spostamenti nel locale di obj1
                var obj2GlobalRotToObj2 = obj1._coordinateSystem.ToLocal(obj2GlobalDisp.rotations);   // rotazioni nel locale di obj1


                return new ResultDisplacement(obj1._coordinateSystem, obj1._d1 - obj2GlobalDispToObj1.X,
                                                                      obj1._d2 - obj2GlobalDispToObj1.Y,
                                                                      obj1._d3 - obj2GlobalDispToObj1.Z,
                                                                      obj1._r1 - obj2GlobalRotToObj2.X,
                                                                      obj1._r2 - obj2GlobalRotToObj2.Y,
                                                                      obj1._r3 - obj2GlobalRotToObj2.Z);
            }
        }


        /// <returns>Multiply the displacements for a given factor</returns>
        public static ResultDisplacement operator *(ResultDisplacement obj1, double factor)
        {
            if (obj1 is null)
                throw new ArgumentNullException();

            return new ResultDisplacement(obj1._coordinateSystem, obj1._d1 * factor,
                                                                  obj1._d2 * factor,
                                                                  obj1._d3 * factor,
                                                                  obj1._r1 * factor,
                                                                  obj1._r2 * factor,
                                                                  obj1._r3 * factor);
        }


        /// <returns>Multiply the displacements for a given factor</returns>
        public static ResultDisplacement operator *(ResultDisplacement obj1, int factor)
        {
            return obj1 * (double)factor;
        }

        #endregion
    }
}
