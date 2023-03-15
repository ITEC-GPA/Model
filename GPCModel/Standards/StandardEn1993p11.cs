using System;
using System.Runtime.Serialization;

namespace GPC.Model.Standards
{
	[Serializable]
	public class StandardEN1993p11 : Standard, ISerializable
	{
		#region Variables

		protected double _gammaM0;
		protected double _gammaM1;
		protected double _gammaM2;
		protected double _gammaM3;
		protected double _gammaM3Ser;
		protected double _gammaM4;
		protected double _gammaM5;
		protected double _gammaM6;
		protected double _gammaM7;

		protected double _nShearBucklingLowGradeOfSteel;
		protected double _nShearBucklingHighGradeOfSteel;

		protected double _alphaImperfectionFactorForCurveA0;
		protected double _alphaImperfectionFactorForCurveA;
		protected double _alphaImperfectionFactorForCurveB;
		protected double _alphaImperfectionFactorForCurveC;
		protected double _alphaImperfectionFactorForCurveD;

		protected double _alphaLTImperfectionFactorForCurveA;
		protected double _alphaLTImperfectionFactorForCurveB;
		protected double _alphaLTImperfectionFactorForCurveC;
		protected double _alphaLTImperfectionFactorForCurveD;

		protected double _betaForLateralTorsionalBuckling;
		protected double _lambdaLT0ForLateralTorsionalBuckling;
		protected double _betaForLateralTorsionalBucklingMod;
		protected double _lambdaLT0ForLateralTorsionalBucklingMod;

		#endregion

		#region Properties

		public double GammaM0 { get => _gammaM0; set => _gammaM0 = value; }
		public double GammaM1 { get => _gammaM1; set => _gammaM1 = value; }
		public double GammaM2 { get => _gammaM2; set => _gammaM2 = value; }
		public double GammaM3 { get => _gammaM3; set => _gammaM3 = value; }
		public double GammaM3Ser { get => _gammaM3Ser; set => _gammaM3Ser = value; }
		public double GammaM4 { get => _gammaM4; set => _gammaM4 = value; }
		public double GammaM5 { get => _gammaM5; set => _gammaM5 = value; }
		public double GammaM6 { get => _gammaM6; set => _gammaM6 = value; }
		public double GammaM7 { get => _gammaM7; set => _gammaM7 = value; }

		public double NShearBucklingLowGradeOfSteel { get => _nShearBucklingLowGradeOfSteel; set => _nShearBucklingLowGradeOfSteel = value; }
		public double NShearBucklingHighGradeOfSteel { get => _nShearBucklingHighGradeOfSteel; set => _nShearBucklingHighGradeOfSteel = value; }

		public double AlphaImperfectionFactorForCurveA0 { get => _alphaImperfectionFactorForCurveA0; set => _alphaImperfectionFactorForCurveA0 = value; }
		public double AlphaImperfectionFactorForCurveA { get => _alphaImperfectionFactorForCurveA; set => _alphaImperfectionFactorForCurveA = value; }
		public double AlphaImperfectionFactorForCurveB { get => _alphaImperfectionFactorForCurveB; set => _alphaImperfectionFactorForCurveB = value; }
		public double AlphaImperfectionFactorForCurveC { get => _alphaImperfectionFactorForCurveC; set => _alphaImperfectionFactorForCurveC = value; }
		public double AlphaImperfectionFactorForCurveD { get => _alphaImperfectionFactorForCurveD; set => _alphaImperfectionFactorForCurveD = value; }
		public double AlphaLTImperfectionFactorForCurveA { get => _alphaLTImperfectionFactorForCurveA; set => _alphaLTImperfectionFactorForCurveA = value; }
		public double AlphaLTImperfectionFactorForCurveB { get => _alphaLTImperfectionFactorForCurveB; set => _alphaLTImperfectionFactorForCurveB = value; }
		public double AlphaLTImperfectionFactorForCurveC { get => _alphaLTImperfectionFactorForCurveC; set => _alphaLTImperfectionFactorForCurveC = value; }
		public double AlphaLTImperfectionFactorForCurveD { get => _alphaLTImperfectionFactorForCurveD; set => _alphaLTImperfectionFactorForCurveD = value; }

		public double BetaForLateralTorsionalBuckling { get => _betaForLateralTorsionalBuckling; set => _betaForLateralTorsionalBuckling = value; }
		public double LambdaLT0ForLateralTorsionalBuckling { get => _lambdaLT0ForLateralTorsionalBuckling; set => _lambdaLT0ForLateralTorsionalBuckling = value; }
		public double BetaForLateralTorsionalBucklingMod { get => _betaForLateralTorsionalBucklingMod; set => _betaForLateralTorsionalBucklingMod = value; }
		public double LambdaLT0ForLateralTorsionalBucklingMod { get => _lambdaLT0ForLateralTorsionalBucklingMod; set => _lambdaLT0ForLateralTorsionalBucklingMod = value; }

