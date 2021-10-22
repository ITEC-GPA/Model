using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
	public class ConcreteMaterialFRC : ConcreteMaterialEN1992
	{
		#region Variables

		protected readonly double _epsilonFu;
		protected readonly double _fFtu;

		#endregion

		#region Properties

		/// <summary>
		/// Ultimate strain in traction
		/// </summary>
		public double EpsilonFu => _epsilonFu;

		/// <summary>
		/// Ultimate stress in traction
		/// </summary>
		public double FFtu => _fFtu;

		public ConcreteMaterialFRC(string name, double fck, double elasticModulusCompression, double epsilonYCompression, double epsilonUCompression,
			double elasticModulusTension, double epsilonYTension, double epsilonUTraction, double fFtu, double ni, double niCracked, double alphaT, double density,
            StressStrainDiagrams stressStrainDiagram, TypeOfCements typeOfCement)
            : base(name, fck, elasticModulusCompression, epsilonYCompression, epsilonUCompression, elasticModulusTension, epsilonYTension, ni, niCracked, 
                  alphaT, density, stressStrainDiagram, typeOfCement)
        {
			_epsilonFu = epsilonUTraction;
			_fFtu = fFtu;
        }

		public ConcreteMaterialFRC(string name, double fck, StressStrainDiagrams stressStrainDiagram, double epsilonUTraction, double fFtu)
			: base(name, fck, stressStrainDiagram)
		{
			_epsilonFu = epsilonUTraction;
			_fFtu = fFtu;
		}

		#endregion
	}
}
