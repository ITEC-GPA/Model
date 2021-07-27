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

        public SectionTypes SectionType => _sectionType;
        public FormedTypes FormedType => _formedType;

        public double R => _r;

        public double D => Height - ThicknessBottomFlange - ThicknessTopFlange - 2.0 * R;

        public bool IsRolled => _sectionType == SectionTypes.Rolled;

        public bool IsWelded => _sectionType == SectionTypes.Welded;

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

            SetMechanicalProperties();
        }

        #endregion


        public override double CalculateJ11()
        {
            return base.CalculateJ11() + CalculateAdditionaJxx();
        }

        public override double CalculateJ22()
        {
            return base.CalculateJ22() + CalculateAdditionaJyy();
        }

        private double CalculateAdditionaJxx()
        {
            if (IsWelded)
            {
                return 4 * (Math.Pow((1.41 * _r), 4) / 24.0) +
                    CalculateAdditionalArea() / 2 * Math.Pow(Height - Centroid.Y - ThicknessTopFlange - R / 4.0, 2) +
                    CalculateAdditionalArea() / 2 * Math.Pow(Centroid.Y - ThicknessBottomFlange - R / 4.0, 2);
            }
            else if (IsRolled)
            {
                return 4.0 * ((1.0 / 3.0) * Math.Pow(_r, 4.0) - (Math.PI / 16.0) * Math.Pow(_r, 4.0)) +
                    CalculateAdditionalArea() / 2 * Math.Pow(Height - Centroid.Y - ThicknessTopFlange - R / 4.0, 2) +
                    CalculateAdditionalArea() / 2 * Math.Pow(Centroid.Y - ThicknessBottomFlange - R / 4.0, 2);
            }
            else
                throw new NotImplementedException("Not Implemented type");
        }

        private double CalculateAdditionaJyy()
        {
            if (IsWelded)
            {
                return 4 * (Math.Pow((1.41 * R), 4) / 24.0) +  CalculateAdditionalArea() * Math.Pow(ThicknessWeb / 2, 2);
            }
            else if (IsRolled)
            {
                return 4.0 * ((1.0 / 3.0) * Math.Pow(R, 4.0) - (Math.PI / 16.0) * Math.Pow(R, 4.0)) + CalculateAdditionalArea() * Math.Pow(ThicknessWeb / 2.0, 2);
            }
            else
                throw new NotImplementedException("Not Implemented type");
        }

        private double CalculateAdditionalArea()
        {
            if (IsWelded)
                return Math.Pow((1.41 * R), 2) / 2.0;

            else if (IsRolled)            
                return 4 * (Math.Pow(R, 2) - Math.Pow(R, 2) * Math.PI / 4.0);            

            else
                throw new NotImplementedException("Not Implemented type");
        }

        public override double CalculateArea()
        {
            return base.CalculateArea() + CalculateAdditionalArea();
        }

    }
}
