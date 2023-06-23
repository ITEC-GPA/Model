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

        protected readonly SectionTypes _sectionType;
        protected readonly FormedTypes _formedType;

        #endregion

        #region Properties

        public SectionTypes SectionType => _sectionType;

        public FormedTypes FormedType => _formedType;

        public bool IsRolled => _sectionType == SectionTypes.Rolled;

        public bool IsWelded => _sectionType == SectionTypes.Welded;

        public SteelMaterial SteelMaterial => (SteelMaterial)_material;

        #endregion

        #region Public Constructors

        public SteelSectionT(double height, double flangeLength, double thicknessWeb, double thicknessFlange, SteelMaterial material, string name,
            double radius = 0, FormedTypes formedType = FormedTypes.HotFinished, SectionTypes sectionType = SectionTypes.Rolled)
            : base(height, flangeLength, thicknessWeb, thicknessFlange, material, name, radius)
        {
            _sectionType = sectionType;
            _formedType = formedType;
            SetEdgeTypeFromSteelType(_sectionType);
        }

		protected SteelSectionT(SerializationInfo info, StreamingContext context) 
            : base(info, context)
		{
            _sectionType = (SectionTypes)info.GetValue("SectionType", typeof(SectionTypes));
            _formedType = (FormedTypes)info.GetValue("FormedType", typeof(FormedTypes));
        }

		#endregion

		#region Public override method

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionType", _sectionType);
            info.AddValue("FormedType", _formedType);
        }

        #endregion
    }
}
