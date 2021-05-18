using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class SteelSectionT : SectionT
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


        #region Public Constructors

        public SteelSectionT(ProfileType type, double hw, double b, double tw, double tf, Material material, string name, double radius) 
            : base(hw, b, tw, tf, material, name)
        {
            if (Type == ProfileType.Rolled)
                _r = radius;        // raggio di curvatura

            else if (Type == ProfileType.Welded)
                _r = radius;        // altezza di gola
        }

        #endregion


        #region Public method

        public double CalculateWpl22()
        {
            if (_area / 2.0 > _b * _tf)
            {
                double yPlastic = _area / 2.0 / _tw;
                SectionT halfSectionTop = new SectionT(H - yPlastic, _b, _tw, _tf, _material, string.Empty);
                return _area / 2.0 * (halfSectionTop.Centroid.Y + yPlastic / 2.0);
            }
            else
            {
                double hTopPlastic = (_area / 2.0) / _b;
                //can't use SectionT because infinite loop
                double Aweb = _tw * (H - _tf);
                double Aflange = _b * (_tf - hTopPlastic);
                double S = Aweb * ((H - _tf) / 2.0 + hTopPlastic) + Aflange * hTopPlastic / 2.0;
                return (_area / 2.0) * (hTopPlastic / 2.0 + S / (Aweb + Aflange));
            }
        }

        public double CalculateWp11()
        {
            return 1.0 / 4.0 * _tf * Math.Pow(_b, 2.0) + 1.0 / 4.0 * (H - _tf) * Math.Pow(_tw, 2.0);
        }


        public double CalculateWel11Left()
        {
            return _jxx / (_centroid.X);
        }

        public double CalculateWel11Right()
        {
            return _jxx / (_b - _centroid.X);
        }

        public double CalculateWel22Bottom()
        {
            return _jyy / (_centroid.Y);
        }

        public double CalculateWel22Top()
        {
            return _jyy / (H - _centroid.Y);
        }

        public double MinSigma(double N, double M2, double M1)
        {
            double sigmaP1 = N / _area - M2 / _wel22Top + M1 / _wel11Left;
            double sigmaP2 = N / _area - M2 / _wel22Top - M1 /_wel11Right;
            double sigmaP3 = N / _area + M2 / _wel22Bottom;
           
            double sigmaMin = Math.Min(sigmaP1, sigmaP2);
            sigmaMin = Math.Min(sigmaMin, sigmaP3);

            return sigmaMin;
        }

        public override string ToString()
        {
            string s = "T section: \n";
            s = s + "Height = " + H + " mm \n";
            s = s + "Thickness Web = " + _tw + " mm \n";
            s = s + "Length Top = " + _b + " mm \n";
            s = s + "Thickness Top = " + _tf + " mm \n";
            return s;
        }

        #endregion
    }
}
