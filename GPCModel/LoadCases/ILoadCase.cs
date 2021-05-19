using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.LoadCases
{
    /// <summary>
    /// The purpose of this interface is to group together the loadcases and the loadcombinations.
    /// </summary>
    public interface ILoadCase
    {

        string Name { get; }

    }
}
