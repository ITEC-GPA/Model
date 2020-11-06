using GPC.Geometry;
using GPC.Model.FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM
{
    public class FEMIntegrationPoint : FEMObject
    {
        #region Variables 
        /// <summary>
        /// </summary>
        /// <param name="_numPoint"> Number of integration points </param>
        /// <param name="_weights"> Array of weighs </param>
        /// <param name="_coords"> Array of integration points coordinates </param>
        protected int _numPoint;         
        protected double[] _weights;        
        protected Point2d[] _coords;         
        #endregion

        #region Properties
        public int NumPoint => _numPoint;         
        public double[] Weights => _weights;       
        public Point2d[] Coords => _coords;    
        #endregion

        #region Public Constructors
        public FEMIntegrationPoint()
        {
        }

        protected FEMIntegrationPoint(SerializationInfo info, StreamingContext context)
        {
        }
        #endregion

        #region Public Methods Specific
        #endregion

        #region Protected Methods virtual
        public virtual void SetValue()
        {
        }
        #endregion
    }
}
