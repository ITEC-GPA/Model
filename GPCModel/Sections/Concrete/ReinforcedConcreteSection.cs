using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Concrete
{
	public class ReinforcedConcreteSection : Section, IConcreteSection
	{
		#region Variables

		protected ShapeEx _shapeEx;
		protected ReinforcedConcreteRebar[] _rebars;

		#endregion


		#region Properties

		public ShapeEx Shape => _shapeEx;

		public ReinforcedConcreteRebar[] Rebars => _rebars;

		public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

		#endregion


		#region Public Constructors

		public ReinforcedConcreteSection(ShapeEx shapeEx, ReinforcedConcreteRebar[] rebars, string name = "")
			: base(name)
		{
			_shapeEx = shapeEx;
			_rebars = rebars;

			#region Input check

			Plane pln = _shapeEx.GetPlane();

			foreach(ReinforcedConcreteRebar rebar in _rebars)
			{
				if (!pln.IsPointOnPlane(rebar.Position))
					throw new ArgumentException("Rebars must be on the plane of the section");
				if(!_shapeEx.IsPointInside(rebar.Position))
					throw new ArgumentException("Rebars must be inside the section");
			}

			#endregion
		}

		#endregion

		public ReinforcedConcreteSection(SerializationInfo info, StreamingContext context) :
			base(info, context)
		{
			_shapeEx = (ShapeEx)info.GetValue("ShapeEx", typeof(ShapeEx));
			_rebars = (ReinforcedConcreteRebar[])info.GetValue("ReinforcedConcreteRebar", typeof(ReinforcedConcreteRebar[]));
		}



		#region Field Serialization

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("ShapeEx", _shapeEx);
			info.AddValue("ReinforcedConcreteRebar", _rebars);
		}

		#endregion

		

		//TODO: implementare metodi di calcolo della sezione
	}
}
