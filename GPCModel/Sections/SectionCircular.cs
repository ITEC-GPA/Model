using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    [Serializable]
    public class SectionCircular : Section, ISection, ISerializable
    {
		#region Variables

		protected double _diameter;

		#endregion

		#region Properties

		/// <summary>
		/// The diameter
		/// </summary>
		public double Diameter => _diameter;

		#endregion

		#region Public Constructors

		/// <summary>
		/// The default constructor
		/// </summary>
		/// <param name="diameter">The diameter</param>
		/// <param name="material">The material</param>
		/// <param name="name">The section name</param>
		public SectionCircular(double diameter, Material material, string name)
            : base(material, name)
        {
            _diameter = diameter;

            SetMechanicalProperties();
        }

        public SectionCircular(SectionCircular sectionCircular)
            : this(sectionCircular.Diameter, sectionCircular.Material, sectionCircular.Name)
        {

        }

        protected SectionCircular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _diameter = info.GetDouble("Diameter");
        }

        #endregion

        #region Public Methods Specific

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Diameter", _diameter);
        }

        #endregion

        #region Protected method

        protected override Shape2d GetShape()
        {
            return new Shape2d(new Polygon2d(_diameter, 32, _centroid));
        }

        protected override void SetMechanicalProperties()
        {
            _area = CalculateArea();
            _j11 = CalculateJ();
            _j22 = CalculateJ();
            _jxx = CalculateJ();
            _jyy = CalculateJ();

            _jxy = CalculateJxy();
            _jp = _jxx + _jyy;

            _jt = CalculateJt();
            _jw = CalculateJw();
            _centroid = CalculateCentroid();
            _shearCenter = CalculateShearCenter();
            _angleX1 = CalculateAngle();
            _wel1Max = CalculateWel();
            _wel1Min = CalculateWel();
            _wel2Max = CalculateWel();
            _wel2Min = CalculateWel();
            _wpl1 = CalculateWpl();
            _wpl2 = CalculateWpl();

            _isSymmetricAlongXLocalAxis = CalculateIsSymmetricAlongXLocalAxis();
            _isSymmetricAlongYLocalAxis = CalculateIsSymmetricAlongYLocalAxis();
        }

        protected override double CalculateArea()
        {
            return Math.Pow(Diameter, 2.0) * Math.PI / 4.0;
        }

        protected virtual double CalculateJ()
        {
            return Math.PI * Math.Pow(Diameter, 4.0) / 64.0;
        }

        protected override double CalculateJt()
        {
            return Math.PI * Math.Pow(Diameter, 4.0) / 32.0;
        }

        protected override double CalculateJw()
        {
            return 0;
        }

        protected override Point2d CalculateCentroid()
        {
            return new Point2d(Diameter / 2.0, Diameter / 2.0);
        }

        protected virtual double CalculateWel()
        {
            return Math.PI * Math.Pow(Diameter, 4.0) / (32.0 * Diameter);
        }

        protected virtual double CalculateWpl()
        {
            return Math.Pow(Diameter, 3.0) / 6.0;
        }
        protected override bool CalculateIsSymmetricAlongXLocalAxis()
        {
            return true;
        }

        protected override bool CalculateIsSymmetricAlongYLocalAxis()
        {
            return true;
        }

		#endregion

		#region Public Method

		public override string ToString()
        {
            return $"Circular {_diameter}";
        }

        public override bool Equals(object obj)
        {
            return obj is SectionCircular circular && base.Equals(obj) && _diameter == circular._diameter;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = 17;
                hashCode = hashCode * -23 + base.GetHashCode();
                hashCode = hashCode * -23 + _diameter.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SectionCircular left, SectionCircular right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(SectionCircular left, SectionCircular right)
        {
            return !(left == right);
        }

		#endregion
	}
}