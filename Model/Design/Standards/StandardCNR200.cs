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
		/// <inheritdoc cref="GammaM_FRP"/>
		protected double _gammaM_FRP;

		/// <summary>
		/// The partial factor of the FRP material (CNR-DT 200 R1/2013)
		/// </summary>
		public double GammaM_FRP { get => _gammaM_FRP; set => _gammaM_FRP = value; }


		/// <summary>
		/// Default Constructor
		/// </summary>
		public StandardCNR200(string name = "CNR-DT 200 R1/2013", string remarks = "CNR-DT 200 R1/2013. AC:2014")
			: base(name, remarks)
		{

		}

		/// <summary>
		/// Creates the standard with the default remarks
		/// </summary>
		/// <param name="name">The name</param>
		public StandardCNR200(string name = "CNR-DT 200 R1/2013")
			: base(name, "CNR-DT 200 R1/2013. AC:2014")
		{

		}

		/// <summary>
		/// Creates the standard with the default name and remarks
		/// </summary>
		public StandardCNR200()
			: base("CNR-DT 200 R1/2013", "CNR-DT 200 R1/2013. AC:2014")
		{

		}

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected StandardCNR200(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
			_gammaM_FRP = info.GetDouble("GammaM_FRP");
		}

		/// <summary>
		/// Serializes the object
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("GammaM_FRP", _gammaM_FRP);
		}

		/// <summary>
		/// The name of the standard
		/// </summary>
		/// <returns>The name</returns>
		public override string ToString()
		{
			return base.ToString();
		}

		/// <summary>
		/// Equality with a standard (it checks the type StandardCNR204: a StandardCNR200 is never equal to another one)
		/// </summary>
		/// <param name="obj">The object to compare</param>
		/// <returns>True if <paramref name="obj"/> is equal</returns>
		public override bool Equals(object obj)
		{
			return obj is StandardCNR204 standard && base.Equals(standard);
		}

		/// <summary>
		/// The hash code of the coefficients and of the base
		/// </summary>
		/// <returns>The hash code</returns>
		public override int GetHashCode()
		{
			return base.GetHashCode();
		}
	}
}
