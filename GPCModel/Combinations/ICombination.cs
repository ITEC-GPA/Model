using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Combinations
{
    public interface ICombination 
    {
        string Name { get; }
        bool isUltimate { get; }
    }
}
