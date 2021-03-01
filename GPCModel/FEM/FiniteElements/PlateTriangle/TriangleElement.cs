using GPC.Geometry;
using GPC.Model.FEM.Properties;

namespace GPC.Model.FEM.FiniteElements
{
    public abstract class TriangleElement : Plate
    {
        //calculated and used in BuildMatrix and used also in BuildF
        protected double _areaElement;

        public TriangleElement(Node[] nodes, PlateProperty property, int id) : base(nodes, property, id)
        {

        }

        /// <summary>
        /// Set local coordinate system and out nodes in local coordinate system
        /// </summary>
        /// <param name="localNodes"></param>
        protected abstract Node[] LocalNodes();
    }
}
