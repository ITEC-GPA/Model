using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace GPC.Model
{
    public abstract class Element : Object
    {
        #region FIELD_CONSTRUCTORS
        #endregion

        #region FIELD_DECONSTRUCTORS
        #endregion

        #region FIELD_COMMANDS
        #endregion

        #region FIELD_METHODS

        #endregion

        #region FIELD_VARIABLES
        /// <summary>
        /// </summary>
        /// <param name="_guid"> Guid of the object </param>

        protected Guid _guid;
        #endregion

        #region FIELD_PROPERTIES
        public Guid Guid { get => _guid; private set => _guid = value; }
        #endregion
    }
}
