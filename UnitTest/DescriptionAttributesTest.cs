using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using GPC.Utilities.Attributes;
using GPC.TestUtilities;

namespace GeneralTest
{
    [TestClass]
    public class DescriptionAttributesTest : UnitTestBase
    {
        [TestMethod]
        public void TestMethod1()
        {
            Type type = typeof(GPC.Model.Materials.SteelMaterial);

            if (type != null)
            {
                UIAttribute attr = (UIAttribute)type.GetCustomAttribute(typeof(UIAttribute));
                Assert.IsNotNull(attr);
            }
        }
    }
}
