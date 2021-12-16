using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
	public class FiberReinforcedConcrete : ConcreteMaterial
	{
		public FiberReinforcedConcrete(string name, double elasticModulus, double poisson, double fck, double density, double alfaThermalExpansion, Guid guid) 
			: base(name, elasticModulus, poisson, fck, density, alfaThermalExpansion, guid)
		{
		}
	}
}
    