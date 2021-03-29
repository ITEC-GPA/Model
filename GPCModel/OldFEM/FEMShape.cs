using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;
using GPC.Geometry;

namespace GPC.Model.FEMOld
{
    public class FEMShape : FEMObject
    {
        #region Variables 
        /// <summary>
        /// <param name="_numIntgrPts"> Number of supporting nodes for the interpolation </param>
        /// <param name="_numNodes"> Number of Nodes in the FE element </param>
        /// <param name="_maxNumDof"> Maximum number of DoF in the single node </param>
        /// <param name="_dim"> Problem dimension (one dimension domain, two-dimension domain, three-dimensions domain </param>
        /// <param name="_order"> Order of shape functions </param>
        /// </summary>
        protected Vector<double> _NShape;
        protected Matrix<double> _dNShape;
        protected List<Point2d> _localNodes;
        protected int _numIntgrPts;
        protected int _numNodes;
        protected int _maxNumDof;   
        protected int _dim;       
        protected int _order;     
        #endregion

        #region Properties
        public Vector<double> NShape => _NShape;
        public Matrix<double> dNShape => _dNShape;
        public int NumIntgrPts => _numIntgrPts;
        public int NumNodes => _numNodes;
        public int MaxNumDof => _maxNumDof;
        public int Dim => _dim;
        public int Order => _order;            
        public List<Point2d> LocalNodes => _localNodes;
        #endregion

        #region Public Constructors
        public FEMShape(int numIntgrPts, int numNodes, int maxNumDof, int dim, int order = 1)
        {
            _localNodes = new List<Point2d>();
            _numIntgrPts = numIntgrPts;
            _numNodes = numNodes;
            _maxNumDof = maxNumDof;
            _dim = dim;
            _order = order;
        }

        protected FEMShape(SerializationInfo info, StreamingContext context)
        {
        }
        #endregion

        #region Public Methods Specific
        #endregion

        #region Protected Methods virtual
        public virtual void SetValue(Point3d coord)
        {
        }
        #endregion
    }
}
