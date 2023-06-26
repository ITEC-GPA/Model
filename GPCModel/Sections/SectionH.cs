using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    [Serializable]
    public class SectionH : ThinWallSection, ISection, ISerializable
    {
        #region Variables

        protected double _h;
        protected double _tw;
        protected double _ttop;
        protected double _tbottom;
        protected double _btop;
        protected double _bbottom;
        private readonly double _r;

        #endregion

        #region Properties

        public override double Height
        {
            get => _h;
            set
            {
                if(_h != value)
                {
                    _h = value;
					CalculateSection();
                }
            }
        }

        public double LenghtBottomFlange
        {
			get => _bbottom;
            set
			{
				if (_bbottom != value)
				{
					_bbottom = value;
					CalculateSection();
				}
			}
		}

        public double LenghtTopFlange 
        {
			get => _btop;
            set
			{
				if (_btop != value)
				{
					_btop = value;
					CalculateSection();

				}
			}
		}

        public double ThicknessTopFlange
        {
			get => _ttop; 
            set
			{
				if (_ttop != value)
				{
					_ttop = value;
					CalculateSection();
				}
			}
		}

        public double ThicknessBottomFlange
        {
			get => _tbottom; 
            set
			{
				if (_tbottom != value)
				{
					_tbottom = value;
					CalculateSection();
				}
			}
		}

        public double ThicknessWeb 
        {
			get => _tw; 
            set
			{
				if (_tw != value)
				{
					_tw = value;
					CalculateSection();
				}
			}
		}

        public double HeightWeb => Height - ThicknessBottomFlange - ThicknessTopFlange;

        /// <summary>
        /// Fillet radius.
        /// </summary>
        public double R => _r;

        public double D => Height - ThicknessBottomFlange - ThicknessTopFlange - 2.0 * R;

        #endregion

        #region Public Constructors

        public SectionH(double height, double thicknessWeb, double topFlangeLength, double topFlangeThickness, double bottomFlangeLength,
            double bottomFlangeThickness, Material material, string name, double radius = 0)
            : base(material, name)
        {
            #region Check inputs

            _h = height < 0 ? throw new ArgumentException($"Web lenght cannot be lower than zero") : height;                               // altezza anima
            _tw = thicknessWeb < 0 ? throw new ArgumentException($"Web _thickness cannot be lower than zero") : thicknessWeb;                            // spessore anima
            _btop = topFlangeLength < 0 ? throw new ArgumentException($"Top flange lenght cannot be lower than zero") : topFlangeLength;                  // larghezza piattabanda superiore
            _bbottom = bottomFlangeLength < 0 ? throw new ArgumentException($"Bottom flange lenght cannot be lower than zero") : bottomFlangeLength;      // larghezza piattabanda inferiore
            _ttop = topFlangeThickness < 0 ? throw new ArgumentException($"Top flange _thickness cannot be lower than zero") : topFlangeThickness;               // spessore piattabanda superiore
            _tbottom = bottomFlangeThickness < 0 ? throw new ArgumentException($"Bottom flange _thickness cannot be lower than zero") : bottomFlangeThickness;   // spessore piattabanda inferiore
            _r = radius < 0.0 ? 0 : radius;        // altezza di gola o raggio di curvatura

            #endregion

            CalculateSection();
        }

        protected SectionH(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionHVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _h = info.GetDouble("Height");
            _bbottom = info.GetDouble("LenghtBottomFlange");
            _btop = info.GetDouble("LenghtTopFlange");
            _ttop = info.GetDouble("ThicknessTopFlange");
            _tbottom = info.GetDouble("ThicknessBottomFlange");
            _tw = info.GetDouble("ThicknessWeb");
            _r = info.GetDouble("R");
        }

        #endregion

        #region Protected override method

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("SectionHVersion", version);

            info.AddValue("Height", _h);
            info.AddValue("LenghtBottomFlange", _bbottom);
            info.AddValue("LenghtTopFlange", _btop);
            info.AddValue("ThicknessTopFlange", _ttop);
            info.AddValue("ThicknessBottomFlange", _tbottom);
            info.AddValue("ThicknessWeb", _tw);
            info.AddValue("R", _r);
        }

        protected override Shape2d GetShape()
        {
            throw new NotImplementedException();
        }

        public virtual double DistanceYCentroidFromBottom()
        {
            return CalculateCentroid().Y;
        }

        public virtual double DistanceYCentroidFromTop()
        {
            return Height - DistanceYCentroidFromBottom();
        }

        public virtual double DistanceXCentroidFromRight()
        {
            return CalculateCentroid().X;
        }

        protected override double CalculateJxx()
        {
            return base.CalculateJxx() + CalculateAdditionaJxx();
        }

        protected override double CalculateJyy()
        {
            return base.CalculateJyy() + CalculateAdditionaJyy();
        }

        protected override double CalculateJxy()
        {
            return 0;
        }

        private double CalculateAdditionaJxx()
        {
            if (_edgeWorking == EdgeType.Chamfer)
            {
                return 4 * (Math.Pow((1.41 * _r), 4) / 24.0) +
                    CalculateAdditionalArea() / 2 * Math.Pow(Height - Centroid.Y - ThicknessTopFlange - R / 6.0, 2) +
                    CalculateAdditionalArea() / 2 * Math.Pow(Centroid.Y - ThicknessBottomFlange - R / 6.0, 2);
            }
            else if (_edgeWorking == EdgeType.Fillet)
            {
                return 4.0 * ((1.0 / 3.0) * Math.Pow(_r, 4.0) - (Math.PI / 16.0) * Math.Pow(_r, 4.0)) +
                    CalculateAdditionalArea() / 2 * Math.Pow(Height - Centroid.Y - ThicknessTopFlange - R / 6.0, 2) +
                    CalculateAdditionalArea() / 2 * Math.Pow(Centroid.Y - ThicknessBottomFlange - R / 6.0, 2);
            }
            else
                return 0.0;
        }

        private double CalculateAdditionaJyy()
        {
            if (_edgeWorking == EdgeType.Chamfer)
            {
                return 4 * (Math.Pow((1.41 * R), 4) / 24.0) + CalculateAdditionalArea() * Math.Pow(ThicknessWeb / 2, 2);
            }
            else if (_edgeWorking == EdgeType.Fillet)
            {
                return 4.0 * ((1.0 / 3.0) * Math.Pow(R, 4.0) - (Math.PI / 16.0) * Math.Pow(R, 4.0)) + CalculateAdditionalArea() * Math.Pow(ThicknessWeb / 2.0, 2);
            }
            else
                return 0.0;
        }

        private double CalculateAdditionalArea()
        {
            if (_edgeWorking == EdgeType.Chamfer)
                return 4 * Math.Pow((1.41 * R), 2) / 2.0;

            else if (_edgeWorking == EdgeType.Fillet)
                return 4 * (Math.Pow(R, 2) - Math.Pow(R, 2) * Math.PI / 4.0);

            else
                return 0.0;
        }

        protected override double CalculateArea()
        {
            return base.CalculateArea() + CalculateAdditionalArea();
        }

        protected override double CalculateWpl2()
        {
            SectionT halfSectionTop = new SectionT(LenghtTopFlange / 2.0, Height / 2.0, ThicknessTopFlange,
                ThicknessWeb / 2.0, Material, string.Empty);
            SectionT halfSectionBottom = new SectionT(LenghtBottomFlange / 2.0, Height / 2.0, ThicknessBottomFlange,
                ThicknessWeb / 2.0, Material, string.Empty);

            double d = (halfSectionTop.Area * (LenghtTopFlange / 2.0 - halfSectionTop.DistanceYCentroidFromBottom()) +
                halfSectionBottom.Area * (LenghtBottomFlange / 2.0 - halfSectionBottom.DistanceYCentroidFromBottom())) /
                (halfSectionBottom.Area + halfSectionTop.Area);

            return 2.0 * d * _area / 2.0;
        }

        protected override double CalculateWpl1()
        {
            if (_area / 2.0 >= LenghtTopFlange * ThicknessTopFlange && _area / 2.0 >= LenghtBottomFlange * ThicknessBottomFlange)
            {
                double hw = (_area / 2.0 - LenghtTopFlange * ThicknessTopFlange) / ThicknessWeb;

                SectionT halfSectionTop = new SectionT(hw + ThicknessTopFlange, LenghtTopFlange, ThicknessWeb,
                    ThicknessTopFlange, Material, string.Empty);
                SectionT halfSectionBottom = new SectionT(Height - ThicknessTopFlange - hw, LenghtBottomFlange,
                    ThicknessWeb, ThicknessBottomFlange, Material, string.Empty);

                return _area / 2.0 * (halfSectionTop.DistanceYCentroidFromBottom() + halfSectionBottom.DistanceYCentroidFromBottom());
            }
            else if (_area / 2.0 <= LenghtTopFlange * ThicknessTopFlange)
            {
                double hHalf = _area / 2.0 / LenghtTopFlange;

                SectionH halfSectionBottom = new SectionH(Height - hHalf, ThicknessWeb, LenghtTopFlange,
                    ThicknessTopFlange - hHalf, LenghtBottomFlange, ThicknessBottomFlange, Material, string.Empty);

                return _area / 2.0 * (hHalf / 2.0 + (Height - hHalf - halfSectionBottom.DistanceYCentroidFromBottom()));
            }
            else if (_area / 2.0 <= LenghtBottomFlange * ThicknessBottomFlange)
            {
                double hHalf = _area / 2.0 / LenghtBottomFlange;

                SectionH halfSectionBottom = new SectionH(Height - hHalf, ThicknessWeb, LenghtTopFlange, ThicknessTopFlange,
                    LenghtBottomFlange, ThicknessBottomFlange - hHalf, Material, string.Empty);

                return _area / 2.0 * (hHalf / 2.0 + halfSectionBottom.DistanceYCentroidFromBottom());
            }
            else
                throw new NotImplementedException("Cannot calculate Wpl : Plastic neutral axis in flanges...to be implemented");
        }

        protected override double CalculateWel2Min()
        {
            return J22 / (LenghtBottomFlange - DistanceXCentroidFromRight());
        }

        protected override double CalculateWel2Max()
        {
            return J22 / (LenghtTopFlange - DistanceXCentroidFromRight());
        }

        protected override double CalculateWel1Min()
        {
            return J11 / DistanceYCentroidFromBottom();
        }

        protected override double CalculateWel1Max()
        {
            return J11 / DistanceYCentroidFromTop();
        }

        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {
            if (_btop == _bbottom && _tbottom == _ttop)
                return true;

            return false;
        }

        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            return true;
        }

        protected override Point2d CalculateShearCenter()
        {
            //CNR DT208_2011 --> to be checked
            double JFlTop = 1.0 / 12.0 * ThicknessTopFlange * Math.Pow(LenghtTopFlange, 3.0);
            double JFlBottom = 1.0 / 12.0 * ThicknessBottomFlange * Math.Pow(LenghtBottomFlange, 3.0);
            double jz = JFlTop + JFlBottom + 1.0 / 12.0 * HeightWeb * Math.Pow(ThicknessWeb, 3.0);
            double zBottom = CalculateCentroid().Y - ThicknessBottomFlange / 2.0;
            double zTop = Height - ThicknessTopFlange / 2.0 - CalculateCentroid().Y;

            return new Point2d(CalculateCentroid().X, CalculateCentroid().Y - (zBottom * JFlBottom - zTop * JFlTop) / jz);
        }

        protected override double CalculateJt()
        {
            if (_edgeWorking == EdgeType.Fillet)
            {
                double b = (LenghtBottomFlange + LenghtTopFlange) / 2;
                double tf = (ThicknessBottomFlange + ThicknessTopFlange) / 2;

                double alpha1 = -0.042 + 0.2204 * ThicknessWeb / tf + 0.1355 * R / tf -
                    0.0865 * R * ThicknessWeb / Math.Pow(tf, 2) - 0.0725 * Math.Pow(ThicknessWeb, 2) / Math.Pow(tf, 2);
                double D1 = (Math.Pow(tf + R, 2.0) + (R + 0.25 * ThicknessWeb) * ThicknessWeb) / (2.0 * R + tf);

                return (2.0 / 3.0) * b * Math.Pow(tf, 3) + (1.0 / 3.0) * (Height - 2 * tf) * Math.Pow(ThicknessWeb, 3) +
                    2.0 * alpha1 * Math.Pow(D1, 4) - 0.420 * Math.Pow(tf, 4);
            }
            else
                return base.CalculateJt();
        }

        protected override double CalculateJw()
        {
            double dmed = _h - ThicknessBottomFlange / 2.0 - ThicknessTopFlange / 2.0;
            double JFlTop = 1.0 / 12.0 * ThicknessTopFlange * Math.Pow(LenghtTopFlange, 3.0);
            double JFlBottom = 1.0 / 12.0 * ThicknessBottomFlange * Math.Pow(LenghtBottomFlange, 3.0);
            double jz = JFlTop + JFlBottom + 1.0 / 12.0 * HeightWeb * Math.Pow(ThicknessWeb, 3.0);

            // CNR DT208_2011
            return dmed * dmed * JFlBottom * JFlTop / jz;
        }

        private void CalculateSection()
        {
			ThinWall web = new ThinWall(HeightWeb, ThicknessWeb, Math.PI / 2,
                new Point2d(Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2.0, ThicknessBottomFlange + HeightWeb / 2.0));
			ThinWall flangeTop = new ThinWall(LenghtTopFlange, ThicknessTopFlange, 0,
                new Point2d(Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2.0, ThicknessBottomFlange + HeightWeb + ThicknessTopFlange / 2.0));
			ThinWall flangeBottom = new ThinWall(LenghtBottomFlange, ThicknessBottomFlange, 0,
                new Point2d(Math.Max(LenghtTopFlange, LenghtBottomFlange) / 2.0, ThicknessBottomFlange / 2.0));

			SetThinWalls(new ThinWall[3] { web, flangeTop, flangeBottom });

			SetMechanicalProperties();
			_mesh = GetMesh();
		}

        public override string ToString()
        {
            return $"H {_h}x{_tw}x{_bbottom}x{_tbottom}x{_btop}x{_ttop}";
        }

		#endregion
	}
}
