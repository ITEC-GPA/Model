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
using GPC.Model.Fem.Materials;

namespace GPC.Model.Sections.Steel
{
    [Serializable]
    public class SteelSectionH : SectionH, ISteelSection, ISerializable
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

        public SteelSectionH(double height, double thicknessWeb, double topFlangeLength, double topFlangeThickness, 
            double bottomFlangeLength, double bottomFlangeThickness, SteelMaterial material, 
            string name = "", SectionTypes type = SectionTypes.Rolled, 
            FormedTypes formedType = FormedTypes.HotFinished, 
            double radius = 0)
            : base(height, thicknessWeb, topFlangeLength, topFlangeThickness, bottomFlangeLength, bottomFlangeThickness, material, name, radius)
        {
            _sectionType = type;
            _formedType = formedType;
            SetEdgeTypeFromSteelType(_sectionType);
            SetMechanicalProperties();
        }

        protected SteelSectionH(SerializationInfo info, StreamingContext context)
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
