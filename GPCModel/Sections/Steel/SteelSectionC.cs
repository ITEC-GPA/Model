using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections.Steel
{
    [Serializable]
    public class SteelSectionC : SectionC, ISteelSection, ISerializable
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

        public SteelSectionC(double height, double thicknessWeb, double lengthTop, double thicknessTop, double lengthBottom,
            double thicknessBottom, SteelMaterial material, string name = "",
            SectionTypes type = SectionTypes.Rolled, FormedTypes formedType = FormedTypes.ColdFormed,
            double radiusInternal = 0, double radiusExternal = 0)
            : base(height, thicknessWeb, lengthTop, thicknessTop, lengthBottom, thicknessBottom, material, name, radiusInternal, radiusExternal)
        {
            _sectionType = type;
            _formedType = formedType;
            SetEdgeTypeFromSteelType(_sectionType);
            SetMechanicalProperties();
        }

        protected SteelSectionC(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _sectionType = (SectionTypes)info.GetValue("SectionType", typeof(SectionTypes));
            _formedType = (FormedTypes)info.GetValue("FormedType", typeof(FormedTypes));
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("SectionType", _sectionType);
            info.AddValue("FormedType", _formedType);
        }

		public override bool Equals(object obj)
		{
			return obj is SteelSectionC c &&
				   base.Equals(obj);
		}

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                return hashCode;
            }
        }

		public static bool operator ==(SteelSectionC left, SteelSectionC right)
		{
			return EqualityComparer<SteelSectionC>.Default.Equals(left, right);
		}

		public static bool operator !=(SteelSectionC left, SteelSectionC right)
		{
			return !(left == right);
		}

		#endregion
	}
}
