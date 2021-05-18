using GPC.Geometry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

using GPC.Model.Materials;
using GPC.Model.FEM.Materials;

namespace GPC.Model.Sections
{
    public class SteelSectionC : SectionC
    {
        #region Enumerator

        public enum ProfileType
        {
            Rolled,
            Welded,
        }

        #endregion

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

        public SteelSectionC(ProfileType type, double h, double tw, double lTop, double tTop, double lBottom, double tBottom, Material material, string name, double radius = 0)
            : base(h, tw, lTop, tTop, lBottom, tBottom, material, name)
        {
            if (radius != 0)
            {
                if (type == ProfileType.Rolled)
                    _r = radius;        // raggio di curvatura

                else if (type == ProfileType.Welded)
                    _r = radius;        // altezza di gola
            }
        }

        #endregion


        /*
        public override double CalculateJxx()
        {
            return base.CalculateJxx() + CalculateAdditionaJxx() ;
        }

        public override double CalculateJyy()
        {
            return base.CalculateJyy() + CalculateAdditionaJyy();
        }

        private double CalculateAdditionaJxx()
        {
            double toAdd = 0;
            Point2d centroid = CalculateCentroid();

            if (IsWelded)
            {
                double area = CalculateAdditionalArea();
                double inertia = Math.Pow((1.41 * _r), 4) / 24;
                toAdd = inertia + area * (_hw - centroid.Y - _r);
            }
            else if (IsRolled)
            {

            }
            else
                throw new NotImplementedException("Not Implemented type");

            return toAdd;
        }

        private double CalculateAdditionaJyy()
        {
            double toAdd = 0;

            if (IsWelded)
            {
                double area = CalculateAdditionalArea();
                double inertia = Math.Pow((1.41 * _r), 4) / 24;
                toAdd = inertia + 4 * area * (_hw / 2);
            }
            else if (IsRolled)
            {

            }
            else
                throw new NotImplementedException("Not Implemented type");

            return toAdd;
        }

        private double CalculateAdditionalArea()
        {
            if (IsWelded)            
                return Math.Pow((1.41 * _r), 2) / 2;
            
            else if (IsRolled)            
                return Math.Pow(_r, 2) - Math.Pow(_r, 2) * Math.PI / 4;
            
            else
                throw new NotImplementedException("Not Implemented type");
        }

        public virtual double MinSigma(double N, double M2, double M1)
        {
            double sigmap1 = N /_area - M2 / _wel22Top + M1 / Jxx * _btop / 2.0;
            double sigmap2 = N / _area - M2 / _wel22Top - M1 / Jxx * _btop / 2.0;
            double sigmap3 = N / _area + M2 / _wel22Bottom + M1 / Jxx * _bbottom / 2.0;
            double sigmap4 = N / _area + M2 / _wel22Bottom - M1 / Jxx * _bbottom / 2.0;

            double sigmaMin = Math.Min(sigmap1, sigmap2);
            sigmaMin = Math.Min(sigmaMin, sigmap3);
            sigmaMin = Math.Min(sigmaMin, sigmap4);

            return sigmaMin;
        }
        */

        public double MinSigma(double N, double M2, double M1)
        {
            if (IsSymmetricAlongYLocalAxis)
            {
                double sigmaP1 = N / _area - M2 / _jyy * (_h - _centroid.Y) + M1 / _jxx * (_centroid.X);
                double sigmaP2 = N / _area - M2 / _jyy * (_h - _centroid.Y) - M1 / _jxx * (_lengthTop - _centroid.X);
                double sigmaP3 = N / _area + M2 / _jyy * (_centroid.Y) + M1 / _jxx * (_centroid.X);
                double sigmaP4 = N / _area + M2 / _jyy * (_centroid.Y) - M1 / _jxx * (_lengthBottom - _centroid.X);

                double sigmaMin = Math.Min(sigmaP1, sigmaP2);
                sigmaMin = Math.Min(sigmaMin, sigmaP3);
                sigmaMin = Math.Min(sigmaMin, sigmaP4);

                return sigmaMin;
            }
            else
                throw new NotImplementedException("calculation of unequal C not yet supported");
        }

        public double CalculateWel11Left()
        {
            return _jxx / CalculateCentroid().X;
        }

        public double CalculateWel11Right()
        {
            return _jxx / Math.Max(_lengthBottom - CalculateCentroid().X, _lengthTop - CalculateCentroid().X);
        }

        public double CalculateWel22Top()
        {
            return _jyy / CalculateCentroid().Y;
        }

        public double CalculateWel22Bottom()
        {
            return _jyy / (_h - CalculateCentroid().Y);
        }

        public double CalculateWpl11()
        {
            if (IsSymmetricAlongYLocalAxis)
            {
                if (_area / 2.0 > _h * _tw)
                {
                    double hDown = _area / 2.0 / (_tTop + _tBottom);
                    SectionT secTop = new SectionT(_lengthBottom - hDown, _h, _tBottom + _tTop, _tw, _material, string.Empty);
                    return _area / 2.0 * (hDown / 2.0 + secTop.Centroid.Y);
                }
                else
                    throw new NotImplementedException("neutral axis in web not yet supported");
            }
            else
                throw new NotImplementedException("Different lenght or thickness not yet supported");
        }

        public double CalculateWpl2()
        {
            if (IsSymmetricAlongYLocalAxis)
            {
                if (_area / 2.0 > _tTop * _lengthTop)
                {
                    double hTop = _tTop + (_area / 2.0 - _tTop * _lengthTop) / _tw;
                    SectionT secTop = new SectionT(hTop, _lengthTop, _tw, _tTop, _material, string.Empty);
                    SectionT secBottom = new SectionT(_h - hTop, _lengthBottom, _tw, _tBottom, _material, string.Empty);
                    return _area / 2.0 * (secTop.Centroid.Y + secBottom.Centroid.Y);
                }
                else                
                    throw new NotImplementedException("neutral axis in flange not yet supported");
                
            }
            else
                throw new NotImplementedException("Different lenght or thickness not yet supported");
        }
    }
}
