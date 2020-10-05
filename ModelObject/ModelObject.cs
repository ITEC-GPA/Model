using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model
{
    [Serializable]
    public abstract class ModelObject
    {

        #region Variables

        protected Guid _guid;

        #endregion

        #region Properties

        public Guid Guid => _guid;

        #endregion

        #region Public Constructors

        public ModelObject(Guid guid)
        {
            _guid = guid;
        }

        public ModelObject(SerializationInfo info, StreamingContext context)
        {
            _guid = (Guid)info.GetValue("Guid", typeof(Guid));
        }

        #endregion

        #region Public Methods Override

        #endregion

        #region Public Methods Specific
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("Guid", _guid);
        }
        #endregion

        #region Private Methods Specific
        #endregion
    }
}

/*
#region Variables
#endregion

#region Properties
#endregion

#region Public Constructors
#endregion

#region Public Methods Override
#endregion

#region Public Methods Specific
#endregion

#region Private Methods Specific
#endregion
*/