using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the CNR-DT 200 R1/2013   
	/// "Istruzioni  per la Progettazione, l’Esecuzione ed il Controllo di Interventi di Consolidamento Statico  mediante l’utilizzo di  Compositi Fibrorinforzati"
	/// </summary>
	/// <remarks>Reference: CNR-DT 200 R1/2013. AC:2014</remarks>
	[Serializable]
	public class StandardCNR200 : StandardNTC2018Concrete, ISerializable
	{
		protected double _gammaM_FRP;

		public double GammaM_FRP { get => _gammaM_FRP; set => _gammaM_FRP = value; }


		/// <summary>
		/// Default Constructor
		/// </summary>
		public StandardCNR200(string name = "CNR-DT 200 R1/2013", string remarks = "CNR-DT 200 R1/2013. AC:2014")
			: base(name, remarks)
		{

		}

		public StandardCNR200(string name = "CNR-DT 200 R1/2013")
			: base(name, "CNR-DT 200 R1/2013. AC:2014")
		{

		}

		public StandardCNR200()
			: base("CNR-DT 200 R1/2013", "CNR-DT 200 R1/2013. AC:2014")
		{

		}

		protected StandardCNR200(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_gammaM_FRP = info.GetDouble("GammaM_FRP");
		}

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("GammaM_FRP", _gammaM_FRP);
		}

		public override string ToString()
		{
			return base.ToString();
		}

		public override bool Equals(object obj)
		{
			return obj is StandardCNR204 standard && base.Equals(standard);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
