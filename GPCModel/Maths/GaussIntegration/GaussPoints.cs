using System.Runtime.Serialization;

namespace GPC.Model.Maths.GaussIntegrations
{
	public class GaussPoint : ModelObjectId
	{
		protected readonly double _csi;
		protected readonly double _eta;
		protected readonly double _zeta;
		protected readonly double _weight;

		public double Csi => _csi;
		public double Eta => _eta;
		public double Zeta => _zeta;
		public double Weight => _weight;

		public GaussPoint(double csi, double eta, double zeta, double weight, int id = IDUNASSIGNED)
			: base(id)
		{
			_csi = csi;
			_eta = eta;
			_zeta = zeta;
			_weight = weight;
		}

		public GaussPoint(double csi, double eta, double weight, int id = IDUNASSIGNED)
			: this(csi, eta, 0.0, weight, id)
		{

		}

		public GaussPoint(double csi, double weight, int id = IDUNASSIGNED)
			: this(csi, 0.0, 0.0, weight, id)
		{

		}

		public GaussPoint(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_csi = info.GetDouble("Csi");
			_eta = info.GetDouble("Eta");
			_zeta = info.GetDouble("Zeta");
			_weight = info.GetDouble("Weight");
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("Csi", _csi);
			info.AddValue("Eta", _eta);
			info.AddValue("Zeta", _zeta);
			info.AddValue("Weight", _weight);
		}

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
