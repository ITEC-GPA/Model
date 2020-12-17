using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    public class SectionH : Section
    {
        #region Variables
        double _hTot;
        double _hw;
        double _tw;
        double _ttop;
        double _tbottom;
        double _btop;
        double _bbottom;

        bool _isWelded;

        Plate[] _plates = new Plate[5];
        #endregion

        public SectionH(double hTot, double tw, double btop, double ttop, double bbottom, double tbottom, bool isWelded, Materials.Material material) : base(material)
        {
            _hTot = hTot;
            _tw = tw;
            _btop = btop;
            _bbottom = bbottom;
            _ttop = ttop;
            _tbottom = tbottom;

            _isWelded = isWelded;

            _hw = _hTot - _tbottom - _ttop;

            
            _plates[0] = new Plate(_tbottom, 0, _tbottom / 2.0, - _bbottom / 2.0, _tbottom / 2.0, ((SteelMaterial)material).Fyk, Plate.TypePlate.outer);
            _plates[1] = new Plate(_tbottom, 0, _tbottom / 2.0, _bbottom / 2.0, _tbottom / 2.0, ((SteelMaterial)material).Fyk, Plate.TypePlate.outer);
            _plates[2] = new Plate(_tw, 0, _tbottom, 0, _hw + _tbottom, ((SteelMaterial)material).Fyk, Plate.TypePlate.inner);
            _plates[3] = new Plate(_ttop, 0, _hTot - _ttop/2.0, -_btop / 2.0, _hTot - _ttop / 2.0, ((SteelMaterial)material).Fyk, Plate.TypePlate.outer);
            _plates[4] = new Plate(_ttop, 0, _hTot - _ttop / 2.0, _btop / 2.0, _hTot - _ttop / 2.0, ((SteelMaterial)material).Fyk, Plate.TypePlate.outer);

            _area =0;
            double Sy = 0;
            double Sx = 0;
            for (int i = 0; i < _plates.Count(); i++)
            {
                double areaPlate = _plates[i].Area;
                double yGPlate = _plates[i].Centroid.Y;
                double xGPlate = _plates[i].Centroid.X;

                _area = _area + areaPlate;
                Sy = Sy + areaPlate * yGPlate;
                Sx = Sx + areaPlate * xGPlate;
            }

            _angleX1 = 0;
            _centroid = new Point2d(Sx / _area, Sy / _area);

            _j11 = 0;
            _j22 = 0;
            for (int i = 0; i < _plates.Count(); i++)
            {
                double areaPlate = _plates[i].Area;
                double yGPlate = _plates[i].Centroid.Y;
                double xGPlate = _plates[i].Centroid.X;
                double j11Plate = _plates[i].JzCentroid;
                double j22Plate = _plates[i].JyCentroid;

                _j11 = _j11 + j11Plate + areaPlate * Math.Pow(xGPlate - _centroid.X,2.0);
                _j22 = _j22 + j22Plate + areaPlate * Math.Pow(yGPlate - _centroid.Y, 2.0);
            }

            double dmed = _hTot - _tbottom / 2.0 - _ttop / 2.0;
            _jt = (_btop * Math.Pow(_ttop,3.0)) + (_bbottom * Math.Pow(_tbottom, 3.0) + dmed * Math.Pow(_tw, 3.0)) / 3.0;

            double JFlTop = 1.0 / 12.0 * _ttop * Math.Pow(_btop, 3.0);
            double JFlBottom = 1.0 / 12.0 * _tbottom * Math.Pow(_bbottom, 3.0);
            double jz = JFlTop + JFlBottom + 1 / 12 * _hw * Math.Pow(_tw, 3.0);

            // CNR DT208_2011-- > to be checked
            _jw = dmed * dmed * JFlBottom * JFlTop / jz;

            //CNR DT208_2011 --> to be checked
            double zBottom = _centroid.Y - _tbottom / 2.0;
            double zTop = _hTot - _ttop / 2.0 - _centroid.Y;
            _shearCenter = new Point2d(0, _centroid.Y - (zBottom * JFlBottom - zTop * JFlTop)/jz);

            _wel11Left = _j11 / Math.Max(_bbottom / 2.0, _btop / 2.0);
            _wel11Right = _wel11Left;
            _wel22Bottom = _j22 / _centroid.Y;
            _wel22Top = _j22 / (_hTot - _centroid.Y);

            _wpl11 = 0;
            {
                SectionT halfSectionTop = new SectionT(_btop / 2.0, _hTot / 2.0, _ttop, _tw / 2.0, material);
                SectionT halfSectionBottom = new SectionT(_bbottom / 2.0, _hTot / 2.0, _tbottom, _tw / 2.0, material);
                double dTop = _btop / 2.0 - halfSectionTop.Centroid.Y;
                double dBottom = _bbottom / 2.0 - halfSectionBottom.Centroid.Y;
                double d = (halfSectionTop.Area * dTop + halfSectionBottom.Area * dBottom) / (halfSectionBottom.Area + halfSectionTop.Area);
                _wpl11 = 2.0 * d * _area / 2.0; 
            }

            _wpl22 = 0;
            {
                if (_area/2.0 > _btop * _ttop && _area/2.0 > _bbottom * _tbottom)
                {
                    double hw = (_area / 2.0 - _btop * _ttop) / _tw;
                    SectionT halfSectionTop = new SectionT(hw + _ttop, _btop, _tw, _ttop, material);
                    SectionT halfSectionBottom = new SectionT(_hTot - _ttop - hw, _bbottom, _tw, _tbottom, material);
                    _wpl22 = _area/2.0 * (halfSectionTop.Centroid.Y + halfSectionBottom.Centroid.Y);
                } else
                {
                    throw new Exception("Cannot calulate Wpl : Plastic neutral axis in flanges...to be implemented");
                }
            }
 
            IsSymmetricAlongYLocalAxis = true;
            if (_btop == _bbottom && _tbottom == _ttop)
            {
                IsSymmetricAlongZLocalAxis = true;
            }
            else
            {
                IsSymmetricAlongZLocalAxis = false;
            }
        }

        #region Properties
        public double LenghtBottomFlange => _bbottom;
        public double LenghtTopFlange => _btop;
        public double ThicknessTopFlange => _ttop;
        public double ThicknessBottomFlange => _tbottom;
        public double ThicknessWeb => _tw;
        public double HeightWeb => _hw;
        public double H => _hTot;
        public double B
        {
            get
            {
                if (_bbottom == _btop)
                {
                    return _btop;
                } else
                {
                    throw new Exception("different B");
                }
            }
        }
        public Plate[] Plates => _plates;

        public bool IsRolled {
            get {
                return !_isWelded;
            }
            set
            {
                _isWelded = !value;
            }
        }
        public bool IsWelded {
            get
            {
                return _isWelded;
            }
            set
            {
                _isWelded = value;
            }
        }
        #endregion
    }
}
