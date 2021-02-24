using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Linq;
using GPC.Geometry;
using GPC.Model.FEM;
using GPC.Model.FreedomCases;

namespace GPC.Model.Restrains
{
    public class PointRestrain : GeometryRestrain
    {
        #region Variables

        private Point3d _point;

        #endregion

        #region Properties

        public Point3d Point => _point;


        #endregion

        #region Constructors

        public PointRestrain(Point3d point, FreedomCase freedomCase, CoordinateSystem coordinateSystem)
            : this(point, freedomCase, coordinateSystem, null, Guid.NewGuid(), string.Empty)
        {

        }

        public PointRestrain(Point3d point, FreedomCase freedomCase, CoordinateSystem coordinateSystem, List<DofRestrain> restrains)
            : this(point, freedomCase, coordinateSystem, restrains, Guid.NewGuid(), string.Empty)
        {

        }

        public PointRestrain(Point3d point, FreedomCase freedomCase, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, Guid guid, string name)
            : base(freedomCase, coordinateSystem, restrains, guid, name)
        {
            this._point = point ?? throw new ArgumentNullException("Base point is null");
        }

        public PointRestrain(SerializationInfo info, StreamingContext context) 
            : base(info, context)
        {
            _point = (Point3d)info.GetValue("Point", typeof(Point3d));
        }


        public static PointRestrain GetAllFixed(Point3d point, FreedomCase freedomCase, CoordinateSystem coordinateSystem) 
        {
            List<DofRestrain> restrains = new List<DofRestrain>();

            foreach (var dof in (LinearSolver.DOF[])Enum.GetValues(typeof(LinearSolver.DOF)))
            {
                restrains.Add(new DofRestrain(dof, true));
            }

            return new PointRestrain(point, freedomCase, coordinateSystem, restrains);                
        }


        #endregion

        #region Public methods

        public override GeometryBase GetGeometry() => _point;

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Point", _point);
        }


        #endregion
    }
}
