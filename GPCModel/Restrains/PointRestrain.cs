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

        /// <summary>
        /// 
        /// </summary>
        /// <param name="point"></param>
        /// <param name="freedomCase"></param>
        /// <param name="restrains"></param>
        /// <remarks><see cref="GeometryRestrain.CoordinateSystem"/> set to Global</remarks>
        public PointRestrain(Point3d point, FreedomCase freedomCase, List<DofRestrain> restrains)
            : this(point, freedomCase, CoordinateSystem.Global, restrains, Guid.NewGuid(), string.Empty)
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


        /// <summary>
        /// Set all the <see cref="Solver.DOF"/> to restrained for the given point and freedomcase
        /// </summary>
        /// <param name="point"></param>
        /// <param name="freedomCase"></param>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        public static PointRestrain GetAllFixed(Point3d point, FreedomCase freedomCase, CoordinateSystem coordinateSystem) 
        {
            List<DofRestrain> restrains = new List<DofRestrain>();

            foreach (var dof in (Solver.DOF[])Enum.GetValues(typeof(Solver.DOF)))
            {
                restrains.Add(new DofRestrain(dof, true));
            }

            return new PointRestrain(point, freedomCase, coordinateSystem, restrains);                
        }

        /// <summary>
        /// Set <see cref="Solver.DOF.DX"/>, <see cref="Solver.DOF.DY"/> and <see cref="Solver.DOF.DZ"/> to restrained for the given line and freedomcase
        /// </summary>
        /// <param name="point"></param>
        /// <param name="freedomCase"></param>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        public static PointRestrain GetAllDisplacementFixed(Point3d point, FreedomCase freedomCase, CoordinateSystem coordinateSystem)
        {
            List<DofRestrain> restrains = new List<DofRestrain>();

            restrains.Add(new DofRestrain(Solver.DOF.DX, true));
            restrains.Add(new DofRestrain(Solver.DOF.DY, true));
            restrains.Add(new DofRestrain(Solver.DOF.DZ, true));

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
