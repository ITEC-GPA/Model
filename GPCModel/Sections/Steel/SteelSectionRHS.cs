using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Steel
{
    [Serializable]
    public class SteelSectionRHS : SectionRHS, ISteelSection, ISerializable
    {
        #region Varibles

        protected readonly SectionTypes _sectionType;
        protected readonly FormedTypes _formedType;

        #endregion

        #region Properties

        public SectionTypes SectionType => _sectionType;
        public FormedTypes FormedType => _formedType;

        public bool IsRolled => _sectionType == SectionTypes.Rolled;

        public bool IsWelded => _sectionType == SectionTypes.Welded;

        public bool IsHotFinished => _formedType == FormedTypes.HotFinished;

        public bool IsColdFormed => _formedType == FormedTypes.ColdFormed;

        public SteelMaterial SteelMaterial => (SteelMaterial)_material;

        #endregion

        #region Public Constructors

        public SteelSectionRHS(double height, double width, double thicknessTopFlange, double thicknessBottomFlange,
                               double thicknessWebLeft, double thickenssWebRight, SteelMaterial material, string name, double radius = 0,
                               FormedTypes formed = FormedTypes.ColdFormed, SectionTypes sectionType = SectionTypes.Rolled)
            : base(height, width, thicknessTopFlange, thicknessBottomFlange, thicknessWebLeft, thickenssWebRight, material, name)
        {
            _formedType = formed;
            _sectionType = sectionType;
            SetEdgeTypeFromSteelType(_sectionType);
        }

        protected SteelSectionRHS(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _sectionType = (SectionTypes)info.GetValue("SectionType", typeof(SectionTypes));
            _formedType = (FormedTypes)info.GetValue("FormedType", typeof(FormedTypes));
        }

        #endregion

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionType", _sectionType);
            info.AddValue("FormedType", _formedType);
        }
    }
}
