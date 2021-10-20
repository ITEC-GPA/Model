using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Maths.GaussIntegrations
{
	public static class QuadrangleGaussPoints 
	{
		public static readonly GaussPoint[] Quad1 = new GaussPoint[] { new GaussPoint(0.0, 0.0, 0.0, 4.0, 1) };

		public static readonly GaussPoint[] Quad4 = new GaussPoint[] { new GaussPoint(+0.577350269189626, +0.577350269189626, 0, 1.0, 1),
																	   new GaussPoint(+0.577350269189626, -0.577350269189626, 0, 1.0, 2),
																	   new GaussPoint(-0.577350269189626, +0.577350269189626, 0, 1.0, 3),
																	   new GaussPoint(-0.577350269189626, -0.577350269189626, 0, 1.0, 4) };

		public static readonly GaussPoint[] Quad8 = new GaussPoint[] { new GaussPoint(+0.683130051063973, 0.0, 0.0, 0.816326530612245, 1),
																	   new GaussPoint(-0.683130051063973, 0.0, 0.0, 0.816326530612245, 2),
																	   new GaussPoint(0.0, +0.683130051063973, 0.0, 0.816326530612245, 3),
																	   new GaussPoint(0.0, -0.683130051063973, 0.0, 0.816326530612245, 4),
																	   new GaussPoint(+0.881917103688197, +0.881917103688197, 0.0, 0.183673469387755, 5),
																	   new GaussPoint(+0.881917103688197, -0.881917103688197, 0.0, 0.183673469387755, 6),
																	   new GaussPoint(-0.881917103688197, +0.881917103688197, 0.0, 0.183673469387755, 7),
																	   new GaussPoint(-0.881917103688197, 0-.881917103688197, 0.0, 0.183673469387755, 8) };

		public static readonly GaussPoint[] Quad12 = new GaussPoint[] { new GaussPoint(+0.925820099772551, 0.0000000000000000, 0.0, 0.241975308641975, 1),
																		new GaussPoint(-0.925820099772551, 0.0000000000000000, 0.0, 0.241975308641975, 2),
																		new GaussPoint(+0.000000000000000, +0.683130051063973, 0.0, 0.241975308641975, 3),
																		new GaussPoint(-0.000000000000000, -0.683130051063973, 0.0, 0.241975308641975, 4),
																		new GaussPoint(+0.805979782918599, +0.805979782918599, 0.0, 0.237431774690630, 5),
																		new GaussPoint(+0.805979782918599, -0.805979782918599, 0.0, 0.237431774690630, 6),
																		new GaussPoint(-0.805979782918599, +0.805979782918599, 0.0, 0.237431774690630, 7),
																		new GaussPoint(-0.805979782918599, -0.805979782918599, 0.0, 0.237431774690630, 8),
																		new GaussPoint(+0.380554433208316, +0.380554433208316, 0.0, 0.520592916667394, 9),
																		new GaussPoint(+0.380554433208316, -0.380554433208316, 0.0, 0.520592916667394, 10),
																		new GaussPoint(-0.380554433208316, +0.380554433208316, 0.0, 0.520592916667394, 11),
																		new GaussPoint(-0.380554433208316, -0.380554433208316, 0.0, 0.520592916667394, 12) };
	}
}
