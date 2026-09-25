using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using MathNet.Numerics.LinearAlgebra;

namespace GPC.Model.Results
{
    /// <summary>
    /// The stresses of a plate at its lower, middle and upper faces
    /// </summary>
    [Serializable]
    public sealed class ResultPlateStress : ResultType, IEquatable<ResultPlateStress>, ISerializable, IPlateResult, IResult<ResultPlateStress>
    {
        #region Variables

        /// <summary>
        /// The stresses at the lower face
        /// </summary>
        private readonly ResultStress _lowerFace;
        /// <summary>
        /// The stresses at the middle face
        /// </summary>
        private readonly ResultStress _midFace;
        /// <summary>
        /// The stresses at the upper face
        /// </summary>
        private readonly ResultStress _upperFace;

        #endregion

        #region Properties

        /// <summary>
        /// The stresses at the lower face
        /// </summary>
        public ResultStress LowerFace => _lowerFace;
        /// <summary>
        /// The stresses at the middle face
        /// </summary>
        public ResultStress MidFace => _midFace;
        /// <summary>
        /// The stresses at the upper face
        /// </summary>
        public ResultStress UpperFace => _upperFace;

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the result (the faces must have the same coordinate system, not checked against <paramref name="coordinateSystem"/>)
        /// </summary>
        /// <param name="coordinateSystem">The coordinate system</param>
        /// <param name="lowerFace">The stresses at the lower face</param>
        /// <param name="midFace">The stresses at the middle face</param>
        /// <param name="upperFace">The stresses at the upper face</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentException">If the faces have different coordinate systems</exception>
        /// <exception cref="NullReferenceException">If a face is null (the coordinate systems are compared before the null checks)</exception>
        public ResultPlateStress(CoordinateSystem coordinateSystem, ResultStress lowerFace, ResultStress midFace, ResultStress upperFace, string name = "", int id = ModelObjectId.IDUNASSIGNED)
            : base(coordinateSystem, name, id)
        {
            if (lowerFace.CoordinateSystem != midFace.CoordinateSystem || lowerFace.CoordinateSystem != upperFace.CoordinateSystem)
                throw new ArgumentException();

            _lowerFace = lowerFace ?? throw new ArgumentNullException(nameof(lowerFace));
            _midFace = midFace ?? throw new ArgumentNullException(nameof(midFace));
            _upperFace = upperFace ?? throw new ArgumentNullException(nameof(upperFace));
        }

        /// <summary>
        /// Deserialization constructor
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        private ResultPlateStress(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _lowerFace = (ResultStress)info.GetValue("ResultStressLower", typeof(ResultStress));
            _midFace = (ResultStress)info.GetValue("ResultStressMid", typeof(ResultStress));
            _upperFace = (ResultStress)info.GetValue("ResultStressUpper", typeof(ResultStress));
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Serializes the result
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ResultStressLower", _lowerFace);
            info.AddValue("ResultStressMid", _midFace);
            info.AddValue("ResultStressUpper", _upperFace);
        }

        /// <summary>
        /// Calculates the principal stresses of the three faces with the simplified method (see <see cref="ResultStress.CalculatePrincipalStressSimplifiedMethod"/>)
        /// </summary>
        public void CalculatePrincipalStressSimplifiedMethod()
        {
            _lowerFace.CalculatePrincipalStressSimplifiedMethod();
            _midFace.CalculatePrincipalStressSimplifiedMethod();
            _upperFace.CalculatePrincipalStressSimplifiedMethod();
        }

        /// <summary>
        /// Calculates the principal stresses of the three faces with the eigenvalues (see <see cref="ResultStress.CalculatePrincipalStressFullMethod"/>)
        /// </summary>
        public void CalculatePrincipalStressFullMethod()
        {
            _lowerFace.CalculatePrincipalStressFullMethod();
            _midFace.CalculatePrincipalStressFullMethod();
            _upperFace.CalculatePrincipalStressFullMethod();
        }

