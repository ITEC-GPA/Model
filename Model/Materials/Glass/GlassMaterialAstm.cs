using GPC.Utilities.Attributes;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Materials
{
	/// <summary>
	/// Glass material for the ASTM standard, according to NCSEA - Engineering structural glass design guide: base strengths of surface and
	/// edge corrected for load duration and probability of breakage
	/// </summary>
	[Serializable]
	[UI(Description = "Glass ASTM", Group = "Materials", Kind = "Material")]
	public sealed class GlassMaterialAstm : GlassMaterial, IEquatable<GlassMaterialAstm>
	{
		#region VARIABLES

		/// <summary>
		/// The coefficient ψ of the surface
		/// </summary>
		private double _psiSurface;
		/// <summary>
		/// The exponent n of the static fatigue (load duration)
		/// </summary>
		private double _nGlassCoefficient;
		/// <summary>
		/// The base strength of the surface
		/// </summary>
		private double _surfaceBaseStress;
		/// <summary>
		/// The base strength of the edge
		/// </summary>
		private double _surfaceBaseEdgeStress;
		/// <summary>
		/// The probability of breakage
		/// </summary>
		private double _probabiltyOfBreakage;

		#endregion

		#region PROPERTIES

		/// <summary>
		/// The coefficient ψ of the surface according to NCSEA
		/// </summary>
		public double PsiSurface { get => _psiSurface; set => _psiSurface = value; }
		/// <summary>
		/// The exponent n of the static fatigue according to NCSEA
		/// </summary>
		public double NGlassCoefficient { get => _nGlassCoefficient; set => _nGlassCoefficient = value; }
		/// <summary>
		/// The base strength of the surface [MPa]
		/// </summary>
		public double SurfaceBaseStress { get => _surfaceBaseStress; set => _surfaceBaseStress = value; }
		/// <summary>
		/// The base strength of the edge [MPa]
		/// </summary>
		public double SurfaceBaseEdgeStress { get => _surfaceBaseEdgeStress; set => _surfaceBaseEdgeStress = value; }
		/// <summary>
		/// The probability of breakage (from 0.001 to 0.01)
		/// </summary>
		public double ProbabiltyOfBreakage { get => _probabiltyOfBreakage; set => _probabiltyOfBreakage = value; }

		#endregion

		#region PUBLIC CONSTRUCTORS

		/// <summary>
		/// Constructor for glass material according to ASTM standard.
		/// </summary>
		/// <param name="name">The name of the material</param>
		/// <param name="elasticModulus">Elastic modulus of the glass [MPa]</param>
		/// <param name="poisson">Poisson's ratio of the glass</param>
		/// <param name="psiSurface">ψ coefficient of surface according to NCSEA</param>
		/// <param name="nGlassCoefficient">Exponent n of the static fatigue according to NCSEA</param>
		/// <param name="surfaceBaseStress">Surface base stress according to NCSEA [MPa]</param>
		/// <param name="surfaceBaseEdgeStress">Surface base edge stress according to NCSEA [MPa]</param>
		/// <param name="probabiltyOfBreakage">Probability of breakage according to NCSEA</param>
		/// <param name="density">Density of the material [T/mm^3]</param>
		/// <param name="alfaThermalExpansion">Alfa linear thermal expansion coefficient</param>
		/// <exception cref="ArgumentException">If a coefficient or a strength is not greater than 0.001 or the probability is out of [0.001, 0.01]</exception>
		public GlassMaterialAstm(string name, double elasticModulus, double poisson, double psiSurface, double nGlassCoefficient,
			double surfaceBaseStress, double surfaceBaseEdgeStress, double probabiltyOfBreakage, double density, double alfaThermalExpansion)
			: base(name, elasticModulus, poisson, density, alfaThermalExpansion)
		{
			if (psiSurface <= 0.001)
			{
				throw new ArgumentException($"{nameof(psiSurface)} cannot be zero or lower");
			}
			if (nGlassCoefficient <= 0.001)
			{
				throw new ArgumentException($"{nameof(nGlassCoefficient)} cannot be zero or lower");
			}
			if (surfaceBaseStress <= 0.001)
			{
				throw new ArgumentException($"{nameof(surfaceBaseStress)} cannot be zero or lower");
			}
			if (surfaceBaseEdgeStress <= 0.001)
			{
				throw new ArgumentException($"{nameof(surfaceBaseEdgeStress)} cannot be zero or lower");
			}

			if (probabiltyOfBreakage < 0.001)
			{
				throw new ArgumentException($"{nameof(probabiltyOfBreakage)} cannot be lower than 0.001");
			}
			else if (probabiltyOfBreakage > 0.01)
			{
				throw new ArgumentException($"{nameof(probabiltyOfBreakage)} cannot be greater than 0.01");
			}

			this._psiSurface = psiSurface;
			this._nGlassCoefficient = nGlassCoefficient;
			this._surfaceBaseStress = surfaceBaseStress;
			this._surfaceBaseEdgeStress = surfaceBaseEdgeStress;
			this._probabiltyOfBreakage = probabiltyOfBreakage;
		}

		/// <summary>
		/// Deserialization constructor: reads the data of <see cref="Material"/> and the coefficients
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		private GlassMaterialAstm(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_psiSurface = info.GetDouble("PsiSurface");
			_nGlassCoefficient = info.GetDouble("NGlassCoefficient");
			_surfaceBaseStress = info.GetDouble("SigmaBase");
			_surfaceBaseEdgeStress = info.GetDouble("SigmaBaseEdge");
			_probabiltyOfBreakage = info.GetDouble("ProbabiltyOfBreakage");
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// The factor of the load duration: 1 / (t / 3)^(1 / n), with t at least 3 seconds
		/// </summary>
		/// <param name="loadDuration">The load duration [seconds]</param>
		/// <returns>The factor</returns>
		/// <exception cref="ArgumentException">If n is zero</exception>
		private double GetLoadDurationFactor(double loadDuration)
		{
			if (_nGlassCoefficient == 0)
				throw new ArgumentException();

			if (loadDuration < 3.00)
				loadDuration = 3;

			return 1.0 / Math.Pow(loadDuration / 3.0, 1.0 / _nGlassCoefficient);
		}

		/// <summary>
		/// The factor of the probability of breakage: (P / 0.008)^(1 / 7)
		/// </summary>
		/// <returns>The factor</returns>
		private double GetProbabiltyOfBreakageFactor()
		{
			return Math.Pow(_probabiltyOfBreakage / 0.008, 1.0 / 7.0);
		}

		#endregion

		#region Public method override 

		/// <summary>
		/// The strength of the glass: base strength (surface or edge) × load duration factor × probability factor × ψ
		/// </summary>
		/// <param name="edgeResistance">if true give the resistance on edge</param>
		/// <param name="loadDuration">load duration [seconds]</param>
		/// <returns>The glass resistance according to NCSEA §3.5</returns>
		/// <exception cref="ArgumentException">If <paramref name="loadDuration"/> is lower than zero</exception>
		public override double GetGlassResistance(bool edgeResistance, double loadDuration)
		{
			if (loadDuration < 0)
				throw new ArgumentException("Load duration lower than zero");

			double loadDurationFactor = GetLoadDurationFactor(loadDuration);
			double probabiltyOfBreakageFactor = GetProbabiltyOfBreakageFactor();

			if (edgeResistance)
				return _surfaceBaseEdgeStress * loadDurationFactor * probabiltyOfBreakageFactor * _psiSurface;

			return _surfaceBaseStress * loadDurationFactor * probabiltyOfBreakageFactor * _psiSurface;
		}


		#endregion

		#region Equals - haschode - operators - serialization

		/// <summary>
		/// Serializes the data of <see cref="Material"/> and the coefficients
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("PsiSurface", _psiSurface);
			info.AddValue("NGlassCoefficient", _nGlassCoefficient);
			info.AddValue("SigmaBase", _surfaceBaseStress);
			info.AddValue("SigmaBaseEdge", _surfaceBaseEdgeStress);
			info.AddValue("ProbabiltyOfBreakage", _probabiltyOfBreakage);
		}

		/// <summary>
		/// Equality of the coefficients and the data of <see cref="Material"/>
		/// </summary>
		/// <param name="other">The glass to compare</param>
		/// <returns>True if the materials are equal</returns>
		public bool Equals(GlassMaterialAstm other)
		{
			if (ReferenceEquals(this, other))
				return true;
			return !(other is null) &&
				other._psiSurface.Equals(_psiSurface) &&
				other._nGlassCoefficient.Equals(_nGlassCoefficient) &&
				other._surfaceBaseStress.Equals(_surfaceBaseStress) &&
				other._surfaceBaseEdgeStress.Equals(_surfaceBaseEdgeStress) &&
				other._probabiltyOfBreakage.Equals(_probabiltyOfBreakage) &&
				base.Equals(other);
		}

		/// <summary>
		/// Equality with another object (see <see cref="Equals(GlassMaterialAstm)"/>)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal glass</returns>
		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;
			return Equals(obj as GlassMaterialAstm);
		}

		/// <summary>
		/// The hash code of the data of <see cref="Material"/> and of the coefficients
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				hashCode = hashCode * -17 + _psiSurface.GetHashCode();
				hashCode = hashCode * -17 + _nGlassCoefficient.GetHashCode();
				hashCode = hashCode * -17 + _surfaceBaseStress.GetHashCode();
				hashCode = hashCode * -17 + _surfaceBaseEdgeStress.GetHashCode();
				hashCode = hashCode * -17 + _probabiltyOfBreakage.GetHashCode();
				return hashCode;
			}
		}

		/// <summary>
		/// Equality operator (see <see cref="Equals(object)"/>); two null materials are equal
		/// </summary>
		/// <param name="obj1">The first material</param>
		/// <param name="obj2">The second material</param>
		/// <returns>True if the materials are equal</returns>
		public static bool operator ==(GlassMaterialAstm obj1, GlassMaterialAstm obj2)
		{
			if (ReferenceEquals(obj1, obj2))
				return true;

			if (obj1 is null || obj2 is null)
				return false;

			return obj1.Equals(obj2);
		}

		/// <summary>
		/// Inequality operator (see <see cref="Equals(object)"/>)
		/// </summary>
		/// <param name="obj1">The first material</param>
		/// <param name="obj2">The second material</param>
		/// <returns>True if the materials are different</returns>
		public static bool operator !=(GlassMaterialAstm obj1, GlassMaterialAstm obj2)
		{
			return !(obj1 == obj2);
		}
		#endregion
	}
}
