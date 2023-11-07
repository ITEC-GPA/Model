using GPC.Geometry;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    [Serializable]
    public class SectionT : ThinWallSection, ISerializable
    {
        #region Variables

        protected double _h;
        protected double _tw;
        protected double _tf;
        protected double _b;
        private readonly double _r;                // raggio di curvatura o altezza di gola

        #endregion

        #region Properties

        public override double Height
        {
            get => _h;
            set
            {
                if (value != _h)
                {
                    _h = value;
                    CalculateSection();
                }
            }
        }

        public double HeightWeb => _h - _tf;

        public double ThicknessWeb
        {
            get => _tw;
            set
            {
                if (value != _tw)
                {
                    _tw = value;
                    CalculateSection();
                }
            }
        }

        public double ThicknessFlange
        {
            get => _tf;
            set
            {
                if (value != _tf)
                {
                    _tf = value;
                    CalculateSection();
                }
            }
        }

        public double LenghtFlange
        {
            get => _b;
            set
            {
                if (value != _b)
                {
                    _b = value;
                    CalculateSection();
                }
            }
        }

        public double R => _r;

        #endregion

        #region Public Constructors

        public SectionT(double height, double flangeLength, double thicknessWeb, double thicknessFlange, string name,
            double radius = 0) : base(name)
        {
            #region Check inputs

            _h = height < 0 ? throw new ArgumentException($"Web lenght cannot be lower than zero") : height;                   // spessore anima;
            _b = flangeLength < 0 ? throw new ArgumentException($"Flange lenght cannot be lower than zero") : flangeLength;                   // spessore anima;
            _tw = thicknessWeb < 0 ? throw new ArgumentException($"Web thickness cannot be lower than zero") : thicknessWeb;                // spessore anima;
            _tf = thicknessFlange < 0 ? throw new ArgumentException($"Flange thickness cannot be lower than zero") : thicknessFlange;             // spessore flangia;
            _r = radius;        // raggio di curvatura o altezza di gola

            #endregion

            CalculateSection();
        }

        public SectionT(SectionT sectionT)
            : this(sectionT.Height, sectionT.LenghtFlange, sectionT.ThicknessWeb, sectionT.ThicknessFlange, sectionT.Name)
        {

        }

        protected SectionT(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionTVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _h = info.GetDouble("Height");
            _tw = info.GetDouble("ThicknessWeb");
            _tf = info.GetDouble("ThicknessFlange");
            _b = info.GetDouble("LenghtFlange");
            _r = info.GetDouble("R");
        }

        #endregion

        #region Public method

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("SectionTVersion", version);

            info.AddValue("Height", _h);
            info.AddValue("ThicknessWeb", _tw);
            info.AddValue("ThicknessFlange", _tf);
            info.AddValue("LenghtFlange", _b);
            info.AddValue("R", _r);
        }

        protected override double CalculateWel2Max()
        {
            return CalculateWelYMax();
        }

        protected override double CalculateWel2Min()
        {
            return CalculateWelYMin();
        }

        protected override double CalculateWel1Max()
        {
            return CalculateWelXMax();
        }

        protected override double CalculateWel1Min()
        {
            return CalculateWelXMin();
        }

        protected override double CalculateWpl1()
        {
            if (_area / 2.0 >= _b * _tf)
            {
                double yPlastic = _area / 2.0 / _tw;
                var halfSectionTop = new SectionT(Height - yPlastic, _b, _tw, _tf, string.Empty);
                return _area / 2.0 * (halfSectionTop.DistanceYCentroidFromBottom() + yPlastic / 2.0);
            }
            else
            {
                double hTopPlastic = (_area / 2.0) / _b;
                //can't use SectionT because infinite loop
                double Aweb = _tw * (Height - _tf);
                double Aflange = _b * (_tf - hTopPlastic);
                double S = Aweb * ((Height - _tf) / 2.0 + hTopPlastic) + Aflange * hTopPlastic / 2.0;
                return (_area / 2.0) * (hTopPlastic / 2.0 + S / (Aweb + Aflange));
            }
        }

        protected override double CalculateWpl2()
        {
            return 1.0 / 4.0 * _tf * Math.Pow(_b, 2.0) + 1.0 / 4.0 * (Height - _tf) * Math.Pow(_tw, 2.0);
        }

        protected override double CalculateWelYMin()
        {
            return J22 / DistanceXCentroidFromRight();
        }

        protected override double CalculateWelYMax()
        {
            return J22 / (_b - DistanceXCentroidFromRight());
        }

        protected override double CalculateWelXMin()
        {
            return J11 / DistanceYCentroidFromBottom();
        }

        protected override double CalculateWelXMax()
        {
            return J11 / (Height - DistanceYCentroidFromBottom());
        }

        internal virtual double DistanceYCentroidFromBottom()
        {
            return CalculateCentroid().Y;
        }

        internal virtual double DistanceYCentroidFromTop()
        {
            return Height - CalculateCentroid().Y;
        }

        internal virtual double DistanceXCentroidFromRight()
        {
            return LenghtFlange - CalculateCentroid().X;
        }

        internal virtual double DistanceXCentroidFromLeft()
        {
            return CalculateCentroid().X;
        }

        #endregion

        #region Public override method

        protected override Shape2d GetShape()
        {
            return new Shape2d(new Polygon2d(new Point2d[] {
                new Point2d(0.0, Height),
                new Point2d(LenghtFlange, Height),
                new Point2d(LenghtFlange, HeightWeb),
                new Point2d(LenghtFlange / 2.0 + ThicknessWeb / 2.0, HeightWeb),
                new Point2d(LenghtFlange / 2.0 + ThicknessWeb / 2.0, 0.0),
                new Point2d(LenghtFlange / 2.0 - ThicknessWeb / 2.0, 0.0),
                new Point2d(LenghtFlange / 2.0 - ThicknessWeb / 2.0, HeightWeb),
                new Point2d(0.0, HeightWeb) }));
        }

        protected override Point2d CalculateShearCenter()
        {
            return new Point2d(_b / 2.0, Height - _tf / 2.0);
        }

        protected override double CalculateJw()
        {
            return Math.Pow(_b, 3.0) * Math.Pow(_tf, 3.0) / 144.0 + Math.Pow(Height - _tf / 2.0, 3.0) * Math.Pow(_tw, 3.0) / 36.0; //Bleich 1952, Picard and Beaulieu 1991
        }

        protected override double CalculateJt()
        {
            return (_b * Math.Pow(_tf, 3.0) + (Height - _tf / 2.0) * Math.Pow(_tw, 3.0)) / 3.0;
        }

        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {
            return false;
        }

        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            return true;
        }

        public override string ToString()
        {
            string s = "T section: \n";
            s = s + "Height = " + Height + " mm \n";
            s = s + "Thickness Web = " + _tw + " mm \n";
            s = s + "Length Top = " + _b + " mm \n";
            s = s + "Thickness Top = " + _tf + " mm \n";
            return s;
        }

        private void CalculateSection()
        {
            ThinWall web = new ThinWall(HeightWeb, ThicknessWeb, Math.PI / 2,
                new Point2d(LenghtFlange / 2, HeightWeb / 2));
            ThinWall flange = new ThinWall(LenghtFlange, ThicknessFlange, 0,
                new Point2d(LenghtFlange / 2, HeightWeb + ThicknessFlange / 2));

            SetThinWalls(new ThinWall[] { web, flange });

            SetMechanicalProperties();
            _shape = null;
            _mesh = GetMesh();
        }

        #endregion
    }
}
