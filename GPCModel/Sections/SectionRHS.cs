using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    public class SectionRHS : Section
    {
        #region Varibles
        double _h;
        double _b;
        double _tf_top;
        double _tf_bottom;
        double _tw1;
        double _tw2;

        double _hw;

        bool _isHotFinished;

        protected SectionRectangular[] _plates;
        protected Point2d[] _positionCentroidsPlates;
        #endregion

        #region Properties
        public double B => _b;
        public double H => _h;
        public double Hw => _hw;
        public double ThicknessFlange {
            get {
                if(_tf_bottom == _tf_top) {
                    return _tf_top;
                } else
                {
                    throw new Exception("not yet supported");
                }
            }
        }
        public double ThicknessWeb
        {
            get
            {
                if (_tw1 == _tw2)
                {
                    return _tw1;
                }
                else
                {
                    throw new Exception("not yet supported");
                }
            }
        }

        public double Thickness
        {
            get
            {
                if (_tf_bottom == _tf_top && _tw1 == _tw2 && _tw1 == _tf_bottom)
                {
                    return _tf_bottom;
                } else
                {
                    throw new Exception("Different thicknesses");
                }
            }
        }

        public bool IsColdFormed {
            get => !_isHotFinished;
            set { _isHotFinished = !value; 
            }
        }
        public bool IsHotFinished {
            get => _isHotFinished;
            set { _isHotFinished = value;
            }
        }
        #endregion

        public SectionRHS(double h, double b, double tf_top, double tf_bottom, double tw1, double tw2, Materials.Material material) : base(material)
        {
            _material = material;

            _h = h;
            _b = b;
            _tf_top = tf_top;
            _tf_bottom = tf_bottom;
            _tw1 = tw1;
            _tw2 = tw2;

            _hw = h - tf_bottom - tf_top;

            if (_tw1 == _tw2)
            {
                IsSymmetricAlongYLocalAxis = true;
            } else
            {
                IsSymmetricAlongYLocalAxis = false;
            }
            if (_tf_bottom == _tf_top)
            {
                IsSymmetricAlongZLocalAxis = true;
            } else
            {
                IsSymmetricAlongZLocalAxis = false;
            }

            _plates = new SectionRectangular[4];
            _positionCentroidsPlates = new Point2d[4];

            _plates[0] = new SectionRectangular(_b, _tf_top, _material);
            _positionCentroidsPlates[0] = new Point2d(_b / 2.0, _h - _tf_top / 2.0);

            _plates[1] = new SectionRectangular(_b, _tf_bottom, _material);
            _positionCentroidsPlates[1] = new Point2d(_b / 2.0, _tf_bottom / 2.0);

            _plates[2] = new SectionRectangular(_tw1, _hw, _material);
            _positionCentroidsPlates[2] = new Point2d(_tw1 / 2.0, _tf_bottom + _hw / 2.0);

            _plates[3] = new SectionRectangular(_tw2, _hw, _material);
            _positionCentroidsPlates[3] = new Point2d(_b - _tw2 / 2.0, _tf_bottom + _hw / 2.0);

            _area = 0;
            double Sy = 0;
            double Sx = 0;
            for (int i = 0; i < _plates.Count(); i++)
            {
                SectionRectangular plate = _plates[i];
                Point2d centerPlate = _positionCentroidsPlates[i];
                _area = _area + plate.Area;
                Sy = Sy + plate.Area * centerPlate.Y;
                Sx = Sx + plate.Area * centerPlate.X;
            }
            _centroid = new Point2d(Sx / _area, Sy / _area);

            _j22 = 0;
            _j11 = 0;
            for (int i = 0; i < _plates.Count(); i++)
            {
                SectionRectangular plate = _plates[i];
                Point2d centerPlate = _positionCentroidsPlates[i];

                _j11 = _j11 + plate.J11 + plate.Area * Math.Pow(centerPlate.X - _centroid.X, 2.0);
                _j22 = _j22 + plate.J22 + plate.Area * Math.Pow(centerPlate.Y - _centroid.Y, 2.0);
            }

            _wel22Top = _j22 / (_h - _centroid.Y);
            _wel22Bottom = _j22 / (_centroid.Y);
            _wel11Left = _j11 / (_centroid.X);
            _wel11Right = _j11 / (_b - _centroid.X);

            _wpl22 = 0;
            if (_area/2.0 > _plates[0].Area)
            {
                if (_tw1 == _tw2) {
                    double hTSection = (_area / 2.0 - _plates[0].Area) / (_tw1 + _tw2);
                    SectionT sec = new SectionT(hTSection + _tf_top, _b, _tw1 + _tw2, _tf_top, _material);
                    _wpl22 = _area / 2.0 * 2.0 * sec.Centroid.Y;
                } else
                {
                    throw new Exception("different thickness not yet supported");
                }
            }
            else
            {
                throw new Exception("not yet supported");
            }

            _wpl11 = 0;
            double ALeftface = _plates[2].Area + _tf_top * _tw1 + _tf_bottom * _tw1;
            if (_area / 2.0 > ALeftface)
            {
                if (_tf_bottom == _tf_top)
                {
                    double hTSection = (_area / 2.0 - ALeftface) / (_tf_top + _tf_bottom);
                    SectionT sec = new SectionT(hTSection + _tw1, _h, _tf_top + _tf_bottom, _tf_top, _material);
                    _wpl11 = _area / 2.0 * 2.0 * sec.Centroid.Y;
                }
                else
                {
                    throw new Exception("different thickness not yet supported");
                }
            }
            else
            {
                throw new Exception("not yet supported");
            }
        }
    }
}
