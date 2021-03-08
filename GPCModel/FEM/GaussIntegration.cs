using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM
{
    public static class GaussIntegration
    {
        public static GaussPoint[] Get(int dimension, int pointsPerDimension)
        {
            HashSet<GaussPoint> pts = new HashSet<GaussPoint>();
            switch (dimension)
            {
                case 1: //only "x" or only "csi"
                    switch (pointsPerDimension)
                    {
                        case 1:
                            pts.Add(new GaussPoint(0, 0, 0, 2.0));
                            break;
                        case 2:
                            pts.Add(new GaussPoint(-1.0 / Math.Sqrt(3.0), 0.0, 0.0, 1.0));
                            pts.Add(new GaussPoint(+1.0 / Math.Sqrt(3.0), 0.0, 0.0, 1.0));
                            break;
                        case 3:
                            pts.Add(new GaussPoint(-Math.Sqrt(3.0 / 5.0), 0.0, 0.0, 5.0 / 9.0));
                            pts.Add(new GaussPoint(0.0, 0.0, 0.0, 8.0 / 9.0));
                            pts.Add(new GaussPoint(+Math.Sqrt(3.0 / 5.0), 0.0, 0.0, 5.0 / 9.0));
                            break;
                    }
                    break;
                case 2: //csi, eta -> x, y
                    switch (pointsPerDimension)
                    {
                        case 1:
                            pts.Add(new GaussPoint(0, 0, 0, 2.0));
                            break;
                        case 2:
                            pts.Add(new GaussPoint(-1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), 0.0, 1.0));
                            pts.Add(new GaussPoint(+1.0 / Math.Sqrt(3.0), -1.0 / Math.Sqrt(3.0), 0.0, 1.0));
                            pts.Add(new GaussPoint(-1.0 / Math.Sqrt(3.0), +1.0 / Math.Sqrt(3.0), 0.0, 1.0));
                            pts.Add(new GaussPoint(+1.0 / Math.Sqrt(3.0), +1.0 / Math.Sqrt(3.0), 0.0, 1.0));
                            break;
                        case 3:
                            pts.Add(new GaussPoint(-Math.Sqrt(3.0 / 5.0), -Math.Sqrt(3.0 / 5.0), 0.0, 25.0 / 81.0));
                            pts.Add(new GaussPoint(                  0.0, -Math.Sqrt(3.0 / 5.0), 0.0, 40.0 / 81.0));
                            pts.Add(new GaussPoint(+Math.Sqrt(3.0 / 5.0), -Math.Sqrt(3.0 / 5.0), 0.0, 25.0 / 81.0));
                            pts.Add(new GaussPoint(-Math.Sqrt(3.0 / 5.0),                   0.0, 0.0, 40.0 / 81.0));
                            pts.Add(new GaussPoint(                    0,                   0.0, 0.0, 64.0 / 81.0));
                            pts.Add(new GaussPoint(+Math.Sqrt(3.0 / 5.0),                   0.0, 0.0, 40.0 / 81.0));
                            pts.Add(new GaussPoint(-Math.Sqrt(3.0 / 5.0), +Math.Sqrt(3.0 / 5.0), 0.0, 25.0 / 81.0));
                            pts.Add(new GaussPoint(                  0.0, +Math.Sqrt(3.0 / 5.0), 0.0, 40.0 / 81.0));
                            pts.Add(new GaussPoint(+Math.Sqrt(3.0 / 5.0), +Math.Sqrt(3.0 / 5.0), 0.0, 25.0 / 81.0));
                            break;
                    }
                    break;
                case 3:
                    break;
            }
            return pts.ToArray();
        }

        public struct GaussPoint
        {
            public Point3d Point;
            public double Weight;
            public GaussPoint(double csi, double eta, double zeta, double weight)
            {
                Point = new Point3d(csi, eta, zeta);
                Weight = weight;
            }
        }
    }
}
