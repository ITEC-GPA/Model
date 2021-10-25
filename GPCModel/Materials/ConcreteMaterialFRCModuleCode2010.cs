using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
	/// <summary>
	/// Concrete material in according to <see cref="Standards.StandardModelCode2010"/>
	/// </summary>
	public class ConcreteMaterialFRCModuleCode2010 : ConcreteMaterialEN1992
	{
		#region Properties

		/// <summary>
		/// Ultimate strain in traction
		/// </summary>
		public double EpsilonFu => _strainFu;

		/// <summary>
		/// Ultimate stress in traction
		/// </summary>
		public double FFtu => _fFtu;

		/// <summary>
		/// The tension stress-strain relationship 
		/// </summary>
		public TensionStressStrainDiagrams TensionStressStrainDiagram => _tensionStressStrainDiagram;

		#endregion

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="name">The name of the material</param>
		/// <param name="fck">Characteristic compressive cylinder strength of concrete at 28 days</param>
		/// <param name="strainYCompression">Yielding compression strain</param>
		/// <param name="strainUCompression">Ultimate compression strain</param>
		/// <param name="strainYTension">Yielding tension strain</param>
		/// <param name="strainUTension">Ultimate tension strain</param>
		/// <param name="fFtu">Ultimate tension stress</param>
		/// <param name="ni">Poisson's ratio</param>
		/// <param name="niCracked">Poisson's ratio in cracked concrete</param>
		/// <param name="alphaT">Linear thermal expasion coefficient</param>
		/// <param name="density">The density of concrete</param> 
		/// <param name="compressionStressStrainDiagram">The compression stress-strain diagram type</param>
		/// <param name="tensionStressStrainDiagrams">The tension stress-strain diagram type</param>
		/// <param name="typeOfCement">The type of cement</param>
		public ConcreteMaterialFRCModuleCode2010(string name, double fck, double strainYCompression, double strainUCompression,
			double strainYTension, double strainUTension, double fFtu, double ni, double niCracked, double alphaT, double density,
            CompressionStressStrainDiagrams compressionStressStrainDiagram, TensionStressStrainDiagrams tensionStressStrainDiagrams, TypeOfCements typeOfCement)
            : base(name, fck, strainYCompression, strainUCompression, strainYTension, ni, niCracked, 
                  alphaT, density, compressionStressStrainDiagram, typeOfCement)
        {
			_strainFu = strainUTension;
			_fFtu = fFtu;
			_tensionStressStrainDiagram = tensionStressStrainDiagrams;
        }

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="fck">Characteristic compressive cylinder strength of concrete at 28 days</param>
		/// <param name="compressionStressStrainDiagram">The compression stress-strain diagram type</param>
		/// <param name="fFty">Yielding compression stress</param>
		/// <param name="strainYTraction">Yielding tension strain</param>
		/// <param name="strainUTraction">Ultimate tension strain</param>
		/// <param name="fFtu">Ultimate tension stress</param>
		/// <param name="name">The name of the material</param>
		/// <remarks>Tension stress-strain diagram is set as bilinear. Missing paramenters are compute according to <see cref="Standards.StandardModelCode2010"/></remarks>
		public ConcreteMaterialFRCModuleCode2010(double fck, CompressionStressStrainDiagrams compressionStressStrainDiagram, double fFty, double strainYTraction, 
			double strainUTraction, double fFtu, string name = "")
			: base(fck, compressionStressStrainDiagram, name)
		{
			_fctk = fFty;
			_strainTensionY = strainYTraction;
			_strainFu = strainUTraction;
			_fFtu = fFtu;
			_tensionStressStrainDiagram = TensionStressStrainDiagrams.Bilinear;
		}

		#endregion
	}
}