		#endregion

		#region Constructor

		public StandardEN1993p11()
		{
			_gammaM0 = 1.00;
			_gammaM1 = 1.00;
			_gammaM2 = 1.25;

			_nShearBucklingLowGradeOfSteel = 1.20;
			_nShearBucklingHighGradeOfSteel = 1.00;

			_alphaImperfectionFactorForCurveA0 = 0.13;
			_alphaImperfectionFactorForCurveA = 0.21;
			_alphaImperfectionFactorForCurveB = 0.34;
			_alphaImperfectionFactorForCurveC = 0.49;
			_alphaImperfectionFactorForCurveD = 0.76;

			_alphaLTImperfectionFactorForCurveA = 0.21;
			_alphaLTImperfectionFactorForCurveB = 0.34;
			_alphaLTImperfectionFactorForCurveC = 0.49;
			_alphaLTImperfectionFactorForCurveD = 0.76;

			_betaForLateralTorsionalBuckling = 1.0;
			_lambdaLT0ForLateralTorsionalBuckling = 0.2;
			_betaForLateralTorsionalBucklingMod = 0.75;
			_lambdaLT0ForLateralTorsionalBucklingMod = 0.40;
		}

		protected StandardEN1993p11(SerializationInfo info, StreamingContext context)
		{
			_gammaM0 = info.GetDouble("GammaM0");
			_gammaM1 = info.GetDouble("GammaM1");
			_gammaM2 = info.GetDouble("GammaM2");
			_nShearBucklingLowGradeOfSteel = info.GetDouble("NShearBucklingLowGradeOfSteel");
			_nShearBucklingHighGradeOfSteel = info.GetDouble("NShearBucklingHighGradeOfSteel");
			_alphaImperfectionFactorForCurveA0 = info.GetDouble("AlphaImperfectionFactorForCurveA0");
			_alphaImperfectionFactorForCurveA = info.GetDouble("AlphaImperfectionFactorForCurveA");
			_alphaImperfectionFactorForCurveB = info.GetDouble("AlphaImperfectionFactorForCurveB");
			_alphaImperfectionFactorForCurveC = info.GetDouble("AlphaImperfectionFactorForCurveC");
			_alphaImperfectionFactorForCurveD = info.GetDouble("AlphaImperfectionFactorForCurveD");
			_alphaLTImperfectionFactorForCurveA = info.GetDouble("AlphaLTImperfectionFactorForCurveA");
			_alphaLTImperfectionFactorForCurveB = info.GetDouble("AlphaLTImperfectionFactorForCurveB");
			_alphaLTImperfectionFactorForCurveC = info.GetDouble("AlphaLTImperfectionFactorForCurveC");
			_alphaLTImperfectionFactorForCurveD = info.GetDouble("AlphaLTImperfectionFactorForCurveD");

			_betaForLateralTorsionalBuckling = info.GetDouble("BetaForLateralTorsionalBuckling");
			_lambdaLT0ForLateralTorsionalBuckling = info.GetDouble("LambdaLT0ForLateralTorsionalBuckling");
			_betaForLateralTorsionalBucklingMod = info.GetDouble("BetaForLateralTorsionalBucklingMod");
			_lambdaLT0ForLateralTorsionalBucklingMod = info.GetDouble("LambdaLT0ForLateralTorsionalBucklingMod");
		}

		#endregion

		#region Public Methods

		public override void GetObjectData(SerializationInfo info, StreamingContext context)
		{
			base.GetObjectData(info, context);
			info.AddValue("GammaM0", _gammaM0);
			info.AddValue("GammaM1", _gammaM1);
			info.AddValue("GammaM2", _gammaM2);
			info.AddValue("NShearBucklingLowGradeOfSteel", _nShearBucklingLowGradeOfSteel);
			info.AddValue("NShearBucklingHighGradeOfSteel", _nShearBucklingHighGradeOfSteel);
			info.AddValue("AlphaImperfectionFactorForCurveA0", _alphaImperfectionFactorForCurveA0);
			info.AddValue("AlphaImperfectionFactorForCurveA", _alphaImperfectionFactorForCurveA);
			info.AddValue("AlphaImperfectionFactorForCurveB", _alphaImperfectionFactorForCurveB);
			info.AddValue("AlphaImperfectionFactorForCurveC", _alphaImperfectionFactorForCurveC);
			info.AddValue("AlphaImperfectionFactorForCurveD", _alphaImperfectionFactorForCurveD);
			info.AddValue("AlphaLTImperfectionFactorForCurveA", _alphaLTImperfectionFactorForCurveA);
			info.AddValue("AlphaLTImperfectionFactorForCurveB", _alphaLTImperfectionFactorForCurveB);
			info.AddValue("AlphaLTImperfectionFactorForCurveC", _alphaLTImperfectionFactorForCurveC);
			info.AddValue("AlphaLTImperfectionFactorForCurveD", _alphaLTImperfectionFactorForCurveD);
			info.AddValue("BetaForLateralTorsionalBuckling", _betaForLateralTorsionalBuckling);
			info.AddValue("LambdaLT0ForLateralTorsionalBuckling", _lambdaLT0ForLateralTorsionalBuckling);
			info.AddValue("BetaForLateralTorsionalBucklingMod", _betaForLateralTorsionalBucklingMod);
			info.AddValue("LambdaLT0ForLateralTorsionalBucklingMod", _lambdaLT0ForLateralTorsionalBucklingMod);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(this, obj))
				return true;

