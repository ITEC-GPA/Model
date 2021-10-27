using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
	public class ConcreteMaterialFRCCNR204 : ConcreteMaterialFRCModuleCode2010
	{
		public ConcreteMaterialFRCCNR204(double fck, CompressionStressStrainDiagrams compressionStressStrainDiagram, 
			double fFty, double strainYTraction, double strainUTraction, double fFtu, string name = "") 
			: base(fck, compressionStressStrainDiagram, fFty, strainYTraction, strainUTraction, fFtu, name)
		{
		}

		public ConcreteMaterialFRCCNR204(string name, double fck, double strainYCompression, double strainUCompression, 
			double strainYTension, double strainUTension, double fFtu, double ni, double niCracked, double alphaT, double density, 
			CompressionStressStrainDiagrams compressionStressStrainDiagram, TensionStressStrainDiagrams tensionStressStrainDiagrams, 
			TypeOfCements typeOfCement) 
			: base(name, fck, strainYCompression, strainUCompression, strainYTension, strainUTension, fFtu, ni, niCracked, 
				  alphaT, density, compressionStressStrainDiagram, tensionStressStrainDiagrams, typeOfCement)
		{
		}
	}
}
