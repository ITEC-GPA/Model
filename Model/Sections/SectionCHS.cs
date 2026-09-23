using GPC.Geometry;
using GPC.Geometry.Meshes;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Sections
{
    [Serializable]
    public class SectionCHS : ThinWallSection, ISerializable, IEquatable<SectionCHS>
    {
        #region Variables

        protected double _externalDiameter; // Diameter external
        protected double _thickness; // Thickness

        #endregion

        #region Properties

        /// <summary>
        /// The external diameter of CHS
        /// </summary>
        public double Diameter
        {
            get => _externalDiameter;
            set
            {
                if (_externalDiameter != value)
                {
                    _externalDiameter = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The Thickness of the section
        /// </summary>
        public double Thickness
        {
            get => _thickness;
            set
            {
                if (_thickness != value)
                {
                    _thickness = value;
                    CalculateSection();
                }
            }
        }

        /// <summary>
        /// The internal diameter of CHS
        /// </summary>
        public double DiameterInternal => _externalDiameter - (2 * _thickness);

        public override double Height
        {
            get => Diameter;
            set => Diameter = value;
        }

        public override double Width => _externalDiameter;

        #endregion

        #region Public Constructors

        public SectionCHS(double externalDiameter, double thickness, string name = "")
            : base(name)
        {
            if (thickness > externalDiameter / 2.0)
                throw new ArgumentException($"Diameter cannot be lower than 2 * thickness ");

            _externalDiameter = externalDiameter < 0 ? throw new ArgumentException($"Diameter cannot be lower than zero") : externalDiameter;
            _thickness = thickness < 0 ? throw new ArgumentException($"Thickness cannot be lower than zero") : thickness;

            CalculateSection();
        }

        public SectionCHS(SectionCHS section)
            : this(section.Diameter, (section.Diameter - section.DiameterInternal) / 2.0, section.Name)
        {

        }

        protected SectionCHS(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            int version;
            try
            {
                version = info.GetInt32("SectionCHSVersion");
            }
            catch (Exception)
            {
                version = 1;
            }

            _externalDiameter = info.GetDouble("D");
            _thickness = info.GetDouble("T");

            _mesh = GetMesh();
        }

        #endregion

        #region Public method

        public override void SetMechanicalProperties()
        {
            _area = CalculateArea();
            _j11 = CalculateJ();
            _j22 = _j11;
            _jxx = _j11;
            _jyy = _j11;

            _jxy = CalculateJxy();
            _jp = _jxx + _jyy;

            _jt = CalculateJt();
            _jw = CalculateJw();
            _centroid = CalculateCentroid();
            _shearCenter = _centroid;
            _angleX1 = CalculateAngle();
            _wel1Max = CalculateWel();
            _wel1Min = _wel1Max;
            _wel2Max = _wel1Max;
            _wel2Min = _wel1Max;
            _wpl1 = CalculateWpl();
            _wpl2 = _wpl1;

            _isSymmetricAlongXLocalAxis = CalculateIsSymmetricAlongXLocalAxis();
            _isSymmetricAlongYLocalAxis = CalculateIsSymmetricAlongYLocalAxis();

        }

        protected override double CalculateArea()
        {
            return (Math.Pow(Diameter, 2.0) * Math.PI) / 4.0 - (Math.Pow(DiameterInternal, 2.0) * Math.PI) / 4.0;
        }

        protected virtual double CalculateJ()
        {
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (64.0);
        }

        protected override double CalculateJxy()
        {
            return 0;
        }

        protected override double CalculateJt()
        {
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (32.0);
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
            return Math.PI * (Math.Pow(Diameter, 4.0) - Math.Pow(DiameterInternal, 4.0)) / (32.0 * _externalDiameter);
        }

        protected virtual double CalculateWpl()
        {
            return (Math.Pow(Diameter, 3.0) - Math.Pow(DiameterInternal, 3.0)) / (6.0);
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

        #region Public override methods 

        protected Shape2d GetShape(int numberOfEdges)
        {
            var center = new Point2d(0.5 * _externalDiameter, 0.5 * _externalDiameter);
            return new Shape2d(new Polygon2d(_externalDiameter, numberOfEdges, center), new[] { new Polygon2d(_externalDiameter - 2.0 * _thickness, numberOfEdges, center) });
        }

        protected override Shape2d GetShape()
        {
            var center = new Point2d(0.5 * _externalDiameter, 0.5 * _externalDiameter);
            return new Shape2d(new Polygon2d(_externalDiameter, origin: center), new[] { new Polygon2d(_externalDiameter - 2.0 * _thickness, origin: center) });
        }

        protected Mesh GetMesh(int numberOfEdges = 32)
        {
            Shape2d shape = GetShape(numberOfEdges);

            Mesh mesh = new Mesh();

            for (int i = 0; i < shape.Fill.Count; i++)
            {
                mesh.AddFaceMesh(new Point3d[]
                {
                    new Point3d(shape.Fill[i]),
                    new Point3d(shape.Fill[shape.Fill.GetNextIndex(i)]),
                    new Point3d(shape.Holes[0][shape.Holes[0].GetNextIndex(i)]),
                    new Point3d(shape.Holes[0][i]),
                });
            }

            return mesh;
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);

            double version = 2;
            info.AddValue("SectionCHSVersion", version);

            info.AddValue("D", _externalDiameter);
            info.AddValue("T", _thickness);
        }

        public override string ToString()
        {
            return $"CHS {_externalDiameter}x{_thickness}";
        }

        protected override Point2d CalculateShearCenter() => _shearCenter;

        protected override double CalculateWpl1() => _wpl1;

        protected override double CalculateWpl2() => _wpl1;

        protected override double CalculateWel1Max() => _wel1Max;

        protected override double CalculateWel1Min() => _wel1Max;

        protected override double CalculateWel2Max() => _wel1Max;

        protected override double CalculateWel2Min() => _wel1Max;

        private void CalculateSection(int numberOfEdges = 32)
        {
            var poly = new Polygon2d(_externalDiameter - _thickness, numberOfEdges, new Point2d(0.5 * _externalDiameter, 0.5 * _externalDiameter));
            var lines = poly.Explode();

            var thinWalls = new ThinWall[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                thinWalls[i] = new ThinWall(line.Start, line.End, _thickness);
            }

            SetThinWalls(thinWalls);

            _mesh = GetMesh();
            SetMechanicalProperties();
            _shape = null;
        }

        #endregion

        #region Equals, hashcode, operators

        public override bool Equals(object obj)
        {
            return Equals(obj as SectionCHS);
        }

        public bool Equals(SectionCHS other)
        {
            return !(other is null) &&
                   base.Equals(other) &&
                   _externalDiameter == other._externalDiameter &&
                   _thickness == other._thickness;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -241850544;
                hashCode = hashCode * -1521134295 + base.GetHashCode();
                hashCode = hashCode * -1521134295 + _externalDiameter.GetHashCode();
                hashCode = hashCode * -1521134295 + _thickness.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(SectionCHS left, SectionCHS right)
        {
            return EqualityComparer<SectionCHS>.Default.Equals(left, right);
        }

        public static bool operator !=(SectionCHS left, SectionCHS right)
        {
            return !(left == right);
        }

        #endregion
    }
}