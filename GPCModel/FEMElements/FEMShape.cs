using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Spatial.Euclidean;
using GPC.Geometry;

namespace GPC.Model.FEM
{
    public class FEMShape : FEMObject
    {
        #region Variables 
        protected Vector<double> _NMatrix;
        protected Matrix<double> _dNMatrix;
        protected List<Point2d> _localNodes;
        protected int _numNodes;       // Numero di punti di appoggio
        protected int _maxNumDof;      // Numero massimo di gdl nel nodo
        protected int _numDim;         // Dimensioni della funzione di forma
        protected int _order;          // Ordine di continuità
        #endregion

        #region Properties
        public Vector<double> NMat => _NMatrix;
        public Matrix<double> DNMat => _dNMatrix;
        public int NumNodes => _numNodes;
        public int MaxNumDof => _maxNumDof;
        public int NumDim => _numDim;
        public int Order => _order;            
        public List<Point2d> LocalNodes => _localNodes;
        #endregion

        #region Public Constructors
        public FEMShape(int numNodes, int maxNumDof, int numDim, int order = 0)
        {
            _localNodes = new List<Point2d>();
            _numNodes = numNodes;
            _maxNumDof = maxNumDof;
            _numDim = numDim;
            _order = order;
        }

        protected FEMShape(SerializationInfo info, StreamingContext context)
        {
        }
        #endregion

        #region Public Methods Specific
        #endregion

        #region Protected Methods virtual
        public virtual void SetValue(Point2d coord)
        {

        }
        #endregion
    }
}
