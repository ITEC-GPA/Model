using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    /// <summary>
    /// Base of the loads: the load case and the coordinate system of the components of the load
    /// </summary>
    [Serializable]
    public abstract class Load : ModelObjectId
    {
        #region Variables

        /// <summary>
        /// The load case
        /// </summary>
        private LoadCaseBase _loadCase;
        /// <summary>
        /// The coordinate system of the components of the load
        /// </summary>
        protected CoordinateSystem _coordinateSystem;

        #endregion

        #region Properties

        /// <summary>
        /// Load case associated with the load
        /// </summary>
        public LoadCaseBase LoadCase { get => _loadCase; set => _loadCase = value; }

        /// <summary>
        /// The name of the load case
        /// </summary>
        public string LoadCaseName { get => _loadCase.Name; }

        /// <summary>
        /// The coordinate system of the components of the load (the geometry is in the global system)
        /// </summary>
        public CoordinateSystem CoordinateSystem { get => _coordinateSystem; set => _coordinateSystem = value; }

        #endregion

        #region Constructor

        /// <summary>
        /// Creates a load
        /// </summary>
        /// <param name="loadCase">The load case</param>
        /// <param name="coordinateSystem">The coordinate system of the components</param>
        /// <param name="name">The name</param>
        /// <param name="id">The id</param>
        protected Load(LoadCaseBase loadCase, CoordinateSystem coordinateSystem, string name, int id)
            : base(id, name)
        {
            _loadCase = loadCase;
            _coordinateSystem = coordinateSystem;
        }

        /// <summary>
        /// Deserialization constructor: reads the data of <see cref="ModelObjectId"/>, the load case (as <see cref="LoadCases.LoadCase"/>: other
        /// types of load case fail) and the coordinate system
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        protected Load(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _loadCase = (LoadCaseBase)info.GetValue("LoadCase", typeof(LoadCaseBase));
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
        }

        #endregion

        #region Equals, HashCode and operators

        /// <summary>
        /// The geometry where the load is applied
        /// </summary>
        /// <returns>The point, line or shape of the load</returns>
        public abstract GeometryBase GetGeometryBase();

        /// <summary>
        /// Serializes the data of <see cref="ModelObjectId"/>, the load case and the coordinate system
        /// </summary>
        /// <param name="info">The serialization data</param>
        /// <param name="context">The serialization context</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCase", _loadCase);
            info.AddValue("CoordinateSystem", _coordinateSystem);
        }

        /// <summary>
        /// Equality of name, load case and coordinate system
        /// </summary>
        /// <param name="obj">The object to compare</param>
        /// <returns>True if <paramref name="obj"/> is an equal load</returns>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return obj is Load load &&
                EqualityComparer<LoadCaseBase>.Default.Equals(_loadCase, load._loadCase) &&
                _coordinateSystem.Equals(load.CoordinateSystem) &&
                base.Equals(obj);
        }

        /// <summary>
        /// The hash code of name, load case and coordinate system
        /// </summary>
        /// <returns>The hash code</returns>
        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23 * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<LoadCaseBase>.Default.GetHashCode(_loadCase);
                hashCode = hashCode * -17 + _coordinateSystem.GetHashCode();

                return hashCode;
            }
        }

        /// <summary>
        /// Equality operator (see <see cref="Equals(object)"/>); two null loads are equal
        /// </summary>
        /// <param name="obj1">The first load</param>
        /// <param name="obj2">The second load</param>
        /// <returns>True if the loads are equal</returns>
        public static bool operator ==(Load obj1, Load obj2)
        {

            if (obj1 is null)
            {
                return obj2 is null;
            }

            if (ReferenceEquals(obj1, obj2))
                return true;

            return obj1.Equals(obj2);
        }

        /// <summary>
        /// Inequality operator (see <see cref="Equals(object)"/>)
        /// </summary>
        /// <param name="obj1">The first load</param>
        /// <param name="obj2">The second load</param>
        /// <returns>True if the loads are different</returns>
        public static bool operator !=(Load obj1, Load obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Equals, HashCode and operators
    }
}
