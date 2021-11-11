using System;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Materials;
using GPC.Model.Sections.Concrete;

namespace GPC.Model.Sections
{

    public class SectionCircular : Section, ISection
    {
        #region Variables

        protected readonly double _diameter;

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
        /// <param name="id">The unique id</param>
        public SectionCircular(double diameter, Material material, string name, int id = IDUNASSIGNED)
            : base(material, name)
        {
            _diameter = diameter;
            _id = id;
            _isSymmetricAlongXLocalAxis = true;
            _isSymmetricAlongYLocalAxis = true;

            SetMechanicalProperties();
        }

        public SectionCircular(SectionCircular sectionCircular)
            : this(sectionCircular.Diameter, sectionCircular.Material, sectionCircular.Name, sectionCircular.Id)
        {

        }

        public SectionCircular(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _diameter = info.GetDouble("Diameter");
            _material = (Material)info.GetValue("Material", typeof(Material));
        }

        #endregion


        #region Public Methods Specific

        protected override Shape2d GetShape()
        {
            return new Shape2d(new Polygon2d(_diameter));
        }


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Diameter", _diameter);
            info.AddValue("Material", _material);
        }

        #endregion


        #region Protected method

        protected void SetMechanicalProperties()
        {
            _area = CalculateArea();
            _j11 = CalculateJ();
            _j22 = CalculateJ();
            _jxx = CalculateJ();
            _jyy = CalculateJ();
            _jxy = 0.0;
            _jt = CalculateJt();
            _jw = CalculateJw();
            _centroid = CalculateCentroid();
            _shearCenter = CalculateCentroid();
            _wel1 = CalculateWel();
            _wel2 = CalculateWel();
            _wpl1 = CalculateWpl();
            _wpl2 = CalculateWpl();
        }

        protected double CalculateArea()
        {
            return Math.Pow(Diameter, 2.0) * Math.PI / 4.0;
        }

        protected double CalculateJ()
        {
            return Math.PI * Math.Pow(Diameter, 4.0) / 64.0;
        }

        protected double CalculateJp()
        {
            return Math.PI * Math.Pow(Diameter, 4.0) / 32.0;
        }

        protected double CalculateJt()
        {
            return Math.PI * Math.Pow(Diameter, 4.0) / 32.0;
        }

        protected double CalculateJw()
        {
            return 0;
        }

        protected Point2d CalculateCentroid()
        {
            return new Point2d(Diameter / 2.0, Diameter / 2.0);
        }

        protected double CalculateWel()
        {
            return Math.PI * Math.Pow(Diameter, 4.0) / (32.0 * Diameter);
        }

        protected double CalculateWpl()
        {
            return Math.Pow(Diameter, 3.0) / 6.0;
        }

        #endregion
    }
}