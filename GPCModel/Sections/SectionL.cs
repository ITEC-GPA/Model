using System;
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

        public double LHor => _lHor;

        public double THor => _tHor;

        public double LVert => _lVert;

        public double TVert => _tVert;

        #endregion


        #region Constructor

        public SectionL(double lHor, double tHor, double lVert, double tVert, Material material, string name) 
            : base(material, name)
        {
            _lHor = lHor < 0 ? throw new ArgumentException($"Horizzontal plate lenght cannot be lower than zero") : lHor; 
            _tHor = tHor < 0 ? throw new ArgumentException($"Horizzontal plate thickness cannot be lower than zero") : tHor; 
            _lVert = lVert < 0 ? throw new ArgumentException($"Vertical plate lenght cannot be lower than zero") : lVert; 
            _tVert = tVert < 0 ? throw new ArgumentException($"Vertical plate thickness cannot be lower than zero") : tVert; 

            ThinWall thinWall1 = new ThinWall(lHor, tHor, 0, new Point2d(lHor / 2, 0));
            ThinWall thinWall2 = new ThinWall(lVert, tVert, Math.PI / 2, new Point2d(0, lVert / 2));

            ThinWalls = new ThinWall[] { thinWall1, thinWall2 };
        }

        #endregion


        #region Public method

        public double CalculateWel11Min()
        {
            return Math.Min(CalculateWel11Left(), CalculateWel11Right());
        }

        public double CalculateWel22Min()
        {
            return Math.Min(CalculateWel22Bottom(), CalculateWel22Top());
        }

        public double CalculateJxy()
        {
            return 0;
        }

        public double CalculateAngle()
        {
            double angle = -1.0 / 2.0 * Math.Atan(2.0 * CalculateJxy() / (_jyy - _jxx));

            if (_jyy < _jxx)            
                angle += Math.PI / 2.0;

            return angle;
        }

        public double CalculateJ11()
        {
            return (_jxx + _jyy) / 2.0 - 0.5 * Math.Sqrt(Math.Pow(_jxx - _jyy, 2.0) + 4.0 * CalculateJxy() * CalculateJxy());
        }

        public double CalculateJ22()
        {
            return (_jxx + _jyy) / 2.0 + 0.5 * Math.Sqrt(Math.Pow(_jxx - _jyy, 2.0) + 4.0 * CalculateJxy() * CalculateJxy());
        }

        public double CalculateWel11Left()
        {
            FivePointsCheck(out double minX, out double _, out double _, out double _);
            return _jxx / Math.Abs(minX);
        }

        public double CalculateWel11Right()
        {
            FivePointsCheck(out double _, out double maxX, out double _, out double _);
            return _jxx / Math.Abs(maxX);
        }

        public double CalculateWel22Bottom()
        {
            FivePointsCheck(out double _, out double _, out double minY, out double _);
            return _jyy / Math.Abs(minY);
        }

        public double CalculateWel22Top()
        {
            FivePointsCheck(out double _, out double _, out double _, out double maxY);
            return _jyy / Math.Abs(maxY);
        }

        private void FivePointsCheck(out double minX, out double maxX, out double minY, out double maxY)
        {
            //check 5 points
            //traslation
            Point2d[] pts = new Point2d[5];
            pts[0] = new Point2d(-CalculateCentroid().X, -CalculateCentroid().Y);
            pts[1] = new Point2d(LHor - CalculateCentroid().X, -CalculateCentroid().Y);
            pts[2] = new Point2d(LHor - CalculateCentroid().X, THor - CalculateCentroid().Y);
            pts[3] = new Point2d(TVert - CalculateCentroid().X, LVert - CalculateCentroid().Y);
            pts[4] = new Point2d(-CalculateCentroid().X, LVert - CalculateCentroid().Y);

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

        #endregion
    }
}
