using GPC.Geometry;
using System;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
    /// <summary>
    /// The displacements and rotations in a coordinate system
    /// </summary>
    [Serializable]
    public sealed class ResultDisplacement : ResultType, IEquatable<ResultDisplacement>, ISerializable, INodeResult, IPlateResult, IBrickResult, IBeamResult, IResult<ResultDisplacement>
    {
        #region Variables

        /// <summary>
        /// Displacement along the first axis
        /// </summary>
        private double _d1;
        /// <summary>
        /// Displacement along the second axis
        /// </summary>
        private double _d2;
        /// <summary>
        /// Displacement along the third axis
        /// </summary>
        private double _d3;
        /// <summary>
        /// Rotation around the first axis
        /// </summary>
        private double _r1;
        /// <summary>
        /// Rotation around the second axis
        /// </summary>
        private double _r2;
        /// <summary>
        /// Rotation around the third axis
        /// </summary>
        private double _r3;

        #endregion

        #region Properties

        /// <summary>
        /// Displacement along <see cref="CoordinateSystem.V1"/>
        /// </summary>
        public double D1 { get => _d1; set => _d1 = value; }
        /// <summary>
        /// Displacement along <see cref="CoordinateSystem.V2"/>
        /// </summary>
        public double D2 { get => _d2; set => _d2 = value; }
        /// <summary>
        /// Displacement along <see cref="CoordinateSystem.V3"/>
        /// </summary>
        public double D3 { get => _d3; set => _d3 = value; }
        /// <summary>
        /// Rotation around <see cref="CoordinateSystem.V1"/>
        /// </summary>
        public double R1 { get => _r1; set => _r1 = value; }
        /// <summary>
        /// Rotation around <see cref="CoordinateSystem.V2"/>
        /// </summary>
        public double R2 { get => _r2; set => _r2 = value; }
        /// <summary>
        /// Rotation around <see cref="CoordinateSystem.V3"/>
        /// </summary>
        public double R3 { get => _r3; set => _r3 = value; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the result
        /// </summary>
        /// <param name="coordinateSystem">Coordinate system where these result are provided</param>
        /// <param name="d1">Displacement along <see cref="CoordinateSystem.V1"/> direction</param>
        /// <param name="d2">Displacement along <see cref="CoordinateSystem.V2"/> direction</param>
        /// <param name="d3">Displacement along <see cref="CoordinateSystem.V3"/> direction</param>
        /// <param name="r1">Rotation around <see cref="CoordinateSystem.V1"/> direction</param>
        /// <param name="r2">Rotation around <see cref="CoordinateSystem.V2"/> direction</param>
        /// <param name="r3">Rotation around <see cref="CoordinateSystem.V3"/> direction</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentNullException">If <paramref name="coordinateSystem"/> is null</exception>
        public ResultDisplacement(CoordinateSystem coordinateSystem, double d1, double d2, double d3, double r1, double r2, double r3, int id = ModelObjectId.IDUNASSIGNED)
            : base(coordinateSystem, string.Empty, id)
        {
            _d1 = d1;
            _d2 = d2;
            _d3 = d3;
            _r1 = r1;
            _r2 = r2;
            _r3 = r3;
        }

        /// <summary>
        /// Creates the result in the global coordinate system
        /// </summary>
        /// <param name="d1">Displacement along X</param>
        /// <param name="d2">Displacement along Y</param>
        /// <param name="d3">Displacement along Z</param>
        /// <param name="r1">Rotation around X</param>
        /// <param name="r2">Rotation around Y</param>
        /// <param name="r3">Rotation around Z</param>
        /// <remarks>Set the <see cref="ResultType.CoordinateSystem"/> to <see cref="CoordinateSystem.Global"/></remarks>
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

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private ResultDisplacement(SerializationInfo info, StreamingContext context)
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
        /// The magnitude of the displacement
        /// </summary>
        /// <returns>sqrt(d1² + d2² + d3²)</returns>
        public double GetResultingDisplacement()
        {
            return Math.Sqrt(Math.Pow(_d1, 2) + Math.Pow(_d2, 2) + Math.Pow(_d3, 2));
        }

        /// <summary>
        /// The displacement vector (local components)
        /// </summary>
        /// <returns>(d1, d2, d3)</returns>
        public Vector3d GetResultingVectorDisplacement()
        {
            return new Vector3d(_d1, _d2, _d3);
        }

        /// <summary>
        /// The magnitude of the rotation
        /// </summary>
        /// <returns>sqrt(r1² + r2² + r3²)</returns>
        public double GetResultingRotation()
        {
            return Math.Sqrt(Math.Pow(_r1, 2) + Math.Pow(_r2, 2) + Math.Pow(_r3, 2));
        }

        /// <summary>
        /// The rotation vector (local components)
        /// </summary>
        /// <returns>(r1, r2, r3)</returns>
        public Vector3d GetResultingVectorRotation()
        {
            return new Vector3d(_r1, _r2, _r3);
        }

        /// <summary>
        /// The displacements and rotations in the global coordinate system
        /// </summary>
        /// <returns>Array with the global displacements X, Y, Z and the global rotations X, Y, Z</returns>
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
        /// The displacements and rotations in the global coordinate system
        /// </summary>
        /// <returns>The global displacement and rotation vectors</returns>
        public (Vector3d displacements, Vector3d rotations) GetGlobalDisplacementsTuple()
        {
            return (_coordinateSystem.ToGlobal(new Vector3d(_d1, _d2, _d3)), _coordinateSystem.ToGlobal(new Vector3d(_r1, _r2, _r3)));
        }

        /// <summary>
        /// The displacements and rotations in the coordinate system of the result
        /// </summary>
        /// <returns>Array with d1, d2, d3, r1, r2, r3</returns>
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
        /// The displacements and rotations in the coordinate system of the result
        /// </summary>
        /// <returns>The local displacement and rotation vectors</returns>
        public (Vector3d displacements, Vector3d rotations) GetLocalDisplacementsTuple()
        {
            return (new Vector3d(_d1, _d2, _d3), new Vector3d(_r1, _r2, _r3));
        }

        /// <summary>
        /// The same result in another coordinate system: the displacement and rotation vectors are rotated (d1, d2, d3 along the new axes). Before,
        /// the components were returned permuted (d1 = local Z, d2 = local X, d3 = local Y, the same for the rotations: copied from
        /// <see cref="ResultBeamForces.ToCoordinateSystem(CoordinateSystem)"/>, where N is along Z) and the id was lost
        /// </summary>
        /// <param name="coordinateSystem">The new coordinate system</param>
        /// <returns>The new result</returns>
        public ResultDisplacement ToCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            Vector3d vector3dDisplacement = new Vector3d(_d1, _d2, _d3);
            Vector3d vector3dRotation = new Vector3d(_r1, _r2, _r3);

            Vector3d vector3dvector3dDisplacementGlobal = CoordinateSystem.ToGlobal(vector3dDisplacement);
            Vector3d vector3dvector3dRotationGlobal = CoordinateSystem.ToGlobal(vector3dRotation);

            var displacementNewCoordinate = coordinateSystem.ToLocal(vector3dvector3dDisplacementGlobal);
            var rotationNewCoordinate = coordinateSystem.ToLocal(vector3dvector3dRotationGlobal);

            return new ResultDisplacement(coordinateSystem, displacementNewCoordinate.X, displacementNewCoordinate.Y, displacementNewCoordinate.Z,
                rotationNewCoordinate.X, rotationNewCoordinate.Y, rotationNewCoordinate.Z, _id);
        }

        /// <summary>
        /// The arithmetic mean of the components, in the coordinate system of the first result. The components are averaged as they are, also
        /// if the coordinate systems are different (the check on the coordinate systems is always true)
        /// </summary>
        /// <param name="values">The results (not empty)</param>
        /// <returns>The mean result</returns>
        public static ResultDisplacement GetArithmeticMean(ResultDisplacement[] values)
        {
            if (values.Select(i => i._coordinateSystem).Distinct().Count() > 0)
            {
                return new ResultDisplacement(
                    values[0]._coordinateSystem,
                    GPC.Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.D1).ToArray()),
                    GPC.Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.D2).ToArray()),
                    GPC.Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.D3).ToArray()),
                    GPC.Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.R1).ToArray()),
                    GPC.Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.R2).ToArray()),
                    GPC.Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.R3).ToArray()));
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Serializes the result
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
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

        /// <summary>
        /// Equality with another result (an object of another type throws <see cref="InvalidCastException"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal result</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return Equals((ResultDisplacement)obj);
        }

        /// <summary>
        /// Exact equality of the components and of the name (the coordinate system is not compared)
        /// </summary>
        /// <param name="other">The result to compare</param>
        /// <returns>True if the results are equal</returns>
        public bool Equals(ResultDisplacement other)
        {
            return !(other is null) &&
                _d1 == other._d1 && _d2 == other._d2 && _d3 == other._d3 &&
                _r1 == other._r1 && _r2 == other._r2 && _r3 == other._r3 &&
                base.Equals(other);
        }

        /// <summary>
        /// The hash code of the name and of the components
        /// </summary>
        /// <returns>The hash code</returns>
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

        /// <summary>
        /// Equality operator (see <see cref="Equals(ResultDisplacement)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are equal</returns>
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

        /// <summary>
        /// Inequality operator (see <see cref="Equals(ResultDisplacement)"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>True if the results are different</returns>
        public static bool operator !=(ResultDisplacement obj1, ResultDisplacement obj2)
        {
            return !(obj1 == obj2);
        }

        /// <summary>
        /// The sum of the displacements (<paramref name="obj2"/> is rotated to the coordinate system of <paramref name="obj1"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>The sum of the displacements written in the <paramref name="obj1"/> <see cref="ResultType.CoordinateSystem"/></returns>
        /// <exception cref="ArgumentNullException">If an operand is null</exception>
        public static ResultDisplacement operator +(ResultDisplacement obj1, ResultDisplacement obj2)
        {
            if (obj1 is null || obj2 is null)
                throw new ArgumentNullException();

            if (obj1._coordinateSystem.Equals(obj2._coordinateSystem))
            {
                return new ResultDisplacement(
                    obj1._coordinateSystem,
                    obj1._d1 + obj2._d1,
                    obj1._d2 + obj2._d2,
                    obj1._d3 + obj2._d3,
                    obj1._r1 + obj2._r1,
                    obj1._r2 + obj2._r2,
                    obj1._r3 + obj2._r3);
            }
            else
            {
                // prendo spostamenti 2 nel globale
                // li giro nel locale di obj1
                // sommo e ritonrno classe con obj1.coordinasystem

                (Vector3d displacements, Vector3d rotations) obj2GlobalDisp = obj2.GetGlobalDisplacementsTuple();

                var obj2GlobalDispToObj1 = obj1._coordinateSystem.ToLocal(obj2GlobalDisp.displacements); // spostamenti nel locale di obj1
                var obj2GlobalRotToObj2 = obj1._coordinateSystem.ToLocal(obj2GlobalDisp.rotations);   // rotazioni nel locale di obj1

                return new ResultDisplacement(
                    obj1._coordinateSystem,
                    obj1._d1 + obj2GlobalDispToObj1.X,
                    obj1._d2 + obj2GlobalDispToObj1.Y,
                    obj1._d3 + obj2GlobalDispToObj1.Z,
                    obj1._r1 + obj2GlobalRotToObj2.X,
                    obj1._r2 + obj2GlobalRotToObj2.Y,
                    obj1._r3 + obj2GlobalRotToObj2.Z);
            }
        }

        /// <summary>
        /// The difference of the displacements (<paramref name="obj2"/> is rotated to the coordinate system of <paramref name="obj1"/>)
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>The difference of the displacements written in the <paramref name="obj1"/> <see cref="ResultType.CoordinateSystem"/></returns>
        /// <exception cref="ArgumentNullException">If an operand is null</exception>
        public static ResultDisplacement operator -(ResultDisplacement obj1, ResultDisplacement obj2)
        {
            if (obj1 is null || obj2 is null)
                throw new ArgumentNullException();

            if (obj1._coordinateSystem.Equals(obj2._coordinateSystem))
            {
                return new ResultDisplacement(
                    obj1._coordinateSystem,
                    obj1._d1 - obj2._d1,
                    obj1._d2 - obj2._d2,
                    obj1._d3 - obj2._d3,
                    obj1._r1 - obj2._r1,
                    obj1._r2 - obj2._r2,
                    obj1._r3 - obj2._r3);
            }
            else
            {
                // prendo spostamenti 2 nel globale
                // li giro nel locale di obj1
                // sommo e ritonrno classe con obj1.coordinasystem

                (Vector3d displacements, Vector3d rotations) obj2GlobalDisp = obj2.GetGlobalDisplacementsTuple();

                var obj2GlobalDispToObj1 = obj1._coordinateSystem.ToLocal(obj2GlobalDisp.displacements); // spostamenti nel locale di obj1
                var obj2GlobalRotToObj2 = obj1._coordinateSystem.ToLocal(obj2GlobalDisp.rotations);   // rotazioni nel locale di obj1

                return new ResultDisplacement(
                    obj1._coordinateSystem,
                    obj1._d1 - obj2GlobalDispToObj1.X,
                    obj1._d2 - obj2GlobalDispToObj1.Y,
                    obj1._d3 - obj2GlobalDispToObj1.Z,
                    obj1._r1 - obj2GlobalRotToObj2.X,
                    obj1._r2 - obj2GlobalRotToObj2.Y,
                    obj1._r3 - obj2GlobalRotToObj2.Z);
            }
        }

        /// <summary>
        /// The displacements multiplied by a factor
        /// </summary>
        /// <param name="obj1">The result</param>
        /// <param name="factor">The factor</param>
        /// <returns>The new result, in the same coordinate system</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="obj1"/> is null</exception>
        public static ResultDisplacement operator *(ResultDisplacement obj1, double factor)
        {
            if (obj1 is null)
                throw new ArgumentNullException();

            return new ResultDisplacement(
                obj1._coordinateSystem,
                obj1._d1 * factor,
                obj1._d2 * factor,
                obj1._d3 * factor,
                obj1._r1 * factor,
                obj1._r2 * factor,
                obj1._r3 * factor);
        }

        /// <summary>
        /// The displacements multiplied by a factor
        /// </summary>
        /// <param name="obj1">The result</param>
        /// <param name="factor">The factor</param>
        /// <returns>The new result, in the same coordinate system</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="obj1"/> is null</exception>
        public static ResultDisplacement operator *(ResultDisplacement obj1, int factor)
        {
            return obj1 * (double)factor;
        }

        #endregion
    }
}
