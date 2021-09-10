using GPC.Model.Elements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Concrete
{
	public class ReinforcedConcreteSection : Section
	{
		#region Variables

		protected ShapeEx _shapeEx;
		protected Rebars _rebars;

		#endregion


		#region Properties

		public ShapeEx Shape => _shapeEx;

		public Rebars Rebars => _rebars;

		#endregion


		#region Public Constructors

		public ReinforcedConcreteSection(ShapeEx shapeEx, Rebars rebars, string name = "")
			: base(name)
		{
			_shapeEx = shapeEx;
			_rebars = rebars;
		}

		#endregion

		//TODO: implementare metodi di calcolo della sezione
	}
}