			return obj is StandardEN1993p11 p &&
				   _gammaM0 == p._gammaM0 &&
				   _gammaM1 == p._gammaM1 &&
				   _gammaM2 == p._gammaM2 &&
				   _nShearBucklingLowGradeOfSteel == p._nShearBucklingLowGradeOfSteel &&
				   _nShearBucklingHighGradeOfSteel == p._nShearBucklingHighGradeOfSteel &&
				   _alphaImperfectionFactorForCurveA0 == p._alphaImperfectionFactorForCurveA0 &&
				   _alphaImperfectionFactorForCurveA == p._alphaImperfectionFactorForCurveA &&
				   _alphaImperfectionFactorForCurveB == p._alphaImperfectionFactorForCurveB &&
				   _alphaImperfectionFactorForCurveC == p._alphaImperfectionFactorForCurveC &&
				   _alphaImperfectionFactorForCurveD == p._alphaImperfectionFactorForCurveD &&
				   _alphaLTImperfectionFactorForCurveA == p._alphaLTImperfectionFactorForCurveA &&
				   _alphaLTImperfectionFactorForCurveB == p._alphaLTImperfectionFactorForCurveB &&
				   _alphaLTImperfectionFactorForCurveC == p._alphaLTImperfectionFactorForCurveC &&
				   _alphaLTImperfectionFactorForCurveD == p._alphaLTImperfectionFactorForCurveD &&
				   _betaForLateralTorsionalBuckling == p._betaForLateralTorsionalBuckling &&
				   _lambdaLT0ForLateralTorsionalBuckling == p._lambdaLT0ForLateralTorsionalBuckling &&
				   _betaForLateralTorsionalBucklingMod == p._betaForLateralTorsionalBucklingMod &&
				   _lambdaLT0ForLateralTorsionalBucklingMod == p._lambdaLT0ForLateralTorsionalBucklingMod;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = 23;
				hashCode = hashCode * -17 + base.GetHashCode();
				hashCode = hashCode * -17 + _gammaM0.GetHashCode();
				hashCode = hashCode * -17 + _gammaM1.GetHashCode();
				hashCode = hashCode * -17 + _gammaM2.GetHashCode();
				hashCode = hashCode * -17 + _nShearBucklingLowGradeOfSteel.GetHashCode();
				hashCode = hashCode * -17 + _nShearBucklingHighGradeOfSteel.GetHashCode();
				hashCode = hashCode * -17 + _alphaImperfectionFactorForCurveA0.GetHashCode();
				hashCode = hashCode * -17 + _alphaImperfectionFactorForCurveA.GetHashCode();
				hashCode = hashCode * -17 + _alphaImperfectionFactorForCurveB.GetHashCode();
				hashCode = hashCode * -17 + _alphaImperfectionFactorForCurveC.GetHashCode();
				hashCode = hashCode * -17 + _alphaImperfectionFactorForCurveD.GetHashCode();
				hashCode = hashCode * -17 + _alphaLTImperfectionFactorForCurveA.GetHashCode();
				hashCode = hashCode * -17 + _alphaLTImperfectionFactorForCurveB.GetHashCode();
				hashCode = hashCode * -17 + _alphaLTImperfectionFactorForCurveC.GetHashCode();
				hashCode = hashCode * -17 + _alphaLTImperfectionFactorForCurveD.GetHashCode();
				hashCode = hashCode * -17 + _betaForLateralTorsionalBuckling.GetHashCode();
				hashCode = hashCode * -17 + _lambdaLT0ForLateralTorsionalBuckling.GetHashCode();
				hashCode = hashCode * -17 + _betaForLateralTorsionalBucklingMod.GetHashCode();
				hashCode = hashCode * -17 + _lambdaLT0ForLateralTorsionalBucklingMod.GetHashCode();
				return hashCode;
			}
		}

		#endregion
	}
}
