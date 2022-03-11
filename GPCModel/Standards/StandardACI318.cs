using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Standards
{
	public class StandardACI318 : Standard
	{
		#region Variables

		protected readonly double _phiCSpiral;
		protected readonly double _phiCTied;
		protected readonly double _phiT;
		protected readonly double _phiDeformationTransitionIncrement;

		#endregion

		#region Properties

		#endregion

		public double PhiCSpiral => _phiCSpiral;

		public double PhiCTied => _phiCTied;

		public double PhiT => _phiT;

		public double PhiDeformationTransitionIncrement => _phiDeformationTransitionIncrement;

		#region Constructors

		public StandardACI318(string name = "ACI 318", string remarks = "Building Code Requirements for Structural Concrete: ACI Standard ACI 318")
			: base(name, remarks)
		{
			_phiCSpiral = 0.75;
			_phiCTied = 0.65;
			_phiT = 0.90; 
			_phiDeformationTransitionIncrement = 0.003;
		}

		#endregion 
	}
}
