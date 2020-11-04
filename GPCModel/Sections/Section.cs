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
        protected double _j;
        protected double _i11;
        protected double _i22;
        protected double _sl1;
        protected double _sl2;
        protected double _sa1;
        protected double _sa2;
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

        public double J
        {
            get => _j;
            set => _j = value;
        }

        public double I11
        {
            get => _i11;
            set => _i11 = value;
        }

        public double I22
        {
            get => _i22;
            set => _i22 = value;
        }

        public double SL1
        {
            get => _sl1;
            set => _sl1 = value;
        }

        public double SL2
        {
            get => _sl2;
            set => _sl2 = value;
        }

        public double SA1
        {
            get => _sa1;
            set => _sa1 = value;
        }

        public double SA2
        {
            get => _sa2;
            set => _sa2 = value;
        }        

        public Point2d Centroid
        {
            get => _centroid;
            set => _centroid = value;
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
            _j = info.GetDouble("J");
            _i11 = info.GetDouble("I11");
            _i22 = info.GetDouble("I22");
            _sl1 = info.GetDouble("SL1");
            _sl2 = info.GetDouble("SL2");
            _sa1 = info.GetDouble("SA1");
            _sa2 = info.GetDouble("SA2");
            _centroid = (Point2d)info.GetValue("Centroid", typeof(Point2d));
            _angleX1 = info.GetDouble("AngleX1");
        }

        #endregion
        #region Public Methods Specific

        
        #endregion
        #region Virtual Methods

        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Area", _area);
            info.AddValue("J", _j);
            info.AddValue("I11", _i11);
            info.AddValue("I22", _i22);
            info.AddValue("SL1", _sl1);
            info.AddValue("SL2", _sl2);
            info.AddValue("SA1", _sa1);
            info.AddValue("SA2", _sa2);
            info.AddValue("Centroid", _centroid, typeof(Point2d));
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
