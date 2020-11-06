using GPC.Geometry;
using GPC.Model.FEM;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEMElements
{
    public class FEMIntegrationPoint : FEMObject
    {
        #region Variables 
        protected int _numPoint;             // Numero dei punti di integrazione
        protected double[] _weights;         // Array dei pesi
        protected Point2d[] _coords;         // Array delle coordinate dei punti di integrazione
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
