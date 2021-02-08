using System;

namespace GPC.Model.FEM
{
    public class FEMObject : ModelObject
    {
        public FEMObject()
            : base(Guid.NewGuid(), "")
        {
        }

        public FEMObject(string name)
             : base(Guid.NewGuid(), name)
        {
        }

        public FEMObject(Guid guid, string name)
              : base(guid, name)
        {
        }

    }
}