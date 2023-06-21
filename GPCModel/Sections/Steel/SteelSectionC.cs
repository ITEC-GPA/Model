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

        private readonly double _r1;                // raggio di curvatura interno o altezza di gola
        private readonly double _r2;                // raggio di curvatura esterno

        protected readonly SectionTypes _sectionType;
        protected readonly FormedTypes _formedType;

        #endregion

        #region Properties

        public SectionTypes SectionType => _sectionType;

        public FormedTypes FormedType => _formedType;

        public double R1 => _r1;

        public double R2 => _r2;

        public bool IsRolled => _sectionType == SectionTypes.Rolled;

        public bool IsWelded => _sectionType == SectionTypes.Welded;

        public SteelMaterial SteelMaterial => (SteelMaterial)_material;

        #endregion

        #region Public Constructors

        public SteelSectionC(double height, double thicknessWeb, double lengthTop, double thicknessTop, double lengthBottom,
            double thicknessBottom, SteelMaterial material, string name = "",
            SectionTypes type = SectionTypes.Rolled, FormedTypes formedType = FormedTypes.ColdFormed,
            double radiusInternal = 0, double radiusExternal = 0)
            : base(height, thicknessWeb, lengthTop, thicknessTop, lengthBottom, thicknessBottom, material, name)
        {
            _r1 = radiusInternal < 0 ? 0 : radiusInternal;         // raggio di curvatura o altezza di gola
            _r2 = radiusExternal < 0 ? 0 : radiusExternal;         // raggio di curvatura o altezza di gola
            _sectionType = type;
            _formedType = formedType;

            SetMechanicalProperties();
        }

        protected SteelSectionC(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _r1 = info.GetDouble("R1");
            _r2 = info.GetDouble("R2");
            _sectionType = (SectionTypes)info.GetValue("SectionType", typeof(SectionTypes));
            _formedType = (FormedTypes)info.GetValue("FormedType", typeof(FormedTypes));
        }

        #endregion

        #region Public Methods

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("R1", _r1);
            info.AddValue("R2", _r2);
            info.AddValue("SectionType", _sectionType);
            info.AddValue("FormedType", _formedType);
        }

        protected override double CalculateArea()
        {
            return base.CalculateArea() + CalculateAdditionalArea();
        }

        protected double CalculateAdditionalArea()
        {
            if (IsWelded)
                return 2 * Math.Pow((1.41 * R1), 2) / 2.0 -
                    2 * (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0);

            else if (IsRolled)
                return 2 * (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0) -
                    2 * (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0);

            else
                throw new NotImplementedException("Not Implemented type");
        }

        protected override double CalculateJxx()
        {
            return base.CalculateJxx() + CalculateAdditionaJxx();
        }

        protected override double CalculateJyy()
        {
            return base.CalculateJyy() + CalculateAdditionaJyy();
        }

        protected override double CalculateJxy()
        {
            return 0;
        }

        private double CalculateAdditionaJxx()
        {
            if (IsWelded)
            {
                return 2.0 * (Math.Pow((1.41 * _r1), 4) / 24.0) +
                    Math.Pow((1.41 * R1), 2) / 2.0 * Math.Pow(Height - Centroid.Y - ThicknessTop - R1 / 3.5, 2) +
                    Math.Pow((1.41 * R1), 2) / 2.0 * Math.Pow(Centroid.Y - ThicknessBottom - R1 / 3.5, 2);
            }
            else if (IsRolled)
            {
                return 2.0 * ((1.0 / 3.0) * Math.Pow(_r1, 4.0) - (Math.PI / 16.0) * Math.Pow(_r1, 4.0)) +
                    (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0) * Math.Pow(Height - Centroid.Y - ThicknessTop - R1 / 3.5, 2) +
                    (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0) * Math.Pow(Centroid.Y - ThicknessBottom - R1 / 3.5, 2);
            }
            else
                throw new NotImplementedException("Not Implemented type");
        }

        private double CalculateAdditionaJyy()
        {
            if (IsWelded)
            {
                return 2.0 * (Math.Pow((1.41 * R1), 4) / 24.0 +
                    Math.Pow((1.41 * R1), 2) / 2.0 * Math.Pow(Centroid.X - ThicknessWeb - R1 / 3.5, 2));
            }
            else if (IsRolled)
            {
                return 2.0 * ((1.0 / 3.0) * Math.Pow(R1, 4.0) - (Math.PI / 16.0) * Math.Pow(R1, 4.0) +
                    Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.Pow(Centroid.X - ThicknessWeb - R1 / 3.5, 2));
            }
            else
                throw new NotImplementedException("Not Implemented type");
        }

        protected override Point2d CalculateCentroid()
        {
            double xSum = 0;
            double ySum = 0;
            double area = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
            {
                xSum += _thinWalls[i].CalculateSy();
                ySum += _thinWalls[i].CalculateSx();
                area += _thinWalls[i].Area;
            }

            if (R1 != 0)
            {
                if (IsWelded)
                {
                    xSum += 2 * Math.Pow((1.41 * R1), 2) / 2.0 *
                        Math.Abs(ThicknessWeb + R1 / 3.5);

                    ySum += Math.Pow((1.41 * R1), 2) / 2.0 *
                        Math.Abs(ThicknessBottom + R1 / 3.5);
                    ySum += Math.Pow((1.41 * R1), 2) / 2.0 *
                        Math.Abs(Height - ThicknessTop - R1 / 3.5);

                    area += 2 * Math.Pow((1.41 * R1), 2) / 2.0;
                }

                else if (IsRolled)
                {
                    xSum += 2 * (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0) *
                        Math.Abs(ThicknessWeb + R1 / 3.5);

                    ySum += (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0) *
                        Math.Abs(ThicknessBottom + R1 / 3.5);
                    ySum += (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0) *
                        Math.Abs(Height - ThicknessTop - R1 / 3.5);

                    area += 2 * (Math.Pow(R1, 2) - Math.Pow(R1, 2) * Math.PI / 4.0);

                }
            }

            if (R2 != 0)
            {
                xSum -= (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0) *
                    Math.Abs(LengthBottom - R2 / 3.5);
                xSum -= (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0) *
                    Math.Abs(LengthTop - R2 / 3.5);

                ySum -= (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0) *
                    Math.Abs(ThicknessBottom + R2 / 3.5);
                ySum -= (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0) *
                    Math.Abs(Height - ThicknessTop - R2 / 3.5);

                area -= 2 * (Math.Pow(R2, 2) - Math.Pow(R2, 2) * Math.PI / 4.0);
            }

            return new Point2d((xSum / area), (ySum / area));
        }

		public override bool Equals(object obj)
		{
			return obj is SteelSectionC c &&
				   base.Equals(obj) &&
				   _r1 == c._r1 &&
				   _r2 == c._r2;
		}

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23;
                hashCode = hashCode * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + _r1.GetHashCode();
                hashCode = hashCode * -17 + _r2.GetHashCode();
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
