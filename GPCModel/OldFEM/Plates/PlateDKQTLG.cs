using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using GPC.Model.Materials;
using GPC.Model.Sections;
using MathNet.Numerics.LinearAlgebra;
using GPC.Geometry;
using GPC.Model.FEM.Properties;
using GPC.Model.Elements;
using GPC.Model.Elements.Glasses;

namespace GPC.Model.FEMOld
{
    /// <summary>
    /// Discrete Kirchhoff Quad Triplex Laminated Glass (Ivanov, 2015)
    /// </summary>
    public class PlateDKQTLG : Plate
    {
        #region Variables
        protected double _t1;
        protected double _t2;
        protected double _t0;
        #endregion

        #region Properties
        public double T1 => _t1;
        public double T2 => _t2;
        public double T0 => _t0;

        #endregion

        #region Public Constructors
        public PlateDKQTLG(Guid guid, LaminatedGlassProperty property, int plateIndex, Node[] nodes, double loadDuration, double temperature)
           : base(guid, property, plateIndex, nodes)
        {
            _integrator = new FEMPlateIntegratorDKQTLG(new Guid(), 2, 1, 2, this, loadDuration, temperature);
            BuildElementDoF();
            _integrator.StartIntegration(this); 
        }

        protected PlateDKQTLG(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Public Methods Override 
        public override object Clone()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
