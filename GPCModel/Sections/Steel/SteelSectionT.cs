using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Steel
{
    [Serializable]
    public class SteelSectionT : SectionT, ISteelSection, ISerializable
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

        public bool IsRolled => _sectionType == SectionTypes.Rolled;

        public bool IsWelded => _sectionType == SectionTypes.Welded;

        public SteelMaterial SteelMaterial => (SteelMaterial)_material;

        #endregion

        #region Public Constructors

        public SteelSectionT(double height, double flangeLength, double thicknessWeb, double thicknessFlange, SteelMaterial material, string name,
            double radius = 0, FormedTypes formedType = FormedTypes.HotFinished, SectionTypes sectionType = SectionTypes.Rolled)
            : base(height, flangeLength, thicknessWeb, thicknessFlange, material, name)
        {
            _sectionType = sectionType;
            _formedType = formedType;
            _r = radius;        // raggio di curvatura o altezza di gola
        }

		protected SteelSectionT(SerializationInfo info, StreamingContext context) 
            : base(info, context)
		{
            _r = info.GetDouble("R");
            _sectionType = (SectionTypes)info.GetValue("SectionType", typeof(SectionTypes));
            _formedType = (FormedTypes)info.GetValue("FormedType", typeof(FormedTypes));
        }

		#endregion

		#region Public override method

		public override string ToString()
        {
            string s = "T section: \n";
            s = s + "Height = " + base.Height + " mm \n";
            s = s + "Thickness Web = " + _tw + " mm \n";
            s = s + "Length Top = " + _b + " mm \n";
            s = s + "Thickness Top = " + _tf + " mm \n";
            return s;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("R", _r);
            info.AddValue("SectionType", _sectionType);
            info.AddValue("FormedType", _formedType);
        }

        #endregion
    }
}
