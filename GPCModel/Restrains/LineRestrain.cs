using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.FEM;

namespace GPC.Model.Restrains
{
    public class LineRestrain : ModelObject, IGeometryRestrain
    {
        #region Variables

        private Line3d _line;

        private List<DofRestrain> _restrains;

        private CoordinateSystem _coordinateSystem;

        #endregion

        #region Properties

        public Line3d Line => _line;

        public List<DofRestrain> Restrains => _restrains;

        #endregion

        #region Public Constructors

        public LineRestrain(Line3d line, CoordinateSystem coordinateSystem)
            : this(line, coordinateSystem, null, Guid.NewGuid(), string.Empty)
        {

        }

        public LineRestrain(Line3d line, CoordinateSystem coordinateSystem, List<DofRestrain> restrains) 
            : this(line, coordinateSystem, restrains, Guid.NewGuid(), string.Empty)
        {

        }

        public LineRestrain(Line3d line, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, Guid guid, string name) 
            : base(guid, name)
        {
            this._line = line ?? throw new ArgumentNullException("Base line is null");
            this._restrains = restrains ?? new List<DofRestrain>();
            this._coordinateSystem = coordinateSystem ?? throw new ArgumentNullException(nameof(coordinateSystem)); ; ;
        }


        public void SetCoordinateSystem(CoordinateSystem coordinateSystem)
        {
            _coordinateSystem = coordinateSystem;
        }

        public void AddRestain(DofRestrain dofRestrain)
        {
            _restrains.Add(dofRestrain);
        }

        public static LineRestrain GetAllFixed(Line3d line, CoordinateSystem coordinateSystem)
        {
            List<DofRestrain> restrains = new List<DofRestrain>();

            foreach (var dof in (LinearSolver.DOF[])Enum.GetValues(typeof(LinearSolver.DOF)))
            {
                restrains.Add(new DofRestrain(dof, true));
            }

            return new LineRestrain(line, coordinateSystem, restrains);
        }

        
        public LineRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _line = (Line3d)info.GetValue("Line", typeof(Line3d));
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
            _restrains = (List<DofRestrain>)info.GetValue("DofRestrain", typeof(List<DofRestrain>));
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
                kvp[i] = new KeyValuePair<LinearSolver.DOF, double>(key: _restrains[i].Dof, value: _restrains[i].Stiffness);
            }

            return kvp;
        }

        public Vector3d GetV1() => _coordinateSystem.V11;

        public Vector3d GetV2() => _coordinateSystem.V22;

        public Vector3d GetV3() => _coordinateSystem.V33;

        public Point3d GetCoordinateSystemOrigin() => _coordinateSystem.Origin;

        public GeometryBase GetGeometry() => _line;

        #endregion


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Line", _line);
            info.AddValue("CoordinateSystem", _coordinateSystem);
            info.AddValue("Restrains", _restrains);
        }

    }
}
