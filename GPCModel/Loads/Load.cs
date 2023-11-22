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

        #endregion

        #region Properties

        /// <summary>
        /// Load case associated with the load
        /// </summary>
        public LoadCaseBase LoadCase { get => _loadCase; set => _loadCase = value; }

        public string LoadCaseName { get => _loadCase.Name; set => _loadCase.Name = value; }

        #endregion

        #region Constructor

        protected Load(LoadCaseBase loadCase)
            : base(Guid.NewGuid())
        {
            _loadCase = loadCase ?? throw new ArgumentNullException(nameof(loadCase));
        }

        protected Load(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _loadCase = (LoadCase)info.GetValue("LoadCase", typeof(LoadCase));
        }

        #endregion

        #region Equals, HashCode and operators

        public abstract GeometryBase GetGeometryBase();

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCase", _loadCase);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return obj is Load load && EqualityComparer<LoadCaseBase>.Default.Equals(_loadCase, load._loadCase) && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = -23 * -17 + base.GetHashCode();
                hashCode = hashCode * -17 + EqualityComparer<LoadCaseBase>.Default.GetHashCode(_loadCase);

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