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
    public class SteelSectionH : SectionH
    {
        #region Variables

        private double _r;                // raggio di curvatura o altezza di gola
        private SectionTypes _type;

        private double _wel11Left;        //Wel calcolato per punto più a snistra
        private double _wel22Top;         //Wel calcolato per punto superiore (+ alto)
        private double _wel11Right;       //Wel calcolato per punto più a destra
        private double _wel22Bottom;      //Wel calcolato per punto inferiore (+ basso)
        private double _wpl11;
        private double _wpl22;

        #endregion


        #region Properties

        public SectionTypes Type => _type;

        public double Wpl11 => _wpl11;

        public double Wpl22 => _wpl22;

        public double Wel11Min => Math.Min(_wel11Left, _wel11Right);

        public double Wel22Min => Math.Min(_wel22Top, _wel22Bottom);

        public double R => _r;

        public bool IsRolled => Type == SectionTypes.Rolled;

        public bool IsWelded => Type == SectionTypes.Welded;

        #endregion


        #region Public Constructors

        public SteelSectionH(double hw, double tw, double btop, double ttop, double bbottom, double tbottom, Material material, string name, SectionTypes type = SectionTypes.Rolled, double radius = 0)
            : base(hw, tw, btop, ttop, bbottom, tbottom, material, name)
        {

            if (type == SectionTypes.Rolled)
                _r = radius;        // raggio di curvatura

            else if (type == SectionTypes.Welded)
                _r = radius;        // altezza di gola
        }

        #endregion


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
                toAdd = inertia + area * (HeightWeb - centroid.Y - _r);
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
                toAdd = inertia + 4 * area * (HeightWeb / 2);
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

        public double CalculateWel11Left()
        {
            return _jxx / Math.Max(_bbottom / 2.0, _btop / 2.0);
        }

        public double CalculateWel11Right()
        {
            return _jxx / Math.Max(_bbottom / 2.0, _btop / 2.0);
        }

        public double CalculateWel22Top()
        {
            return _jyy / (_h - CalculateCentroid().Y);
        }

        public double CalculateWel22Bottom()
        {
            return _jyy / CalculateCentroid().Y;
        }

        public double CalculateWpl1()
        {
            SectionT halfSectionTop = new SectionT(_btop / 2.0, Height / 2.0, _ttop, _tw / 2.0, _material, string.Empty);
            SectionT halfSectionBottom = new SectionT(_bbottom / 2.0, Height / 2.0, _tbottom, _tw / 2.0, _material, string.Empty);
            double dTop = _btop / 2.0 - halfSectionTop.Centroid.Y;
            double dBottom = _bbottom / 2.0 - halfSectionBottom.Centroid.Y;
            double d = (halfSectionTop.Area * dTop + halfSectionBottom.Area * dBottom) / (halfSectionBottom.Area + halfSectionTop.Area);
            return 2.0 * d * _area / 2.0;
        }

        public double CalculateWpl2()
        {
            if (_area / 2.0 > _btop * _ttop && _area / 2.0 > _bbottom * _tbottom)
            {
                double hw = (_area / 2.0 - _btop * _ttop) / _tw;
                SectionT halfSectionTop = new SectionT(hw + _ttop, _btop, _tw, _ttop, _material, string.Empty);
                SectionT halfSectionBottom = new SectionT(Height - _ttop - hw, _bbottom, _tw, _tbottom, _material, string.Empty);
                return _area / 2.0 * (halfSectionTop.Centroid.Y + halfSectionBottom.Centroid.Y);
            }
            else if (_area / 2.0 <= _btop * _ttop)
            {
                double hHalf = _area / 2.0 / _btop;
                SectionH halfSectionBottom = new SectionH(Height - hHalf, _tw, _btop, _ttop - hHalf, _bbottom, _tbottom, _material, string.Empty);
                return _area / 2.0 * (hHalf / 2.0 + (Height - hHalf - halfSectionBottom.Centroid.Y));
            }
            else if (_area / 2.0 <= _bbottom * _tbottom)
            {
                double hHalf = _area / 2.0 / _bbottom;
                SectionH halfSectionBottom = new SectionH(Height - hHalf, _tw, _btop, _ttop, _bbottom, _tbottom - hHalf, _material, string.Empty);
                return _area / 2.0 * (hHalf / 2.0 + halfSectionBottom.Centroid.Y);
            }
            else
            {
                throw new Exception("Cannot calculate Wpl : Plastic neutral axis in flanges...to be implemented");
            }
        }
    }
}
