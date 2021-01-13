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
        double _lHor;
        double _tHor;
        double _lVert;
        double _tVert;
        Plate[] _plates;

        double _jyy = 0;
        double _jxx = 0;
        double _jxy = 0;
        #endregion

        #region Properties
        public double LHor => _lHor;
        public double THor => _tHor;
        public double LVert => _lVert;
        public double TVert => _tVert;

        public double Jxx => _jxx;
        public double Jxy => _jxy;
        public double Jyy => _jyy;
        public Plate[] Plates => _plates;
        #endregion

        public SectionL(double lHor, double tHor, double lVert, double tVert, Material material) : base(material)
        {
            _lHor = lHor;
            _tHor = tHor;
            _lVert = lVert;
            _tVert = tVert;
            double fy = ((SteelMaterial)material).Fyk;

            _plates = new Plate[2];
            _plates[0] = new Plate(tHor, _tVert, _tHor/2.0, _lHor, _tHor/2.0, fy, Plate.TypePlate.outer);
            _plates[1] = new Plate(tVert, _tVert / 2.0, 0, _tVert / 2.0, _lVert, fy, Plate.TypePlate.outer);

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
                _jxy = _jxy + 0.0 + _plates[i].Area * (_plates[i].Centroid.X - _centroid.X) * (_plates[i].Centroid.Y - _centroid.Y);
            }
            _j11 = (_jxx + _jyy) / 2.0 - 0.5 * Math.Sqrt(Math.Pow(_jxx - _jyy,2.0) + 4.0 * _jxy * _jxy);
            _j22 = (_jxx + _jyy) / 2.0 + 0.5 * Math.Sqrt(Math.Pow(_jxx - _jyy,2.0) + 4.0 * _jxy * _jxy);
            _angleX1 = - 1.0 / 2.0 * Math.Atan(2.0 * _jxy / (_jyy - _jxx));
            
            if (_jyy < _jxx) { 
 
                _angleX1 = _angleX1 + Math.PI / 2.0;
            }            

            _jt = 1.0 / 3.0 * (_lHor - _tVert / 2.0) * Math.Pow(_tHor, 3.0) + 1.0 / 3.0 * (_lVert - _tHor / 2.0) * Math.Pow(_tVert, 3.0);
            _jw = (Math.Pow(_lHor - _tVert / 2.0, 3.0) * Math.Pow(_tHor, 3.0) + Math.Pow(_lVert - _tHor / 2.0, 3.0) * Math.Pow(_tVert, 3.0)) / 36.0; //CNR DT 208/2011

            _shearCenter = new Point2d(_tHor / 2.0, _tVert / 2.0);

            //check 5 points
            //traslation
            Point2d[] pts = new Point2d[5];
            pts[0] = new Point2d(- _centroid.X, - _centroid.Y);
            pts[1] = new Point2d(LHor - _centroid.X, - _centroid.Y);
            pts[2] = new Point2d(LHor - _centroid.X, _tHor -_centroid.Y);
            pts[3] = new Point2d(_tVert - _centroid.X, _lVert - _centroid.Y);
            pts[4] = new Point2d(- _centroid.X, _lVert - _centroid.Y);

            //rotation
            double minX = 0;
            double maxX = 0;
            double minY = 0;
            double maxY = 0;
            for (int i = 0; i < 5; i++)
            {
                double x = pts[i].X;
                double y = pts[i].Y;
                double newX = x * Math.Cos(_angleX1) + y * Math.Sin(_angleX1);
                double newY = - x * Math.Sin(_angleX1) + y * Math.Cos(_angleX1);
                pts[i] = new Point2d(newX, newY);

                minX = Math.Min(minX, pts[i].X);
                maxX = Math.Max(maxX, pts[i].X);
                minY = Math.Min(minY, pts[i].Y);
                maxY = Math.Max(maxY, pts[i].Y);
            }

            _wel11Left = _j11 / Math.Abs(minX);
            _wel11Right = _j11 / Math.Abs(maxX);
            _wel22Top = _j22 / Math.Abs(maxY);
            _wel22Bottom = _j22 / Math.Abs(minY);

            //Assumed as elastic
            _wpl11 = Math.Min(_wel11Left, _wel11Right);
            _wpl22 = Math.Min(_wel22Bottom, _wel22Top);
        }
    }
}
