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
    public class SteelSectionH : SectionH, ISteelSection
    {
        #region Variables

        private readonly double _r;                // raggio di curvatura o altezza di gola

        protected readonly SectionTypes _sectionType;
        protected readonly FormedTypes _formedType;
        #endregion


        #region Properties

        public SectionTypes Type => _sectionType;

        public double R => _r;

        public double D => Height - ThicknessBottomFlange - ThicknessTopFlange - 2.0 * R;

        public bool IsRolled => Type == SectionTypes.Rolled;

        public bool IsWelded => Type == SectionTypes.Welded;

        public SteelMaterial SteelMaterial => (SteelMaterial)_material;

        #endregion


        #region Public Constructors

        public SteelSectionH(double height, double thicknessWeb, double topFlangeLength, double topFlangeThickness, 
                              double bottomFlangeLength, double bottomFlangeThickness, SteelMaterial material, 
                              string name, SectionTypes type = SectionTypes.Rolled, 
                              FormedTypes formedType = FormedTypes.HotFinished, 
                              double radius = 0)
            : base(height, thicknessWeb, topFlangeLength, topFlangeThickness, bottomFlangeLength, bottomFlangeThickness, material, name)
        {
            _r = radius < 0.0 ? 0 : radius;        // altezza di gola o raggio di curvatura
            _sectionType = type;
            _formedType = formedType;
        }

        #endregion


        public override double CalculateJ11()
        {
            return base.CalculateJ11();     //+ CalculateAdditionaJxx()
        }

        public override double CalculateJ22()
        {
            return base.CalculateJ22();     //+ CalculateAdditionaJyy()
        }

        private double CalculateAdditionaJxx()
        {
            Point2d centroid = CalculateCentroid();

            if (IsWelded)
            {
                return Math.Pow((1.41 * _r), 4) / 24.0 + CalculateAdditionalArea() * (HeightWeb - centroid.Y - _r);
            }
            else if (IsRolled)
            {
                return 0;
            }
            else
                throw new NotImplementedException("Not Implemented type");
        }

        private double CalculateAdditionaJyy()
        {
            if (IsWelded)
            {
                return Math.Pow((1.41 * _r), 4) / 24.0 + 4 * CalculateAdditionalArea() * (HeightWeb / 2);
            }
            else if (IsRolled)
            {
                return 0;
            }
            else
                throw new NotImplementedException("Not Implemented type");
        }

        private double CalculateAdditionalArea()
        {
            if (IsWelded)            
                return Math.Pow((1.41 * _r), 2) / 2.0;
            
            else if (IsRolled)            
                return Math.Pow(_r, 2) - Math.Pow(_r, 2) * Math.PI / 4.0;
            
            else
                throw new NotImplementedException("Not Implemented type");
        }

    }
}
