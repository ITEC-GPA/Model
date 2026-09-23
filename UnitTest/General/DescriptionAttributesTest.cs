using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Reflection;
using GPC.Utilities.Attributes;

namespace GeneralTest
{
    [TestClass]
    public class DescriptionAttributesTest 
    {
        [TestMethod]
        public void TestMethod1()
        {
            Type type = typeof(GPC.Model.Materials.SteelMaterial);

            if (type != null)
            {
                UIAttribute? attr = type.GetCustomAttribute(typeof(UIAttribute)) as UIAttribute;
                Assert.IsNotNull(attr);
            }
        }
    }
}
