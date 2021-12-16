using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
#if False
    public class SteelSectionGeneric : ThinWallSection
    {
        public SteelSectionGeneric(Line2d[] thinWalls, double[] thickness, SteelMaterial material, string name)
            : base(material, name)
        {
            if (thinWalls.Length != thickness.Length)
                throw new ArgumentException("thinWalls and thickness must have the same length");

            ThinWall[] tws = new ThinWall[thinWalls.Length];
            Point2d[] pts = new Point2d[thinWalls.Length];

            for (int i = 0; i < thinWalls.Length; i++)
            {
                tws[i] = new ThinWall(thinWalls[i].GetLength(), thickness[i], thinWalls[i].ToVector().AngleTo(new Vector2d(1, 0)));
                pts[i] = new Point2d((thinWalls[i].Start + thinWalls[i].End) / 2.0);
            }

            Points = pts;
            ThinWalls = tws;
        }

        public override Shape2d GetShape()
        {
            throw new NotImplementedException();
        }

        protected override double CalculateJw()
        {
            return 0;
            // TODO: implementare
        }

        protected override Point2d CalculateShearCenter()
        {
            return new Point2d(0, 0);
            // TODO: implementare
        }

        protected override double CalculateWel1()
        {
            return 0;
            // TODO: implementare
        }

        protected override double CalculateWel2()
        {
            return 0;
            // TODO: implementare
        }

        protected override double CalculateWpl1()
        {
            return 0;
            // TODO: implementare
        }

        protected override double CalculateWpl2()
        {
            return 0;
            // TODO: implementare
        }


    } 



#endif
}
