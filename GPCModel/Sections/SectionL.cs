using System;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    [Serializable]
    public class SectionL : ThinWallSection, ISection, ISerializable
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
        public SectionL(double horizontalLegLength, double horizontalLegThickness, double verticalLegLength, double verticalLegThickness, 
            Material material, string name)
            : base(material, name)
        {
            _lHor = horizontalLegLength < 0 ? throw new ArgumentException($"Horizzontal plate lenght cannot be lower than zero") : horizontalLegLength;
            _tHor = horizontalLegThickness < 0 ? throw new ArgumentException($"Horizzontal plate thickness cannot be lower than zero") : horizontalLegThickness;
            _lVert = verticalLegLength < 0 ? throw new ArgumentException($"Vertical plate lenght cannot be lower than zero") : verticalLegLength;
            _tVert = verticalLegThickness < 0 ? throw new ArgumentException($"Vertical plate thickness cannot be lower than zero") : verticalLegThickness;

            ThinWall thinWall1 = new ThinWall(HorizontalLegLength, HorizontalLegThickness, 0);
            ThinWall thinWall2 = new ThinWall(VerticalLegLength - HorizontalLegThickness, VerticalLegThickness, Math.PI / 2);

            SetThinWalls(new ThinWall[] { thinWall1, thinWall2 },
                    new Point2d[] { new Point2d(HorizontalLegLength / 2, HorizontalLegThickness / 2),
                    new Point2d(VerticalLegThickness / 2, HorizontalLegThickness + (VerticalLegLength - HorizontalLegThickness) / 2)});

            SetMechanicalProperties();
        }

        protected SectionL(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _lHor = info.GetDouble("HorizontalLegLength");
            _tHor = info.GetDouble("HorizontalLegThickness");
            _lVert = info.GetDouble("VerticalLegLength");
            _tVert = info.GetDouble("VerticalLegThickness");
        }

        #endregion

        #region Public method

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("HorizontalLegLength", _lHor);
            info.AddValue("HorizontalLegThickness", _tHor);
            info.AddValue("VerticalLegLength", _lVert);
            info.AddValue("VerticalLegThickness", _tVert);
        }

        protected override void SetMechanicalProperties()
        {
            _area = CalculateArea();

            _centroid = CalculateCentroid();
            _isSymmetricAlongXLocalAxis = CalculateIsSymmetricAlongXLocalAxis();
            _isSymmetricAlongYLocalAxis = CalculateIsSymmetricAlongYLocalAxis();

            _jxx = CalculateJxx();
            _jyy = CalculateJyy();
            _jxy = CalculateJxy();

            _angleX1 = CalculateAngle();
            _j11 = CalculateJ11();
            _j22 = CalculateJ22();

            _jp = _jxx + _jyy;
            _jt = CalculateJt();
            _jw = CalculateJw();

            _shearCenter = CalculateShearCenter();

            var (WelTop, WelBottom, WelLeft, WelRight) = CalculateWel(AngleX1);
            _wel1Max = WelTop;
            _wel1Min = WelBottom;
            _wel2Max = WelRight;
            _wel2Min = WelLeft;
            _wpl1 = Math.Min(WelTop, WelBottom);
            _wpl2 = Math.Min(WelRight, WelLeft);

            var welL = CalculateWel(0.0);
            _welXMax = welL.WelTop;
            _welXMin = welL.WelBottom;
            _welYMax = welL.WelRight;
            _welYMin = welL.WelLeft;
            _wplX = Math.Min(welL.WelTop, welL.WelBottom);
            _wplY = Math.Min(welL.WelRight, welL.WelLeft);
        }

        protected override double CalculateWelXMax()
        {
            return CalculateWel(0.0).WelTop;
        }

        protected override double CalculateWelXMin()
        {
            return CalculateWel(0.0).WelBottom;
        }

        protected override double CalculateWelYMax()
        {
            return CalculateWel(0.0).WelRight;
        }

        protected override double CalculateWelYMin()
        {
            return CalculateWel(0.0).WelLeft;
        }

        protected override double CalculateWel1Max()
        {
            return CalculateWel(AngleX1).WelTop;
        }

        protected override double CalculateWel1Min()
        {
            return CalculateWel(AngleX1).WelBottom;
        }

        protected override double CalculateWel2Max()
        {
            return CalculateWel(AngleX1).WelRight;
        }

        protected override double CalculateWel2Min()
        {
            return CalculateWel(AngleX1).WelLeft;
        }

        protected override double CalculateAngle()
        {
            return -1.0 / 2.0 * Math.Atan(2.0 * CalculateJxy() / (Jyy - Jxx));
        }

        protected override double CalculateJ11()
        {
            return (Jxx + Jyy) / 2.0 + 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(CalculateJxy(), 2));
        }

        protected override double CalculateJ22()
        {
            return (Jxx + Jyy) / 2.0 - 0.5 * Math.Sqrt(Math.Pow(Jxx - Jyy, 2.0) + 4.0 * Math.Pow(CalculateJxy(), 2));
        }

        private (double WelTop, double WelBottom, double WelLeft, double WelRight) CalculateWel(double teta)
        {
            var (minX, maxX, minY, maxY) = FivePointsCheck(teta);
            double WelTop = Jxx / Math.Abs(maxY);
            double WelBottom = Jxx / Math.Abs(minY);
            double WelLeft = Jyy / Math.Abs(minX);
            double WelRight = Jyy / Math.Abs(maxX);

            return(WelTop, WelBottom, WelLeft, WelRight);
        }

        private (double minX, double maxX, double minY, double maxY) FivePointsCheck(double angle)
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
            double minX = 0;
            double maxX = 0;
            double minY = 0;
            double maxY = 0;
            for (int i = 0; i < 5; i++)
            {
                double x = pts[i].X;
                double y = pts[i].Y;
                double newX = x * Math.Cos(angle) + y * Math.Sin(angle);
                double newY = -x * Math.Sin(angle) + y * Math.Cos(angle);
                pts[i] = new Point2d(newX, newY);

                minX = Math.Min(minX, pts[i].X);
                maxX = Math.Max(maxX, pts[i].X);
                minY = Math.Min(minY, pts[i].Y);
                maxY = Math.Max(maxY, pts[i].Y);
            }

            return(minX, maxX, minY, maxY);
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


        protected override Shape2d GetShape()
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

        protected override double CalculateWpl1()
        {
            return Math.Min(_wel1Max, _wel1Min);
        }

        protected override double CalculateWpl2()
        {
            return Math.Min(_wel2Max, _wel2Min); 
        }

        protected override Point2d CalculateCentroid()
        {
            double xc = ((_points[0].X * _thinWalls[0].Area) + (_points[1].X * _thinWalls[1].Area)) / Area;
            double yc = ((_points[0].Y * _thinWalls[0].Area) + (_points[1].Y * _thinWalls[1].Area)) / Area;
            return new Point2d(xc, yc);
        }

        #endregion

        public override string ToString()
        {
            return $"L {_lVert}x{_tVert}x{_lHor}x{_thinWalls}";
        }
    }
}
