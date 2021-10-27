using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
	public class ConcreteMaterialGeneric : ConcreteMaterial
	{
		#region Variables

		protected double _strainT1;
		protected double _strainT2;
		protected double _strainT3;
		protected double _strainT4;
		protected double _strainC1;
		protected double _strainC2;
		protected double _strainC3;
		protected double _strainC4;

		protected double _stressT1;
		protected double _stressT2;
		protected double _stressT3;
		protected double _stressT4;
		protected double _stressC1;
		protected double _stressC2;
		protected double _stressC3;
		protected double _stressC4;

		#endregion

		#region Properties

		public double StrainT1 => _strainT1;
		public double StrainT2 => _strainT2;
		public double StrainT3 => _strainT3;
		public double StrainT4 => _strainT4;
		public double StrainC1 => _strainC1;
		public double StrainC2 => _strainC2;
		public double StrainC3 => _strainC3;
		public double StrainC4 => _strainC4;


		public double StressT1 => _stressT1;
		public double StressT2 => _stressT2;
		public double StressT3 => _stressT3;
		public double StressT4 => _stressT4;
		public double StressC1 => _stressC1;
		public double StressC2 => _stressC2;
		public double StressC3 => _stressC3;
		public double StressC4 => _stressC4;

		#endregion


		public ConcreteMaterialGeneric(double stressC4, double strainC4, double stressC3, double strainC3, double stressC2, double strainC2,
			double stressC1, double strainC1, double stressT1, double strainT1, double stressT2, double strainT2, double stressT3, double strainT3,
			double stressT4, double strainT4, string name, double ni, double alphaThermalExpansion, double density)
			: base(name, stressC1)
		{
			_strainT1 = strainT1;
			_strainT2 = strainT2;
			_strainT3 = strainT3;
			_strainT4 = strainT4;
			_strainC1 = strainC1;
			_strainC2 = strainC2;
			_strainC3 = strainC3;
			_strainC4 = strainC4;

			_stressT1 = stressT1;
			_stressT2 = stressT2;
			_stressT3 = stressT3;
			_stressT4 = stressT4;
			_stressC1 = stressC1;
			_stressC2 = stressC2;
			_stressC3 = stressC3;
			_stressC4 = stressC4;

			_elasticModulus = stressC1 / strainC1;
			_ni = ni;
			_alfaThermalExpansion = alphaThermalExpansion;
			_density = density;
		}


		


	}
}
