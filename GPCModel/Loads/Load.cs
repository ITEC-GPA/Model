using GPC.Geometry;
using GPC.Model.LoadCases;
using System;
using System.Runtime.Serialization;

namespace GPC.Model.Loads
{
    [Serializable]
    public abstract class Load : ModelObject
    {
        private LoadCase _loadCase;

        public LoadCase LoadCase => _loadCase;

        protected Load(LoadCase loadCase, Guid guid, string name)
            : base(guid, name)
        {
            _loadCase = loadCase ?? throw new ArgumentNullException(nameof(loadCase));
        }

        protected Load(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            _loadCase = (LoadCase)info.GetValue("LoadCase", typeof(LoadCase));
        }

        public abstract GeometryBase GetGeometry();


        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
            info.AddValue("LoadCase", _loadCase);
        }
    }
}