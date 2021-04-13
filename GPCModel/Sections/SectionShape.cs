using GPC.Geometry;
using GPC.Model.Materials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Sections
{
    public class SectionShape : Section
    {
        private class MultiMaterial : Material
        {
            protected List<Material> _materials;

            public List<Material> Materials => _materials;

            public MultiMaterial(Material[] materials)
                : base(string.Empty, 1, 0.1, Guid.NewGuid())
            {
                _materials = new List<Material>(materials);
            }
        }

        protected List<Shape> _shapes;

        public List<Shape> Shapes => _shapes;


        public SectionShape(Shape[] shapes, Material[] materials, string name)
            : base(materials, name)
        {
            _shapes = new List<Shape>(shapes);
            _material = new MultiMaterial(materials);
        }

        public override ShapeMaterial[] GetShapes()
        {
            MultiMaterial mm = (MultiMaterial)_material;
            List<ShapeMaterial> sm = new List<ShapeMaterial>();
            for (int i = 0; i < _shapes.Count; i++)
            {
                sm.Add(new ShapeMaterial() { Material = mm.Materials[i], Shape = _shapes[i] });
            }
            return sm.ToArray();
        }
    }
}