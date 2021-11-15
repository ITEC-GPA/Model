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
    public sealed class ResultPlateStress : ResultType, IEquatable<ResultPlateStress>, ISerializable, IPlateResult, IResult<ResultPlateStress>
    {

        private readonly ResultStress _lowerFace;
        private readonly ResultStress _midFace;
        private readonly ResultStress _upperFace;


        public ResultStress LowerFace => _lowerFace;
        public ResultStress MidFace => _midFace;
        public ResultStress UpperFace => _upperFace;




        public ResultPlateStress(CoordinateSystem coordinateSystem, ResultStress lowerFace, ResultStress midFace, ResultStress upperFace, string name = "")
            : base(coordinateSystem, name)
        {
            if (lowerFace.CoordinateSystem != midFace.CoordinateSystem || lowerFace.CoordinateSystem != upperFace.CoordinateSystem)
                throw new ArgumentException();


            _lowerFace = lowerFace ?? throw new ArgumentNullException(nameof(lowerFace));
            _midFace = midFace ?? throw new ArgumentNullException(nameof(midFace));
            _upperFace = upperFace ?? throw new ArgumentNullException(nameof(upperFace));
        }


        public ResultPlateStress(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _lowerFace = (ResultStress)info.GetValue("ResultStressLower", typeof(ResultStress));
            _midFace = (ResultStress)info.GetValue("ResultStressMid", typeof(ResultStress));
            _upperFace = (ResultStress)info.GetValue("ResultStressUpper", typeof(ResultStress));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("ResultStressLower", _coordinateSystem, typeof(ResultStress));
            info.AddValue("ResultStressMid", _coordinateSystem, typeof(ResultStress));
            info.AddValue("ResultStressUpper", _coordinateSystem, typeof(ResultStress));
        }


        /// <summary>
        /// Return the VonMises Stress
        /// </summary>
        private (double lowerFace, double midFace, double upperFace) GetVMStress()
        {
            return (_lowerFace.SVM, _midFace.SVM, _upperFace.SVM);
        }


        /// <inheritdoc cref="ResultStress.CalculatePrincipalStressSimplifiedMethod"/>
        public void CalculatePrincipalStressSimplifiedMethod()
        {
            _lowerFace.CalculatePrincipalStressSimplifiedMethod();
            _midFace.CalculatePrincipalStressSimplifiedMethod();
            _upperFace.CalculatePrincipalStressSimplifiedMethod();
        }


        /// <inheritdoc cref="ResultStress.CalculatePrincipalStressFullMethod"/>
        public void CalculatePrincipalStressFullMethod()
        {
            _lowerFace.CalculatePrincipalStressFullMethod();
            _midFace.CalculatePrincipalStressFullMethod();
            _upperFace.CalculatePrincipalStressFullMethod();
        }


        /// <summary>
        /// Return the stress of the point in global coordinate
        /// </summary>
        /// <returns>Tuple of Array of stress</returns>
        public (double[] lowerFace, double[] midFace, double[] upperFace) GetGlobalStress()
        {
            return (_lowerFace.GetGlobalStress(), _midFace.GetGlobalStress(), _upperFace.GetGlobalStress());
        }


        /// <returns>Return the stress tensor</returns>
        public (Matrix<double> lowerFace, Matrix<double> midFace, Matrix<double> upperFace) GetTensor(bool toGlobal = false)
        {
            return (_lowerFace.GetTensor(toGlobal), _midFace.GetTensor(toGlobal), _upperFace.GetTensor(toGlobal));
        }


        public ResultPlateStress ToCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            return new ResultPlateStress(coordinateSystem, _lowerFace.ToCoordinateSystem(coordinateSystem),
                                                           _midFace.ToCoordinateSystem(coordinateSystem),
                                                           _upperFace.ToCoordinateSystem(coordinateSystem),
                                                           _name
                );
        }


        #region Equals, hascode, operators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            return Equals(obj as ResultPlateStress);
        }

        public bool Equals(ResultPlateStress other)
        {
            if (ReferenceEquals(this, other))
                return true;

            return !(other is null) && other._lowerFace.Equals(_lowerFace)
                                    && other._midFace.Equals(_midFace)
                                    && other._upperFace.Equals(_upperFace)
                                    && base.Equals(other);
        }


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

        public static bool operator ==(ResultPlateStress left, ResultPlateStress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(ResultPlateStress left, ResultPlateStress right)
        {
            return !(left == right);
        }


        /// <returns>The sum of the two stress tensor written in the <paramref name="obj1"/> <see cref="CoordinateSystem"/></returns>
        public static ResultPlateStress operator +(ResultPlateStress obj1, ResultPlateStress obj2)
        {
            if (obj1 is null || obj2 is null)
                throw new ArgumentNullException();

            return new ResultPlateStress(obj1.CoordinateSystem, obj1.LowerFace + obj2.LowerFace,
                                                                obj1.MidFace + obj2.MidFace,
                                                                obj1.UpperFace + obj2.UpperFace,
                                                                string.Join(" ", new string[] { obj1.Name, obj2.Name }.ToHashSet())
                                                                );
        }


        public static ResultPlateStress operator -(ResultPlateStress obj1, ResultPlateStress obj2)
        {

            if (obj1 is null || obj2 is null)
                throw new ArgumentNullException();

            return new ResultPlateStress(obj1.CoordinateSystem, obj1.LowerFace - obj2.LowerFace,
                                                                obj1.MidFace - obj2.MidFace,
                                                                obj1.UpperFace - obj2.UpperFace,
                                                                string.Join(" ", new string[] { obj1.Name, obj2.Name }.ToHashSet())
                                                                );
        }



        /// <returns>This will produce the multipltication of <paramref name="obj1"/> Tensor in global coordinate by <paramref name="matrix"/>. M * T * M^t</returns>
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
        /// Returns a <see cref="ResultPlateStress"/> that represent the arithmetic mean between the <paramref name="values"/>
        /// </summary>
        // statico perchè è come se fosse un operatore
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
        /// Returns a <see cref="ResultPlateStress"/> that represent the arithmetic mean between the <paramref name="values"/>
        /// </summary>
        // statico perchè è come se fosse un operatore
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
