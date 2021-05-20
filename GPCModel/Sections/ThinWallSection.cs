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

        #endregion


        #region Properties

        internal ThinWall[] ThinWalls
        {
            get
            {
                return _thinWalls;
            }
            set
            {
                _thinWalls = value;
                SetMechanicalProperties();
            }
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

        }

        internal ThinWallSection(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            _thinWalls = (ThinWall[])info.GetValue("ThinWall", typeof(ThinWall));
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
            _centroid = CalculateCentroid();
            _jxx = CalculateJxx();
            _jyy = CalculateJyy();
            _jw = CalculateJw();
            _jt = CalculateJt();
            _shearCenter = CalculateShearCenter();
            _sx = CalculateSx();
            _sy = CalculateSy();
            _area = CalculateArea();
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

            foreach (ThinWall tw in _thinWalls)
            {
                xSum += tw.CalculateArea() * tw.Centroid.X;
                ySum += tw.CalculateArea() * tw.Centroid.Y;
                area += tw.CalculateArea();
            }
            return new Point2d((xSum / area), (ySum / area));
        }

        public virtual double CalculateJt()
        {
            double jt = 0;
            foreach (ThinWall tw in _thinWalls)
            {
                jt += tw.CalculateJt();
            }
            return jt;
        }

        /// <summary>
        /// Calculate the first moment of inertia respect the X-axis (the Y-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        public virtual double CalculateJxx()
        {
            double j = 0;
            Point2d centroid = CalculateCentroid();
            foreach (ThinWall tw in _thinWalls)
            {
                j += tw.CalculateJxRespectCentroid();
                j += tw.CalculateArea() * Math.Pow((centroid.Y - tw.Centroid.Y), 2);
            }
            return j;
        }

        /// <summary>
        /// Calculate the first moment of inertia respect the Y-axis (the Z-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        public virtual double CalculateJyy()
        {
            double j = 0;
            Point2d centroid = CalculateCentroid();
            foreach (ThinWall tw in _thinWalls)
            {
                j += tw.CalculateJyRespectCentroid();
                j += tw.CalculateArea() * Math.Pow((centroid.X - tw.Centroid.X), 2);
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
            foreach (ThinWall tw in _thinWalls)            
                area += tw.CalculateArea();
            
            return area;
        }

        /// <summary>
        /// Calculate the first moment of area respect the X-axis (the Y-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        public virtual double CalculateSx()
        {
            double Sx = 0;
            for (int i = 0; i < _thinWalls.Count(); i++)
                Sx = Sx + _thinWalls[i].CalculateArea() * _thinWalls[i].Centroid.X;

            return Sx;
        }

        /// <summary>
        /// Calculate the first moment of area respect the Y-axis (the Z-axis for Eurocode)
        /// </summary>
        /// <returns></returns>
        public virtual double CalculateSy()
        {
            double Sy = 0;
            for (int i = 0; i < _thinWalls.Count(); i++)
                Sy = Sy + _thinWalls[i].CalculateArea() * _thinWalls[i].Centroid.Y;

            return Sy;
        }

        #endregion


        #region Nested classes ThinWall

        internal class ThinWall
        {
            #region Variables

            private readonly double _t;
            private readonly double _l;
            private readonly double _angle;
            private readonly Point2d _centroid;

            #endregion


            #region Protected constructor

            /// <summary>
            /// The default constructor of generic ThinWallSection
            /// </summary>
            /// <param name="lenght"></param>
            /// <param name="thickness"></param>
            /// <param name="angle"></param>
            /// <param name="centroid"></param>
            internal ThinWall(double lenght, double thickness, double angle, Point2d centroid)
                : base()
            {
                _t = thickness < 0 ? throw new ArgumentException($"Thickness cannot be lower than zero") : thickness;
                _l = lenght < 0 ? throw new ArgumentException($"Lenght cannot be lower than zero") : lenght;
                _angle = angle;
                _centroid = centroid;
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
            /// The centroid of the wall
            /// </summary>
            internal Point2d Centroid => _centroid;

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
            /// Calculate the first moment of inertia of the wall respect the X-axis passing throw the centroid
            /// </summary>
            /// <returns></returns>
            internal double CalculateJx()
            {
                if (_angle == 0)
                    return CalculateJxRespectCentroid() + CalculateArea() * Math.Pow((Centroid.X), 2);

                else if (_angle == Math.PI / 2)
                    return CalculateJxRespectCentroid() + CalculateArea() * Math.Pow((Centroid.X), 2);

                else
                    throw new NotImplementedException("Not implemented angle");
            }

            /// <summary>
            /// Calculate the first moment of inertia of the wall respect the Y-axis passing throw the centroid
            /// </summary>
            /// <returns></returns>
            internal double CalculateJy()
            {
                if (_angle == 0)
                    return CalculateJyRespectCentroid() + CalculateArea() * Math.Pow((Centroid.Y), 2);

                else if (_angle == Math.PI / 2)
                    return CalculateJyRespectCentroid() + CalculateArea() * Math.Pow((Centroid.Y), 2);

                else
                    throw new NotImplementedException("Not implemented angle");
            }

            internal double CalculateJyRespectCentroid()
            {
                if (_angle == 0)
                    return _t * Math.Pow(_l, 3) / 12;

                else if (_angle == Math.PI / 2)
                    return _l * Math.Pow(_t, 3) / 12;

                else
                    throw new NotImplementedException("Not implemented angle");
            }

            internal double CalculateJxRespectCentroid()
            {
                if (_angle == 0)
                    return _l * Math.Pow(_t, 3) / 12;

                else if (_angle == Math.PI / 2)
                    return _t * Math.Pow(_l, 3) / 12;

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
