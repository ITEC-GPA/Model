using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;
using GPC.Model.Elements;
using GPC.Model.FEM.Materials;
using GPC.Model.Materials;

namespace GPC.Model.Sections
{
    [Serializable]
    public class ConcreteSectionShape : SectionShape
    {
        #region Variables
        protected List<Rebar> _rebars;
        protected ConcreteMaterial _concreteMat;
        #endregion

    //    #region Properties
    //    public List<Rebar> Rebars { get => _rebars; set => _rebars = value; }
    //    public ConcreteMaterial ConcreteMat { get => _concreteMat; set => _concreteMat = value; }
    //    #endregion

    //    #region Public 
    //    public ConcreteSectionShape(Shape[] shapes, IsotropicFemMaterial[] materials, Rebar[] rebars, string name) : base (shapes, materials, name)
    //    {
    //        _rebars = new List<Rebar>(rebars);
    //    }

    //    #endregion

    //    #region Public Methods Specific
    //    #endregion

    //    #region Private Methods Specific
    //    #endregion

    //    #region Public Methods Override
    //    #endregion
    //}
}
