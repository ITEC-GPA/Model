using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.FEM.Collections
{
    public interface IFEMObjectStageCollection<T, D> where T : FEMObject where D : Stage.StageProperty
    {
        void Add(T item, D stageFiniteElementProperty);
        D GetStageProperty(int elementID);
    }
}
