using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionT : Section
    {
        #region Variables
        protected double _h;
        protected double _tw;
        protected double _tf;
        protected double _b;

        protected SectionRectangular[] _plates;
        protected Point2d[] _positionCentroidPlates;
        #endregion

        #region Properties
        public double H => _h;
        public double Tw => _tw;
        public double Tf => _tf;
        public double B => _b;
        #endregion

        public SectionT(double h, double b, double tw, double tf, Material material) : base(material)
        {
            _h = h;
            _b = b;
            _tw = tw;
            _tf = tf;

            IsSymmetricAlongZLocalAxis = false;
            IsSymmetricAlongYLocalAxis = true;

            _plates = new SectionRectangular[2];
            _positionCentroidPlates = new Point2d[2];

            _plates[0] = new SectionRectangular(_b, _tf, material);
            _positionCentroidPlates[0] = new Point2d(_b / 2.0, _h - _tf / 2.0);
            _plates[1] = new SectionRectangular(_h-_tf, _tw, material);
            _positionCentroidPlates[1] = new Point2d(_b / 2.0, (_h - _tf) / 2.0);

            _area = _plates[0].Area + _plates[1].Area;

            double Sy = _plates[0].Area * _positionCentroidPlates[0].Y + _plates[1].Area * _positionCentroidPlates[1].Y;

            _centroid = new Point2d(_b / 2, Sy / _area);
        }
    }
}
