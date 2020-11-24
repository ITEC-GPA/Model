using System;
using System.Runtime.Serialization;
using GPC.Geometry;

namespace GPC.Model.Elements
{
    public class PointRestrain : Element, IRestrain
    {
        #region Variables

        private Point3d _point;

        private CoordinateSystem _coordinateSystem;

        // true means restrained
        private bool _d1;
        private bool _d2;
        private bool _d3;
        private bool _r1;
        private bool _r2;
        private bool _r3;

        private double _kd1;
        private double _kd2;
        private double _kd3;
        private double _kr1;
        private double _kr2;
        private double _kr3;


        #endregion

        #region Properties
        
        public Point3d Point => _point;

        public bool D1 => _d1;
        public bool D2 => _d2;
        public bool D3 => _d3;
        public bool R1 => _r1;
        public bool R2 => _r2;
        public bool R3 => _r3;
        public double Kd1 => _kd1;
        public double Kd2 => _kd2;
        public double Kd3 => _kd3;
        public double Kr1 => _kr1;
        public double Kr2 => _kr2;
        public double Kr3 => _kr3;

        #endregion

        #region Constructors
        /// <summary>
        /// All fixed constructor
        /// </summary>
        /// <param name="coordinateSystem"></param>
        /// <param name="point"></param>
        public PointRestrain(CoordinateSystem coordinateSystem, Point3d point)
            : this(coordinateSystem, point, true, true, true, true, true, true, 0, 0, 0, 0, 0, 0, Guid.NewGuid())
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="coordinateSystem"></param>
        /// <param name="point"></param>
        /// <param name="d1">True if axis 1 displacement is restrained</param>
        /// <param name="d2">True if axis 2 displacement is restrained</param>
        /// <param name="d3">True if axis 3 displacement is restrained</param>
        /// <param name="r1">True if axis 1 rotation is restrained</param>
        /// <param name="r2">True if axis 2 rotation is restrained</param>
        /// <param name="r3">True if axis 3 rotation is restrained</param>
        public PointRestrain(CoordinateSystem coordinateSystem, Point3d point, bool d1, bool d2, bool d3, bool r1, bool r2, bool r3)
            : this(coordinateSystem, point, d1, d2, d3, r1, r2, r3, 0, 0, 0, 0, 0, 0, Guid.NewGuid())
        {

        }

        /// <summary>
        /// If stiffness is != 0, bool restrain parameter is setted to false
        /// </summary>
        /// <param name="coordinateSystem"></param>
        /// <param name="point"></param>
        /// <param name="d1">True if axis 1 displacement is restrained</param>
        /// <param name="d2">True if axis 2 displacement is restrained</param>
        /// <param name="d3">True if axis 3 displacement is restrained</param>
        /// <param name="r1">True if axis 1 rotation is restrained</param>
        /// <param name="r2">True if axis 2 rotation is restrained</param>
        /// <param name="r3">True if axis 3 rotation is restrained</param>
        /// <param name="kd1">Stiffness associated to displacement along axis 1</param>
        /// <param name="kd2">Stiffness associated to displacement along axis 2</param>
        /// <param name="kd3">Stiffness associated to displacement along axis 3</param>
        /// <param name="kr1">Stiffness associated to rotation around axis 1</param>
        /// <param name="kr2">Stiffness associated to rotation around axis 2</param>
        /// <param name="kr3">Stiffness associated to rotation around axis 3</param>
        /// <param name="guid"></param>
        public PointRestrain(CoordinateSystem coordinateSystem, Point3d point, bool d1, bool d2, bool d3, bool r1, bool r2, bool r3, double kd1, double kd2, double kd3, double kr1, double kr2, double kr3, Guid guid)
            : base(guid)
        {
            this._point = point ?? throw new ArgumentNullException("Base point is null");
            this._coordinateSystem = coordinateSystem ?? throw new ArgumentNullException("Coordinate system cannot be null");

            this._d1 = d1;
            this._d2 = d2;
            this._d3 = d3;
            this._r1 = r1;
            this._r2 = r2;
            this._r3 = r3;

            this._kd1 = kd1;
            this._kd2 = kd2;
            this._kd3 = kd3;
            this._kr1 = kr1;
            this._kr2 = kr2;
            this._kr3 = kr3;

            if (kd1 != 0)
                _d1 = false;
            if (kd2 != 0)
                _d2 = false;
            if (kd3 != 0)
                _d3 = false;
            if (kr1 != 0)
                _r1 = false;
            if (kr2 != 0)
                _r2 = false;
            if (kr3 != 0)
                _r3 = false;
        }


        public PointRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _d1 = info.GetBoolean("D1");
            _d2 = info.GetBoolean("D2");
            _d3 = info.GetBoolean("D3");
            _r1 = info.GetBoolean("R1");
            _r2 = info.GetBoolean("R2");
            _r3 = info.GetBoolean("R3");
            _kd1 = info.GetDouble("KD1");
            _kd2 = info.GetDouble("KD2");
            _kd3 = info.GetDouble("KD3");
            _kr1 = info.GetDouble("KR1");
            _kr2 = info.GetDouble("KR2");
            _kr3 = info.GetDouble("KR3");
            _point = (Point3d)info.GetValue("Point", typeof(Point3d));
        }

        #endregion

        #region Public methods

        public bool[] GetRestrains() => new bool[6] { D1, D2, D3, R1, R2, R3 };        

        public double[] GetStiffnesses() => new double[6] { Kd1, Kd2, Kd3, Kr1, Kr2, Kr3 };
        
        public Vector3d GetV1() => _coordinateSystem.V11;

        public Vector3d GetV2() => _coordinateSystem.V22;

        public Vector3d GetV3() => _coordinateSystem.V33;

        public Point3d GetCoordinateSystemOrigin() => _coordinateSystem.Origin;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("D1", _d1);
            info.AddValue("D2", _d2);
            info.AddValue("D3", _d3);
            info.AddValue("R1", _r1);
            info.AddValue("R2", _r2);
            info.AddValue("R3", _r3);
            info.AddValue("KD1", _kd1);
            info.AddValue("KD2", _kd2);
            info.AddValue("KD3", _kd3);
            info.AddValue("KR1", _kr1);
            info.AddValue("KR2", _kr2);
            info.AddValue("KR3", _kr3);
            info.AddValue("Point", _point);
        }

        #endregion
    }
}
