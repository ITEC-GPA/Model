using GPC.TestUtilities;
using MathNet.Numerics.Integration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MathTest
{
	[TestClass]
	public class MathNetIntegrationTest : UnitTestBase
	{
		[TestMethod]
		public void MathNumerics_GaussIntegration_Test1()
		{
			DateTime dateTime01 = DateTime.Now;
			// 2D integration using a 5-point Gauss-Legendre rule over the integration interval [0, 10] X [1, 2]
			double integrate2D01 = GaussLegendreRule.Integrate((x, y) => (x * x) * (y * y), 0.0, 10.0, 1.0, 2.0, 2);
			DateTime dateTime02 = DateTime.Now;

			DateTime dateTime1 = DateTime.Now;
			// 2D integration using a 5-point Gauss-Legendre rule over the integration interval [0, 10] X [1, 2]
			double integrate2D1 = GaussLegendreRule.Integrate((x, y) => (x * x) * (y * y), 0.0, 10.0, 1.0, 2.0, 5);
			DateTime dateTime2 = DateTime.Now;

			DateTime dateTime3 = DateTime.Now;
			// 2D integration using a 5-point Gauss-Legendre rule over the integration interval [0, 10] X [1, 2]
			double integrate2D2 = GaussLegendreRule.Integrate((x, y) => (x * x) * (y * y), 0.0, 10.0, 1.0, 2.0, 20);
			DateTime dateTime4 = DateTime.Now;

			DateTime dateTime5 = DateTime.Now;
			// 2D integration using a 5-point Gauss-Legendre rule over the integration interval [0, 10] X [1, 2]
			double integrate2D3 = GaussLegendreRule.Integrate((x, y) => (x * x) * (y * y), 0.0, 10.0, 1.0, 2.0, 1024);
			DateTime dateTime6 = DateTime.Now;

			Console.WriteLine($"5 points: {(dateTime02 - dateTime01).TotalSeconds}");
			Console.WriteLine($"5 points: {(dateTime2 - dateTime1).TotalSeconds}");
			Console.WriteLine($"20 points: {(dateTime4 - dateTime3).TotalSeconds}");
			Console.WriteLine($"1024 points: {(dateTime6 - dateTime5).TotalSeconds}");
		}

        [TestMethod]
        public void PerformanceTest1()
        {
            Action action1 = new Action(() =>
            {
                GaussLegendreRule.Integrate((x, y) => (x * x) * (y * y), 0.0, 10.0, 1.0, 2.0, 1024);
            });

			GPC.Utilities.Time.MeasureTime.FunctionExecutionTime(1000, action1, true, "1024 Gauss Points");

			Action action2 = new Action(() =>
			{
				GaussLegendreRule.Integrate((x, y) => (x * x) * (y * y), 0.0, 10.0, 1.0, 2.0, 512);
			});

			GPC.Utilities.Time.MeasureTime.FunctionExecutionTime(1000, action2, true, "512 Gauss Points");

			Action action3 = new Action(() =>
			{
				GaussLegendreRule.Integrate((x, y) => (x * x) * (y * y), 0.0, 10.0, 1.0, 2.0, 256);
			});

			GPC.Utilities.Time.MeasureTime.FunctionExecutionTime(1000, action3, true, "256 Gauss Points");

			Action action4 = new Action(() =>
			{
				GaussLegendreRule.Integrate((x, y) => (x * x) * (y * y), 0.0, 10.0, 1.0, 2.0, 128);
			});

			GPC.Utilities.Time.MeasureTime.FunctionExecutionTime(1000, action4, true, "128 Gauss Points");

			Action action5 = new Action(() =>
			{
				GaussLegendreRule.Integrate((x, y) => (x * x) * (y * y), 0.0, 10.0, 1.0, 2.0, 64);
			});

			GPC.Utilities.Time.MeasureTime.FunctionExecutionTime(1000, action5, true, "64 Gauss Points");

			Action action6 = new Action(() =>
			{
				GaussLegendreRule.Integrate((x, y) => (x * x) * (y * y), 0.0, 10.0, 1.0, 2.0, 32);
			});

			GPC.Utilities.Time.MeasureTime.FunctionExecutionTime(1000, action6, true, "32 Gauss Points");
		}
    }
}
