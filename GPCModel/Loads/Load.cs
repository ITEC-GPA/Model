using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    [Serializable]
    public abstract class Load : ModelObjectId
    {
        #region Variables

        private LoadCaseBase _loadCase;
        protected CoordinateSystem _coordinateSystem;

        #endregion

        #region Properties

        /// <summary>
        /// Load case associated with the load
        /// </summary>
        public LoadCaseBase LoadCase { get => _loadCase; set => _loadCase = value; }

        public string LoadCaseName { get => _loadCase.Name; set => _loadCase.Name = value; }

        /// <summary>
        /// reference system of the load
        /// </summary>
        public CoordinateSystem CoordinateSystem { get => _coordinateSystem; set => _coordinateSystem = value; }

        #endregion

        #region Constructor

        protected Load(LoadCaseBase loadCase, CoordinateSystem coordinateSystem, string name, int id)
            : base(id, name)
        {
            _loadCase = loadCase;
            _coordinateSystem = coordinateSystem;
        }

        protected Load(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _loadCase = (LoadCase)info.GetValue("LoadCase", typeof(LoadCase));
            _coordinateSystem = (CoordinateSystem)info.GetValue("CoordinateSystem", typeof(CoordinateSystem));
        }

        #endregion

        #region Equals, HashCode and operators

        public abstract GeometryBase GetGeometryBase();

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCase", _loadCase);
            info.AddValue("CoordinateSystem", _coordinateSystem);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return obj is Load load &&
                EqualityComparer<LoadCaseBase>.Default.Equals(_loadCase, load._loadCase) &&
                _coordinateSystem.Equals(load.CoordinateSystem) &&
                base.Equals(obj);
        }

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

        public static bool operator !=(Load obj1, Load obj2)
        {
            return !(obj1 == obj2);
        }

        #endregion Equals, HashCode and operators
    }
}