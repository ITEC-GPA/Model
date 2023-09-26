using System;
using System.Collections.Generic;
using System.Text;

namespace GPC.Model.Results.ResultTypes
{
	public class GlassResult
	{
		protected double _stress;

		protected double _stressResistance;

		public double Stress => _stress;
		public double StressResistance => _stressResistance;


		public GlassResult(double stress, double stressResistance)
		{
			_stress = stress;
			_stressResistance = stressResistance;
		}
	}
}
