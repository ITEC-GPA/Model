namespace UnitTest;

[TestClass]
public class ApiCompatibilityTest
{
    [DataTestMethod]
    [DataRow("T|C|Public|base=Object|interfaces=IA|", "T|C|Public|base=Object|interfaces=IA,IB|", true)]
    [DataRow("T|C|Public|base=Object|interfaces=IA,IB|", "T|C|Public|base=Object|interfaces=IA|", false)]
    [DataRow("T|C|Public|base=Object|interfaces=IA|", "T|C|Public|base=Other|interfaces=IA,IB|", false)]
    [DataRow("T|C|Public|base=Object|interfaces=IA|", "T|C|Public, Abstract|base=Object|interfaces=IA,IB|", false)]
    [DataRow("T|I|Public, Interface, Abstract|base=-|interfaces=IA|", "T|I|Public, Interface, Abstract|base=-|interfaces=IA,IB|", false)]
    [DataRow("M|C|Method()|void|Public|", "T|C|Public|base=Object|interfaces=IA|", false)]
    [DataRow("T|C|Public|base=Object|interfaces=I<A,B>|", "T|C|Public|base=Object|interfaces=I<A,B>,J<X[,]>|", true)]
    [DataRow("T|C|Public|base=Object|interfaces=I<A,B>|", "T|C|Public|base=Object|interfaces=I<A,C>,J<X[,]>|", false)]
    [DataRow("T|C|Public|base=Object|interfaces=IA|0:None:", "T|C|Public|base=Object|interfaces=IA,IB|0:ReferenceTypeConstraint:", false)]
    public void InterfaceAdditionsDoNotMaskRemovedContracts(string previous, string current, bool compatible) =>
        Assert.AreEqual(compatible, GPC.Tools.ApiCompatibility.IsAdditiveInterfaceImplementation(previous, current));
}
