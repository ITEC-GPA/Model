using GPC.TestUtilities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Reflection;

namespace GeneralTest
{
    [TestClass]
    public class SerializationTest : UnitTestBase
    {
        /// <summary>
        /// Testa che tutte le classi nell'assembly siano abbiano l'attributo [Serializable]
        /// </summary>
        [TestMethod]
        public void SerializableAttributeTest()
        {
            var assemblyName = "GPCModel";
            var nameSpace = "GPC.Model";

            var assembly = Assembly.Load(assemblyName);
            var classes = assembly.GetTypes().Where(a => a.IsClass && a.Namespace != null && a.Namespace.Contains(nameSpace)).ToList();

            foreach (var cl in classes)
            {
                if (!cl.IsSerializable)
                    Assert.Fail($"Class {cl.Name} is not serializable");
            }
        }
    }
}