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
        // Classe load e derivate deve rimanere immutabile 

        private readonly LoadCaseBase _loadCase;

        public LoadCaseBase LoadCase => _loadCase;

        protected Load(LoadCaseBase loadCase)
            : this(loadCase, Guid.NewGuid())
        {
        }

        protected Load(LoadCaseBase loadCase, Guid guid)
            : base(guid)
        {
            _loadCase = loadCase ?? throw new ArgumentNullException(nameof(loadCase));
        }

        protected Load(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _loadCase = (LoadCase)info.GetValue("LoadCase", typeof(LoadCase));
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCase", _loadCase);
        }

        public abstract GeometryBase GetGeometryBase();

        #region Equals, HashCode and operators

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(obj, this))
                return true;

            return obj is Load load && EqualityComparer<LoadCaseBase>.Default.Equals(_loadCase, load._loadCase) && base.Equals(obj);
        }

        public override int GetHashCode()
        {
            int hashCode = -23 * -17 + base.GetHashCode();
            hashCode = hashCode * -17 + EqualityComparer<LoadCaseBase>.Default.GetHashCode(_loadCase);

            return hashCode;
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