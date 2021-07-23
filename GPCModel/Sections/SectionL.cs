using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionL : ThinWallSection
    {
        #region Variables

        private readonly double _lHor;
        private readonly double _tHor;
        private readonly double _lVert;
        private readonly double _tVert;

        #endregion


        #region Properties

        public double LengthHor => _lHor;

        public double ThicknessHor => _tHor;

        public double LengthVert => _lVert;

        public double ThicknessVert => _tVert;

        #endregion


        #region Constructor

        public SectionL(double lHor, double tHor, double lVert, double tVert, Material material, string name)
            : base(material, name)
        {
            _lHor = lHor < 0 ? throw new ArgumentException($"Horizzontal plate lenght cannot be lower than zero") : lHor;
            _tHor = tHor < 0 ? throw new ArgumentException($"Horizzontal plate thickness cannot be lower than zero") : tHor;
            _lVert = lVert < 0 ? throw new ArgumentException($"Vertical plate lenght cannot be lower than zero") : lVert;
            _tVert = tVert < 0 ? throw new ArgumentException($"Vertical plate thickness cannot be lower than zero") : tVert;

            ThinWall thinWall1 = new ThinWall(LengthHor, ThicknessHor, 0);
            ThinWall thinWall2 = new ThinWall(LengthVert - ThicknessHor, ThicknessVert, Math.PI / 2);

            Points = new Point2d[] { new Point2d(LengthHor / 2, ThicknessHor / 2),
                    new Point2d(ThicknessVert / 2, ThicknessHor + (ThicknessVert - ThicknessHor) / 2)};

            ThinWalls = new ThinWall[] { thinWall1, thinWall2 };
        }

        #endregion


        #region Public method

        public override double CalculateWel1()
        {
            return Math.Min(CalculateWel11Left(), CalculateWel11Right());
        }

        public override double CalculateWel2()
        {
            return Math.Min(CalculateWel22Bottom(), CalculateWel22Top());
        }

        public double CalculateAngle()
        {
            double angle = -1.0 / 2.0 * Math.Atan(2.0 * CalculateJxy() / (_jyy - _jxx));

            if (Jyy < Jxx)
                angle += Math.PI / 2.0;

            return angle;
        }

        public override double CalculateJ11()
        {
            return (Jxx + Jyy) / 2.0 - 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * CalculateJxy() * CalculateJxy());
        }

        public override double CalculateJ22()
        {
            return (Jxx + Jyy) / 2.0 + 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * CalculateJxy() * CalculateJxy());
        }

        public double CalculateWel11Left()
        {
            FivePointsCheck(out double minX, out double _, out double _, out double _);
            return Jxx / Math.Abs(minX);
        }

        public double CalculateWel11Right()
        {
            FivePointsCheck(out double _, out double maxX, out double _, out double _);
            return Jxx / Math.Abs(maxX);
        }

        public double CalculateWel22Bottom()
        {
            FivePointsCheck(out double _, out double _, out double minY, out double _);
            return Jyy / Math.Abs(minY);
        }

        public double CalculateWel22Top()
        {
            FivePointsCheck(out double _, out double _, out double _, out double maxY);
            return Jyy / Math.Abs(maxY);
        }

        private void FivePointsCheck(out double minX, out double maxX, out double minY, out double maxY)
        {
            //check 5 points
            //traslation
            Point2d[] pts = new Point2d[5];
            pts[0] = new Point2d(-Centroid.X, -Centroid.Y);
            pts[1] = new Point2d(LengthHor - Centroid.X, -Centroid.Y);
            pts[2] = new Point2d(LengthHor - Centroid.X, ThicknessHor - Centroid.Y);
            pts[3] = new Point2d(ThicknessVert - Centroid.X, LengthVert - Centroid.Y);
            pts[4] = new Point2d(-Centroid.X, LengthVert - Centroid.Y);

            //rotation
            minX = 0;
            maxX = 0;
            minY = 0;
            maxY = 0;
            for (int i = 0; i < 5; i++)
            {
                double x = pts[i].X;
                double y = pts[i].Y;
                double newX = x * Math.Cos(AngleX1) + y * Math.Sin(AngleX1);
                double newY = -x * Math.Sin(AngleX1) + y * Math.Cos(AngleX1);
                pts[i] = new Point2d(newX, newY);

                minX = Math.Min(minX, pts[i].X);
                maxX = Math.Max(maxX, pts[i].X);
                minY = Math.Min(minY, pts[i].Y);
                maxY = Math.Max(maxY, pts[i].Y);
            }
        }

        public override double CalculateJxx()
        {
            double jxx = 0;
            for (int i = 0; i < ThinWalls.Count(); i++)
                jxx += +DistanceXCentroidFromLeft() + ThinWalls[i].Area * Math.Pow(Points[i].X - Centroid.X, 2.0);
            return jxx;
        }

        public override double CalculateJyy()
        {
            double jyy = 0;
            for (int i = 0; i < ThinWalls.Count(); i++)
                jyy += Points[i].Y + ThinWalls[i].Area * Math.Pow(Points[i].Y - Centroid.Y, 2.0);
            return jyy;
        }

        public double CalculateJxy()
        {
            double jxy = 0;
            for (int i = 0; i < ThinWalls.Count(); i++)
                jxy += +0.0 + ThinWalls[i].Area * (Points[i].X - Centroid.X) * (Points[i].Y - Centroid.Y);
            return jxy;
        }

        public double DistanceYCentroidFromBottom()
        {
            return Centroid.Y;
        }

        public double DistanceYCentroidFromTop()
        {
            return LengthVert - Centroid.Y;
        }

        public double DistanceXCentroidFromRight()
        {
            return Centroid.X;
        }

        public double DistanceXCentroidFromLeft()
        {
            return LengthHor - Centroid.X;
        }



        #endregion


        #region Public override method

        public override double CalculateJw()
        {
            return (Math.Pow(_lHor - _tVert / 2.0, 3.0) * Math.Pow(_tHor, 3.0) + Math.Pow(_lVert - _tHor / 2.0, 3.0) * Math.Pow(_tVert, 3.0)) / 36.0; //CNR DT 208/2011
        }

        public override double CalculateJt()
        {
            return 1.0 / 3.0 * (_lHor - _tVert / 2.0) * Math.Pow(_tHor, 3.0) + 1.0 / 3.0 * (_lVert - _tHor / 2.0) * Math.Pow(_tVert, 3.0);
        }

        public override Point2d CalculateShearCenter()
        {
            return new Point2d(_tHor / 2.0, _tVert / 2.0);
        }

        public override string ToString()
        {
            string s = "L section: \n";
            s = s + "height vertical = " + _lVert + " mm \n";
            s = s + "thickeness vertical = " + _tVert + " mm \n";
            s = s + "Length Bottom = " + _lHor + " mm \n";
            s = s + "Thickness Bottom = " + _tHor + " mm \n";

            return s;
        }

        public override double CalculateWpl1()
        {
            return CalculateWel1();     // TODO: implementare SectionL
        }

        public override double CalculateWpl2()
        {
            return CalculateWel2();
        }


        #endregion
    }


}
