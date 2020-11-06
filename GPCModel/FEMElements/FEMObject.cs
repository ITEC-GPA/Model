using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM
{
    public class FEMObject : ModelObject
    {
        #region Variables
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
        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        #endregion

        #region Public Methods Specific
        #endregion
    }
}
