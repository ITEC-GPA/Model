using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    [Serializable]
    public class SteelSectionCHS : SectionCHS, ISteelSection, ISerializable
    {
        #region Variables 

        protected readonly SectionTypes _sectionType;
        protected readonly FormedTypes _formedType;

        #endregion

        #region Properties

        public SectionTypes SectionType => _sectionType;

        public FormedTypes FormedType => _formedType;

        public bool IsColdFormed => _formedType == FormedTypes.ColdFormed;

        public bool IsHotFinished => _formedType == FormedTypes.HotFinished;

        public SteelMaterial SteelMaterial => (SteelMaterial)_material;

        public double Height => Diameter;

        #endregion

        #region Public Constructors

        public SteelSectionCHS(double diameter, double thickness, SteelMaterial material, string name = "",
            FormedTypes type = FormedTypes.ColdFormed, SectionTypes sectionType = SectionTypes.Rolled)
            : base(diameter, thickness, material, name)
        {
            _formedType = type;
            _sectionType = sectionType;
        }

        public SteelSectionCHS(SectionCHS section, FormedTypes type = FormedTypes.ColdFormed, 
            SectionTypes sectionType = SectionTypes.Rolled)
            : this(section.Diameter, section.Thickness, (SteelMaterial)section.Material, section.Name, type, sectionType)
        {

        }

        public SteelSectionCHS(SteelSectionCHS section)
            : this(section.Diameter, section.Thickness, (SteelMaterial)section.Material, 
                  section.Name, section.FormedType, section.SectionType)
        {

        }

        protected SteelSectionCHS(SerializationInfo info, StreamingContext context)
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