        /// <summary>
        /// The stresses of the faces in global coordinates (see <see cref="ResultStress.GetGlobalStress"/>)
        /// </summary>
        /// <returns>The arrays of the three faces</returns>
        public (double[] lowerFace, double[] midFace, double[] upperFace) GetGlobalStress()
        {
            return (_lowerFace.GetGlobalStress(), _midFace.GetGlobalStress(), _upperFace.GetGlobalStress());
        }

        /// <summary>
        /// The stress tensors of the faces
        /// </summary>
        /// <param name="toGlobal">True for the global coordinate system, false for the local one</param>
        /// <returns>The stress tensors of the three faces</returns>
        public (Matrix<double> lowerFace, Matrix<double> midFace, Matrix<double> upperFace) GetTensor(bool toGlobal = false)
        {
            return (_lowerFace.GetTensor(toGlobal), _midFace.GetTensor(toGlobal), _upperFace.GetTensor(toGlobal));
        }

        /// <summary>
        /// The same result in another coordinate system (the tensors are rotated)
        /// </summary>
        /// <param name="coordinateSystem">The new coordinate system</param>
        /// <returns>The new result with the same name (the id is lost)</returns>
        public ResultPlateStress ToCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            return new ResultPlateStress(coordinateSystem, _lowerFace.ToCoordinateSystem(coordinateSystem),
                _midFace.ToCoordinateSystem(coordinateSystem), _upperFace.ToCoordinateSystem(coordinateSystem), _name);
        }

        /// <summary>
        /// The Von Mises stresses of the faces (not used)
        /// </summary>
        /// <returns>The Von Mises stresses of the three faces</returns>
        private (double lowerFace, double midFace, double upperFace) GetVMStress()
        {
            return (_lowerFace.SVM, _midFace.SVM, _upperFace.SVM);
        }

		#endregion

		#region Equals, hascode, operators

