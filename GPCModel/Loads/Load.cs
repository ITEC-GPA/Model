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

        protected Load(LoadCase loadCase, Guid guid)
            : base(guid)
        {
            _loadCase = loadCase;
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
    }
}