using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using static GPC.Model.Sections.ThinWallSection;
using static System.Net.WebRequestMethods;

namespace GPC.Model.Sections.Steel
{
    [Serializable]
    public class SteelSection : ModelObjectId, ISteelSection, ISerializable, IEquatable<SteelSection>
    {
        #region Varibles

        protected readonly ISection _sectionShape;
        protected readonly Section.SectionTypes _sectionType;
        protected readonly Section.FormedTypes _formedType;

        #endregion

        #region Shape properties

        public double Height => _sectionShape.Height;

        public Shape2d Shape => _sectionShape.Shape;

        public double Area => _sectionShape.Area;

        public double R11 => _sectionShape.R11;

        public double R22 => _sectionShape.R22;

        public Point2d Centroid => _sectionShape.Centroid;

        public Point2d ShearCenter => _sectionShape.ShearCenter;

        public double J11 => _sectionShape.J11;

        public double J22 => _sectionShape.J22;

        public double Jxx => _sectionShape.Jxx;

        public double Jyy => _sectionShape.Jyy;

        public double Jxy => _sectionShape.Jxy;

        public double Jp => _sectionShape.Jp;

        public double Jt => _sectionShape.Jt;

        public double Jw => _sectionShape.Jw;

        public double Wpl1 => _sectionShape.Wpl1;

        public double Wpl2 => _sectionShape.Wpl2;

        public double Wel1 => _sectionShape.Wel1;

        public double Wel2 => _sectionShape.Wel2;

        public bool IsSymmetricAlongXLocalAxis => _sectionShape.IsSymmetricAlongXLocalAxis;

        public bool IsSymmetricAlongYLocalAxis => _sectionShape.IsSymmetricAlongYLocalAxis;

        public bool IsDoubleSymmetric => _sectionShape.IsDoubleSymmetric;

        public Material Material => _sectionShape.Material;

        #endregion

        #region Constructor

        public SteelSection(ISection sectionShape,
            Section.SectionTypes sectionType = Section.SectionTypes.Rolled,
            Section.FormedTypes formedType = Section.FormedTypes.HotFinished)
        {
            _sectionShape = sectionShape ?? throw new ArgumentNullException(nameof(_sectionShape));
            _sectionType = sectionType;
            _formedType = formedType;
        }

        protected SteelSection(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version = info.GetInt32("SteelSectionVersion");

            _sectionShape = (ISection)info.GetValue("SectionShape", typeof(ISection));
            _sectionType = (Section.SectionTypes)info.GetValue("SectionType", typeof(Section.SectionTypes));
            _formedType = (Section.FormedTypes)info.GetValue("FormedType", typeof(Section.FormedTypes));
        }

        #endregion

        #region Steel section

        public Section.SectionTypes SectionType => _sectionType;

        public Section.FormedTypes FormedType => _formedType;

        public bool IsRolled => _sectionType == Section.SectionTypes.Rolled;

        public bool IsWelded => _sectionType == Section.SectionTypes.Welded;

        public bool IsHotFinished => _formedType == Section.FormedTypes.HotFinished;

        public bool IsColdFormed => _formedType == Section.FormedTypes.ColdFormed;

        public SteelMaterial SteelMaterial => (SteelMaterial)_sectionShape.Material;

        public ISection SectionShape => _sectionShape;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            double version = 1;
            info.AddValue("SteelSectionVersion", version);

            info.AddValue("SectionShape", _sectionShape);
            info.AddValue("SectionType", _sectionType);
            info.AddValue("FormedType", _formedType);
        }

        #endregion

        #region Comparer

        public override bool Equals(object obj)
        {
            return Equals(obj as SteelSection);
        }

        public bool Equals(SteelSection other)
        {
            return !(other is null) &&
                   EqualityComparer<ISection>.Default.Equals(_sectionShape, other._sectionShape) &&
                   _sectionType == other._sectionType &&
                   _formedType == other._formedType;
        }

        public override int GetHashCode()
        {
            int hashCode = -1194127724;
            hashCode = hashCode * -1521134295 + EqualityComparer<ISection>.Default.GetHashCode(_sectionShape);
            hashCode = hashCode * -1521134295 + _sectionType.GetHashCode();
            hashCode = hashCode * -1521134295 + _formedType.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(SteelSection left, SteelSection right)
        {
            return EqualityComparer<SteelSection>.Default.Equals(left, right);
        }

        public static bool operator !=(SteelSection left, SteelSection right)
        {
            return !(left == right);
        }

        #endregion
    }
}
