using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the CNR-DT 204/2006  
	/// "Istruzioni per la Progettazione, l’Esecuzione ed il Controllo di Strutture di Calcestruzzo Fibrorinforzato"
	/// </summary>
	/// <remarks>Reference: CNR-DT 204/2006. AC:2008</remarks>
	[Serializable]
	public class StandardCNR204 : StandardModelCode2010, ISerializable
	{
		/// <summary>
		/// Default Constructor
		/// </summary>
		public StandardCNR204(string name = "CNR-DT 204/2006", string remarks = "CNR-DT 204/2006. AC:2008")
			: base(name, remarks)
		{

		}

		/// <summary>
		/// Creates the standard with the default remarks
		/// </summary>
		/// <param name="name">The name</param>
		public StandardCNR204(string name = "CNR-DT 204/2006")
			: base(name, "CNR-DT 204/2006. AC:2008")
		{

		}

		/// <summary>
		/// Creates the standard with the default name and remarks
		/// </summary>
		public StandardCNR204()
			: base("CNR-DT 204/2006", "CNR-DT 204/2006. AC:2008")
		{

		}

		/// <summary>
		/// Deserialization constructor
		/// </summary>
		/// <param name="info">The serialization data</param>
		/// <param name="context">The serialization context</param>
		protected StandardCNR204(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

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
		/// Equality with an object of the same type
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
