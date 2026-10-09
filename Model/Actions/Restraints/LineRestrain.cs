using GPC.Geometry;
using GPC.Model.Elements;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Restraints
{
    /// <summary>
    /// Restrains of the degrees of freedom along a line
    /// </summary>
    public class LineRestrain : GeometryRestrain
    {
        #region Variables

        /// <summary>
        /// The restrained line
        /// </summary>
        private Line3d _line;

        #endregion

        #region Properties

        /// <summary>
        /// The restrained line
        /// </summary>
        public Line3d Line { get => _line; set => _line = value; }

        #endregion

        #region Public Constructors

        /// <summary>
        /// Creates the restrain in the global coordinate system
        /// </summary>
        /// <param name="line">The line</param>
        /// <param name="restrains">The restrains of the degrees of freedom</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentNullException">If <paramref name="line"/> is null</exception>
        public LineRestrain(Line3d line, List<DofRestrain> restrains, string name = "", int id = IDUNASSIGNED)
            : this(line, CoordinateSystem.Global, restrains, name, id)
        {

        }

        /// <summary>
        /// Creates the restrain
        /// </summary>
        /// <param name="line">The line</param>
        /// <param name="coordinateSystem">The coordinate system</param>
        /// <param name="restrains">The restrains of the degrees of freedom</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        /// <exception cref="ArgumentNullException">If <paramref name="line"/> or <paramref name="coordinateSystem"/> is null</exception>
        public LineRestrain(Line3d line, CoordinateSystem coordinateSystem, List<DofRestrain> restrains, string name = "", int id = IDUNASSIGNED)
            : base(coordinateSystem, restrains, name, id)
        {
            _line = line ?? throw new ArgumentNullException("Base line is null");
        }

        /// <summary>
        /// Deserialization constructor (the base one is not implemented)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        /// <exception cref="NotImplementedException">Always</exception>
        protected LineRestrain(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _line = (Line3d)info.GetValue("Line", typeof(Line3d));
        }

        #endregion

        #region Methods

        /// <summary>
        /// A restrain of a line with all the degrees of freedom restrained
        /// </summary>
        /// <param name="line">The line</param>
        /// <param name="coordinateSystem">The coordinate system</param>
        /// <returns>The new restrain</returns>
        public static LineRestrain GetAllFixed(Line3d line, CoordinateSystem coordinateSystem)
        {
            return new LineRestrain(line, coordinateSystem, new List<DofRestrain>
                                                                        {
                                                                            new DofRestrain(DOF.DX),
                                                                            new DofRestrain(DOF.DY),
                                                                            new DofRestrain(DOF.DZ),
                                                                            new DofRestrain(DOF.RX),
                                                                            new DofRestrain(DOF.RY),
                                                                            new DofRestrain(DOF.RZ)
                                                                        }
            );
        }

        /// <summary>
        /// A restrain of a line with <see cref="GeometryRestrain.DOF.DX"/>, <see cref="GeometryRestrain.DOF.DY"/> and
        /// <see cref="GeometryRestrain.DOF.DZ"/> restrained
        /// </summary>
        /// <param name="line">The line</param>
        /// <param name="coordinateSystem">The coordinate system</param>
        /// <returns>The new restrain</returns>
        public static LineRestrain GetAllDisplacementFixed(Line3d line, CoordinateSystem coordinateSystem)
        {
            return new LineRestrain(line, coordinateSystem, new List<DofRestrain>
                                                                            {
                                                                                new DofRestrain(DOF.DX),
                                                                                new DofRestrain(DOF.DY),
                                                                                new DofRestrain(DOF.DZ)
                                                                            }
            );
        }

        /// <summary>
        /// The line
        /// </summary>
        /// <returns>The line</returns>
        public override GeometryBase GetGeometry() => _line;

        /// <summary>
        /// The restrained element (not implemented)
        /// </summary>
        /// <returns>Nothing</returns>
        /// <exception cref="NotImplementedException">Always</exception>
        public override Element GetElement()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Equals, hashcode, operators

        /// <summary>
        /// Serializes the restrain (the base one is not implemented)
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        /// <exception cref="NotImplementedException">Always</exception>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("Line", _line);
        }

        /// <summary>
        /// Equality of the lines and of the base (see <see cref="GeometryRestrain.Equals(object)"/>)
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal restrain</returns>
        public override bool Equals(object obj)
        {
            if (obj is null)
                return false;

            if (ReferenceEquals(this, obj))
                return true;

            return (obj is LineRestrain objCasted) && _line.Equals(objCasted.Line) && base.Equals(objCasted);
        }

        /// <summary>
        /// The hash code of the line and of the base
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (-391 + base.GetHashCode()) * -17 + _line.GetHashCode();
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first restrain</param>
        /// <param name="obj2">The second restrain</param>
        /// <returns>True if the restrains are equal</returns>
        public static bool operator ==(LineRestrain obj1, LineRestrain obj2)
        {
            if (obj1 is null)
            {
                return obj2 is null;
            }

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first restrain</param>
        /// <param name="obj2">The second restrain</param>
        /// <returns>True if the restrains are different</returns>
        public static bool operator !=(LineRestrain obj1, LineRestrain obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion

    }
}
