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
    public class FEMGaussIntegration : FEMObject
    {
        #region Variables 
        /// <summary>
        /// </summary>
        /// <param name="_numPoint"> Number of integration points </param>
        /// <param name="_weights"> Array of weighs </param>
        /// <param name="_coords"> Array of integration points coordinates </param>
        protected int _numPoints;         
        protected double[] _weights;        
        protected Point3d[] _coords;
        protected int _order;
        protected int _dimension;
        #endregion

        #region Properties
        public int NumPoints => _numPoints;         
        public double[] Weights => _weights;       
        public Point3d[] Coords => _coords;
        public int Order => _order;
        public int Dimension => _dimension;
        #endregion

        #region Public Constructors
        public FEMGaussIntegration()
        {
        }

        protected FEMGaussIntegration(SerializationInfo info, StreamingContext context)
        {
        }
        #endregion

        #region Public Methods Specific
        #endregion

        #region Protected Methods virtual
        public virtual void SetValue(int order, int dim)
        {
        }
        #endregion
    }
}
