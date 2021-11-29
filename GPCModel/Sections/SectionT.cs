using System;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionT : ThinWallSection, ISection
    {
        #region Variables

        protected readonly double _h;
        protected readonly double _tw;
        protected readonly double _tf;
        protected readonly double _b;

        #endregion


        #region Properties

        public double Height => _h;

        public double HeightWeb => _h - _tf;

        public double ThicknessWeb => _tw;

        public double ThicknessFlange => _tf;

        public double LenghtFlange => _b;

        #endregion


        #region Public Constructors

        public SectionT(double height, double flangeLength, double thicknessWeb, double thicknessFlange, Material material, string name)
            : base(material, name)
        {
            #region Check inputs

            _h = height < 0 ? throw new ArgumentException($"Web lenght cannot be lower than zero") : height;                   // spessore anima;
            _b = flangeLength < 0 ? throw new ArgumentException($"Flange lenght cannot be lower than zero") : flangeLength;                   // spessore anima;
            _tw = thicknessWeb < 0 ? throw new ArgumentException($"Web thickness cannot be lower than zero") : thicknessWeb;                // spessore anima;
            _tf = thicknessFlange < 0 ? throw new ArgumentException($"Flange thickness cannot be lower than zero") : thicknessFlange;             // spessore flangia;

            #endregion

            ThinWall web = new ThinWall(HeightWeb, thicknessWeb, Math.PI / 2);
            ThinWall flange = new ThinWall(flangeLength, thicknessFlange, 0);


            SetThinWalls(new ThinWall[] { web, flange },
                    new Point2d[] { new Point2d(LenghtFlange / 2, HeightWeb / 2) ,
                    new Point2d(LenghtFlange / 2, HeightWeb + thicknessFlange / 2)});

            SetMechanicalProperties();

        }

        public SectionT(SectionT sectionT)
            : this(sectionT.Height, sectionT.LenghtFlange, sectionT.ThicknessWeb, sectionT.ThicknessFlange, sectionT.Material, sectionT.Name)
        {

        }

        #endregion


        #region Public method

        protected override double CalculateWel2()
        {
            return Math.Min(CalculateWelyLeft(), CalculateWelyRight());
        }

        protected override double CalculateWel1()
        {
            return Math.Min(CalculateWelxBottom(), CalculateWelxTop());
        }

        protected override double CalculateWpl1()
        {
            if (_area / 2.0 >= _b * _tf)
            {
                double yPlastic = _area / 2.0 / _tw;
                SectionT halfSectionTop = new SectionT(Height - yPlastic, _b, _tw, _tf, _material, string.Empty);
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

        protected virtual double CalculateWelyLeft()
        {
            return J22 / DistanceXCentroidFromRight();
        }

        protected virtual double CalculateWelyRight()
        {
            return J22 / (_b - DistanceXCentroidFromRight());
        }

        protected virtual double CalculateWelxBottom()
        {
            return J11 / DistanceYCentroidFromBottom();
        }

        protected virtual double CalculateWelxTop()
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
                                                            new Point2d(LenghtFlange / 2.0 + ThicknessWeb / 2.0 , HeightWeb),
                                                            new Point2d(LenghtFlange / 2.0 + ThicknessWeb / 2.0 , 0.0),
                                                            new Point2d(LenghtFlange / 2.0 - ThicknessWeb / 2.0 , 0.0),
                                                            new Point2d(LenghtFlange / 2.0 - ThicknessWeb / 2.0 , HeightWeb),
                                                            new Point2d(0.0 , HeightWeb)
                                                        }));
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
            return $"T {_h}x{_tw}x{_b}x{_tf}";
        }


        #endregion

    }
}
