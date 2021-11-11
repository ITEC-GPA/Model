using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GPC.Geometry;

namespace GPC.Model.Sections
{

    public interface ISection
    {
        Shape2d GetShape();
    }
}
