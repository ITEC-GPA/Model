using System;
using System.Runtime.Serialization;
using GPC.Geometry;

namespace GPC.Model.Elements
{
    public class PointRestrain : Element, IGeometryRestrain
    {
        #region Variables

        private Point3d _point;

        private CoordinateSystem _coordinateSystem;

        private Restrain _restrain;

        #endregion

        #region Properties

        public Point3d Point => _point;

        public Restrain Restrain => _restrain;

        #endregion

        #region Constructors

        public PointRestrain(CoordinateSystem coordinateSystem, Point3d point, Restrain restrain)
            : this(coordinateSystem, point, restrain, Guid.NewGuid())
        {

        }

        public PointRestrain(CoordinateSystem coordinateSystem, Point3d point, Restrain restrain, Guid guid)
            : base(guid)
        {
            this._point = point ?? throw new ArgumentNullException("Base point is null");
            this._coordinateSystem = coordinateSystem ?? throw new ArgumentNullException("Coordinate system cannot be null");
            this._restrain = restrain ?? throw new ArgumentNullException("Restrain cannot be null");
        }

        /// <summary>
        /// All fixed constructor
        /// </summary>
        /// <param name="coordinateSystem"></param>
        /// <param name="point"></param>
        public PointRestrain(CoordinateSystem coordinateSystem, Point3d point)
            : this(coordinateSystem, point, Restrain.AllFixed)
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

            this._restrain = new Restrain(d1, d2, d3, r1, r2, r3, kd1, kd2, kd3, kr1, kr2, kr3, guid);
        }


        public PointRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _point = (Point3d)info.GetValue("Point", typeof(Point3d));
            _restrain = (Restrain)info.GetValue("Restrain", typeof(Restrain));
        }

        #endregion

        #region Public methods

        public bool[] GetRestrains() => _restrain.GetRestrains();

        public double[] GetStiffnesses() => _restrain.GetStiffnesses();

        public Vector3d GetV1() => _coordinateSystem.V11;

        public Vector3d GetV2() => _coordinateSystem.V22;

        public Vector3d GetV3() => _coordinateSystem.V33;

        public Point3d GetCoordinateSystemOrigin() => _coordinateSystem.Origin;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Point", _point);
            info.AddValue("Restrain", _restrain);
        }

        #endregion
    }
}
