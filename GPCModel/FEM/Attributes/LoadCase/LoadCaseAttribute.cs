using GPC.Model.LoadCases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Attributes
{
    public abstract class LoadCaseAttribute : Attribute
    {
        private LoadCase _loadCase;

        public LoadCase LoadCase => _loadCase;

        public LoadCaseAttribute(LoadCase loadCase) : this(loadCase, string.Empty, Guid.NewGuid())
        {
            
        }

        public LoadCaseAttribute(LoadCase loadCase, string name, Guid guid) : base(guid, name)
        {
            _loadCase = loadCase ?? throw new ArgumentNullException("Loadcase cannot be null");
        }

        public LoadCaseAttribute(LoadCase loadCase, string name) : this(loadCase, name, Guid.NewGuid())
        {

        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
