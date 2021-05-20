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

namespace GPC.Model.Sections.Steel
{
    public class SteelSectionH : SectionH
    {
        #region Variables

        private readonly double _r;                // raggio di curvatura o altezza di gola

        #endregion


        #region Properties

        public SectionTypes Type => _sectionType;

        public double R => _r;

        public double D => H - ThicknessBottomFlange - ThicknessTopFlange - 2 * R;

        public bool IsRolled => Type == SectionTypes.Rolled;

        public bool IsWelded => Type == SectionTypes.Welded;

        #endregion


        #region Public Constructors

        public SteelSectionH(double hw, double tw, double btop, double ttop, double bbottom, double tbottom, SteelMaterial material, string name, SectionTypes type = SectionTypes.Rolled, double radius = 0)
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
            return base.CalculateJxx();     //+ CalculateAdditionaJxx()
        }

        public override double CalculateJyy()
        {
            return base.CalculateJyy();     //+ CalculateAdditionaJyy()
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
    }
}
