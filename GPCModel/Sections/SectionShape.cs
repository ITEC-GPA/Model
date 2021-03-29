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

            public MultiMaterial(Material[] materials, Guid guid)
                : base(guid)
            {
                _materials = new List<Material>(materials);
            }
        }

        #region Variables
        protected List<Shape> _shapes;
        #endregion

        #region Properties
        public List<Shape> Shapes => _shapes;
        #endregion

        #region Public Constructors
        #endregion

        #region Public Methods Specific
        #endregion

        #region Private Methods Specific
        #endregion

        #region Public Methods Override
        #endregion


        public SectionShape(Shape[] shapes, Material[] materials, string name)
            : base(materials, name)
        {
            _shapes = new List<Shape>(shapes);
            _material = new MultiMaterial(materials, new Guid());
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