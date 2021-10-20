using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.GaussIntegration
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
		{
			_csi = csi;
			_eta = eta;
			_zeta = zeta;
			_weight = weight;
			_id = id;
		}

		public GaussPoint(double csi, double eta, double weight, int id = IDUNASSIGNED)
		{
			_csi = csi;
			_eta = eta;
			_zeta = -1;
			_weight = weight;
			_id = id;
		}

		public GaussPoint(double csi, double weight, int id = IDUNASSIGNED)
		{
			_csi = csi;
			_eta = -1;
			_zeta = -1;
			_weight = weight;
			_id = id;
		}
	}
}
