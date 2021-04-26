using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using GPC.Model.FEM;
using mnl = MathNet.Numerics.LinearAlgebra;
using GPC.Model.FEM.FiniteElements;
using GPC.Model.FEM.Properties;
using GPC.Model.Materials;
using System.Linq;

namespace FemTest.SolverTest
{
    [TestClass]
    public class BrickTest
    {
        [TestMethod]
        public void Dest1()
        {
            mnl.Matrix<double> D = Brick.GetD(96, 1.0 / 3.0);

            mnl.Matrix<double> manual = mnl.Matrix<double>.Build.Dense(1, 6);
            mnl.Vector<double>[] rows = new mnl.Vector<double>[6];
            rows[0] = mnl.Vector<double>.Build.Dense(new double[] { 144, 72, 72, 0, 0, 0 });
            rows[1] = mnl.Vector<double>.Build.Dense(new double[] { 72,  144,   72,   0,   0,   0 });
            rows[2] = mnl.Vector<double>.Build.Dense(new double[] { 72,   72,  144,   0,   0,   0 });
            rows[3] = mnl.Vector<double>.Build.Dense(new double[] { 0,    0,    0,  36,   0,   0 });
            rows[4] = mnl.Vector<double>.Build.Dense(new double[] { 0,    0,    0,   0,  36,   0 });
            rows[5] = mnl.Vector<double>.Build.Dense(new double[] { 0,    0,    0,   0,   0,  36 });

            int index = 0;
            rows.ToList().ForEach(r => { manual = manual.InsertRow(index, r); index++; });

            for (int r = 0; r < 6; r++)
            {
                for (int c = 0; c < 6; c++)
                {
                    Assert.AreEqual(D[r,c], manual[r,c], 0.0001);
                }
            }
        }
    }
}