		/// <summary>
		/// Equality with another result (see <see cref="Equals(ResultPlateStress)"/>)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal result</returns>
		public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as ResultPlateStress);
        }

        /// <summary>
        /// Equality of the stresses of the faces and of the name
        /// </summary>
        /// <param name="other">The result to compare</param>
        /// <returns>True if the results are equal</returns>
        public bool Equals(ResultPlateStress other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._lowerFace.Equals(_lowerFace)
                                    && other._midFace.Equals(_midFace)
                                    && other._upperFace.Equals(_upperFace)
                                    && base.Equals(other);
        }

        /// <summary>
        /// The hash code of the name and of the faces
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -19 + base.GetHashCode();
                hashCode = hashCode * -19 + _lowerFace.GetHashCode();
                hashCode = hashCode * -19 + _midFace.GetHashCode();
                hashCode = hashCode * -19 + _upperFace.GetHashCode();
                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(ResultPlateStress)"/>; a null <paramref name="left"/> throws <see cref="NullReferenceException"/>)
        /// </summary>
        /// <param name="left">The first result</param>
        /// <param name="right">The second result</param>
        /// <returns>True if the results are equal</returns>
        public static bool operator ==(ResultPlateStress left, ResultPlateStress right)
        {
            return left.Equals(right);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(ResultPlateStress)"/>)
        /// </summary>
        /// <param name="left">The first result</param>
        /// <param name="right">The second result</param>
        /// <returns>True if the results are different</returns>
        public static bool operator !=(ResultPlateStress left, ResultPlateStress right)
        {
            return !(left == right);
        }

        /// <summary>
        /// The sum of the stresses face by face (see <see cref="ResultStress"/> operator +); the name joins the two names
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>The sum of the two stress tensor written in the <paramref name="obj1"/> <see cref="ResultType.CoordinateSystem"/></returns>
        /// <exception cref="ArgumentNullException">If an operand is null</exception>
        public static ResultPlateStress operator +(ResultPlateStress obj1, ResultPlateStress obj2)
        {
            if (obj1 is null || obj2 is null)
                throw new ArgumentNullException();

			var hashset = new HashSet<string>(new string[] { obj1.Name, obj2.Name });

			return new ResultPlateStress(obj1.CoordinateSystem, obj1.LowerFace + obj2.LowerFace,
                                                                obj1.MidFace + obj2.MidFace,
                                                                obj1.UpperFace + obj2.UpperFace,
                                                                string.Join(" ", hashset)
                                                                );
        }

        /// <summary>
        /// The difference of the stresses face by face (see <see cref="ResultStress"/> operator -); the name joins the two names
        /// </summary>
        /// <param name="obj1">The first result</param>
        /// <param name="obj2">The second result</param>
        /// <returns>The difference of the two stress tensor written in the <paramref name="obj1"/> <see cref="ResultType.CoordinateSystem"/></returns>
        /// <exception cref="ArgumentNullException">If an operand is null</exception>
        public static ResultPlateStress operator -(ResultPlateStress obj1, ResultPlateStress obj2)
        {

            if (obj1 is null || obj2 is null)
                throw new ArgumentNullException();

			var hashset = new HashSet<string>(new string[] { obj1.Name, obj2.Name });

			return new ResultPlateStress(obj1.CoordinateSystem, obj1.LowerFace - obj2.LowerFace,
                                                                obj1.MidFace - obj2.MidFace,
                                                                obj1.UpperFace - obj2.UpperFace,
                                                                string.Join(" ", hashset)
                                                                );
        }

        /// <summary>
        /// The stresses of each face transformed by a matrix (see <see cref="ResultStress"/> operator *)
        /// </summary>
        /// <param name="obj1">The result</param>
        /// <param name="matrix">The matrix</param>
        /// <returns>This will produce the multipltication of <paramref name="obj1"/> Tensor in global coordinate by <paramref name="matrix"/>. M * T * M^t</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="obj1"/> is null</exception>
        public static ResultPlateStress operator *(ResultPlateStress obj1, Matrix<double> matrix)
        {

            if (obj1 is null)
                throw new ArgumentNullException();

            return new ResultPlateStress(obj1.CoordinateSystem, obj1.LowerFace * matrix,
                                                                obj1.MidFace * matrix,
                                                                obj1.UpperFace * matrix,
                                                                obj1.Name
                                                                );
        }

        /// <summary>
        /// Returns a <see cref="ResultPlateStress"/> that represent the arithmetic mean between the <paramref name="values"/> (the three faces
        /// in parallel). The result has the mean of the LOWER face also as middle and upper faces
        /// </summary>
        /// <param name="values">The results</param>
        /// <returns>The mean result, with the coordinate system and the name of the first one</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="values"/> is null</exception>
        public static async Task<ResultPlateStress> GetArithmeticMeanAsync(ResultPlateStress[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            Func<ResultStress[], ResultStress> func = new Func<ResultStress[], ResultStress>((value) =>
            {
                return ResultStress.GetArithmeticMean(value);
            });


            var task1 = Task.Factory.StartNew(() => func(values.Select(i => i.LowerFace).ToArray()));
            var task2 = Task.Factory.StartNew(() => func(values.Select(i => i.MidFace).ToArray()));
            var task3 = Task.Factory.StartNew(() => func(values.Select(i => i.UpperFace).ToArray()));

            await Task.WhenAll(task1, task2, task3);

            return new ResultPlateStress(values.First().CoordinateSystem, task1.Result, task1.Result, task1.Result, values.First().Name);
        }

        /// <summary>
        /// Returns a <see cref="ResultPlateStress"/> that represent the arithmetic mean between the <paramref name="values"/> (see
        /// <see cref="ResultStress.GetArithmeticMean(ResultStress[])"/>)
        /// </summary>
        /// <param name="values">The results</param>
        /// <returns>The mean result, with the coordinate system and the name of the first one</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="values"/> is null</exception>
        public static ResultPlateStress GetArithmeticMean(ResultPlateStress[] values)
        {
            if (values is null)
            {
                throw new ArgumentNullException(nameof(values));
            }

            return new ResultPlateStress(values.First().CoordinateSystem, ResultStress.GetArithmeticMean(values.Select(i => i.LowerFace).ToArray()),
                                                                          ResultStress.GetArithmeticMean(values.Select(i => i.MidFace).ToArray()),
                                                                          ResultStress.GetArithmeticMean(values.Select(i => i.UpperFace).ToArray()),
                                                                          values.First().Name);
        }

        #endregion
    }
}
