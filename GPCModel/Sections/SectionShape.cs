using GPC.Geometry;
using GPC.Model.FEM.Materials;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    //public class SectionShape : Section
    //{
    //    private class MultiMaterial : IsotropicFemMaterial
    //    {
    //        protected List<IsotropicFemMaterial> _materials;

    //        public List<IsotropicFemMaterial> Materials => _materials;

    //        public MultiMaterial(IsotropicFemMaterial[] materials) : base(1, 0.1, 0, 0)
    //        {
    //            _materials = new List<IsotropicFemMaterial>(materials);
    //        }
    //    }

    //    protected List<Shape> _shapes;

    //    public List<Shape> Shapes => _shapes;


    //    public SectionShape(Shape[] shapes, IsotropicFemMaterial[] materials, string name)
    //        : base(materials, name)
    //    {
    //        _shapes = new List<Shape>(shapes);
    //        _material = new MultiMaterial(materials);
    //    }

    //    public override ShapeMaterial[] GetShapes()
    //    {
    //        MultiMaterial mm = (MultiMaterial)_material;
    //        List<ShapeMaterial> sm = new List<ShapeMaterial>();
    //        for (int i = 0; i < _shapes.Count; i++)
    //        {
    //            sm.Add(new ShapeMaterial() { Material = mm.Materials[i], Shape = _shapes[i] });
    //        }
    //        return sm.ToArray();
    //    }
    //}
}