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
    public class SectionRHS : ThinWallSection, ISection, ISerializable
    {
        #region Varibles

        private readonly double _h;
        private readonly double _b;
        private readonly double _tfTop;
        private readonly double _tfBottom;
        private readonly double _twL;
        private readonly double _twR;

        #endregion

        #region Properties

        public double Base => _b;

        public double BaseInternal => _b - _twL - _twR;

        public double Height => _h;

        public double Heightinternal => _h - _tfBottom - _tfTop;

        public double ThicknessTop => _tfTop;

        public double ThicknessBottom => _tfBottom;

        public double ThicknessWebLeft => _twL;

        public double ThicknessWebRight => _twR;

        #endregion

        #region Public Constructors

        public SectionRHS(double height, double width, double thicknessTopFlange, double thicknessBottomFlange,
            double thicknessWebLeft, double thickenssWebRight, Material material, string name)
            : base(material, name)
        {
            _h = height;
            _b = width;
            _tfTop = thicknessTopFlange;
            _tfBottom = thicknessBottomFlange;
            _twL = thicknessWebLeft;
            _twR = thickenssWebRight;


            ThinWall webSx = new ThinWall(Heightinternal, _twL, Math.PI / 2);
            ThinWall webDx = new ThinWall(Heightinternal, _twR, Math.PI / 2);
            ThinWall flangeTop = new ThinWall(Base, _tfTop, 0);
            ThinWall flangeBottom = new ThinWall(Base, _tfBottom, 0);


            SetThinWalls(new ThinWall[] { webSx, webDx, flangeBottom, flangeTop },
                    new Point2d[] { new Point2d(_twL / 2, Heightinternal / 2 + _tfBottom),
                new Point2d(Base - _twR / 2, Heightinternal / 2 + _tfBottom),
                new Point2d(Base / 2, _tfBottom + Heightinternal + _tfTop / 2),
                new Point2d(Base / 2, _tfBottom / 2) });

            SetMechanicalProperties();
            _mesh = GetMesh();
        }

        protected SectionRHS(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionRHSVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _h = info.GetDouble("Height");
            _b = info.GetDouble("Base");
            _tfTop = info.GetDouble("ThicknessTop");
            _tfBottom = info.GetDouble("ThicknessBottom");
            _twL = info.GetDouble("ThicknessWebLeft");
            _twR = info.GetDouble("ThicknessWebRight");
        }

        #endregion

        #region Public method

        public double DistanceYCentroidFromBottom()
        {
            return CalculateCentroid().Y;
        }

        public double DistanceYCentroidFromTop()
        {
            return Height + CalculateCentroid().Y;
        }

        public double DistanceXCentroidFromRight()
        {
            return CalculateCentroid().X;
        }

        public double DistanceXCentroidFromLeft()
        {
            return Base - CalculateCentroid().X;
        }

        #endregion

        #region Public override method

        protected override Shape2d GetShape()
        {
            throw new NotImplementedException();
        }

        protected override Point2d CalculateShearCenter()
        {
            if (_tfBottom == _tfTop && _twL == _twR)
                return _centroid;
            else
                throw new Exception("Section RHS with different thickness not yet implemented");
        }

        protected override double CalculateJw()
        {
            return 0;
        }

        protected override double CalculateJt()
        {
            double Amed = (_h - (_tfTop / 2.0) - (_tfBottom / 2.0)) * (_b - (_twL / 2.0) - (_twR / 2.0));
            double LmedTop = _b - _twL / 2.0 - _twR / 2.0;
            double LmedBottom = LmedTop;
            double LmedWeb1 = _h - _tfTop / 2.0 - _tfBottom / 2.0;
            double LmedWeb2 = LmedWeb1;
            return 4.0 * Amed * Amed / (LmedBottom / _tfBottom + LmedTop / _tfTop + LmedWeb1 / _twL + LmedWeb2 / _twR);
        }

        protected override double CalculateWpl2()
        {
            if (_area / 2.0 >= _twL * Heightinternal + _tfTop * _twL + _tfBottom * _twL)
            {
                if (IsSymmetricAlongYLocalAxis)
                {
                    SectionC halfSectionLeft = new SectionC(Height, ThicknessWebLeft, Base / 2, ThicknessTop, Base / 2, ThicknessBottom, _material, string.Empty);
                    SectionC halfSectionRigth = new SectionC(Height, ThicknessWebRight, Base / 2, ThicknessTop, Base / 2, ThicknessBottom, _material, string.Empty);
                    return (_area / 2.0) * (halfSectionLeft.DistanceXCentroidFromRight() + halfSectionRigth.DistanceXCentroidFromRight());
                }
                else
                    throw new Exception("different thickness not yet supported");
            }
            else
                throw new Exception("not yet supported");

        }

        protected override double CalculateWpl1()
        {
            if (_area / 2.0 >= (_twR * Heightinternal)) //plateTop
            {
                if (IsSymmetricAlongXLocalAxis)
                {
                    SectionC halfSectionTop = new SectionC(Base, ThicknessTop, Height / 2, _twR, Height / 2, _twL, _material, string.Empty);
                    SectionC halfSectionBottom = new SectionC(Base, ThicknessBottom, Height / 2, _twL, Height / 2, _twR, Material, string.Empty);
                    return (_area / 2.0) * (halfSectionTop.DistanceXCentroidFromRight() + halfSectionBottom.DistanceXCentroidFromRight());
                }
                else
                    throw new Exception("different thickness not yet supported");
            }
            else
                throw new Exception("not yet supported");
        }

        protected override double CalculateWelYMin()
        {
            return Jyy / DistanceXCentroidFromRight();
        }

        protected override double CalculateWelYMax()
        {
            return Jyy / (_b - DistanceXCentroidFromRight());
        }

        protected override double CalculateWelXMin()
        {
            return Jxx / DistanceYCentroidFromBottom();
        }

        protected override double CalculateWelXMax()
        {
            return Jxx / (Height - DistanceYCentroidFromBottom());
        }

        protected override double CalculateWel2Min()
        {
            return J22 / DistanceXCentroidFromRight();
        }

        protected override double CalculateWel2Max()
        {
            return J22 / (_b - DistanceXCentroidFromRight());
        }

        protected override double CalculateWel1Min()
        {
            return J11 / DistanceYCentroidFromBottom();
        }

        protected override double CalculateWel1Max()
        {
            return J11 / (Height - DistanceYCentroidFromBottom());
        }

        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {
            if (_tfBottom == _tfTop)
                return true;

            return false;
        }

        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            if (_twL == _twR)
                return true;

            return false;
        }

        #endregion

        public override string ToString()
        {
            return $"RHS {_h}x{_twL}x{_twR}x{_b}x{_tfBottom}x{_b}x{_tfTop}";
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("SectionRHSVersion", version);

            info.AddValue("Height", _h);
            info.AddValue("Base", _b);
            info.AddValue("ThicknessTop", _tfTop);
            info.AddValue("ThicknessBottom", _tfBottom);
            info.AddValue("ThicknessWebLeft", _twL);
            info.AddValue("ThicknessWebRight", _twR);
        }
    }
}
