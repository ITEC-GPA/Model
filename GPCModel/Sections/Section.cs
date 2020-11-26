using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    public class Section
    {
        public struct ShapeMaterial
        {
            public Shape2d Shape { get; set; }
            public Material Material { get; set; }
        }

        #region Variables
        protected Material _material;
        protected double _area;
        protected double _jy;
        protected double _jz;
        protected double _wply;
        protected double _wplz;
        protected double _jt;
        protected double _jw;
        protected Point2d _shearCenter;
        protected Point2d _centroid;
        protected double _angleX1;
        #endregion

        #region Properties

        public Material Material
        {
            get => _material;
            set => _material = value;
        }

        public double Area
        {
            get => _area;
            set => _area = value;
        }

        public double Jt
        {
            get => _jt;
            set => _jt = value;
        }
        public double Jw
        {
            get => _jw;
            set => _jw = value;
        }

        public double Jy
        {
            get => _jy;
            set => _jy= value;
        }

        public double Wply
        {
            get => _wply;
            set => _wply = value;
        }

        public double Wplz
        {
            get => _wplz;
            set => _wplz = value;
        }

        public double Jz
        {
            get => _jz;
            set => _jz = value;
        }   

        public Point2d Centroid
        {
            get => _centroid;
            set => _centroid = value;
        }

        public Point2d ShearCenter
        {
            get => _shearCenter;
            set => _shearCenter = value;
        }

        public double AngleX1
        {
            get => _angleX1;
            set => _angleX1 = value;
        }
        #endregion

        #region Public Constructors

        public Section()
        {
            Calculate();
        }

        public Section(SerializationInfo info, StreamingContext context)
        {
            _area = info.GetDouble("Area");
            _jt = info.GetDouble("J");
            _jw = info.GetDouble("I11");
            _jz = info.GetDouble("I22");
            _jy = info.GetDouble("SL1");
            _centroid = (Point2d)info.GetValue("Centroid", typeof(Point2d));
            _shearCenter = (Point2d)info.GetValue("ShearCenter", typeof(Point2d));
            _angleX1 = info.GetDouble("AngleX1");
        }

        #endregion
        public double Wely(double z) {
            return z / Jy;
        }
        public double Welz(double y)
        {
            return y / Jz;
        }
        #region Public Methods Specific


        #endregion
        #region Virtual Methods
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Area", _area);
            info.AddValue("Jt", _jt);
            info.AddValue("Jw", _jw);
            info.AddValue("Jy", _jy);
            info.AddValue("Jz", _jz);
            info.AddValue("Centroid", _centroid, typeof(Point2d));
            info.AddValue("ShearCenter", _shearCenter, typeof(Point2d));
            info.AddValue("AngleX1", _angleX1);
        }

        public virtual void Calculate()
        { 
        }

        public virtual ShapeMaterial[] GetShapes()
        {
            return null;
        }
        #endregion
    }
}
