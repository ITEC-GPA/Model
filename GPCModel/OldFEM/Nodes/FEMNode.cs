using GPC.Model.FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;

namespace GPC.Model.FEMOld
{
    public class FEMNode : FEMObject
    {
        #region Variables
        protected Point3d _point;
        protected int m_DoF;
        protected int m_NumDoF;
        protected int m_TypeDoF;
        protected int m_MaxNumDoF;

        #endregion

        #region Properties
        #endregion

        #region Public Constructors
        public FEMNode() :
            base()
        {

        }
        #endregion

        #region Public Methods Specific
        #endregion
    }
}
