using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections.Steel
{
    class SteelSectionCHS : SectionCHS
    {
        #region Enumerator

        public enum ProfileType
        {
            HotFinished,
            ColdFormed,
        }

        #endregion


        #region Variables 

        private ProfileType _profileType;
        private double _wel;       
        private double _wpl;

        #endregion


        #region Properties

        public ProfileType ProductionType => _profileType;

        public double Wel => _wel;

        public double Wpl => _wpl;

        public bool IsColdFormed 
        {
            get
            {
                if (ProductionType == ProfileType.ColdFormed)
                    return true;
                else
                    return false;
            }
            
        }

        public bool IsHotFinished
        {
            get
            {
                if (ProductionType == ProfileType.HotFinished)
                    return true;
                else
                    return false;
            }
        }

        #endregion


        #region Public Constructors

        public SteelSectionCHS(ProfileType type, double diameter, double t, Material material, string name)
            : base(diameter, t, material, name)
        {
            _profileType = type;
        }

        #endregion


        #region Public method

        public double CalculateWel()
        {
            return Math.PI * (Math.Pow(D, 4.0) - Math.Pow(Dint, 4.0)) / (32.0 * _d);
        }

        public double CalculateWpl()
        {
            return (Math.Pow(D, 3.0) - Math.Pow(Dint, 3.0)) / (6.0);
        }

        public double MinSigma(double NEd, double M2, double M1)
        {
            double sigmaN = NEd / _area;
            double M = Math.Sqrt(M1 * M1 + M2 * M2);
            double sigmaM = -M / Wel;

            return sigmaN + sigmaM;
        }

        #endregion
    }
}
