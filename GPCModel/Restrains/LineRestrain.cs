using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using GPC.Geometry;
using GPC.Model.FEM;
using GPC.Model.FreedomCases;

namespace GPC.Model.Restrains
{
    public class LineRestrain : GeometryRestrain
    {
        #region Variables

        private Line3d _line;

        #endregion

        #region Properties

        public Line3d Line => _line;

        #endregion

        #region Public Constructors


        /// <summary>
        /// 
        /// </summary>
        /// <param name="line"></param>
        /// <param name="freedomCase"></param>
        /// <param name="restrains"></param>
        /// <remarks><see cref="GeometryRestrain.CoordinateSystem"/> set to Global</remarks>
        public LineRestrain(Line3d line, FreedomCase freedomCase, List<DofRestrain> restrains)
            : this(line, freedomCase, CoordinateSystem.Global, restrains, Guid.NewGuid(), string.Empty)
        {

        }

        public LineRestrain(Line3d line, FreedomCase freedomCase, CoordinateSystem coordinateSystem, List<DofRestrain> restrains) 
            : this(line, freedomCase, coordinateSystem, restrains, Guid.NewGuid(), string.Empty)
        {

        }

        public LineRestrain(Line3d line, FreedomCase freedomCase, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, Guid guid, string name) 
            : base(freedomCase, coordinateSystem, restrains, guid, name)
        {
            this._line = line ?? throw new ArgumentNullException("Base line is null");
        }

        /// <summary>
        /// Set all the <see cref="LinearSolver.DOF"/> to restrained for the given line and freedomcase
        /// </summary>
        /// <param name="line"></param>
        /// <param name="freedomCase"></param>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        public static LineRestrain GetAllFixed(Line3d line, FreedomCase freedomCase, CoordinateSystem coordinateSystem)
        {
            List<DofRestrain> restrains = new List<DofRestrain>();

            foreach (var dof in (LinearSolver.DOF[])Enum.GetValues(typeof(LinearSolver.DOF)))
            {
                restrains.Add(new DofRestrain(dof, true));
            }

            return new LineRestrain(line, freedomCase, coordinateSystem, restrains);
        }

        /// <summary>
        /// Set <see cref="LinearSolver.DOF.DX"/>, <see cref="LinearSolver.DOF.DY"/> and <see cref="LinearSolver.DOF.DZ"/> to restrained for the given line and freedomcase
        /// </summary>
        /// <param name="line"></param>
        /// <param name="freedomCase"></param>
        /// <param name="coordinateSystem"></param>
        /// <returns></returns>
        public static LineRestrain GetAllDisplacementFixed(Line3d line, FreedomCase freedomCase, CoordinateSystem coordinateSystem)
        {
            List<DofRestrain> restrains = new List<DofRestrain>();

            restrains.Add(new DofRestrain(LinearSolver.DOF.DX, true));
            restrains.Add(new DofRestrain(LinearSolver.DOF.DY, true));
            restrains.Add(new DofRestrain(LinearSolver.DOF.DZ, true));

            return new LineRestrain(line, freedomCase, coordinateSystem, restrains);
        }

        public LineRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _line = (Line3d)info.GetValue("Line", typeof(Line3d));
        }


        #endregion

      
        public override GeometryBase GetGeometry() => _line;


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Line", _line);
        }

    }
}
