using GPC.Model.Combinations;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	/// <summary>
	/// This class collects all the coefficient of the Eurocode0 Standard 
	/// </summary>
	/// <remarks>Reference: EN 1990:2002/A1:2005</remarks>
	[Serializable]
	public class StandardEN1990 : Standard, Standard.ICombinationsGenerator, ISerializable
	{
		#region PUBLIC ENUMS

		/// <summary>
		/// The sets for structural and geotechical ultimate limit states. Reference: EN 1990:2002/A1:2005 Annex A1
		/// </summary>
		public enum ULSStructuralGeotechicalCombinationSets
		{
			SetB,
			SetC,
		}

		/// <summary>
		/// The limit states. Reference: EN 1990:2002/A1:2005 
		/// </summary>
		public enum LimitStates
		{
			UltimateEquilibrium,
			UltimateStructural,
			UltimateGeotechnical,
			UltimateFatigue,
			UltimateSeismic,
			UltimateAccidental,
			ServiceabilityCharacteristic,
			ServiceabilityFrequent,
			ServiceabilityQuasiPermanent
		}

		/// <summary>
		/// The category of buildings for imposed loads. Reference: EN 1990:2002/A1:2005 Annex A1. EN 1991-1-1:2002
		/// </summary>
		public enum ImposedLoadCategories
		{
			[Description("Category A")] CategoryA,
			[Description("Category B")] CategoryB,
			[Description("Category C")] CategoryC,
			[Description("Category D")] CategoryD,
			[Description("Category E")] CategoryE,
			[Description("Category F")] CategoryF,
			[Description("Category G")] CategoryG,
			[Description("Category H")] CategoryH,
		}

		#endregion

		#region VARIABLES

		// Gamma G
		private double _gammaGFavourableSetA;
		private double _gammaGUnfavourableSetA;
		private double _gammaGFavourableSetB;
		private double _gammaGUnfavourableSetB;
		private double _gammaGFavourableSetC;
		private double _gammaGUnfavourableSetC;

		// Gamma Q
		private double _gammaQFavourableSetA;
		private double _gammaQUnfavourableSetA;
		private double _gammaQFavourableSetB;
		private double _gammaQUnfavourableSetB;
		private double _gammaQFavourableSetC;
		private double _gammaQUnfavourableSetC;

		// Gamma P
		private double _gammaPFavourableSetA;
		private double _gammaPUnfavourableSetA;
		private double _gammaPFavourableSetB;
		private double _gammaPUnfavourableSetB;
		private double _gammaPFavourableSetC;
		private double _gammaPUnfavourableSetC;

		// Imposed Load Psi
		private double _psi0ImposedLoadCategoryA;
		private double _psi0ImposedLoadCategoryB;
		private double _psi0ImposedLoadCategoryC;
		private double _psi0ImposedLoadCategoryD;
		private double _psi0ImposedLoadCategoryE;
		private double _psi0ImposedLoadCategoryF;
		private double _psi0ImposedLoadCategoryG;
		private double _psi0ImposedLoadCategoryH;

		private double _psi1ImposedLoadCategoryA;
		private double _psi1ImposedLoadCategoryB;
		private double _psi1ImposedLoadCategoryC;
		private double _psi1ImposedLoadCategoryD;
		private double _psi1ImposedLoadCategoryE;
		private double _psi1ImposedLoadCategoryF;
		private double _psi1ImposedLoadCategoryG;
		private double _psi1ImposedLoadCategoryH;

		private double _psi2ImposedLoadCategoryA;
		private double _psi2ImposedLoadCategoryB;
		private double _psi2ImposedLoadCategoryC;
		private double _psi2ImposedLoadCategoryD;
		private double _psi2ImposedLoadCategoryE;
		private double _psi2ImposedLoadCategoryF;
		private double _psi2ImposedLoadCategoryG;
		private double _psi2ImposedLoadCategoryH;

		// Snow Psi
		private double _psi0SnowHighAltitude;
		private double _psi0SnowLowAltitude;
		private double _psi1SnowHighAltitude;
		private double _psi1SnowLowAltitude;
		private double _psi2SnowHighAltitude;
		private double _psi2SnowLowAltitude;

		// Wind Psi
		private double _psi0Wind;
		private double _psi1Wind;
		private double _psi2Wind;

		// Temperature psi
		private double _psi0Temperature;
		private double _psi1Temperature;
		private double _psi2Temperature;

		#endregion

		#region PROPERTIES

		// Gamma G
		public double GammaGFavourableSetA { get => _gammaGFavourableSetA; set => _gammaGFavourableSetA = value; }
		public double GammaGUnfavourableSetA { get => _gammaGUnfavourableSetA; set => _gammaGUnfavourableSetA = value; }
		public double GammaGFavourableSetB { get => _gammaGFavourableSetB; set => _gammaGFavourableSetB = value; }
		public double GammaGUnfavourableSetB { get => _gammaGUnfavourableSetB; set => _gammaGUnfavourableSetB = value; }
		public double GammaGFavourableSetC { get => _gammaGFavourableSetC; set => _gammaGFavourableSetC = value; }
		public double GammaGUnfavourableSetC { get => _gammaGUnfavourableSetC; set => _gammaGUnfavourableSetC = value; }

		// Gamma Q
		public double GammaQFavourableSetA { get => _gammaQFavourableSetA; set => _gammaQFavourableSetA = value; }
		public double GammaQUnfavourableSetA { get => _gammaQUnfavourableSetA; set => _gammaQUnfavourableSetA = value; }
		public double GammaQFavourableSetB { get => _gammaQFavourableSetB; set => _gammaQFavourableSetB = value; }
		public double GammaQUnfavourableSetB { get => _gammaQUnfavourableSetB; set => _gammaQUnfavourableSetB = value; }
		public double GammaQFavourableSetC { get => _gammaQFavourableSetC; set => _gammaQFavourableSetC = value; }
		public double GammaQUnfavourableSetC { get => _gammaQUnfavourableSetC; set => _gammaQUnfavourableSetC = value; }

		// Gamma P
		public double GammaPFavourableSetA { get => _gammaPFavourableSetA; set => _gammaPFavourableSetA = value; }
		public double GammaPUnfavourableSetA { get => _gammaPUnfavourableSetA; set => _gammaPUnfavourableSetA = value; }
		public double GammaPFavourableSetB { get => _gammaPFavourableSetB; set => _gammaPFavourableSetB = value; }
		public double GammaPUnfavourableSetB { get => _gammaPUnfavourableSetB; set => _gammaPUnfavourableSetB = value; }
		public double GammaPFavourableSetC { get => _gammaPFavourableSetC; set => _gammaPFavourableSetC = value; }
		public double GammaPUnfavourableSetC { get => _gammaPUnfavourableSetC; set => _gammaPUnfavourableSetC = value; }

		// Imposed Load Psi
		public double ImposedLoadPsi0CategoryA { get => _psi0ImposedLoadCategoryA; set => _psi0ImposedLoadCategoryA = value; }
		public double ImposedLoadPsi0CategoryB { get => _psi0ImposedLoadCategoryB; set => _psi0ImposedLoadCategoryB = value; }
		public double ImposedLoadPsi0CategoryC { get => _psi0ImposedLoadCategoryC; set => _psi0ImposedLoadCategoryC = value; }
		public double ImposedLoadPsi0CategoryD { get => _psi0ImposedLoadCategoryD; set => _psi0ImposedLoadCategoryD = value; }
		public double ImposedLoadPsi0CategoryE { get => _psi0ImposedLoadCategoryE; set => _psi0ImposedLoadCategoryE = value; }
		public double ImposedLoadPsi0CategoryF { get => _psi0ImposedLoadCategoryF; set => _psi0ImposedLoadCategoryF = value; }
		public double ImposedLoadPsi0CategoryG { get => _psi0ImposedLoadCategoryG; set => _psi0ImposedLoadCategoryG = value; }
		public double ImposedLoadPsi0CategoryH { get => _psi0ImposedLoadCategoryH; set => _psi0ImposedLoadCategoryH = value; }

		public double ImposedLoadPsi1CategoryA { get => _psi1ImposedLoadCategoryA; set => _psi1ImposedLoadCategoryA = value; }
		public double ImposedLoadPsi1CategoryB { get => _psi1ImposedLoadCategoryB; set => _psi1ImposedLoadCategoryB = value; }
		public double ImposedLoadPsi1CategoryC { get => _psi1ImposedLoadCategoryC; set => _psi1ImposedLoadCategoryC = value; }
		public double ImposedLoadPsi1CategoryD { get => _psi1ImposedLoadCategoryD; set => _psi1ImposedLoadCategoryD = value; }
		public double ImposedLoadPsi1CategoryE { get => _psi1ImposedLoadCategoryE; set => _psi1ImposedLoadCategoryE = value; }
		public double ImposedLoadPsi1CategoryF { get => _psi1ImposedLoadCategoryF; set => _psi1ImposedLoadCategoryF = value; }
		public double ImposedLoadPsi1CategoryG { get => _psi1ImposedLoadCategoryG; set => _psi1ImposedLoadCategoryG = value; }
		public double ImposedLoadPsi1CategoryH { get => _psi1ImposedLoadCategoryH; set => _psi1ImposedLoadCategoryH = value; }

		public double ImposedLoadPsi2CategoryA { get => _psi2ImposedLoadCategoryA; set => _psi2ImposedLoadCategoryA = value; }
		public double ImposedLoadPsi2CategoryB { get => _psi2ImposedLoadCategoryB; set => _psi2ImposedLoadCategoryB = value; }
		public double ImposedLoadPsi2CategoryC { get => _psi2ImposedLoadCategoryC; set => _psi2ImposedLoadCategoryC = value; }
		public double ImposedLoadPsi2CategoryD { get => _psi2ImposedLoadCategoryD; set => _psi2ImposedLoadCategoryD = value; }
		public double ImposedLoadPsi2CategoryE { get => _psi2ImposedLoadCategoryE; set => _psi2ImposedLoadCategoryE = value; }
		public double ImposedLoadPsi2CategoryF { get => _psi2ImposedLoadCategoryF; set => _psi2ImposedLoadCategoryF = value; }
		public double ImposedLoadPsi2CategoryG { get => _psi2ImposedLoadCategoryG; set => _psi2ImposedLoadCategoryG = value; }
		public double ImposedLoadPsi2CategoryH { get => _psi2ImposedLoadCategoryH; set => _psi2ImposedLoadCategoryH = value; }

		// Snow Psi
		public double Psi0SnowHighAltitude { get => _psi0SnowHighAltitude; set => _psi0SnowHighAltitude = value; }
		public double Psi0SnowLowAltitude { get => _psi0SnowLowAltitude; set => _psi0SnowLowAltitude = value; }
		public double Psi1SnowHighAltitude { get => _psi1SnowHighAltitude; set => _psi1SnowHighAltitude = value; }
		public double Psi1SnowLowAltitude { get => _psi1SnowLowAltitude; set => _psi1SnowLowAltitude = value; }
		public double Psi2SnowHighAltitude { get => _psi2SnowHighAltitude; set => _psi2SnowHighAltitude = value; }
		public double Psi2SnowLowAltitude { get => _psi2SnowLowAltitude; set => _psi2SnowLowAltitude = value; }

		// Wind Psi
		public double Psi0Wind { get => _psi0Wind; set => _psi0Wind = value; }
		public double Psi1Wind { get => _psi1Wind; set => _psi1Wind = value; }
		public double Psi2Wind { get => _psi2Wind; set => _psi2Wind = value; }

		// Temperature Psi
		public double Psi0Temperature { get => _psi0Temperature; set => _psi0Temperature = value; }
		public double Psi1Temperature { get => _psi1Temperature; set => _psi1Temperature = value; }
		public double Psi2Temperature { get => _psi2Temperature; set => _psi2Temperature = value; }

		#endregion

		#region PUBLIC CONSTRUCTOR

		public StandardEN1990()
		{
			_gammaGUnfavourableSetA = 1.10;
			_gammaGFavourableSetA = 0.90;
			_gammaGUnfavourableSetB = 1.35;
			_gammaGFavourableSetB = 1.00;
			_gammaGUnfavourableSetC = 1.00;
			_gammaGFavourableSetC = 1.00;

			_gammaQUnfavourableSetA = 1.50;
			_gammaQFavourableSetA = 0.00;
			_gammaQUnfavourableSetB = 1.50;
			_gammaQFavourableSetB = 0.00;
			_gammaQUnfavourableSetC = 1.30;
			_gammaQFavourableSetC = 0.00;

			_gammaPFavourableSetA = 1.00;
			_gammaPUnfavourableSetA = 1.00;
			_gammaPFavourableSetB = 1.00;
			_gammaPUnfavourableSetB = 1.00;
			_gammaPFavourableSetC = 1.00;
			_gammaPUnfavourableSetC = 1.00;

			_psi0ImposedLoadCategoryA = 0.70;
			_psi0ImposedLoadCategoryB = 0.70;
			_psi0ImposedLoadCategoryC = 0.70;
			_psi0ImposedLoadCategoryD = 0.70;
			_psi0ImposedLoadCategoryE = 1.00;
			_psi0ImposedLoadCategoryF = 0.70;
			_psi0ImposedLoadCategoryG = 0.70;
			_psi0ImposedLoadCategoryH = 0.00;

			_psi1ImposedLoadCategoryA = 0.50;
			_psi1ImposedLoadCategoryB = 0.50;
			_psi1ImposedLoadCategoryC = 0.70;
			_psi1ImposedLoadCategoryD = 0.70;
			_psi1ImposedLoadCategoryE = 0.90;
			_psi1ImposedLoadCategoryF = 0.70;
			_psi1ImposedLoadCategoryG = 0.50;
			_psi1ImposedLoadCategoryH = 0.00;

			_psi2ImposedLoadCategoryA = 0.30;
			_psi2ImposedLoadCategoryB = 0.30;
			_psi2ImposedLoadCategoryC = 0.60;
			_psi2ImposedLoadCategoryD = 0.60;
			_psi2ImposedLoadCategoryE = 0.80;
			_psi2ImposedLoadCategoryF = 0.60;
			_psi2ImposedLoadCategoryG = 0.30;
			_psi2ImposedLoadCategoryH = 0.00;

			_psi0SnowHighAltitude = 0.70;
			_psi0SnowLowAltitude = 0.50;
			_psi1SnowHighAltitude = 0.50;
			_psi1SnowLowAltitude = 0.20;
			_psi2SnowHighAltitude = 0.20;
			_psi2SnowLowAltitude = 0.00;

			_psi0Wind = 0.60;
			_psi1Wind = 0.20;
			_psi2Wind = 0.00;

			_psi0Temperature = 0.60;
			_psi1Temperature = 0.50;
			_psi2Temperature = 0.0;
		}

		protected StandardEN1990(SerializationInfo info, StreamingContext context)
		{
			_gammaGFavourableSetA = info.GetDouble("GammaGFavourableSetA");
			_gammaGUnfavourableSetA = info.GetDouble("GammaGUnfavourableSetA");
			_gammaGFavourableSetB = info.GetDouble("GammaGFavourableSetB");
			_gammaGUnfavourableSetB = info.GetDouble("GammaGUnfavourableSetB");
			_gammaGFavourableSetC = info.GetDouble("GammaGFavourableSetC");
			_gammaGUnfavourableSetC = info.GetDouble("GammaGUnfavourableSetC");

			_gammaQFavourableSetA = info.GetDouble("GammaQFavourableSetA");
			_gammaQUnfavourableSetA = info.GetDouble("GammaQUnfavourableSetA");
			_gammaQFavourableSetB = info.GetDouble("GammaQFavourableSetB");
			_gammaQUnfavourableSetB = info.GetDouble("GammaQUnfavourableSetB");
			_gammaQFavourableSetC = info.GetDouble("GammaQFavourableSetC");
			_gammaQUnfavourableSetC = info.GetDouble("GammaQUnfavourableSetC");

			_gammaPFavourableSetA = info.GetDouble("GammaPFavourableSetA");
			_gammaPUnfavourableSetA = info.GetDouble("GammaPUnfavourableSetA");
			_gammaPFavourableSetB = info.GetDouble("GammaPFavourableSetB");
			_gammaPUnfavourableSetB = info.GetDouble("GammaPUnfavourableSetB");
			_gammaPFavourableSetC = info.GetDouble("GammaPFavourableSetC");
			_gammaPUnfavourableSetC = info.GetDouble("GammaPUnfavourableSetC");

			_psi0ImposedLoadCategoryA = info.GetDouble("ImposedLoadPsi0CategoryA");
			_psi0ImposedLoadCategoryB = info.GetDouble("ImposedLoadPsi0CategoryB");
			_psi0ImposedLoadCategoryC = info.GetDouble("ImposedLoadPsi0CategoryC");
			_psi0ImposedLoadCategoryD = info.GetDouble("ImposedLoadPsi0CategoryD");
			_psi0ImposedLoadCategoryE = info.GetDouble("ImposedLoadPsi0CategoryE");
			_psi0ImposedLoadCategoryF = info.GetDouble("ImposedLoadPsi0CategoryF");
			_psi0ImposedLoadCategoryG = info.GetDouble("ImposedLoadPsi0CategoryG");
			_psi0ImposedLoadCategoryH = info.GetDouble("ImposedLoadPsi0CategoryH");

			_psi1ImposedLoadCategoryA = info.GetDouble("ImposedLoadPsi1CategoryA");
			_psi1ImposedLoadCategoryB = info.GetDouble("ImposedLoadPsi1CategoryB");
			_psi1ImposedLoadCategoryC = info.GetDouble("ImposedLoadPsi1CategoryC");
			_psi1ImposedLoadCategoryD = info.GetDouble("ImposedLoadPsi1CategoryD");
			_psi1ImposedLoadCategoryE = info.GetDouble("ImposedLoadPsi1CategoryE");
			_psi1ImposedLoadCategoryF = info.GetDouble("ImposedLoadPsi1CategoryF");
			_psi1ImposedLoadCategoryG = info.GetDouble("ImposedLoadPsi1CategoryG");
			_psi1ImposedLoadCategoryH = info.GetDouble("ImposedLoadPsi1CategoryH");

			_psi2ImposedLoadCategoryA = info.GetDouble("ImposedLoadPsi2CategoryA");
			_psi2ImposedLoadCategoryB = info.GetDouble("ImposedLoadPsi2CategoryB");
			_psi2ImposedLoadCategoryC = info.GetDouble("ImposedLoadPsi2CategoryC");
			_psi2ImposedLoadCategoryD = info.GetDouble("ImposedLoadPsi2CategoryD");
			_psi2ImposedLoadCategoryE = info.GetDouble("ImposedLoadPsi2CategoryE");
			_psi2ImposedLoadCategoryF = info.GetDouble("ImposedLoadPsi2CategoryF");
			_psi2ImposedLoadCategoryG = info.GetDouble("ImposedLoadPsi2CategoryG");
			_psi2ImposedLoadCategoryH = info.GetDouble("ImposedLoadPsi2CategoryH");

			_psi0SnowHighAltitude = info.GetDouble("Psi0SnowHighAltitude");
			_psi0SnowLowAltitude = info.GetDouble("Psi0SnowLowAltitude");
			_psi1SnowHighAltitude = info.GetDouble("Psi1SnowHighAltitude");
			_psi1SnowLowAltitude = info.GetDouble("Psi1SnowLowAltitude");
			_psi2SnowHighAltitude = info.GetDouble("Psi2SnowHighAltitude");
			_psi2SnowLowAltitude = info.GetDouble("Psi2SnowLowAltitude");

			_psi0Wind = info.GetDouble("Psi0Wind");
			_psi1Wind = info.GetDouble("Psi1Wind");
			_psi2Wind = info.GetDouble("Psi2Wind");

			_psi0Temperature = info.GetDouble("Psi0Temperature");
			_psi1Temperature = info.GetDouble("Psi1Temperature");
			_psi2Temperature = info.GetDouble("Psi2Temperature");
		}

		#endregion

		#region Equals - hashcode - operators

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			info.AddValue("GammaGFavourableSetA", _gammaGFavourableSetA);
			info.AddValue("GammaGUnfavourableSetA", _gammaGUnfavourableSetA);
			info.AddValue("GammaGFavourableSetB", _gammaGFavourableSetB);
			info.AddValue("GammaGUnfavourableSetB", _gammaGUnfavourableSetB);
			info.AddValue("GammaGFavourableSetC", _gammaGFavourableSetC);
			info.AddValue("GammaGUnfavourableSetC", _gammaGUnfavourableSetC);

			info.AddValue("GammaQFavourableSetA", _gammaQFavourableSetA);
			info.AddValue("GammaQUnfavourableSetA", _gammaQUnfavourableSetA);
			info.AddValue("GammaQFavourableSetB", _gammaQFavourableSetB);
			info.AddValue("GammaQUnfavourableSetB", _gammaQUnfavourableSetB);
			info.AddValue("GammaQFavourableSetC", _gammaQFavourableSetC);
			info.AddValue("GammaQUnfavourableSetC", _gammaQUnfavourableSetC);

			info.AddValue("GammaPFavourableSetA", _gammaPFavourableSetA);
			info.AddValue("GammaPUnfavourableSetA", _gammaPUnfavourableSetA);
			info.AddValue("GammaPFavourableSetB", _gammaPFavourableSetB);
			info.AddValue("GammaPUnfavourableSetB", _gammaPUnfavourableSetB);
			info.AddValue("GammaPFavourableSetC", _gammaPFavourableSetC);
			info.AddValue("GammaPUnfavourableSetC", _gammaPUnfavourableSetC);

			info.AddValue("ImposedLoadPsi0CategoryA", _psi0ImposedLoadCategoryA);
			info.AddValue("ImposedLoadPsi0CategoryB", _psi0ImposedLoadCategoryB);
			info.AddValue("ImposedLoadPsi0CategoryC", _psi0ImposedLoadCategoryC);
			info.AddValue("ImposedLoadPsi0CategoryD", _psi0ImposedLoadCategoryD);
			info.AddValue("ImposedLoadPsi0CategoryE", _psi0ImposedLoadCategoryE);
			info.AddValue("ImposedLoadPsi0CategoryF", _psi0ImposedLoadCategoryF);
			info.AddValue("ImposedLoadPsi0CategoryG", _psi0ImposedLoadCategoryG);
			info.AddValue("ImposedLoadPsi0CategoryH", _psi0ImposedLoadCategoryH);

			info.AddValue("ImposedLoadPsi1CategoryA", _psi1ImposedLoadCategoryA);
			info.AddValue("ImposedLoadPsi1CategoryB", _psi1ImposedLoadCategoryB);
			info.AddValue("ImposedLoadPsi1CategoryC", _psi1ImposedLoadCategoryC);
			info.AddValue("ImposedLoadPsi1CategoryD", _psi1ImposedLoadCategoryD);
			info.AddValue("ImposedLoadPsi1CategoryE", _psi1ImposedLoadCategoryE);
			info.AddValue("ImposedLoadPsi1CategoryF", _psi1ImposedLoadCategoryF);
			info.AddValue("ImposedLoadPsi1CategoryG", _psi1ImposedLoadCategoryG);
			info.AddValue("ImposedLoadPsi1CategoryH", _psi1ImposedLoadCategoryH);

			info.AddValue("ImposedLoadPsi2CategoryA", _psi2ImposedLoadCategoryA);
			info.AddValue("ImposedLoadPsi2CategoryB", _psi2ImposedLoadCategoryB);
			info.AddValue("ImposedLoadPsi2CategoryC", _psi2ImposedLoadCategoryC);
			info.AddValue("ImposedLoadPsi2CategoryD", _psi2ImposedLoadCategoryD);
			info.AddValue("ImposedLoadPsi2CategoryE", _psi2ImposedLoadCategoryE);
			info.AddValue("ImposedLoadPsi2CategoryF", _psi2ImposedLoadCategoryF);
			info.AddValue("ImposedLoadPsi2CategoryG", _psi2ImposedLoadCategoryG);
			info.AddValue("ImposedLoadPsi2CategoryH", _psi2ImposedLoadCategoryH);

			info.AddValue("Psi0SnowHighAltitude", _psi0SnowHighAltitude);
			info.AddValue("Psi0SnowLowAltitude", _psi0SnowLowAltitude);
			info.AddValue("Psi1SnowHighAltitude", _psi1SnowHighAltitude);
			info.AddValue("Psi1SnowLowAltitude", _psi1SnowLowAltitude);
			info.AddValue("Psi2SnowHighAltitude", _psi2SnowHighAltitude);
			info.AddValue("Psi2SnowLowAltitude", _psi2SnowLowAltitude);

			info.AddValue("Psi0Wind", _psi0Wind);
			info.AddValue("Psi1Wind", _psi1Wind);
			info.AddValue("Psi2Wind", _psi2Wind);

			info.AddValue("Psi0Temperature", _psi0Temperature);
			info.AddValue("Psi1Temperature", _psi1Temperature);
			info.AddValue("Psi2Temperature", _psi2Temperature);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;

			return obj is StandardEN1990 eN &&
				   _gammaGFavourableSetA == eN._gammaGFavourableSetA &&
				   _gammaGUnfavourableSetA == eN._gammaGUnfavourableSetA &&
				   _gammaGFavourableSetB == eN._gammaGFavourableSetB &&
				   _gammaGUnfavourableSetB == eN._gammaGUnfavourableSetB &&
				   _gammaGFavourableSetC == eN._gammaGFavourableSetC &&
				   _gammaGUnfavourableSetC == eN._gammaGUnfavourableSetC &&
				   _gammaQFavourableSetA == eN._gammaQFavourableSetA &&
				   _gammaQUnfavourableSetA == eN._gammaQUnfavourableSetA &&
				   _gammaQFavourableSetB == eN._gammaQFavourableSetB &&
				   _gammaQUnfavourableSetB == eN._gammaQUnfavourableSetB &&
				   _gammaQFavourableSetC == eN._gammaQFavourableSetC &&
				   _gammaQUnfavourableSetC == eN._gammaQUnfavourableSetC &&
				   _gammaPFavourableSetA == eN._gammaPFavourableSetA &&
				   _gammaPUnfavourableSetA == eN._gammaPUnfavourableSetA &&
				   _gammaPFavourableSetB == eN._gammaPFavourableSetB &&
				   _gammaPUnfavourableSetB == eN._gammaPUnfavourableSetB &&
				   _gammaPFavourableSetC == eN._gammaPFavourableSetC &&
				   _gammaPUnfavourableSetC == eN._gammaPUnfavourableSetC &&
				   _psi0ImposedLoadCategoryA == eN._psi0ImposedLoadCategoryA &&
				   _psi0ImposedLoadCategoryB == eN._psi0ImposedLoadCategoryB &&
				   _psi0ImposedLoadCategoryC == eN._psi0ImposedLoadCategoryC &&
				   _psi0ImposedLoadCategoryD == eN._psi0ImposedLoadCategoryD &&
				   _psi0ImposedLoadCategoryE == eN._psi0ImposedLoadCategoryE &&
				   _psi0ImposedLoadCategoryF == eN._psi0ImposedLoadCategoryF &&
				   _psi0ImposedLoadCategoryG == eN._psi0ImposedLoadCategoryG &&
				   _psi0ImposedLoadCategoryH == eN._psi0ImposedLoadCategoryH &&
				   _psi1ImposedLoadCategoryA == eN._psi1ImposedLoadCategoryA &&
				   _psi1ImposedLoadCategoryB == eN._psi1ImposedLoadCategoryB &&
				   _psi1ImposedLoadCategoryC == eN._psi1ImposedLoadCategoryC &&
				   _psi1ImposedLoadCategoryD == eN._psi1ImposedLoadCategoryD &&
				   _psi1ImposedLoadCategoryE == eN._psi1ImposedLoadCategoryE &&
				   _psi1ImposedLoadCategoryF == eN._psi1ImposedLoadCategoryF &&
				   _psi1ImposedLoadCategoryG == eN._psi1ImposedLoadCategoryG &&
				   _psi1ImposedLoadCategoryH == eN._psi1ImposedLoadCategoryH &&
				   _psi2ImposedLoadCategoryA == eN._psi2ImposedLoadCategoryA &&
				   _psi2ImposedLoadCategoryB == eN._psi2ImposedLoadCategoryB &&
				   _psi2ImposedLoadCategoryC == eN._psi2ImposedLoadCategoryC &&
				   _psi2ImposedLoadCategoryD == eN._psi2ImposedLoadCategoryD &&
				   _psi2ImposedLoadCategoryE == eN._psi2ImposedLoadCategoryE &&
				   _psi2ImposedLoadCategoryF == eN._psi2ImposedLoadCategoryF &&
				   _psi2ImposedLoadCategoryG == eN._psi2ImposedLoadCategoryG &&
				   _psi2ImposedLoadCategoryH == eN._psi2ImposedLoadCategoryH &&
				   _psi0SnowHighAltitude == eN._psi0SnowHighAltitude &&
				   _psi0SnowLowAltitude == eN._psi0SnowLowAltitude &&
				   _psi1SnowHighAltitude == eN._psi1SnowHighAltitude &&
				   _psi1SnowLowAltitude == eN._psi1SnowLowAltitude &&
				   _psi2SnowHighAltitude == eN._psi2SnowHighAltitude &&
				   _psi2SnowLowAltitude == eN._psi2SnowLowAltitude &&
				   _psi0Wind == eN._psi0Wind &&
				   _psi1Wind == eN._psi1Wind &&
				   _psi2Wind == eN._psi2Wind &&
				   _psi0Temperature == eN._psi0Temperature &&
				   _psi1Temperature == eN._psi1Temperature &&
				   _psi2Temperature == eN._psi2Temperature;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + _gammaGFavourableSetA.GetHashCode();
				hashCode = hashCode * -17 + _gammaGUnfavourableSetA.GetHashCode();
				hashCode = hashCode * -17 + _gammaGFavourableSetB.GetHashCode();
				hashCode = hashCode * -17 + _gammaGUnfavourableSetB.GetHashCode();
				hashCode = hashCode * -17 + _gammaGFavourableSetC.GetHashCode();
				hashCode = hashCode * -17 + _gammaGUnfavourableSetC.GetHashCode();
				hashCode = hashCode * -17 + _gammaQFavourableSetA.GetHashCode();
				hashCode = hashCode * -17 + _gammaQUnfavourableSetA.GetHashCode();
				hashCode = hashCode * -17 + _gammaQFavourableSetB.GetHashCode();
				hashCode = hashCode * -17 + _gammaQUnfavourableSetB.GetHashCode();
				hashCode = hashCode * -17 + _gammaQFavourableSetC.GetHashCode();
				hashCode = hashCode * -17 + _gammaQUnfavourableSetC.GetHashCode();
				hashCode = hashCode * -17 + _gammaPFavourableSetA.GetHashCode();
				hashCode = hashCode * -17 + _gammaPUnfavourableSetA.GetHashCode();
				hashCode = hashCode * -17 + _gammaPFavourableSetB.GetHashCode();
				hashCode = hashCode * -17 + _gammaPUnfavourableSetB.GetHashCode();
				hashCode = hashCode * -17 + _gammaPFavourableSetC.GetHashCode();
				hashCode = hashCode * -17 + _gammaPUnfavourableSetC.GetHashCode();
				hashCode = hashCode * -17 + _psi0ImposedLoadCategoryA.GetHashCode();
				hashCode = hashCode * -17 + _psi0ImposedLoadCategoryB.GetHashCode();
				hashCode = hashCode * -17 + _psi0ImposedLoadCategoryC.GetHashCode();
				hashCode = hashCode * -17 + _psi0ImposedLoadCategoryD.GetHashCode();
				hashCode = hashCode * -17 + _psi0ImposedLoadCategoryE.GetHashCode();
				hashCode = hashCode * -17 + _psi0ImposedLoadCategoryF.GetHashCode();
				hashCode = hashCode * -17 + _psi0ImposedLoadCategoryG.GetHashCode();
				hashCode = hashCode * -17 + _psi0ImposedLoadCategoryH.GetHashCode();
				hashCode = hashCode * -17 + _psi1ImposedLoadCategoryA.GetHashCode();
				hashCode = hashCode * -17 + _psi1ImposedLoadCategoryB.GetHashCode();
				hashCode = hashCode * -17 + _psi1ImposedLoadCategoryC.GetHashCode();
				hashCode = hashCode * -17 + _psi1ImposedLoadCategoryD.GetHashCode();
				hashCode = hashCode * -17 + _psi1ImposedLoadCategoryE.GetHashCode();
				hashCode = hashCode * -17 + _psi1ImposedLoadCategoryF.GetHashCode();
				hashCode = hashCode * -17 + _psi1ImposedLoadCategoryG.GetHashCode();
				hashCode = hashCode * -17 + _psi1ImposedLoadCategoryH.GetHashCode();
				hashCode = hashCode * -17 + _psi2ImposedLoadCategoryA.GetHashCode();
				hashCode = hashCode * -17 + _psi2ImposedLoadCategoryB.GetHashCode();
				hashCode = hashCode * -17 + _psi2ImposedLoadCategoryC.GetHashCode();
				hashCode = hashCode * -17 + _psi2ImposedLoadCategoryD.GetHashCode();
				hashCode = hashCode * -17 + _psi2ImposedLoadCategoryE.GetHashCode();
				hashCode = hashCode * -17 + _psi2ImposedLoadCategoryF.GetHashCode();
				hashCode = hashCode * -17 + _psi2ImposedLoadCategoryG.GetHashCode();
				hashCode = hashCode * -17 + _psi2ImposedLoadCategoryH.GetHashCode();
				hashCode = hashCode * -17 + _psi0SnowHighAltitude.GetHashCode();
				hashCode = hashCode * -17 + _psi0SnowLowAltitude.GetHashCode();
				hashCode = hashCode * -17 + _psi1SnowHighAltitude.GetHashCode();
				hashCode = hashCode * -17 + _psi1SnowLowAltitude.GetHashCode();
				hashCode = hashCode * -17 + _psi2SnowHighAltitude.GetHashCode();
				hashCode = hashCode * -17 + _psi2SnowLowAltitude.GetHashCode();
				hashCode = hashCode * -17 + _psi0Wind.GetHashCode();
				hashCode = hashCode * -17 + _psi1Wind.GetHashCode();
				hashCode = hashCode * -17 + _psi2Wind.GetHashCode();
				hashCode = hashCode * -17 + _psi0Temperature.GetHashCode();
				hashCode = hashCode * -17 + _psi1Temperature.GetHashCode();
				hashCode = hashCode * -17 + _psi2Temperature.GetHashCode();
				return hashCode;
			}
		}

		#endregion

		#region COMBINATIONS OPTIONS

		public class EN1990CombinationsOptions : CombinationsOptions
		{
			public LimitStates LimitState { get; set; }

			public ImposedLoadCategories Category { get; set; } = ImposedLoadCategories.CategoryA;

			public ULSStructuralGeotechicalCombinationSets ULS { get; set; } = ULSStructuralGeotechicalCombinationSets.SetB;

			public bool HighAltitude { get; set; } = true;

			public EN1990CombinationsOptions(LimitStates limitState)
			{
				LimitState = limitState;
			}

			public EN1990CombinationsOptions(LimitStates limitState, ULSStructuralGeotechicalCombinationSets uLS = ULSStructuralGeotechicalCombinationSets.SetB, ImposedLoadCategories imposedLoadCategories = ImposedLoadCategories.CategoryA, bool highAltitude = true)
			{
				LimitState = limitState;
				Category = imposedLoadCategories;
				ULS = uLS;
				HighAltitude = highAltitude;
			}

			public EN1990CombinationsOptions(LimitStates limitState, ImposedLoadCategories imposedLoadCategories = ImposedLoadCategories.CategoryA, bool highAltitude = true)
			{
				LimitState = limitState;
				Category = imposedLoadCategories;
				HighAltitude = highAltitude;
			}

			public override bool Equals(object obj)
			{
				if (obj is null)
					return false;

				if (ReferenceEquals(this, obj))
					return true;

				EN1990CombinationsOptions objCasted = obj as EN1990CombinationsOptions;

				return !(objCasted is null) && objCasted.Category.Equals(Category) && objCasted.LimitState.Equals(LimitState) && objCasted.ULS.Equals(ULS) && objCasted.HighAltitude.Equals(HighAltitude);
			}

			public override int GetHashCode()
			{
				unchecked
				{
					var hashCode = 23;
					hashCode = 17 * hashCode + LimitState.GetHashCode();
					hashCode = 17 * hashCode + Category.GetHashCode();
					hashCode = 17 * hashCode + ULS.GetHashCode();
					hashCode = 17 * hashCode + HighAltitude.GetHashCode();

					return hashCode;
				}
			}
		}



		#endregion

		#region PUBLIC METHOD Gamma e Psi

		/// <summary>
		/// Get the coefficient gamma G unfavourable 
		/// </summary>
		/// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
		/// <param name="limitState">The limit state of combinations</param>
		/// <returns>The value of the coefficient</returns>
		public double GetGammaGUnfavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState)
		{
			if (limitState == LimitStates.UltimateEquilibrium)
			{
				return _gammaGUnfavourableSetA;
			}
			else if (limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateStructural)
			{
				if (set == ULSStructuralGeotechicalCombinationSets.SetB)
					return _gammaGUnfavourableSetB;
				else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
					return _gammaGUnfavourableSetC;
				else
					throw new NotImplementedException("Failed to set coefficient gamma unfavourable");
			}
			else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
			{
				return 1.0;
			}
			else if (limitState == LimitStates.ServiceabilityQuasiPermanent || limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent)
			{
				return 1.0;
			}
			else
				throw new ArgumentException("Failed to set coefficient gammaG unfavourable");

		}

		/// <summary>
		/// Get the coefficient gamma G favourable 
		/// </summary>
		/// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
		/// <param name="limitState">The limit state of combinations</param>
		/// <returns>The value of the coefficient</returns>
		public double GetGammaGFavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState)
		{
			if (limitState == LimitStates.UltimateEquilibrium)
			{
				return _gammaGFavourableSetA;
			}
			else if (limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateStructural)
			{
				if (set == ULSStructuralGeotechicalCombinationSets.SetB)
					return _gammaGFavourableSetB;
				else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
					return _gammaGFavourableSetC;
				else
					throw new NotImplementedException("Failed to set coefficient gamma G favourable");
			}
			else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
			{
				return 1.0;
			}
			else if (limitState == LimitStates.ServiceabilityQuasiPermanent || limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent)
			{
				return 1.0;
			}
			else
				throw new ArgumentException("Failed to set coefficient gamma G favourable");
		}

		/// <summary>
		/// Get the coefficient gamma P favourable 
		/// </summary>
		/// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
		/// <param name="limitState">The limit state of combinations</param>
		/// <returns>The value of the coefficient</returns>
		public double GetGammaPFavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState)
		{
			if (limitState == LimitStates.UltimateEquilibrium)
			{
				return _gammaPFavourableSetA;
			}
			else if (limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateStructural)
			{
				switch (set)
				{
					case ULSStructuralGeotechicalCombinationSets.SetB:
						return _gammaPFavourableSetB;
					case ULSStructuralGeotechicalCombinationSets.SetC:
						return _gammaPFavourableSetC;
					default:
						throw new NotImplementedException("Failed to set coefficient gamma P favourable");
				}
			}
			else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
			{
				return 1.0;
			}
			else if (limitState == LimitStates.ServiceabilityQuasiPermanent || limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent)
			{
				return 1.00;
			}
			else
				throw new ArgumentException("Failed to set coefficient gamma P favourable");
		}

		/// <summary>
		/// Get the coefficient gamma P unfavourable 
		/// </summary>
		/// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
		/// <param name="limitState">The limit state of combinations</param>
		/// <returns>The value of the coefficient</returns>
		public double GetGammaPUnfavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState)
		{
			if (limitState == LimitStates.UltimateEquilibrium)
			{
				return _gammaPUnfavourableSetA;
			}
			else if (limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateStructural)
			{
				switch (set)
				{
					case ULSStructuralGeotechicalCombinationSets.SetB:
						return _gammaPUnfavourableSetB;
					case ULSStructuralGeotechicalCombinationSets.SetC:
						return _gammaPUnfavourableSetC;
					default:
						throw new NotImplementedException("Failed to set coefficient gamma P unfavourable");
				}
			}
			else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
			{
				return 1.0;
			}
			else if (limitState == LimitStates.ServiceabilityQuasiPermanent || limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent)
			{
				return 1.00;
			}
			else
				throw new ArgumentException("Failed to set coefficient gamma P favourable");
		}

		/// <summary>
		/// Get the coefficient gamma Q unfavourable 
		/// </summary>
		/// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
		/// <param name="limitState">The limit state of combinations</param>
		/// <param name="loadCase">The load case</param>
		/// <returns>The value of the coefficient</returns>
		public double GetGammaQUnfavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState, LoadCase loadCase)
		{
			if (limitState == LimitStates.UltimateEquilibrium)
			{
				switch (loadCase.LoadCaseType)
				{
					case LoadCase.LoadCaseTypes.LiveLoad:
					case LoadCase.LoadCaseTypes.WindPressure:
					case LoadCase.LoadCaseTypes.WindSuction:
					case LoadCase.LoadCaseTypes.Snow:
					case LoadCase.LoadCaseTypes.Maintenance:
					case LoadCase.LoadCaseTypes.Earthquake:
					case LoadCase.LoadCaseTypes.Temperature:
						return _gammaQUnfavourableSetA;
					default:
						throw new NotImplementedException("Not implemented coefficient for load case type");
				}
			}
			else if (limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateStructural)
			{
				if (set == ULSStructuralGeotechicalCombinationSets.SetB)
				{
					switch (loadCase.LoadCaseType)
					{
						case LoadCase.LoadCaseTypes.LiveLoad:
						case LoadCase.LoadCaseTypes.WindPressure:
						case LoadCase.LoadCaseTypes.WindSuction:
						case LoadCase.LoadCaseTypes.Snow:
						case LoadCase.LoadCaseTypes.Maintenance:
						case LoadCase.LoadCaseTypes.Earthquake:
						case LoadCase.LoadCaseTypes.Temperature:
							return _gammaQUnfavourableSetB;
						default:
							throw new NotImplementedException("Not implemented coefficient for load case type");
					}
				}
				else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
				{
					switch (loadCase.LoadCaseType)
					{
						case LoadCase.LoadCaseTypes.LiveLoad:
						case LoadCase.LoadCaseTypes.WindPressure:
						case LoadCase.LoadCaseTypes.WindSuction:
						case LoadCase.LoadCaseTypes.Snow:
						case LoadCase.LoadCaseTypes.Maintenance:
						case LoadCase.LoadCaseTypes.Earthquake:
						case LoadCase.LoadCaseTypes.Temperature:
							return _gammaQUnfavourableSetC;
						default:
							throw new NotImplementedException("Not implemented coefficient for load case type");
					}
				}
				else
					throw new NotImplementedException("Not implemented Annex");
			}
			else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
			{
				return 1.0;
			}
			else if (limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent || limitState == LimitStates.ServiceabilityQuasiPermanent)
			{
				switch (loadCase.LoadCaseType)
				{
					case LoadCase.LoadCaseTypes.LiveLoad:
					case LoadCase.LoadCaseTypes.WindPressure:
					case LoadCase.LoadCaseTypes.WindSuction:
					case LoadCase.LoadCaseTypes.Snow:
					case LoadCase.LoadCaseTypes.Maintenance:
					case LoadCase.LoadCaseTypes.Earthquake:
					case LoadCase.LoadCaseTypes.Temperature:
						return 1.0;

					default:
						throw new NotImplementedException("Not implemented coefficient for load case type");
				}
			}

			throw new ArgumentException("Failed to set coefficient gamma favourable");
		}

		/// <summary>
		/// Get the coefficient gamma Q favourable 
		/// </summary>
		/// <param name="set">The ULS combination set (if <paramref name="limitState"/> is an ultimate state limit</param>
		/// <param name="limitState">The limit state of combinations</param>
		/// <param name="loadCase">The load case</param>
		/// <returns>The value of the coefficient</returns>
		public double GetGammaQFavourable(ULSStructuralGeotechicalCombinationSets set, LimitStates limitState, LoadCase loadCase)
		{
			if (limitState == LimitStates.UltimateEquilibrium)
			{
				switch (loadCase.LoadCaseType)
				{
					case LoadCase.LoadCaseTypes.LiveLoad:
					case LoadCase.LoadCaseTypes.WindPressure:
					case LoadCase.LoadCaseTypes.WindSuction:
					case LoadCase.LoadCaseTypes.Snow:
					case LoadCase.LoadCaseTypes.Maintenance:
					case LoadCase.LoadCaseTypes.Earthquake:
					case LoadCase.LoadCaseTypes.Temperature:
						return _gammaQFavourableSetA;
					default:
						throw new NotImplementedException("Not implemented coefficient for load case type");
				}
			}
			else if (limitState == LimitStates.UltimateGeotechnical || limitState == LimitStates.UltimateFatigue || limitState == LimitStates.UltimateStructural)
			{
				if (set == ULSStructuralGeotechicalCombinationSets.SetB)
				{
					switch (loadCase.LoadCaseType)
					{
						case LoadCase.LoadCaseTypes.LiveLoad:
						case LoadCase.LoadCaseTypes.WindPressure:
						case LoadCase.LoadCaseTypes.WindSuction:
						case LoadCase.LoadCaseTypes.Snow:
						case LoadCase.LoadCaseTypes.Maintenance:
						case LoadCase.LoadCaseTypes.Earthquake:
						case LoadCase.LoadCaseTypes.Temperature:
							return _gammaQFavourableSetB;
						default:
							throw new NotImplementedException("Not implemented coefficient for load case type");
					}
				}
				else if (set == ULSStructuralGeotechicalCombinationSets.SetC)
				{
					switch (loadCase.LoadCaseType)
					{
						case LoadCase.LoadCaseTypes.LiveLoad:
						case LoadCase.LoadCaseTypes.WindPressure:
						case LoadCase.LoadCaseTypes.WindSuction:
						case LoadCase.LoadCaseTypes.Snow:
						case LoadCase.LoadCaseTypes.Maintenance:
						case LoadCase.LoadCaseTypes.Earthquake:
						case LoadCase.LoadCaseTypes.Temperature:
							return _gammaQFavourableSetC;
						default:
							throw new NotImplementedException("Not implemented coefficient for load case type");
					}
				}
				else
					throw new NotImplementedException("Not implemented Annex");
			}
			else if (limitState == LimitStates.UltimateSeismic || limitState == LimitStates.UltimateAccidental)
			{
				return 1.0;
			}
			else if (limitState == LimitStates.ServiceabilityQuasiPermanent || limitState == LimitStates.ServiceabilityCharacteristic || limitState == LimitStates.ServiceabilityFrequent)
			{
				return 1.00;
			}

			throw new ArgumentException("Failed to set coefficient gamma favourable");

		}

		/// <summary>
		/// Get the coefficient psi 0 for buildings
		/// </summary>
		/// <param name="category">The category of the imposed load</param>
		/// <param name="loadCase">The load case</param>
		/// <param name="highAltitude">If true, set the snow load with high altitude</param>
		/// <returns>The value of the coefficient</returns>
		public double GetPsi0(ImposedLoadCategories category, LoadCase loadCase, bool highAltitude = true)
		{
			var loadCaseType = loadCase.LoadCaseType;

			if (loadCaseType == LoadCase.LoadCaseTypes.Snow)
			{
				if (highAltitude)
					return Psi0SnowHighAltitude;
				else if (!highAltitude)
					return Psi0SnowLowAltitude;
				else
					throw new NotImplementedException("Failed to set coefficient psi0 for snow load");
			}
			else if (loadCaseType == LoadCase.LoadCaseTypes.LiveLoad || loadCaseType == LoadCase.LoadCaseTypes.Maintenance)
			{
				switch (category)
				{
					case ImposedLoadCategories.CategoryA:
						return _psi0ImposedLoadCategoryA;
					case ImposedLoadCategories.CategoryB:
						return _psi0ImposedLoadCategoryB;
					case ImposedLoadCategories.CategoryC:
						return _psi0ImposedLoadCategoryC;
					case ImposedLoadCategories.CategoryD:
						return _psi0ImposedLoadCategoryD;
					case ImposedLoadCategories.CategoryE:
						return _psi0ImposedLoadCategoryE;
					case ImposedLoadCategories.CategoryF:
						return _psi0ImposedLoadCategoryF;
					case ImposedLoadCategories.CategoryG:
						return _psi0ImposedLoadCategoryG;
					case ImposedLoadCategories.CategoryH:
						return _psi0ImposedLoadCategoryH;
					default:
						throw new NotImplementedException("Failed to set coefficient psi0 for live load load or maintenance load");
				}
			}
			else
			{
				switch (loadCaseType)
				{
					case LoadCase.LoadCaseTypes.SelfWeight:
					case LoadCase.LoadCaseTypes.SuperImposedDeadLoad:
					case LoadCase.LoadCaseTypes.Earthquake:
						throw new ArgumentException("Don't exist coefficient for this load case type");
					case LoadCase.LoadCaseTypes.WindPressure:
					case LoadCase.LoadCaseTypes.WindSuction:
						return _psi0Wind;
					case LoadCase.LoadCaseTypes.Temperature:
						return _psi0Temperature;
					default:
						throw new NotImplementedException("Not implemented coefficient for load case type");
				}
			}

			throw new ArgumentException("Don't exist coefficient for this load case");
		}

		/// <summary>
		/// Get the coefficient psi 1 for buildings
		/// </summary>
		/// <param name="category">The category of the imposed load</param>
		/// <param name="loadCase">The load case</param>
		/// <param name="highAltitude">If true, set the snow load with high altitude</param>        
		/// <returns>The value of the coefficient</returns>
		public double GetPsi1(ImposedLoadCategories category, LoadCase loadCase, bool highAltitude = true)
		{
			var loadCaseType = loadCase.LoadCaseType;

			if (loadCaseType == LoadCase.LoadCaseTypes.Snow)
			{
				if (highAltitude)
					return Psi1SnowHighAltitude;
				else if (!highAltitude)
					return Psi1SnowLowAltitude;
				else
					throw new NotImplementedException("Failed to set coefficient psi0 for snow load");
			}
			else if (loadCaseType == LoadCase.LoadCaseTypes.LiveLoad || loadCaseType == LoadCase.LoadCaseTypes.Maintenance)
			{
				switch (category)
				{
					case ImposedLoadCategories.CategoryA:
						return _psi1ImposedLoadCategoryA;
					case ImposedLoadCategories.CategoryB:
						return _psi1ImposedLoadCategoryB;
					case ImposedLoadCategories.CategoryC:
						return _psi1ImposedLoadCategoryC;
					case ImposedLoadCategories.CategoryD:
						return _psi1ImposedLoadCategoryD;
					case ImposedLoadCategories.CategoryE:
						return _psi1ImposedLoadCategoryE;
					case ImposedLoadCategories.CategoryF:
						return _psi1ImposedLoadCategoryF;
					case ImposedLoadCategories.CategoryG:
						return _psi1ImposedLoadCategoryG;
					case ImposedLoadCategories.CategoryH:
						return _psi1ImposedLoadCategoryH;
					default:
						throw new NotImplementedException("Failed to set coefficient psi0 for live load load or maintenance load");
				}
			}
			else
			{
				switch (loadCaseType)
				{
					case LoadCase.LoadCaseTypes.SelfWeight:
					case LoadCase.LoadCaseTypes.Earthquake:
					case LoadCase.LoadCaseTypes.SuperImposedDeadLoad:
						throw new ArgumentException("Don't exist coefficient for this load case type");
					case LoadCase.LoadCaseTypes.WindPressure:
					case LoadCase.LoadCaseTypes.WindSuction:
						return _psi1Wind;
					case LoadCase.LoadCaseTypes.Temperature:
						return _psi1Temperature;
					default:
						throw new NotImplementedException("Not implemented coefficient for load case type");
				}
			}
		}

		/// <summary>
		/// Get the coefficient psi 2 for buildings
		/// </summary>
		/// <param name="category">The category of the imposed load</param>
		/// <param name="loadCase">The load case</param>
		/// <param name="highAltitude">If true, set the snow load with high altitude</param>
		/// <returns>The value of the coefficient</returns>
		public double GetPsi2(ImposedLoadCategories category, LoadCase loadCase, bool highAltitude = true)
		{
			var loadCaseType = loadCase.LoadCaseType;

			if (loadCaseType == LoadCase.LoadCaseTypes.Snow)
			{
				if (highAltitude)
					return Psi2SnowHighAltitude;
				else if (!highAltitude)
					return Psi2SnowLowAltitude;
				else
					throw new NotImplementedException("Failed to set coefficient psi0 for snow load");
			}
			else if (loadCaseType == LoadCase.LoadCaseTypes.LiveLoad || loadCaseType == LoadCase.LoadCaseTypes.Maintenance)
			{
				switch (category)
				{
					case ImposedLoadCategories.CategoryA:
						return _psi2ImposedLoadCategoryA;
					case ImposedLoadCategories.CategoryB:
						return _psi2ImposedLoadCategoryB;
					case ImposedLoadCategories.CategoryC:
						return _psi2ImposedLoadCategoryC;
					case ImposedLoadCategories.CategoryD:
						return _psi2ImposedLoadCategoryD;
					case ImposedLoadCategories.CategoryE:
						return _psi2ImposedLoadCategoryE;
					case ImposedLoadCategories.CategoryF:
						return _psi2ImposedLoadCategoryF;
					case ImposedLoadCategories.CategoryG:
						return _psi2ImposedLoadCategoryG;
					case ImposedLoadCategories.CategoryH:
						return _psi2ImposedLoadCategoryH;
					default:
						throw new NotImplementedException("Failed to set coefficient psi0 for live load load or maintenance load");
				}
			}
			else
			{
				switch (loadCaseType)
				{
					case LoadCase.LoadCaseTypes.SelfWeight:
					case LoadCase.LoadCaseTypes.SuperImposedDeadLoad:
					case LoadCase.LoadCaseTypes.Earthquake:
						throw new ArgumentException("Don't exist coefficient for this load case type");
					case LoadCase.LoadCaseTypes.WindPressure:
					case LoadCase.LoadCaseTypes.WindSuction:
						return _psi2Wind;
					case LoadCase.LoadCaseTypes.Temperature:
						return _psi2Temperature;
					default:
						throw new NotImplementedException("Not implemented coefficient for load case type");
				}
			}
		}

		#endregion

		#region PUBLIC GENERATION METHODS

		/// <summary>
		/// Generate all the combinations with the load cases in <paramref name="loadCasesInput"/> and the settings <paramref name="options"/>
		/// </summary>
		/// <param name="loadCasesInput">List of load cases</param>
		/// <param name="options">The standard options</param>
		/// <param name="prefix">The common prefix for each combination in the collection (default name is "cmb")</param>
		/// <returns>A collection of combinations</returns>
		/// <exception cref="ArgumentException"> If there are any  climate load in the <paramref name="loadCasesInput"/></exception>
		public virtual CombinationsCollection CreateCombinations(LoadCaseBase[] loadCasesInput, CombinationsOptions options, string prefix = "cmb")
		{
			List<LoadCase> loadCases = new List<LoadCase>();
			foreach (LoadCaseBase loadCase in loadCasesInput)
			{
				if (loadCase is ClimateLoadCase climateLoadCase)
					throw new ArgumentException("EN not support climate load: Load case must not be a climate load case");

				else if (loadCase is LoadCase LoadCaseNormal)
					loadCases.Add(LoadCaseNormal);
			}

			CombinationsCollection combinations = new CombinationsCollection();
			Combination.CombinationCoefficientEqualityComparer equalityComparer = new Combination.CombinationCoefficientEqualityComparer();
			HashSet<Combination> combinationsHashSet = new HashSet<Combination>(equalityComparer);
			int idProg = 1;

			List<List<Combination.LoadCaseCoefficient>> listFavourable = GetFavourableCombinations(loadCases.ToArray(), (EN1990CombinationsOptions)options);
			for (int i = 0; i < listFavourable.Count(); i++)
			{
				Combination combo = new Combination(prefix + $" {idProg}", options);

				for (int j = 0; j < listFavourable[i].Count(); j++)
				{
					combo.AddLoadCaseCoefficient(listFavourable[i][j].LoadCase, listFavourable[i][j].Coefficient);
				}
				if (!combinationsHashSet.Contains(combo))
				{
					combinationsHashSet.Add(combo);
					idProg++;
				}
			}

			List<List<Combination.LoadCaseCoefficient>> listUnfavourable = GetUnfavourableCombinations(loadCases.ToArray(), (EN1990CombinationsOptions)options);
			for (int i = 0; i < listUnfavourable.Count(); i++)
			{
				Combination combo = new Combination(prefix + $" {idProg}", options);

				for (int j = 0; j < listUnfavourable[i].Count(); j++)
				{
					combo.AddLoadCaseCoefficient(listUnfavourable[i][j].LoadCase, listUnfavourable[i][j].Coefficient);
				}
				if (!combinationsHashSet.Contains(combo))
				{
					combinationsHashSet.Add(combo);
					idProg++;
				}
			}

			List<List<Combination.LoadCaseCoefficient>> listFavourableBase = GetBasicCombinationsMinCoeff(loadCases.ToArray(), (EN1990CombinationsOptions)options);
			for (int i = 0; i < listFavourableBase.Count(); i++)
			{
				Combination comboBaseFav = new Combination(prefix + $" {idProg}", options);
				for (int j = 0; j < listFavourableBase[i].Count(); j++)
				{
					comboBaseFav.AddLoadCaseCoefficient(listFavourableBase[i][j].LoadCase, listFavourableBase[i][j].Coefficient);
				}
				if (!combinationsHashSet.Contains(comboBaseFav))
				{
					combinationsHashSet.Add(comboBaseFav);
					idProg++;
				}
			}

			List<List<Combination.LoadCaseCoefficient>> listUnfavourableBase = GetBasicCombinationsMaxCoeff(loadCases.ToArray(), (EN1990CombinationsOptions)options);
			for (int i = 0; i < listUnfavourableBase.Count(); i++)
			{
				Combination comboBaseUnfav = new Combination(prefix + $" {idProg}", options);
				for (int j = 0; j < listUnfavourableBase[i].Count(); j++)
				{
					comboBaseUnfav.AddLoadCaseCoefficient(listUnfavourableBase[i][j].LoadCase, listUnfavourableBase[i][j].Coefficient);
				}
				if (!combinationsHashSet.Contains(comboBaseUnfav))
				{
					combinationsHashSet.Add(comboBaseUnfav);
					idProg++;
				}
			}

			foreach (Combination cmb in combinationsHashSet)
				combinations.Add(cmb);

			return combinations;
		}

		#endregion

		#region PROTECTED METHODS

		/// <summary>
		/// Generate all the combination with favourable coefficients
		/// </summary>
		/// <param name="loadCases">List of load cases</param>
		/// <param name="optionsInput">The normative options</param>
		/// <returns>A list of load case coefficient</returns>
		protected virtual List<List<Combination.LoadCaseCoefficient>> GetFavourableCombinations(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
		{
			if (optionsInput is EN1990CombinationsOptions options)
			{
				List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficients = new List<List<Combination.LoadCaseCoefficient>>();
				List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficientsBuffer = GetBasicCombinationsMinCoeff(loadCases, options);

				List<LoadCase> list = new List<LoadCase>();
				foreach (LoadCase loadCase in loadCases)
				{
					if (loadCase is LoadCase lc && (lc.LoadCaseType != LoadCase.LoadCaseTypes.Prestress && lc.LoadCaseType != LoadCase.LoadCaseTypes.SelfWeight &&
						lc.LoadCaseType != LoadCase.LoadCaseTypes.SuperImposedDeadLoad && lc.LoadCaseType != LoadCase.LoadCaseTypes.Earthquake))
						list.Add(loadCase);
				}

				List<List<Combination.LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list.ToArray(), options);

				for (int i = 0; i < randomList.Count(); i++)
				{
					foreach (List<Combination.LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
					{
						List<Combination.LoadCaseCoefficient> tempList = new List<Combination.LoadCaseCoefficient>();
						tempList.AddRange(l);
						tempList.AddRange(randomList[i]);
						loadCaseCoefficients.Add(tempList);
					}
				}

				return loadCaseCoefficients;
			}
			throw new ArgumentException("CombinationsOptions must be EN1990CombinationsOptions");
		}

		/// <summary>
		/// Generate all the combination with unfavourable coefficients
		/// </summary>
		/// <param name="loadCases">List of load cases</param>
		/// <param name="optionsInput">The normative options</param>
		/// <returns>A list of load case coefficient</returns>
		protected virtual List<List<Combination.LoadCaseCoefficient>> GetUnfavourableCombinations(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
		{
			if (optionsInput is EN1990CombinationsOptions options)
			{
				List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficients = new List<List<Combination.LoadCaseCoefficient>>();
				List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficientsBuffer = GetBasicCombinationsMaxCoeff(loadCases, options);

				List<LoadCase> list = new List<LoadCase>();
				foreach (LoadCase loadCase in loadCases)
				{
					if (loadCase is LoadCase lc && (lc.LoadCaseType != LoadCase.LoadCaseTypes.Prestress && lc.LoadCaseType != LoadCase.LoadCaseTypes.SelfWeight &&
						lc.LoadCaseType != LoadCase.LoadCaseTypes.SuperImposedDeadLoad && lc.LoadCaseType != LoadCase.LoadCaseTypes.Earthquake))
						list.Add(loadCase);
				}

				List<List<Combination.LoadCaseCoefficient>> randomList = RandomizeVariableLoads(list.ToArray(), options);

				for (int i = 0; i < randomList.Count(); i++)
				{
					foreach (List<Combination.LoadCaseCoefficient> l in loadCaseCoefficientsBuffer)
					{
						List<Combination.LoadCaseCoefficient> tempList = new List<Combination.LoadCaseCoefficient>();
						tempList.AddRange(l);
						tempList.AddRange(randomList[i]);
						loadCaseCoefficients.Add(tempList);
					}
				}

				return loadCaseCoefficients;
			}
			throw new ArgumentException("CombinationsOptions must be EN1990CombinationsOptions");
		}

		/// <summary>
		/// Generate all the combination for permanent loads with favourable coefficients
		/// </summary>
		/// <param name="loadCases">List of load cases</param>
		/// <param name="optionsInput">The normative options</param>
		/// <returns>A list of load case coefficient</returns>
		protected virtual List<List<Combination.LoadCaseCoefficient>> GetBasicCombinationsMinCoeff(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
		{
			if (optionsInput is EN1990CombinationsOptions options)
			{
				List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();
				List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBase = new List<Combination.LoadCaseCoefficient>();


				// aggiungo i SelfWeight
				foreach (LoadCase loadCase in loadCases.Where(x => x is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight))
				{
					Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
					loadCaseCoefficientsBase.Add(lc);
				}
				// aggiungo i SuperImposedDeadLoad
				foreach (LoadCase loadCase in loadCases.Where(x => x is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad))
				{
					Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
					loadCaseCoefficientsBase.Add(lc);
				}
				// aggiunto i Prestress
				foreach (LoadCase loadCase in loadCases.Where(x => x is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress))
				{
					Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
					loadCaseCoefficientsBase.Add(lc);
				}
				// aggiunto il carico sismico se siamo in condizione sismica (come se fosse un permanente perchè non deve variare)
				if (options.LimitState == LimitStates.UltimateSeismic)
				{
					foreach (LoadCase loadCase in loadCases.Where(x => x is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake))
					{
						Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientFavourablePermanentActions(loadCase, options), loadCase);
						loadCaseCoefficientsBase.Add(lc);
					}
				}

				outList.Add(loadCaseCoefficientsBase);

				return outList;
			}
			throw new ArgumentException("CombinationsOptions must be EN1990CombinationsOptions");
		}

		/// <summary>
		/// Generate all the combination for permanent loads with unfavourable coefficients
		/// </summary>
		/// <param name="loadCases">List of load cases</param>
		/// <param name="optionsInput">The normative options</param>
		/// <returns>A list of load case coefficient</returns>
		protected virtual List<List<Combination.LoadCaseCoefficient>> GetBasicCombinationsMaxCoeff(LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
		{
			if (optionsInput is EN1990CombinationsOptions options)
			{
				List<List<Combination.LoadCaseCoefficient>> outList = new List<List<Combination.LoadCaseCoefficient>>();
				List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBase = new List<Combination.LoadCaseCoefficient>();

				// aggiungo i SelfWeight
				foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight))
				{
					Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
					loadCaseCoefficientsBase.Add(lc);
				}
				// aggiungo i SuperImposedDeadLoad
				foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad))
				{
					Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
					loadCaseCoefficientsBase.Add(lc);
				}
				// aggiunto i Prestress
				foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress))
				{
					Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
					loadCaseCoefficientsBase.Add(lc);
				}
				// aggiunto il carico sismico se siamo in condizione sismica (come se fosse un permanente perchè non deve variare)
				if (options.LimitState == StandardEN1990.LimitStates.UltimateSeismic)
				{
					foreach (LoadCase loadCase in loadCases.Where(i => i is LoadCase lc && lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake))
					{
						Combination.LoadCaseCoefficient lc = new Combination.LoadCaseCoefficient(GetCoefficientUnfavourablePermanentActions(loadCase, options), loadCase);
						loadCaseCoefficientsBase.Add(lc);
					}
				}

				outList.Add(loadCaseCoefficientsBase);

				return outList;
			}
			throw new ArgumentException("CombinationsOptions must be EN1990CombinationsOptions");
		}

		/// <summary>
		/// Generate all the combination for the variable loads in <paramref name="loadCasesInput"/> with the options <paramref name="optionsInput"/>
		/// </summary>
		/// <param name="loadCasesInput">List of load cases</param>
		/// <param name="optionsInput">The combination generation options</param>
		/// <returns>A list of list of load case coefficient</returns>
		/// <exception cref="ArgumentException"> If there are any permanent load case or climate load in the <paramref name="loadCasesInput"/></exception>
		protected virtual List<List<Combination.LoadCaseCoefficient>> RandomizeVariableLoads(LoadCaseBase[] loadCasesInput, CombinationsOptions optionsInput)
		{
			if (optionsInput is EN1990CombinationsOptions options)
			{
				List<List<Combination.LoadCaseCoefficient>> loadCaseCoefficients = new List<List<Combination.LoadCaseCoefficient>>();
				List<LoadCase> loadCasesList = new List<LoadCase>();

				// controllo che i carichi siano variabili
				foreach (LoadCaseBase loadCase in loadCasesInput)
				{
					if (loadCase is LoadCase lc)
					{
						if (lc.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || lc.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad ||
						lc.LoadCaseType == LoadCase.LoadCaseTypes.Prestress || lc.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
							throw new ArgumentException("Load case must be Variable");
						else
							loadCasesList.Add(lc);
					}

					if (loadCase is ClimateLoadCase climateLoadCase)
						throw new ArgumentException("EN not support climate load: Load case must not be a climate load case");
				}

				LoadCase[] loadCases = loadCasesList.ToArray();

				for (int i = 0; i < loadCases.Count(); i++)
				{
					#region LIST, HASHSET E BOOL

					HashSet<LoadCase.LoadCaseTypes> hash = new HashSet<LoadCase.LoadCaseTypes>();
					List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<Combination.LoadCaseCoefficient>();
					List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer2 = new List<Combination.LoadCaseCoefficient>();
					List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer3 = new List<Combination.LoadCaseCoefficient>();
					List<Combination.LoadCaseCoefficient> loadCaseCoefficientsWindPressure = new List<Combination.LoadCaseCoefficient>();
					List<Combination.LoadCaseCoefficient> loadCaseCoefficientsWindSuction = new List<Combination.LoadCaseCoefficient>();
					HashSet<LoadCase.LoadCaseTypes> hashAcc = new HashSet<LoadCase.LoadCaseTypes>();
					bool haveWindPressure = false;
					bool haveWindSuction = false;

					#endregion

					#region LEAD LOAD ADD

					// crea un load lead, cerca tutti i carichi dello stesso tipo e li coefficienta alla stessa maniera.
					LoadCase loadCaseLead = loadCases[i];
					loadCaseCoefficientsBuffer = AddLoadCaseLead(loadCaseLead.LoadCaseType, loadCases, options);
					hash.Add(loadCaseLead.LoadCaseType);

					#endregion

					// aggiunge tutti i carichi secondari che non siano wind pressure o wind suction. quei due vanno trattati a parte
					foreach (LoadCase loadCaseAccompanying in loadCases)
					{
						#region NORMAL LOAD ADD

						if (!hashAcc.Contains(loadCaseAccompanying.LoadCaseType) && !loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType) &&
							loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction && loadCaseAccompanying.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure)
						{
							loadCaseCoefficientsBuffer.AddRange(AddLoadCaseAccompanying(loadCaseAccompanying.LoadCaseType, loadCases, options));
							hashAcc.Add(loadCaseAccompanying.LoadCaseType);
						}

						#endregion

						#region BOOL CHECK

						// controllo se sono presenti carichi WindPressure o WindSuction per l'assemblaggio finale delle liste
						if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure)
							haveWindPressure = true;
						if (loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction)
							haveWindSuction = true;

						#endregion
					}

					#region WIND LOAD ADD

					// gestione carichi secondari windsuction
					foreach (LoadCase loadCaseAccompanying in loadCases)
					{
						if (loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.WindPressure && loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindSuction &&
							!loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType) && !hashAcc.Contains(loadCaseAccompanying.LoadCaseType))
						{
							loadCaseCoefficientsWindSuction.AddRange(AddLoadCaseAccompanying(LoadCase.LoadCaseTypes.WindSuction, loadCases, options));
							hashAcc.Add(loadCaseAccompanying.LoadCaseType);
						}
					}

					// gestione carichi secondari windpressure
					foreach (LoadCase loadCaseAccompanying in loadCases)
					{
						if (loadCaseLead.LoadCaseType != LoadCase.LoadCaseTypes.WindSuction && loadCaseAccompanying.LoadCaseType == LoadCase.LoadCaseTypes.WindPressure &&
							!loadCaseAccompanying.LoadCaseType.Equals(loadCaseLead.LoadCaseType) && !hashAcc.Contains(loadCaseAccompanying.LoadCaseType))
						{
							loadCaseCoefficientsWindPressure.AddRange(AddLoadCaseAccompanying(LoadCase.LoadCaseTypes.WindPressure, loadCases, options));
							hashAcc.Add(loadCaseAccompanying.LoadCaseType);
						}
					}

					#endregion

					#region ASSEMBLY

					if (haveWindPressure == true && haveWindSuction == true)
					{
						loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();
						loadCaseCoefficientsBuffer3 = loadCaseCoefficientsBuffer.ToArray().ToList();

						if (loadCaseCoefficientsWindPressure.Count() != 0)
						{
							loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindPressure);
							loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
						}
						if (loadCaseCoefficientsWindSuction.Count() != 0)
						{
							loadCaseCoefficientsBuffer3.AddRange(loadCaseCoefficientsWindSuction);
							loadCaseCoefficients.Add(loadCaseCoefficientsBuffer3);
						}
						if (loadCaseCoefficientsWindSuction.Count() == 0 && loadCaseCoefficientsWindPressure.Count() == 0)
							loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
					}
					else if (haveWindPressure == false && haveWindSuction == true)
					{
						loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();

						if (loadCaseCoefficientsWindSuction.Count() != 0)
						{
							loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindSuction);
							loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
						}
						if (loadCaseCoefficientsWindSuction.Count() == 0)
							loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
					}
					else if (haveWindPressure == true && haveWindSuction == false)
					{
						loadCaseCoefficientsBuffer2 = loadCaseCoefficientsBuffer.ToArray().ToList();

						if (loadCaseCoefficientsWindPressure.Count() != 0)
						{
							loadCaseCoefficientsBuffer2.AddRange(loadCaseCoefficientsWindPressure);
							loadCaseCoefficients.Add(loadCaseCoefficientsBuffer2);
						}
						if (loadCaseCoefficientsWindPressure.Count() == 0)
							loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);
					}
					else
						loadCaseCoefficients.Add(loadCaseCoefficientsBuffer);

					#endregion

				}

				return loadCaseCoefficients;
			}
			throw new ArgumentException("CombinationsOptions must be EN1990CombinationsOptions");
		}

		/// <summary>
		/// Return a list of load case coefficients with all the load of type <paramref name="types"/> in the array <paramref name="loadCases"/> with the leading variable action coefficient
		/// </summary>
		/// <param name="types">The load case lead type (only EN1990 loads are supported)</param>
		/// <param name="loadCases">The array of load cases</param>
		/// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
		/// <returns>A list of load case coefficients</returns>
		protected List<Combination.LoadCaseCoefficient> AddLoadCaseLead(LoadCase.LoadCaseTypes types, LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
		{
			if (optionsInput is EN1990CombinationsOptions options)
			{
				List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<Combination.LoadCaseCoefficient>();
				foreach (LoadCase loadCaseL in loadCases.Where(j => j is LoadCase lc && lc.LoadCaseType == types))
				{
					Combination.LoadCaseCoefficient loadCaseCoefficientLead = new Combination.LoadCaseCoefficient(GetCoefficientLeadingVariableAction(loadCaseL, options), loadCaseL);
					loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
				}
				return loadCaseCoefficientsBuffer;
			}
			else
				throw new ArgumentException("Lead load must not be a climate load or CombinationsOptions must be EN1990");
		}

		/// <summary>
		/// Return a list of load case coefficients with all the load of type <paramref name="types"/> in the array <paramref name="loadCases"/> with the accompanying variable action coefficient
		/// </summary>
		/// <param name="types">The load case lead type(only EN1990 loads are supported)</param>
		/// <param name="loadCases">The array of load cases</param>
		/// <param name="optionsInput">The normative options (only EN16612 is supported)</param>
		/// <returns>A list of load case coefficients</returns>
		protected List<Combination.LoadCaseCoefficient> AddLoadCaseAccompanying(LoadCase.LoadCaseTypes types, LoadCaseBase[] loadCases, CombinationsOptions optionsInput)
		{
			if (optionsInput is EN1990CombinationsOptions options)
			{
				List<Combination.LoadCaseCoefficient> loadCaseCoefficientsBuffer = new List<Combination.LoadCaseCoefficient>();
				foreach (LoadCase loadCase in loadCases.Where(j => j is LoadCase lc && lc.LoadCaseType == types))
				{
					Combination.LoadCaseCoefficient loadCaseCoefficientLead = new Combination.LoadCaseCoefficient(GetCoefficientAccompanyingVariableAction(loadCase, options), loadCase);
					loadCaseCoefficientsBuffer.Add(loadCaseCoefficientLead);
				}
				return loadCaseCoefficientsBuffer;
			}
			else
				throw new ArgumentException("Lead load must not be a climate load or CombinationsOptions must be EN1990");
		}

		#endregion

		#region COEFFICIENT

		/// <summary>
		/// Return the coefficient of unfavourable permanent actions
		/// </summary>
		/// <param name="loadCase">The load cases (only permanent loads are accepted)</param>
		/// <param name="options">The normative options</param>
		/// <returns>The coefficient</returns>
		/// <exception cref="ArgumentException"> If don't exist the coefficient for the <paramref name="loadCase"/> with options <paramref name="options"/></exception>
		protected double GetCoefficientUnfavourablePermanentActions(LoadCase loadCase, EN1990CombinationsOptions options)
		{
			if (loadCase.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || loadCase.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
				return GetGammaGUnfavourable(options.ULS, options.LimitState);
			else if (loadCase.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
				return GetGammaGUnfavourable(options.ULS, options.LimitState);
			else if (loadCase.LoadCaseType == LoadCase.LoadCaseTypes.Prestress)
				return GetGammaPUnfavourable(options.ULS, options.LimitState);

			throw new ArgumentException("Failed to set coefficient favourable for permanent actions");
		}

		/// <summary>
		/// Return the coefficient of favourable permanent actions
		/// </summary>
		/// <param name="loadCase">The load cases (only permanent loads are accepted)</param>
		/// <param name="options">The normative options</param>
		/// <returns>The coefficient</returns>
		/// <exception cref="ArgumentException"> If don't exist the coefficient for the <paramref name="loadCase"/> with options <paramref name="options"/></exception>
		protected double GetCoefficientFavourablePermanentActions(LoadCase loadCase, EN1990CombinationsOptions options)
		{
			if (loadCase.LoadCaseType == LoadCase.LoadCaseTypes.SelfWeight || loadCase.LoadCaseType == LoadCase.LoadCaseTypes.SuperImposedDeadLoad)
				return GetGammaGFavourable(options.ULS, options.LimitState);
			else if (loadCase.LoadCaseType == LoadCase.LoadCaseTypes.Earthquake)
				return GetGammaGFavourable(options.ULS, options.LimitState);
			else if (loadCase.LoadCaseType == LoadCase.LoadCaseTypes.Prestress)
				return GetGammaPFavourable(options.ULS, options.LimitState);

			throw new ArgumentException("Failed to set coefficient favourable for permanent actions");
		}

		/// <summary>
		/// Return the coefficient of leading variable actions
		/// </summary>
		/// <param name="loadCase">The load cases (only variable loads are accepted)MO</param>
		/// <param name="options">The normative options</param>
		/// <returns>The coefficient</returns>
		/// <exception cref="ArgumentException"> If don't exist the coefficient for the <paramref name="loadCase"/> with options <paramref name="options"/></exception>
		protected double GetCoefficientLeadingVariableAction(LoadCase loadCase, EN1990CombinationsOptions options)
		{
			double psi1;
			double psi2;
			double gamma;
			double gammaQ;

			if (options.LimitState == LimitStates.UltimateEquilibrium || options.LimitState == LimitStates.UltimateFatigue
			|| options.LimitState == LimitStates.UltimateGeotechnical || options.LimitState == LimitStates.UltimateStructural)
			{
				gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
				return gamma;
			}
			else if (options.LimitState == LimitStates.UltimateSeismic)
			{
				gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
				psi2 = GetPsi2(options.Category, loadCase, options.HighAltitude);
				return gammaQ * psi2;
			}
			else if (options.LimitState == LimitStates.UltimateAccidental)
			{
				gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
				psi1 = GetPsi1(options.Category, loadCase, options.HighAltitude);
				return gammaQ * psi1;
			}
			else if (options.LimitState == LimitStates.ServiceabilityCharacteristic)
			{
				gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
				psi2 = GetPsi2(options.Category, loadCase, options.HighAltitude);
				return gamma * psi2;
			}
			else if (options.LimitState == LimitStates.ServiceabilityFrequent)
			{
				gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
				psi1 = GetPsi1(options.Category, loadCase, options.HighAltitude);
				return gamma * psi1;
			}
			else if (options.LimitState == LimitStates.ServiceabilityQuasiPermanent)
			{
				gamma = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
				psi2 = GetPsi2(options.Category, loadCase, options.HighAltitude);
				return gamma * psi2;
			}

			throw new ArgumentException("Failed to set the coefficient for leading variable actions");
		}

		/// <summary>
		/// Return the coefficient of accompanying variable actions
		/// </summary>
		/// <param name="loadCase">The load cases (only variable loads are accepted)</param>
		/// <param name="options">The normative options</param>
		/// <returns>The coefficient</returns>
		/// <exception cref="ArgumentException"> If don't exist the coefficient for the <paramref name="loadCase"/> with options <paramref name="options"/></exception>
		protected double GetCoefficientAccompanyingVariableAction(LoadCase loadCase, EN1990CombinationsOptions options)
		{
			double psi0;
			double psi2;
			double gammaQ;

			if (options.LimitState == LimitStates.UltimateEquilibrium || options.LimitState == LimitStates.UltimateFatigue
			|| options.LimitState == LimitStates.UltimateGeotechnical || options.LimitState == LimitStates.UltimateStructural)
			{
				gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
				psi0 = GetPsi0(options.Category, loadCase, options.HighAltitude);
				return gammaQ * psi0;
			}
			else if (options.LimitState == LimitStates.ServiceabilityCharacteristic)
			{
				gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
				psi0 = GetPsi0(options.Category, loadCase, options.HighAltitude);
				return gammaQ * psi0;
			}
			else if (options.LimitState == LimitStates.UltimateSeismic)
			{
				gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
				psi2 = GetPsi2(options.Category, loadCase, options.HighAltitude);
				return gammaQ * psi2;
			}
			else if (options.LimitState == LimitStates.UltimateAccidental)
			{
				gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
				psi2 = GetPsi2(options.Category, loadCase, options.HighAltitude);
				return gammaQ * psi2;
			}
			else if (options.LimitState == LimitStates.ServiceabilityFrequent || options.LimitState == LimitStates.ServiceabilityQuasiPermanent)
			{
				gammaQ = GetGammaQUnfavourable(options.ULS, options.LimitState, loadCase);
				psi2 = GetPsi2(options.Category, loadCase, options.HighAltitude);
				return gammaQ * psi2;
			}

			throw new ArgumentException("Failed to set the coefficient for leading variable actions");
		}

		#endregion

	}
}
