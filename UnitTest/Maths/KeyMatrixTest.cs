using System;
using System.Linq;
using System.Collections.Generic;
using GPC.TestUtilities;
using GPC.Model.Maths.Matrices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MathNet.Numerics.LinearAlgebra.Double;


namespace MathTest
{
    [TestClass]
    public class KeyMatrixTest : UnitTestBase
    {

        public class Key
        {
            public int Value;
            public int Value2;

            public Key(int value, int value2)
            {
                Value = value;
                Value2 = value2;
            }

            public override bool Equals(object obj)
            {
                return obj is Key key &&
                       Value == key.Value && Value2 == key.Value2;
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    return -1;
                }
            }
        }


        [TestMethod]
        public void KeyMatrixTest1()
        {
            Key key1 = new Key(1, 1);
            Key key2 = new Key(1, 2);
            Key key3 = new Key(1, 3);


            KeyDenseMatrix<Key, Key> keyDenseMatrix = new KeyDenseMatrix<Key, Key>(new[] { key1, key2 }, new[] { key3 });
            keyDenseMatrix.SetElementAt(key1, key3, 1);
            double a = keyDenseMatrix.GetElementAt(key1, key3);

            Assert.AreEqual(1, a);

        }



    }
}
