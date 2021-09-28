using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Sections.Rebar;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Concrete
{
	public class ReinforcedConcreteRebar : ModelObjectId
	{
		#region Variables

		protected readonly IRebarSection _rebarSection;
		protected readonly Point3d _position;
		protected readonly double _epsilonP;

		#endregion

		#region Properties

		public double Area => _rebarSection.Area;

		public RebarMaterial RebarMaterial => _rebarSection.RebarMaterial;

		public IRebarSection RebarSection => _rebarSection;

		public Point3d Position => _position;

		public double EpsilonP => _epsilonP;

		#endregion

		#region Public Constructors

		public ReinforcedConcreteRebar(IRebarSection section, Point3d position, double epsilonP, int id, Guid guid)
			: base(guid)
		{
			_rebarSection = section;
			_position = position;
			_epsilonP = epsilonP;
			_id = id;
		}

		public ReinforcedConcreteRebar(IRebarSection section, Point3d position, double epsilonP, int id)
			: this(section, position, epsilonP, id, new Guid())
		{

		}

		public ReinforcedConcreteRebar(IRebarSection section, Point3d position, int id = IDUNASSIGNED, double epsilonP = 0.0)
			: this(section, position, epsilonP, id, new Guid())
		{

		}

		#endregion
	}
}
