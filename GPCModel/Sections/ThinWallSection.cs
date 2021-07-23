using System;
using System.Linq;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public abstract class ThinWallSection : Section
    {
        #region Variables

        private ThinWall[] _thinWalls;
        private Point2d[] _points;

        #endregion


        #region Properties

        internal Point2d[] Points
        {
            get => _points;
            set { _points = value; }
        }

        internal ThinWall[] ThinWalls
        {
            get => _thinWalls;
            set { _thinWalls = value; SetMechanicalProperties(); }
        }

        #endregion


        #region Public Constructors

        /// <summary>
        /// The default constructor of generic ThinWallSection
        /// </summary>
        /// <param name="material">The <see cref="Materials"/> of the section </param>
        /// <param name="name">The name of the section</param>
        internal ThinWallSection(Material material, string name)
            : base(material, name)
        {
            // TODO: implementare
            // throw new NotImplementedException();
        }

        internal ThinWallSection(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _thinWalls = (ThinWall[])info.GetValue("ThinWall", typeof(ThinWall));
            _points = (Point2d[])info.GetValue("ThinWall", typeof(Point2d));
        }

        #endregion


        #region Public abstract method

        public abstract double CalculateJw();

        public abstract Point2d CalculateShearCenter();

        #endregion


        #region Public method

        /// <summary>
        /// Internal method to set the mechanical properties to the section
        /// </summary>
        internal virtual void SetMechanicalProperties()
        {
            _area = CalculateArea();
            _centroid = CalculateCentroid();
            _jxx = CalculateJxx();
            _jyy = CalculateJyy();
            _j11 = CalculateJ11();
            _j22 = CalculateJ22();
            _jw = CalculateJw();
            _jt = CalculateJt();
            _shearCenter = CalculateShearCenter();
            _wel1 = CalculateWel1();
            _wel2 = CalculateWel2();
            _wpl1 = CalculateWpl1();
            _wpl2 = CalculateWpl2();
        }

        /// <summary>
        /// Calculate the centroid point of the section in X-Y plane 
        /// </summary>
        /// <returns></returns>
        public virtual Point2d CalculateCentroid()
        {
            double xSum = 0;
            double ySum = 0;
            double area = 0;

            for(int i = 0; i < _thinWalls.Length; i++)
            {
                xSum += _thinWalls[i].Area * _points[i].X;
                ySum += _thinWalls[i].Area * _points[i].Y;
                area += _thinWalls[i].Area;
            }

            return new Point2d((xSum / area), (ySum / area));
        }

        public virtual double CalculateJt()
        {
            double jt = 0;

            for(int i = 0; i < _thinWalls.Length; i++)
            {
                jt += _thinWalls[i].CalculateJt();
            }

            return jt;
        }

        /// <summary>
        /// Calculate the first moment of inertia respect the X-axis (the Y-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        public virtual double CalculateJ11()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
            {
                j += _thinWalls[i].CalculateJx();
                j += _thinWalls[i].Area * Math.Pow((Centroid.Y - _points[i].Y), 2);
            }

            return j;
        }

        /// <summary>
        /// Calculate the first moment of inertia respect the Y-axis (the Z-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        public virtual double CalculateJ22()
        {
            double j = 0;

            for (int i = 0; i < _thinWalls.Length; i++)
            {
                j += _thinWalls[i].CalculateJy();
                j += _thinWalls[i].Area * Math.Pow((Centroid.X - _points[i].X), 2);
            }

            return j;
        }
        
        /// <summary>
        /// Calculate the area of the section
        /// </summary>
        /// <returns>The value of the area</returns>
        public virtual double CalculateArea()
        {
            double area = 0;

            for(int i = 0; i < _thinWalls.Length; i++)
                area += _thinWalls[i].Area;
            
            return area;
        }

        public virtual double CalculateJxx()
        {
            return CalculateJ11();
        }

        public virtual double CalculateJyy()
        {
            return CalculateJ22();
        }

        public abstract double CalculateWpl1();
        public abstract double CalculateWpl2();
        public abstract double CalculateWel1();
        public abstract double CalculateWel2();

        #endregion


        #region Nested classes ThinWall

        internal class ThinWall
        {
            #region Variables

            private readonly double _t;
            private readonly double _l;
            private readonly double _angle;

            #endregion


            #region Protected constructor

            /// <summary>
            /// The default constructor of generic ThinWall
            /// </summary>
            /// <param name="length">The length of the ThinWall</param>
            /// <param name="thickness">The thickness of the ThinWall</param>
            /// <param name="angle">The angle of the ThinWall. 0 is orizontal, Math.PI / 2.0 is vertical</param>
            internal ThinWall(double length, double thickness, double angle)
                : base()
            {
                _t = thickness < 0 ? throw new ArgumentException($"Thickness cannot be lower than zero") : thickness;
                _l = length < 0 ? throw new ArgumentException($"Lenght cannot be lower than zero") : length;
                _angle = angle;
            }

            #endregion


            #region Properties

            /// <summary>
            /// The thickness of the wall
            /// </summary>
            internal double T => _t;

            /// <summary>
            /// The lenght of the wall
            /// </summary>
            internal double L => _l;

            /// <summary>
            /// The angle of rotation of the principal axis. Angle = 0 is the Y-axis, 90° is the Z-axis
            /// </summary>
            internal double Angle => _angle;

            /// <summary>
            /// The area og the thin wal
            /// </summary>
            public double Area => CalculateArea();

            #endregion


            #region Internal method

            /// <summary>
            /// Calculate the area of the wall 
            /// </summary>
            /// <returns></returns>
            internal double CalculateArea()
            {
                return _t * _l;
            }

            /// <summary>
            /// Calculate the first moment of inertia of the wall respect the X-axis passing throw the centroid of the <see cref="ThinWallSection"/>
            /// </summary>
            /// <returns></returns>
            internal double CalculateJx(Point2d sectionCentroid)
            {
                if (_angle == 0)
                    return CalculateJx() + CalculateArea() * Math.Pow((sectionCentroid.X), 2);

                else if (_angle == Math.PI / 2.0)
                    return CalculateJx() + CalculateArea() * Math.Pow((sectionCentroid.X), 2);

                else
                    throw new NotImplementedException("Not implemented angle");
            }

            /// <summary>
            /// Calculate the first moment of inertia of the wall respect the Y-axis passing throw the centroid of the <see cref="ThinWallSection"/>
            /// </summary>
            /// <returns></returns>
            internal double CalculateJy(Point2d sectionCentroid)
            {
                if (_angle == 0)
                    return CalculateJy() + CalculateArea() * Math.Pow((sectionCentroid.Y), 2);

                else if (_angle == Math.PI / 2.0)
                    return CalculateJy() + CalculateArea() * Math.Pow((sectionCentroid.Y), 2);

                else
                    throw new NotImplementedException("Not implemented angle");
            }

            internal double CalculateJy()
            {
                if (_angle == 0)
                    return _t * Math.Pow(_l, 3) / 12.0;

                else if (_angle == Math.PI / 2)
                    return _l * Math.Pow(_t, 3) / 12.0;

                else
                    throw new NotImplementedException("Not implemented angle");
            }

            internal double CalculateJx()
            {
                if (_angle == 0)
                    return _l * Math.Pow(_t, 3) / 12.0;

                else if (_angle == Math.PI / 2)
                    return _t * Math.Pow(_l, 3) / 12.0;

                else
                    throw new NotImplementedException("Not implemented angle");
            }

            internal virtual double CalculateJt()
            {
                return L* Math.Pow(T, 3) / GetAlpha();
            }

            internal virtual double CalculateJw()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Calculate the polar moment of inertia 
            /// </summary>
            /// <returns></returns>
            internal virtual double CalculateJpolar()
            {
                return CalculateJx() + CalculateJy();
            }

            internal double GetAlpha()
            {
                return 3 + 1.8 * T / L;
            }

            #endregion

        }

        #endregion
    }
}
