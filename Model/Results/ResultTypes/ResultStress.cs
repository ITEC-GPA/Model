using GPC.Geometry;
using MathNet.Numerics.LinearAlgebra;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Results
{
	/// <summary>
	/// The stress tensor at a point in a coordinate system, with the principal and Von Mises stresses (calculated on demand)
	/// </summary>
	[Serializable]
	public sealed class ResultStress : ResultType, IEquatable<ResultStress>, ISerializable, IBrickResult, IResult<ResultStress>
	{
		#region Variables

		/// <summary>
		/// Normal stress xx (local)
		/// </summary>
		private readonly double _sxx;
		/// <summary>
		/// Normal stress yy (local)
		/// </summary>
		private readonly double _syy;
		/// <summary>
		/// Normal stress zz (local)
		/// </summary>
		private readonly double _szz;
		/// <summary>
		/// Shear stress xy (local)
		/// </summary>
		private readonly double _sxy;
		/// <summary>
		/// Shear stress xz (local)
		/// </summary>
		private readonly double _sxz;
		/// <summary>
		/// Shear stress yz (local)
		/// </summary>
		private readonly double _syz;

		/// <summary>
		/// True if the principal stresses have been calculated, to avoid calculating them twice (the values cannot be used: they could be zero).
		/// It does not tell if they have been calculated with the full or the simplified method
		/// </summary>
		private bool _principalStressCalculated;

		/// <summary>
		/// The first (largest) principal stress
		/// </summary>
		private double _s11;
		/// <summary>
		/// The second principal stress
		/// </summary>
		private double _s22;
		/// <summary>
		/// The third (smallest) principal stress
		/// </summary>
		private double _s33;

		/// <summary>
		/// True if the Von Mises stress has been calculated
		/// </summary>
		private bool _vonMisesStressCalculated;
		/// <summary>
		/// The Von Mises stress
		/// </summary>
		private double _vM;

		#endregion

		#region Properties

		/// <summary>
		/// Normal stress xx (local)
		/// </summary>
		public double Sxx => _sxx;
		/// <summary>
		/// Normal stress yy (local)
		/// </summary>
		public double Syy => _syy;
		/// <summary>
		/// Normal stress zz (local)
		/// </summary>
		public double Szz => _szz;
		/// <summary>
		/// Shear stress xy (local)
		/// </summary>
		public double Sxy => _sxy;
		/// <summary>
		/// Shear stress xz (local)
		/// </summary>
		public double Sxz => _sxz;
		/// <summary>
		/// Shear stress yz (local)
		/// </summary>
		public double Syz => _syz;

		/// <summary>
		/// The first (largest) principal stress (calculated with <see cref="CalculatePrincipalStressFullMethod"/> if not yet calculated)
		/// </summary>
		public double S11
		{
			get
			{
				if (!_principalStressCalculated)
					CalculatePrincipalStressFullMethod();

				return _s11;
			}
		}

		/// <summary>
		/// The second principal stress (calculated with <see cref="CalculatePrincipalStressFullMethod"/> if not yet calculated)
		/// </summary>
		public double S22
		{
			get
			{
				if (!_principalStressCalculated)
					CalculatePrincipalStressFullMethod();

				return _s22;
			}
		}

		/// <summary>
		/// The third (smallest) principal stress (calculated with <see cref="CalculatePrincipalStressFullMethod"/> if not yet calculated)
		/// </summary>
		public double S33
		{
			get
			{
				if (!_principalStressCalculated)
					CalculatePrincipalStressFullMethod();

				return _s33;
			}
		}

		/// <summary>
		/// The Von Mises stress, from the principal stresses (calculated once)
		/// </summary>
		public double SVM
		{
			get
			{
				if (!_vonMisesStressCalculated)
					_vM = GetVMStress();

				return _vM;
			}
		}

		#endregion

		#region Public Constructors

		/// <summary>
		/// Creates the stresses
		/// </summary>
		/// <param name="coordinateSystem">The coordinate system of the components</param>
		/// <param name="sxx">Stress on <see cref="CoordinateSystem.V1"/> side along <see cref="CoordinateSystem.V1"/> direction</param>
		/// <param name="syy">Stress on <see cref="CoordinateSystem.V2"/> side along <see cref="CoordinateSystem.V2"/> direction</param>
		/// <param name="szz">Stress on <see cref="CoordinateSystem.V3"/> side along <see cref="CoordinateSystem.V3"/> direction</param>
		/// <param name="sxy">Stress on <see cref="CoordinateSystem.V1"/> side along <see cref="CoordinateSystem.V2"/> direction</param>
		/// <param name="sxz">Stress on <see cref="CoordinateSystem.V1"/> side along <see cref="CoordinateSystem.V3"/> direction</param>
		/// <param name="syz">Stress on <see cref="CoordinateSystem.V2"/> side along <see cref="CoordinateSystem.V3"/> direction</param>
		/// <param name="name">The name</param>
		/// <param name="id">The id</param>
		/// <exception cref="ArgumentNullException">If <paramref name="coordinateSystem"/> is null</exception>
		public ResultStress(CoordinateSystem coordinateSystem, double sxx, double syy, double szz, double sxy, double sxz, double syz, string name = "", int id = ModelObjectId.IDUNASSIGNED)
			: base(coordinateSystem, name, id)
		{
			_sxx = sxx;
			_syy = syy;
			_szz = szz;
			_sxy = sxy;
			_sxz = sxz;
			_syz = syz;
		}

		/// <summary>
		/// Deserialization constructor (the principal stresses are calculated again on demand)
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		private ResultStress(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_sxx = (double)info.GetValue("Sxx", typeof(double));
			_syy = (double)info.GetValue("Syy", typeof(double));
			_szz = (double)info.GetValue("Szz", typeof(double));
			_sxy = (double)info.GetValue("Sxy", typeof(double));
			_sxz = (double)info.GetValue("Sxz", typeof(double));
			_syz = (double)info.GetValue("Syz", typeof(double));
		}

		#endregion

		#region Public Methods  

		/// <summary>
		/// Serializes the stresses (the six components)
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Sxx", _sxx);
			info.AddValue("Syy", _syy);
			info.AddValue("Szz", _szz);
			info.AddValue("Sxy", _sxy);
			info.AddValue("Sxz", _sxz);
			info.AddValue("Syz", _syz);
		}

		#endregion

		#region Public method - Stresses

		/// <summary>
		/// Calculate the Principal stresses in the plane xy (<see cref="S33"/> = 0)
		/// <para>This method use an approximate solution.</para>
		/// <para>If <see cref="Szz"/>, <see cref="Sxz"/> and <see cref="Syz"/> are relevant then the
		/// <seealso cref="CalculatePrincipalStressFullMethod"/> must be used</para>
		/// <para>If <see cref="Szz"/>, <see cref="Sxz"/> and <see cref="Syz"/> are 0 this method gives the exact solution</para>
		/// </summary>
		internal void CalculatePrincipalStressSimplifiedMethod()
		{
			_s11 = ((_sxx + _syy) / 2.0) + Math.Sqrt((Math.Pow((_sxx - _syy), 2.0) / 4.0) + Math.Pow(_sxy, 2.0));
			_s22 = ((_sxx + _syy) / 2.0) - Math.Sqrt((Math.Pow((_sxx - _syy), 2.0) / 4.0) + Math.Pow(_sxy, 2.0));
			_s33 = 0;

			_principalStressCalculated = true;
		}

		/// <summary>
		/// Calculates the principal stresses as the eigenvalues of the tensor (the simplified method if <see cref="Szz"/>, <see cref="Sxz"/> and
		/// <see cref="Syz"/> are 0)
		/// </summary>
		public void CalculatePrincipalStressFullMethod()
		{

			if (_sxz == 0 && _syz == 0 && _szz == 0)
			{
				CalculatePrincipalStressSimplifiedMethod();
				_s33 = 0;
			}
			else
			{
				Matrix<double> m = CreateMatrix.Dense<double>(3, 3);
				m[0, 0] = _sxx;
				m[1, 1] = _syy;
				m[2, 2] = _szz;

				m[0, 1] = _sxy;
				m[1, 0] = _sxy;

				m[0, 2] = _sxz;
				m[2, 0] = _sxz;

				m[1, 2] = _syz;
				m[2, 1] = _syz;

				MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> eigen = m.Evd();

				_s11 = eigen.EigenValues[2].Real;
				_s22 = eigen.EigenValues[1].Real;
				_s33 = eigen.EigenValues[0].Real;
			}

			_principalStressCalculated = true;
		}

		/// <summary>
		/// Calculates the Von Mises stress from the principal stresses
		/// </summary>
		/// <returns>The Von Mises stress</returns>
		private double GetVMStress()
		{
			double svm;

			if (S33 == 0)
				svm = Math.Sqrt(Math.Pow(S11, 2.0) + Math.Pow(S22, 2.0) - (S22 * S11));
			else
				svm = Math.Sqrt(0.5 * (Math.Pow(S11 - S22, 2.0) + Math.Pow(S22 - S33, 2.0) + Math.Pow(S33 - S11, 2.0)));

			_vonMisesStressCalculated = true;
			return svm;
		}

		/// <summary>
		/// The stresses in global coordinates. The normal stresses (sxx, syy, 0) and the shear (0, 0, sxy) are rotated as VECTORS, not as a tensor
		/// (see <see cref="GetTensor(bool)"/> for the correct transformation): the result is wrong for a rotated coordinate system
		/// </summary>
		/// <returns>Array with the rotated normal stresses X, Y, Z and the rotated shear X, Y, Z</returns>
		public double[] GetGlobalStress()
		{
			Vector3d SigmaResult = new Vector3d(_sxx, _syy, 0);
			Vector3d GlobalSigmaResult = _coordinateSystem.ToGlobal(SigmaResult);

			Vector3d TauResult = new Vector3d(0, 0, _sxy);
			Vector3d GlobalTauResult = _coordinateSystem.ToGlobal(TauResult);

			double[] globalstress = new double[6];

			globalstress[0] = GlobalSigmaResult.X;
			globalstress[1] = GlobalSigmaResult.Y;
			globalstress[2] = GlobalSigmaResult.Z;
			globalstress[3] = GlobalTauResult.X;
			globalstress[4] = GlobalTauResult.Y;
			globalstress[5] = GlobalTauResult.Z;

			return globalstress;
		}

		/// <summary>
		/// The stress tensor (3 x 3, symmetric)
		/// </summary>
		/// <param name="toGlobal">True for the global coordinate system (R T R^t), false for the local one</param>
		/// <returns>Return the stress tensor</returns>
		public Matrix<double> GetTensor(bool toGlobal = false)
		{
			if (toGlobal)
			{
				Matrix<double> stress = Matrix<double>.Build.Sparse(3, 3);
				stress[0, 0] = _sxx;
				stress[0, 1] = _sxy;
				stress[0, 2] = _sxz;
				stress[1, 0] = _sxy;
				stress[1, 1] = _syy;
				stress[1, 2] = _syz;
				stress[2, 0] = _sxz;
				stress[2, 1] = _syz;
				stress[2, 2] = _szz;

				return _coordinateSystem.TrfMatrix.Resize(3, 3) * stress * _coordinateSystem.TrfMatrix.Resize(3, 3).Transpose();
			}
			else
			{
				Matrix<double> stress = Matrix<double>.Build.Sparse(3, 3);
				stress[0, 0] = _sxx;
				stress[0, 1] = _sxy;
				stress[0, 2] = _sxz;
				stress[1, 0] = _sxy;
				stress[1, 1] = _syy;
				stress[1, 2] = _syz;
				stress[2, 0] = _sxz;
				stress[2, 1] = _syz;
				stress[2, 2] = _szz;

				return stress;
			}
		}

		/// <summary>
		/// The same stresses in another coordinate system (the tensor is rotated)
		/// </summary>
		/// <param name="coordinateSystem">The new coordinate system</param>
		/// <returns>The new stresses (the name and the id are lost)</returns>
		public ResultStress ToCoordinateSystem(CoordinateSystem coordinateSystem)
		{
			var globalTensor = GetTensor(true);

			var rotatedTensor = coordinateSystem.TrfMatrix.Resize(3, 3).Transpose() * (globalTensor) * coordinateSystem.TrfMatrix.Resize(3, 3);

			return new ResultStress(coordinateSystem, rotatedTensor[0, 0], rotatedTensor[1, 1], rotatedTensor[2, 2], rotatedTensor[0, 1], rotatedTensor[0, 2], rotatedTensor[1, 2]);
		}

		#endregion

		#region Equals, hashcode, operators

		/// <summary>
		/// Equality with another result (see <see cref="Equals(ResultStress)"/>)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> are equal stresses</returns>
		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			return Equals(obj as ResultStress);
		}

		/// <summary>
		/// Exact equality of the six components and of the name (the coordinate system is not compared)
		/// </summary>
		/// <param name="other">The stresses to compare</param>
		/// <returns>True if the stresses are equal</returns>
		public bool Equals(ResultStress other)
		{
			if (other is null)
				return false;

			if (ReferenceEquals(this, other))
				return true;

			return !(other is null) && _sxx == other._sxx && _syy == other._syy
									&& _szz == other._szz && _sxy == other._sxy
									&& _sxz == other._sxz && _syz == other._syz
									&& base.Equals(other);
		}

		/// <summary>
		/// The hash code of the name and of the six components
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				hashCode = hashCode * -17 + _sxx.GetHashCode();
				hashCode = hashCode * -17 + _syy.GetHashCode();
				hashCode = hashCode * -17 + _szz.GetHashCode();
				hashCode = hashCode * -17 + _sxy.GetHashCode();
				hashCode = hashCode * -17 + _sxz.GetHashCode();
				hashCode = hashCode * -17 + _syz.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Equality operator (see <see cref="Equals(ResultStress)"/>)
		/// </summary>
		/// <param name="obj1">The first stresses</param>
		/// <param name="obj2">The second stresses</param>
		/// <returns>True if the stresses are equal</returns>
		public static bool operator ==(ResultStress obj1, ResultStress obj2)
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
		/// Returns a <see cref="ResultStress"/> that represent the arithmetic mean between the <paramref name="values"/>, in the coordinate system
		/// of the first one. The components are averaged as they are, also if the coordinate systems are different (the check on the coordinate
		/// systems is always true: the branch that rotates the tensors is never used, and it would throw <see cref="ArgumentOutOfRangeException"/>)
		/// </summary>
		/// <param name="values">The stresses (not empty)</param>
		/// <returns>The mean stresses; the name joins the distinct names</returns>
		public static ResultStress GetArithmeticMean(ResultStress[] values)
		{
			IEnumerable<string> sss = values.Select(i => i.Name);
			HashSet<string> set = new HashSet<string>(sss);

			if (values.Select(i => i._coordinateSystem).Distinct().Count() > 0)
			{
				return new ResultStress(values[0]._coordinateSystem,
										GPC.Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.Sxx).ToArray()), // TODO: rimuovere toarray e metter ienumer
										GPC.Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.Syy).ToArray()),
										GPC.Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.Szz).ToArray()),
										GPC.Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.Sxy).ToArray()),
										GPC.Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.Sxz).ToArray()),
                                        GPC.Utilities.Maths.Averages.ArithmeticMean(values.Select(i => i.Syz).ToArray()),
										string.Join(" ", set.ToArray())
										);
			}
			else
			{
				var rotated = new List<ResultStress>
				{
					[0] = values[0]
				};

				rotated.AddRange(values.Skip(1).Select(i => i.ToCoordinateSystem(values[0]._coordinateSystem)));

				return new ResultStress(values[0]._coordinateSystem,
												GPC.Utilities.Maths.Averages.ArithmeticMean(rotated.Select(i => i.Sxx).ToArray()), // TODO: rimuovere toarray e metter ienumer
												GPC.Utilities.Maths.Averages.ArithmeticMean(rotated.Select(i => i.Syy).ToArray()),
												GPC.Utilities.Maths.Averages.ArithmeticMean(rotated.Select(i => i.Szz).ToArray()),
												GPC.Utilities.Maths.Averages.ArithmeticMean(rotated.Select(i => i.Sxy).ToArray()),
												GPC.Utilities.Maths.Averages.ArithmeticMean(rotated.Select(i => i.Sxz).ToArray()),
                                                GPC.Utilities.Maths.Averages.ArithmeticMean(rotated.Select(i => i.Syz).ToArray()),
												string.Join(" ", set.ToArray())
										);
			}
		}


		/// <summary>
		/// Inequality operator (see <see cref="Equals(ResultStress)"/>)
		/// </summary>
		/// <param name="obj1">The first stresses</param>
		/// <param name="obj2">The second stresses</param>
		/// <returns>True if the stresses are different</returns>
		public static bool operator !=(ResultStress obj1, ResultStress obj2)
		{
			return !(obj1 == obj2);
		}

		/// <summary>
		/// The sum of the stresses (the tensor of <paramref name="obj2"/> is rotated to the coordinate system of <paramref name="obj1"/>); the name
		/// joins the two names
		/// </summary>
		/// <param name="obj1">The first stresses</param>
		/// <param name="obj2">The second stresses</param>
		/// <returns>The sum of the two stress tensor written in the <paramref name="obj1"/> <see cref="ResultType.CoordinateSystem"/></returns>
		/// <exception cref="ArgumentNullException">If an operand is null</exception>
		public static ResultStress operator +(ResultStress obj1, ResultStress obj2)
		{
			if (obj1 is null || obj2 is null)
				throw new ArgumentNullException();

			var hashset = new HashSet<string>(new string[] { obj1.Name, obj2.Name });

			if (obj1._coordinateSystem.Equals(obj2._coordinateSystem))
			{

				return new ResultStress(obj1._coordinateSystem, obj1._sxx + obj2._sxx,
																obj1._syy + obj2._syy,
																obj1._szz + obj2._szz,
																obj1._sxy + obj2._sxy,
																obj1._sxz + obj2._sxz,
																obj1._syz + obj2._syz,
																string.Join(" ", hashset)
																);
			}
			else
			{
				// prendo tensori rotati nel globale
				// li sommo
				// li ruoto nel sistema obj1

				var sumRotated = obj1._coordinateSystem.TrfMatrix.Resize(3, 3).Transpose() * (obj1.GetTensor(true) + obj2.GetTensor(true)) * obj1._coordinateSystem.TrfMatrix.Resize(3, 3);

				return new ResultStress(obj1._coordinateSystem,
										sumRotated[0, 0],
										sumRotated[1, 1],
										sumRotated[2, 2],
										sumRotated[0, 1],
										sumRotated[0, 2],
										sumRotated[1, 2],
										string.Join(" ", hashset)
										);
			}
		}

		/// <summary>
		/// The difference of the stresses (the tensor of <paramref name="obj2"/> is rotated to the coordinate system of <paramref name="obj1"/>);
		/// the name joins the two names
		/// </summary>
		/// <param name="obj1">The first stresses</param>
		/// <param name="obj2">The second stresses</param>
		/// <returns>The difference of the two stress tensor written in the <paramref name="obj1"/> <see cref="ResultType.CoordinateSystem"/></returns>
		/// <exception cref="ArgumentNullException">If an operand is null</exception>
		public static ResultStress operator -(ResultStress obj1, ResultStress obj2)
		{
			if (obj1 is null || obj2 is null)
				throw new ArgumentNullException();

			var hashset = new HashSet<string>(new string[] { obj1.Name, obj2.Name });

			if (obj1._coordinateSystem.Equals(obj2._coordinateSystem))
			{
				return new ResultStress(obj1._coordinateSystem, obj1._sxx - obj2._sxx,
																obj1._syy - obj2._syy,
																obj1._szz - obj2._szz,
																obj1._sxy - obj2._sxy,
																obj1._sxz - obj2._sxz,
																obj1._syz - obj2._syz,
																string.Join(" ", hashset)
																);
			}
			else
			{
				// prendo tensori rotati nel globale
				// li sommo
				// li ruoto nel sistema obj1

				var sumRotated = obj1._coordinateSystem.TrfMatrix.Resize(3, 3).Transpose() * (obj1.GetTensor(true) - obj2.GetTensor(true)) * obj1._coordinateSystem.TrfMatrix.Resize(3, 3);

				return new ResultStress(obj1._coordinateSystem,
										sumRotated[0, 0],
										sumRotated[1, 1],
										sumRotated[2, 2],
										sumRotated[0, 1],
										sumRotated[0, 2],
										sumRotated[1, 2],
										string.Join(" ", hashset)
										);
			}
		}


		/// <summary>
		/// The global tensor transformed by a matrix, M T M^t; the result keeps the coordinate system of <paramref name="obj1"/>
		/// </summary>
		/// <param name="obj1">The stresses</param>
		/// <param name="matrix">The matrix (of rank 3)</param>
		/// <returns>This will produce the multipltication of <paramref name="obj1"/> Tensor in global coordinate by <paramref name="matrix"/>. M * T * M^t</returns>
		/// <exception cref="ArgumentNullException">If an operand is null</exception>
		/// <exception cref="NotSupportedException">If the rank of <paramref name="matrix"/> is not 3</exception>
		public static ResultStress operator *(ResultStress obj1, Matrix<double> matrix)
		{
			if (obj1 is null || matrix is null)
				throw new ArgumentNullException();

			if (matrix.Rank() != 3)
			{
				throw new NotSupportedException();
			}

			var rotated = matrix * obj1.GetTensor(true) * matrix.Transpose();

			return new ResultStress(obj1._coordinateSystem, rotated[0, 0], rotated[1, 1], rotated[2, 2], rotated[0, 1], rotated[0, 2], rotated[1, 2], obj1.Name);
		}

		#endregion
	}
}
