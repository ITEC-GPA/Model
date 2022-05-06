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


        void SetElementAt(TRow row, TColumn column, double value);
        void SumElementAt(TRow row, TColumn column, double value);

        void AddMatrix(TRow[] row, TColumn[] column, MathNet.Numerics.LinearAlgebra.Matrix<double> matrix);

        int GetIndex(TRow row);
        int GetIndex(TColumn column);


        bool ContainsKey(TRow row);
        bool ContainsKey(TColumn column);

        TColumn[] GetColumnKeys();
        TRow[] GetRowKeys();
    }
}
