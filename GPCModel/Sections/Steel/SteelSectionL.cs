using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections.Steel
{
    [Serializable]
    public class SteelSectionL : SectionL, ISteelSection, ISerializable
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

        #region Constructor

        public SteelSectionL(double lHor, double tHor, double lVert, double tVert, SteelMaterial material,
            string name, SectionTypes sectionTypes = SectionTypes.Rolled,
            FormedTypes formedType = FormedTypes.ColdFormed, double radius = 0)
            : base(lHor, tHor, lVert, tVert, material, name, radius)
        {
            _sectionType = sectionTypes;
            _formedType = formedType;
            SetEdgeTypeFromSteelType(_sectionType);
        }

        protected SteelSectionL(SerializationInfo info, StreamingContext context)
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
