using GPC.Model.FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEMOld
{
    public class FEMDoF : FEMObject
    {
        #region Variables
        /// <summary>
        /// 
        /// </summary>
        /// <param name="_active"></param>
        /// 0 - DOF active
        /// 1 - DOF nonactive
        /// <param name="_id">
        /// 1 - DOF X diplacements in X direction
        /// 2 - DOF Y diplacements in Y direction
        /// 3 - DOF Z diplacements in Z direction
        /// 4 - DOF RX rotation around X direction
        /// 5 - DOF RY rotation around Y direction
        /// 6 - DOF RZ rotation around Z direction
        protected int _id;

        protected int _active;

        #endregion

        #region Properties

        public int Id => _id;
        public int Active => _active;

        #endregion

        #region Public Constructors
        public FEMDoF(int id, int active)
        {
            _id = id;
            _active = active;
        }
        #endregion

        #region Public Methods Specific
        #endregion
    }
}
