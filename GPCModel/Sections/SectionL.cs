using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SectionL : Section
    {
        #region Variables
        double _l1;
        double _t1;
        double _l2;
        double _t2;
        Plate[] _plates;

        double _jyy = 0;
        double _jxx = 0;
        double _jxy = 0;
        #endregion

        #region Properties
        public double L1 => _l1;
        public double T1 => _t1;
        public double L2 => _l2;
        public double T2 => _t2;

        public double Jxx => _jxx;
        public double Jxy => _jxy;
        public double Jyy => _jyy;
        public Plate[] Plates => _plates;
        #endregion

        public SectionL(double l1, double t1, double l2, double t2, Material material) : base(material)
        {
            _l1 = l1;
            _t1 = t1;
            _l2 = l2;
            _t2 = t2;
            double fy = ((SteelMaterial)material).Fyk;

            _plates = new Plate[2];
            _plates[0] = new Plate(t1, t1/2.0, 0, t1/2.0, l1, fy, Plate.TypePlate.outer);
            _plates[1] = new Plate(t2, t1, t2/2.0, l2, t2/2.0, fy, Plate.TypePlate.outer);

            _area = _plates[0].Area + _plates[1].Area;

            double Sy = 0;
            double Sx = 0;
            for (int i = 0; i < _plates.Count(); i++)
            {
                Sx = Sx + _plates[i].Area * _plates[i].Centroid.X;
                Sy = Sy + _plates[i].Area * _plates[i].Centroid.Y;
            }

            _centroid = new Point2d(Sx / _area, Sy / _area);
                        
            for (int i = 0; i < _plates.Count(); i++)
            {
                _jyy = _jyy + _plates[i].JyCentroid + _plates[i].Area * Math.Pow(_plates[i].Centroid.Y - _centroid.Y, 2.0);
                _jxx = _jxx + _plates[i].JzCentroid + _plates[i].Area * Math.Pow(_plates[i].Centroid.X - _centroid.X, 2.0);
                _jxy = _jxy + 0 + _plates[i].Area * (_plates[i].Centroid.X - _centroid.X) * (_plates[i].Centroid.Y - _centroid.Y);
            }
            _j11 = (_jxx + _jyy) / 2.0 - 0.5 * Math.Sqrt(Math.Pow(_jxx - _jyy,2.0) + 4.0 * _jxy * _jxy);
            _j22 = (_jxx + _jyy) / 2.0 + 0.5 * Math.Sqrt(Math.Pow(_jxx - _jyy,2.0) + 4.0 * _jxy * _jxy);

            _jt = 1.0 / 3.0 * (_l1 - _t2 / 2.0) * Math.Pow(_t1, 3.0) + 1.0 / 3.0 * (_l2 - _t1 / 2.0) * Math.Pow(_t2, 3.0);
            _jw = (Math.Pow(_l1 - _t2 / 2.0, 3.0) * Math.Pow(_t1, 3.0) + Math.Pow(_l2 - _t1 / 2.0, 3.0) * Math.Pow(_t2, 3.0)) / 36.0; //CNR DT 208/2011

            _shearCenter = new Point2d(_t1 / 2.0, _t2 / 2.0);
        }
    }
}
