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
		public StandardCNR204()
		{

		}

		protected StandardCNR204(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{

		}
	}
}
