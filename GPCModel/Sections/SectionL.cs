using System;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionL : ThinWallSection, ISection
    {
        #region Variables

        private readonly double _lHor;
        private readonly double _tHor;
        private readonly double _lVert;
        private readonly double _tVert;

        #endregion


        #region Properties

        public double HorizontalLegLength => _lHor;

        public double HorizontalLegThickness => _tHor;

        public double VerticalLegLength => _lVert;

        public double VerticalLegThickness => _tVert;

        #endregion


        #region Constructor

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="horizontalLegLength">The horizontal leg length</param>
        /// <param name="horizontalLegThickness">The horizontal leg thickness</param>
        /// <param name="verticalLegLength">The vertical leg length</param>
        /// <param name="verticalLegThickness">The vertical leg thickness</param>
        /// <param name="material">Material of the section</param>
        /// <param name="name">Name of the section</param>
        public SectionL(double horizontalLegLength, double horizontalLegThickness, double verticalLegLength, double verticalLegThickness, Material material, string name)
            : base(material, name)
        {
            _lHor = horizontalLegLength < 0 ? throw new ArgumentException($"Horizzontal plate lenght cannot be lower than zero") : horizontalLegLength;
            _tHor = horizontalLegThickness < 0 ? throw new ArgumentException($"Horizzontal plate thickness cannot be lower than zero") : horizontalLegThickness;
            _lVert = verticalLegLength < 0 ? throw new ArgumentException($"Vertical plate lenght cannot be lower than zero") : verticalLegLength;
            _tVert = verticalLegThickness < 0 ? throw new ArgumentException($"Vertical plate thickness cannot be lower than zero") : verticalLegThickness;

            ThinWall thinWall1 = new ThinWall(HorizontalLegLength, HorizontalLegThickness, 0);
            ThinWall thinWall2 = new ThinWall(VerticalLegLength - HorizontalLegThickness, VerticalLegThickness, Math.PI / 2);

            Points = new Point2d[] { new Point2d(HorizontalLegLength / 2, HorizontalLegThickness / 2),
                    new Point2d(VerticalLegThickness / 2, HorizontalLegThickness + (VerticalLegLength - HorizontalLegThickness) / 2)};

            ThinWalls = new ThinWall[] { thinWall1, thinWall2 };
        }

        #endregion


        #region Public method

        protected override void SetMechanicalProperties()
        {
            _area = CalculateArea();
            _centroid = CalculateCentroid();
            _jxx = CalculateJxx();
            _jyy = CalculateJyy();
            _j11 = CalculateJ11();
            _j22 = CalculateJ22();
            _jw = CalculateJw();
            _jt = CalculateJt();
            _shearCenter = CalculateShearCenter();

            CalculateWel(out double Wel11Top, out double Wel11Bottom, out double Wel22Left, out double Wel22Right);

            _wel1 = Math.Min(Wel11Bottom, Wel11Top);
            _wel2 = Math.Min(Wel22Left, Wel22Right);
            _wpl1 = CalculateWpl1();
            _wpl2 = CalculateWpl2();
            _angleX1 = CalculateAngle();
        }

        protected override double CalculateWel1()
        {
            CalculateWel(out double Wel11Top, out double Wel11Bottom, out double _, out double _);
            return Math.Min(Wel11Bottom, Wel11Top);
        }

        protected override double CalculateWel2()
        {
            CalculateWel(out double _, out double _, out double Wel22Left, out double Wel22Right);
            return Math.Min(Wel22Left, Wel22Right);
        }

        protected override double CalculateAngle()
        {
            double angle = -1.0 / 2.0 * Math.Atan(2.0 * CalculateJxy() / (Jyy - Jxx));

            if (Jyy < Jxx)
                angle += Math.PI / 2.0;

            return angle;
        }

        protected override double CalculateJ11()
        {
            return (Jxx + Jyy) / 2.0 + 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(CalculateJxy(), 2));
        }

        protected override double CalculateJ22()
        {
            return (Jxx + Jyy) / 2.0 - 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(CalculateJxy(), 2));
        }
               
        private void CalculateWel(out double Wel11Top, out double Wel11Bottom, out double Wel22Left, out double Wel22Right)
		{
            FivePointsCheck(out double minX, out double maxX, out double minY, out double maxY);
            Wel11Top = Jxx / Math.Abs(maxY);
            Wel11Bottom = Jxx / Math.Abs(minY);
            Wel22Left = Jyy / Math.Abs(minX);
            Wel22Right = Jyy / Math.Abs(maxX);
        }

        private void FivePointsCheck(out double minX, out double maxX, out double minY, out double maxY)
        {
            //check 5 points
            //traslation
            Point2d[] pts = new Point2d[5];
            pts[0] = new Point2d(-Centroid.X, -Centroid.Y);
            pts[1] = new Point2d(HorizontalLegLength - Centroid.X, -Centroid.Y);
            pts[2] = new Point2d(HorizontalLegLength - Centroid.X, HorizontalLegThickness - Centroid.Y);
            pts[3] = new Point2d(VerticalLegThickness - Centroid.X, VerticalLegLength - Centroid.Y);
            pts[4] = new Point2d(-Centroid.X, VerticalLegLength - Centroid.Y);

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

        protected override double CalculateJxx()
        {
            return (1.0 / 3.0) * (HorizontalLegLength * Math.Pow(VerticalLegLength, 3) - (HorizontalLegLength - VerticalLegThickness) * Math.Pow(VerticalLegLength - HorizontalLegThickness, 3)) - 
                Area * Math.Pow(VerticalLegLength - Centroid.Y, 2);
        }

        protected override double CalculateJyy()
        {
            return (1.0 / 3.0) * (VerticalLegLength * Math.Pow(HorizontalLegLength, 3) - (VerticalLegLength - HorizontalLegThickness) * Math.Pow(HorizontalLegLength - VerticalLegThickness, 3)) -
                Area * Math.Pow(HorizontalLegLength - Centroid.X, 2);
        }

        #endregion


        #region Public override method

        public override Shape2d GetShape()
        {
            throw new NotImplementedException();
        }

        protected override double CalculateJw()
        {
            return (Math.Pow(_lHor - _tVert / 2.0, 3.0) * Math.Pow(_tHor, 3.0) + Math.Pow(_lVert - _tHor / 2.0, 3.0) * Math.Pow(_tVert, 3.0)) / 36.0; //CNR DT 208/2011
        }

        protected override double CalculateJt()
        {
            return 1.0 / 3.0 * (_lHor - _tVert / 2.0) * Math.Pow(_tHor, 3.0) + 1.0 / 3.0 * (_lVert - _tHor / 2.0) * Math.Pow(_tVert, 3.0);
        }

        protected override Point2d CalculateShearCenter()
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

        protected override double CalculateWpl1()
        {
            return _wel1;
        }

        protected override double CalculateWpl2()
        {
            return _wel2;
        }

        protected override Point2d CalculateCentroid()
        {
            double xc = ((Points[0].X * ThinWalls[0].Area) + (Points[1].X * ThinWalls[1].Area)) / Area;
            double yc = ((Points[0].Y * ThinWalls[0].Area) + (Points[1].Y * ThinWalls[1].Area)) / Area;
            return new Point2d(xc, yc);
        }

        #endregion
    }


}
