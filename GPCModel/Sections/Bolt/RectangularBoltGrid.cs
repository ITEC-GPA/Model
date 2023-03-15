using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GPC.Model.Sections.Bolt
{
	public class RectangularBoltGrid : BoltGrid
	{
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
			List<BoltPosition> bolts = GetBoltPositions(stepX, stepY, diameter, mat, origin);	
			_bolts.AddRange(bolts);
		}

		#endregion

		private List<BoltPosition> GetBoltPositions(IEnumerable<double> stepX, IEnumerable<double> stepY, double diameter, SteelMaterial mat, Point2d origin)
		{
			var boltList = new List<BoltPosition>();

			// Create list of absolute cooridnates.
			var absX = new List<double>();
			var absY = new List<double>();
			absX.Add(origin.X);
			absY.Add(origin.Y);
			foreach (var x in stepX)
				absX.Add(absX.Last() + x);
			foreach (var y in stepY)
				absY.Add(absY.Last() + y);

			// Add bolts respecting a rectangular grid.			
			foreach (var x in absX)
				foreach (var y in absY)
					boltList.Add(new BoltPosition(new Point2d(x, y), new BoltSection(diameter, mat), new Hole()));

			return boltList;
		}
	}
}
