using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Materials
{
    public class GeneralElasticMaterial : Material
    {
        public GeneralElasticMaterial(string name, double E, double ni) : base(name, E, ni, Guid.NewGuid())
        {
        }
    }
}
