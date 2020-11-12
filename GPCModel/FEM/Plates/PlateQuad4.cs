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
using GPC.Model.CoordinateSystems;

namespace GPC.Model.FEM
{
    public class PlateQuad4 : Plate
    {
        #region Variables
        #endregion 

        #region Properties
        #endregion

        #region Public Constructors
        public PlateQuad4(Guid guid, PlateProperty property, Node[] nodes)
            : base(guid, property, nodes)
        {
            _integrator = new FEMPlateIntegratorDKQ(new Guid(), 2, 1, 2, this);
        }

        protected PlateQuad4(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
        #endregion

        #region Public Methods Override     
        #endregion
    }
}
