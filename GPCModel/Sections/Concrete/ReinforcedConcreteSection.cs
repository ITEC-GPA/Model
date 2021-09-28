using GPC.Model.Elements;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Concrete
{
	public class ReinforcedConcreteSection : Section, IConcreteSection
	{
		#region Variables

		protected ShapeEx _shapeEx;
		protected Elements.Rebar[] _rebars;

		#endregion


		#region Properties

		public ShapeEx Shape => _shapeEx;

		public Elements.Rebar[] Rebars => _rebars;

		public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

		#endregion


		#region Public Constructors

		public ReinforcedConcreteSection(ShapeEx shapeEx, Elements.Rebar[] rebars, string name = "")
			: base(name)
		{
			_shapeEx = shapeEx;
			_rebars = rebars;
		}

		#endregion

		//TODO: implementare metodi di calcolo della sezione
	}
}
