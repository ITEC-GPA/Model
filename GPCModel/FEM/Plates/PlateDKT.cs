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
using GPC.Model.Elements;

namespace GPC.Model.FEM
{
    /// <summary>
    /// Discrete Kirchhoff Triangle  (Batoz, 1982)
    /// </summary>
    public class PlateDKT : Plate
    {
        #region Variables
        #endregion 

        #region Properties
        #endregion

        #region Public Constructors
        public PlateDKT(Guid guid, PlateProperty property, Node[] nodes)
            : base(guid, property, nodes)
        {
            _integrator = new FEMPlateIntegratorDKQ(new Guid(), 2, 1, 2, this);
            BuildElementDoF();
            _integrator.StartIntegration(this);
        }

        protected PlateDKT(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Public Methods Override     
        #endregion
    }
}
