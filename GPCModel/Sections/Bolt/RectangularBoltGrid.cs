using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static GPC.Model.Sections.Bolt.BoltGrid;

namespace GPC.Model.Sections.Bolt
{
	public class RectangularBoltGrid : BoltGrid
	{
		protected IEnumerable<double> _stepX;
		protected IEnumerable<double> _stepY;

		public IEnumerable<double> StepX => _stepX;
		public IEnumerable<double> StepY => _stepY;

		public BoltPosition[][] BoltPositions { get; }

		#region Public Constructors

		/// <summary>
		/// Creates a rectangular grid of bolts.
		/// </summary>
		/// <param name="stepX">Steps in X.</param>
		/// <param name="stepY">Steps in Y.</param>
		/// <param name="diameter"></param>
		/// <param name="mat"></param>
		/// <param name="origin">Starting point, bottom right corner.</param>
		public RectangularBoltGrid(IEnumerable<double> stepX, IEnumerable<double> stepY, double diameter, SteelMaterial mat, Point2d origin = default(Point2d), string name = "")
			: base(name)
		{
			_stepX = stepX;
			_stepY = stepY;

			List<BoltPosition> bolts = GetBoltPositions(stepX, stepY, diameter, mat, out BoltPosition[][] positions, origin);
			BoltPositions = positions;
			_bolts.AddRange(bolts);
		}

		#endregion

		private List<BoltPosition> GetBoltPositions(IEnumerable<double> stepX, IEnumerable<double> stepY, double diameter, SteelMaterial mat, out BoltPosition[][] positions, Point2d origin = null)
		{
			var boltList = new List<BoltPosition>();

			if (origin == null)
				origin = Point2d.Origin;

			// Create list of absolute cooridnates.
			var absX = new List<double>();
			var absY = new List<double>();
			absX.Add(origin.X);
			absY.Add(origin.Y);
			foreach (double x in stepX)
				absX.Add(absX.Last() + x);
			foreach (double y in stepY)
				absY.Add(absY.Last() + y);

			// Add bolts respecting a rectangular grid.	
			int count = 1;
			positions = new BoltPosition[absX.Count][];
			for (int i = 0; i < absX.Count; i++)
			{
				positions[i] = new BoltPosition[absY.Count];
				double x = absX[i];
				for (int j = 0; j < absY.Count; j++)
				{
					double y = absY[j];
					var point = new Point2d(x, y);
					var boltPos = new BoltPosition(point, new BoltSection(diameter, mat), new Hole(diameter + 1), count);
					boltList.Add(boltPos);
					count++;

					positions[i][j] = boltPos;
				}
			}

			return boltList;
		}
	}
}
