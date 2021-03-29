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
using GPC.Model.FEM.Properties;
using GPC.Model.Results;

namespace GPC.Model.FEMOld
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
        public PlateDKT(Guid guid, PlateProperty property, int plateIndex, Node[] nodes)
            : base(guid, property, plateIndex, nodes)
        {
            throw new NotImplementedException();
            //_integrator = new FEMPlateIntegratorDKQ(new Guid(), 2, 1, 2, this);
            //BuildElementDoF();
            //_integrator.StartIntegration(this);
        }

        protected PlateDKT(SerializationInfo info, StreamingContext context)
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
