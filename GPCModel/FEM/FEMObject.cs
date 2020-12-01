using System;

namespace GPC.Model.FEM
{
    public class FEMObject : ModelObject
    {
        public FEMObject()
            : base(Guid.Empty, "")
        {
        }

        public FEMObject(string name)
             : base(Guid.Empty, name)
        {
        }

        public FEMObject(Guid guid, string name)
              : base(guid, name)
        {
        }

    }
}