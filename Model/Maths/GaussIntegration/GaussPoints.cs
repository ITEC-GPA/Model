using System.Runtime.Serialization;

namespace GPC.Model.Maths.GaussIntegrations
{
	/// <summary>
	/// A point of a Gauss integration rule: natural coordinates (csi, eta, zeta) in the reference element and weight
	/// </summary>
	public class GaussPoint : ModelObjectId
	{
		/// <summary>
		/// The first natural coordinate
		/// </summary>
		protected readonly double _csi;
		/// <summary>
		/// The second natural coordinate
		/// </summary>
		protected readonly double _eta;
		/// <summary>
		/// The third natural coordinate
		/// </summary>
		protected readonly double _zeta;
		/// <summary>
		/// The weight
		/// </summary>
		protected readonly double _weight;

		/// <summary>
		/// The first natural coordinate
		/// </summary>
		public double Csi => _csi;
		/// <summary>
		/// The second natural coordinate (0 for the lines)
		/// </summary>
		public double Eta => _eta;
		/// <summary>
		/// The third natural coordinate (0 for the lines and the surfaces)
		/// </summary>
		public double Zeta => _zeta;
		/// <summary>
		/// The weight
		/// </summary>
		public double Weight => _weight;

		/// <summary>
		/// Creates a point of a volume
		/// </summary>
		/// <param name="csi">The first natural coordinate</param>
		/// <param name="eta">The second natural coordinate</param>
		/// <param name="zeta">The third natural coordinate</param>
		/// <param name="weight">The weight</param>
		/// <param name="id">The number of the point in the rule</param>
		public GaussPoint(double csi, double eta, double zeta, double weight, int id = IDUNASSIGNED)
			: base(id)
		{
			_csi = csi;
			_eta = eta;
			_zeta = zeta;
			_weight = weight;
		}

		/// <summary>
		/// Creates a point of a surface (zeta = 0)
		/// </summary>
		/// <param name="csi">The first natural coordinate</param>
		/// <param name="eta">The second natural coordinate</param>
		/// <param name="weight">The weight</param>
		/// <param name="id">The number of the point in the rule</param>
		public GaussPoint(double csi, double eta, double weight, int id = IDUNASSIGNED)
			: this(csi, eta, 0.0, weight, id)
		{

		}

		/// <summary>
		/// Creates a point of a line (eta = zeta = 0)
		/// </summary>
		/// <param name="csi">The natural coordinate</param>
		/// <param name="weight">The weight</param>
		/// <param name="id">The number of the point in the rule</param>
		public GaussPoint(double csi, double weight, int id = IDUNASSIGNED)
			: this(csi, 0.0, 0.0, weight, id)
		{

		}

		/// <summary>
		/// Deserialization constructor: reads the data of <see cref="ModelObjectId"/>, coordinates and weight
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public GaussPoint(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_csi = info.GetDouble("Csi");
			_eta = info.GetDouble("Eta");
			_zeta = info.GetDouble("Zeta");
			_weight = info.GetDouble("Weight");
		}

		/// <summary>
		/// Serializes the data of <see cref="ModelObjectId"/>, coordinates and weight
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Csi", _csi);
			info.AddValue("Eta", _eta);
			info.AddValue("Zeta", _zeta);
			info.AddValue("Weight", _weight);
		}

		/// <summary>
		/// Equality of name, coordinates and weight (exact)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is an equal point</returns>
		public override bool Equals(object obj)
		{
			if (obj is null)
				return false;

			if (ReferenceEquals(this, obj))
				return true;

			return obj is GaussPoint point &&
				   base.Equals(obj) &&
				   _csi == point._csi &&
				   _eta == point._eta &&
				   _zeta == point._zeta &&
				   _weight == point._weight;
		}

		/// <summary>
		/// The hash code of name, coordinates and weight
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				hashCode = hashCode * -17 + _csi.GetHashCode();
				hashCode = hashCode * -17 + _eta.GetHashCode();
				hashCode = hashCode * -17 + _zeta.GetHashCode();
				hashCode = hashCode * -17 + _weight.GetHashCode();
				return hashCode;
			}
		}
	}
}
