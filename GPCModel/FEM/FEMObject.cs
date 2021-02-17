using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM
{
    class FEMObject : ModelObject
    {
        int _index;

        public FEMObject(int index) : base()
        {

        }

        public FEMObject(string name) : base(Guid.NewGuid(), name)
        {

        }
    }
}
