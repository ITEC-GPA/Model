using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM;

namespace GPC.Model.Restrains
{
    public class PointRestrain : ModelObject, IGeometryRestrain
    {
        #region Variables

        private Point3d _point;

        private List<DofRestrain> _restrains;

        private CoordinateSystem _coordinateSystem;

        #endregion

        #region Properties

        public Point3d Point => _point;

        public List<DofRestrain> Restrains => _restrains;

        public CoordinateSystem CoordinateSystem => _coordinateSystem;
        #endregion

        #region Constructors

        public PointRestrain(Point3d point, CoordinateSystem coordinateSystem)
            : this(point, coordinateSystem, null, Guid.NewGuid(), string.Empty)
        {

        }

        public PointRestrain(Point3d point, CoordinateSystem coordinateSystem, List<DofRestrain> restrains)
            : this(point, coordinateSystem, restrains, Guid.NewGuid(), string.Empty)
        {

        }

        public PointRestrain(Point3d point, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, Guid guid, string name)
            : base(guid, name)
        {
            this._point = point ?? throw new ArgumentNullException("Base point is null");
            this._restrains = restrains ?? new List<DofRestrain>();
            this._coordinateSystem = coordinateSystem ?? throw new ArgumentNullException(nameof(coordinateSystem)); ;
        }

        public PointRestrain(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _point = (Point3d)info.GetValue("Point", typeof(Point3d));
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
            _restrains = (List<DofRestrain>)info.GetValue("DofRestrain", typeof(List<DofRestrain>));
        }

        public void SetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            _coordinateSystem = coordinateSystem;
        }

        public void AddRestain(DofRestrain dofRestrain)
        {
            _restrains.Add(dofRestrain);
        }

        public static PointRestrain GetAllFixed(Point3d point, CoordinateSystem coordinateSystem) 
        {
            List<DofRestrain> restrains = new List<DofRestrain>();

            foreach (var dof in (LinearSolver.DOF[])Enum.GetValues(typeof(LinearSolver.DOF)))
            {
                restrains.Add(new DofRestrain(dof, true));
            }

            return new PointRestrain(point, coordinateSystem, restrains);                
        }


        #endregion

        #region Public methods

        public KeyValuePair<LinearSolver.DOF, bool>[] GetRestrains()
        {
            KeyValuePair<LinearSolver.DOF, bool>[] kvp = new KeyValuePair<LinearSolver.DOF, bool>[Restrains.Count];

            for (int i = 0; i < _restrains.Count; i++)
            {
                kvp[i] = new KeyValuePair<LinearSolver.DOF, bool>(key: _restrains[i].Dof, value: _restrains[i].Restrained);
            }

            return kvp;
        }

        public KeyValuePair<LinearSolver.DOF, double>[] GetStiffnesses() 
        {
            KeyValuePair<LinearSolver.DOF, double>[] kvp = new KeyValuePair<LinearSolver.DOF, double>[Restrains.Count];

            for (int i = 0; i < _restrains.Count; i++)
            {
                kvp[i] = new KeyValuePair<LinearSolver.DOF, double>( key:_restrains[i].Dof, value:_restrains[i].Stiffness );    
            }

            return kvp;
        }

        public Vector3d GetV1() => _coordinateSystem.V11;

        public Vector3d GetV2() => _coordinateSystem.V22;

        public Vector3d GetV3() => _coordinateSystem.V33;

        public Point3d GetCoordinateSystemOrigin() => _coordinateSystem.Origin;

        public GeometryBase GetGeometry() => _point;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Point", _point);
            info.AddValue("CoordinateSystem", _coordinateSystem);
            info.AddValue("Restrains", _restrains);
        }


        #endregion
    }
}
