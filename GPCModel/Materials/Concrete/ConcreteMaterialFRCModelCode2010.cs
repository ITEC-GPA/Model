using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
	/// <summary>
	/// Fiber Reinforced Concrete material in according to <see cref="Standards.StandardModelCode2010"/>
	/// </summary>
	public class ConcreteMaterialFRCModelCode2010 : ConcreteMaterialModelCode2010
	{

		#region Constructors

		/// <summary>
		/// Default constructor
		/// </summary>
		/// <param name="name">The name of the material</param>
		/// <param name="fck">Characteristic compressive cylinder strength of concrete at 28 days</param>
		/// <param name="strainYCompression">Yielding compression strain</param>
		/// <param name="strainUCompression">Ultimate compression strain</param>
		/// <param name="epsilonTy">Yielding tension strain</param>
		/// <param name="fcty">Characteristic tensile cylinder strength of concrete at 28 days</param>
		/// <param name="epsilonTu">Ultimate tension strain</param>
		/// <param name="fFtu">Ultimate tension stress</param>
		/// <param name="ni">Poisson's ratio</param>
		/// <param name="niCracked">Poisson's ratio in cracked concrete</param>
		/// <param name="alphaT">Linear thermal expasion coefficient</param>
		/// <param name="density">The density of concrete</param> 
		/// <param name="compressionStressStrainDiagram">The compression stress-strain diagram type</param>
		/// <param name="tensionStressStrainDiagrams">The tension stress-strain diagram type</param>
		/// <param name="typeOfCement">The type of cement</param>
		public ConcreteMaterialFRCModelCode2010(string name, double fck, double strainYCompression, double strainUCompression,
			double epsilonTy, double fcty, double epsilonTu, double fFtu, double ni, double niCracked, double alphaT, double density,
            CompressionStressStrainDiagrams compressionStressStrainDiagram, TensionStressStrainDiagrams tensionStressStrainDiagrams, 
			TypeOfCements typeOfCement)
            : base(name, fck, strainYCompression, strainUCompression, fcty, epsilonTy, ni, niCracked, 
                  alphaT, density, compressionStressStrainDiagram, typeOfCement)
        {
			if (Math.Abs(fFtu) < Math.Abs(fcty))
				throw new ArgumentException("");
			if (Math.Abs(epsilonTu) < Math.Abs(epsilonTy))
				throw new ArgumentException("");

			_epsilonTu = epsilonTu;
			_fFtu = fFtu;
			_tensionStressStrainDiagram = tensionStressStrainDiagrams;
        }

		/// <summary>
		/// Default constructor with <see cref="ConcreteMaterialModelCode2010.TensionStressStrainDiagrams.Bilinear"/> stress-strain diagram
		/// </summary>
		/// <param name="fck">Characteristic compressive cylinder strength of concrete at 28 days</param>
		/// <param name="compressionStressStrainDiagram">The compression stress-strain diagram type</param>
		/// <param name="fFty">Yielding compression stress</param>
		/// <param name="epsilonTy">Yielding tension strain</param>
		/// <param name="epsilonTu">Ultimate tension strain</param>
		/// <param name="fFtu">Ultimate tension stress</param>
		/// <param name="name">The name of the material</param>
		/// <remarks>Tension stress-strain diagram is set as bilinear. Missing paramenters are compute according to <see cref="Standards.StandardModelCode2010"/></remarks>
		public ConcreteMaterialFRCModelCode2010(double fck, CompressionStressStrainDiagrams compressionStressStrainDiagram, double fFty, double epsilonTy, 
			double epsilonTu, double fFtu, string name = "")
			: base(fck, compressionStressStrainDiagram, name)
		{
			if (Math.Abs(fFtu) < Math.Abs(fFty))
				throw new ArgumentException("");
			if (Math.Abs(epsilonTu) < Math.Abs(epsilonTy))
				throw new ArgumentException("");

			_fctk = fFty;
			_fFtu = fFtu;

			_epsilonTy = epsilonTy;
			_epsilonTu = epsilonTu;

			_tensionStressStrainDiagram = TensionStressStrainDiagrams.Bilinear;
		}

		/// <summary>
		/// Default constructor with <see cref="ConcreteMaterialModelCode2010.TensionStressStrainDiagrams.RigidPlastic"/> stress-strain diagram
		/// </summary>
		/// <param name="fck">Characteristic compressive cylinder strength of concrete at 28 days</param>
		/// <param name="compressionStressStrainDiagram">The compression stress-strain diagram type</param>
		/// <param name="epsilonTu">Ultimate tension strain</param>
		/// <param name="fFtu">Ultimate tension stress</param>
		/// <param name="name">The name of the material</param>
		/// <remarks>Tension stress-strain diagram is set as rigid-plastic. Missing paramenters are compute according to <see cref="Standards.StandardModelCode2010"/></remarks>
		public ConcreteMaterialFRCModelCode2010(double fck, CompressionStressStrainDiagrams compressionStressStrainDiagram, 
			double epsilonTu, double fFtu, string name = "")
			: base(fck, compressionStressStrainDiagram, name)
		{
			if (Math.Abs(fFtu) < 0.0)
				throw new ArgumentException("");
			if (Math.Abs(epsilonTu) < 0.0)
				throw new ArgumentException("");

			_fctk = fFtu;
			_fFtu = fFtu;

			_epsilonTy = 0.0;
			_epsilonTu = epsilonTu;

			_tensionStressStrainDiagram = TensionStressStrainDiagrams.RigidPlastic;
		}

		#endregion
	}
}
