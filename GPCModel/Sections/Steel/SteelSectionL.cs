using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    
    public class SteelSectionL : SectionL
    {
        public enum ProfileType
        {
            Rolled,
            Welded,
        }

        #region Variables

        private double _r;                // raggio di curvatura o altezza di gola
        private ProfileType _type;

        private double _wel11Left;        //Wel calcolato per punto più a snistra
        private double _wel22Top;         //Wel calcolato per punto superiore (+ alto)
        private double _wel11Right;       //Wel calcolato per punto più a destra
        private double _wel22Bottom;      //Wel calcolato per punto inferiore (+ basso)
        private double _wpl11;
        private double _wpl22;

        #endregion


        #region Properties

        public ProfileType Type => _type;

        public double Wpl11 => _wpl11;

        public double Wpl22 => _wpl22;

        public double Wel11Min => Math.Min(_wel11Left, _wel11Right);

        public double Wel22Min => Math.Min(_wel22Top, _wel22Bottom);

        public double R => _r;

        public bool IsRolled
        {
            get
            {
                if (Type == ProfileType.Rolled)
                    return true;
                else
                    return false;
            }
        }

        public bool IsWelded
        {
            get
            {
                if (Type == ProfileType.Welded)
                    return true;
                else
                    return false;
            }
        }

        #endregion


        #region Constructor

        public SteelSectionL(double lHor, double tHor, double lVert, double tVert, Material material, string name, double radius = 0) 
            : base(lHor, tHor, lVert, tVert, material, name)
        {
            if (Type == ProfileType.Rolled)
                _r = radius;        // raggio di curvatura

            else if (Type == ProfileType.Welded)
                _r = radius;        // altezza di gola
        }

        #endregion


        #region Public method

        public double CalculateWel11Left()
        {
            FivePointsCheck(out double minX, out double _, out double _, out double _);
            return _jxx / Math.Abs(minX);
        }

        public double CalculateWel11Right()
        {
            FivePointsCheck(out double _, out double maxX, out double _, out double _);
            return _jxx / Math.Abs(maxX);
        }

        public double CalculateWel22Bottom()
        {
            FivePointsCheck(out double _, out double _, out double minY, out double _);
            return _jyy / Math.Abs(minY);
        }

        public double CalculateWel22Top()
        {
            FivePointsCheck(out double _, out double _, out double _, out double maxY);
            return _jyy / Math.Abs(maxY);
        }

        private void FivePointsCheck(out double minX, out double maxX, out double minY, out double maxY)
        {
            //check 5 points
            //traslation
            Point2d[] pts = new Point2d[5];
            pts[0] = new Point2d(-Centroid.X, -Centroid.Y);
            pts[1] = new Point2d(LHor - Centroid.X, -Centroid.Y);
            pts[2] = new Point2d(LHor - Centroid.X, THor - Centroid.Y);
            pts[3] = new Point2d(TVert - Centroid.X, LVert - Centroid.Y);
            pts[4] = new Point2d(-Centroid.X, LVert - Centroid.Y);

            //rotation
            minX = 0;
            maxX = 0;
            minY = 0;
            maxY = 0;
            for (int i = 0; i < 5; i++)
            {
                double x = pts[i].X;
                double y = pts[i].Y;
                double newX = x * Math.Cos(AngleX1) + y * Math.Sin(AngleX1);
                double newY = -x * Math.Sin(AngleX1) + y * Math.Cos(AngleX1);
                pts[i] = new Point2d(newX, newY);

                minX = Math.Min(minX, pts[i].X);
                maxX = Math.Max(maxX, pts[i].X);
                minY = Math.Min(minY, pts[i].Y);
                maxY = Math.Max(maxY, pts[i].Y);
            }
        }

        #endregion
    }
}
