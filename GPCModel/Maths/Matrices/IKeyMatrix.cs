using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GPC.Model.Maths.Matrices
{
    public interface IKeyMatrix<TRow, TColumn>
    {
        double this[int row, int column] { get; set; }

        int GetIndex(TRow row);
        int GetIndex(TColumn column);

        bool ContainsKey(TRow row);
        bool ContainsKey(TColumn column);
    }
}
