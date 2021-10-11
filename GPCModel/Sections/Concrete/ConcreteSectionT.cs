using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Concrete
{
	public class ConcreteSectionT : SectionT, IConcreteSection
	{
		#region Variables

		protected ReinforcedConcreteRebar[] _rebars;

		#endregion

		#region Properties

		public ReinforcedConcreteRebar[] Rebars => _rebars;

		public ConcreteMaterial ConcreteMaterial => (ConcreteMaterial)_material;

		public Shape Shape => throw new NotImplementedException();

		#endregion

		#region Public Constructors

		public ConcreteSectionT(double height, double flangeLength, double thicknessWeb, double thicknessFlange, ConcreteMaterial material, ReinforcedConcreteRebar[] rebars, string name = "")
			: base(height, flangeLength, thicknessWeb, thicknessFlange, material, name)
		{
			_rebars = rebars;
		}

		public ConcreteSectionT(SectionT section, ReinforcedConcreteRebar[] rebars)
			: base(section)
		{
			_rebars = rebars;

			if (section.Material.GetType() != ConcreteMaterial.GetType())
				throw new ArgumentException("Material must be a ConcreteMaterial");
		}

		public double GetHomogenizedArea(double n)
		{
			throw new NotImplementedException();
		}

		public double GetHomogenizedArea()
		{
			throw new NotImplementedException();
		}

		public double GetHomogeneizedJ11(double n)
		{
			throw new NotImplementedException();
		}

		public double GetHomogeneizedJ22()
		{
			throw new NotImplementedException();
		}

		public double GetHomogeneizedJ11()
		{
			throw new NotImplementedException();
		}

		public double GetHomogeneizedJ22(double n)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
