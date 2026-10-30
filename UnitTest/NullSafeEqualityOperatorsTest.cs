using System.Reflection;
using System.Runtime.CompilerServices;
using GPC.Converter;
using GPC.Examples;
using GPC.Geometry;
using GPC.Model;
using GPC.Model.Data.Steel;
using GPC.Model.ElementProperties;
using GPC.Model.Materials;
using GPC.Model.Results;
using GPC.Model.Sections;
using GPC.Model.Sections.Bolt;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Rebar;
using GPC.Model.Sections.Steel;

namespace UnitTest;

/// <summary>
/// The equality operators with null operands. Before, the operators of these classes called Equals on the first operand:
/// NullReferenceException for null == x (and for null == null without the reference check). The non-null result stays the one of Equals.
/// </summary>
[TestClass]
public class NullSafeEqualityOperatorsTest
{
    private static ConcreteMaterialEN1992 C30(double fck = 30) =>
        new ConcreteMaterialEN1992("C30/37", fck, ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle);
    private static ConcreteMaterialModelCode2010 Mc2010(ConcreteMaterial.CompressionStressStrainDiagrams diagram) =>
        new ConcreteMaterialModelCode2010("C25/30", 25, diagram);
    private static ResultPlateStress PlateStress(double upperSxx) => new ResultPlateStress(CoordinateSystem.Global,
        new ResultStress(CoordinateSystem.Global, 1, 2, 0, 3, 4, 5), new ResultStress(CoordinateSystem.Global, 0, 0, 0, 0, 0, 0),
        new ResultStress(CoordinateSystem.Global, upperSxx, -2, 0, -3, -4, -5));
    private static ShapeEx Shape(double diameter) => new ShapeEx(new Polygon2d(diameter), new Material("test", 10, 0.2, 20, 5));

    /// <summary>
    /// The type that declares the operator; a value; an equal copy and a different value come from the second and third factory
    /// (an abstract type is tested with a concrete derived class, through its own operator)
    /// </summary>
    public static IEnumerable<object[]> Operators => new[]
    {
        Case<ConcreteMaterial>(() => C30(), () => C30(), () => C30(35)),
        Case<ConcreteMaterialACI318>(() => new ConcreteMaterialACI318("C40", 40, ConcreteMaterial.CompressionStressStrainDiagrams.StressBlock),
            () => new ConcreteMaterialACI318("C40", 40, ConcreteMaterial.CompressionStressStrainDiagrams.StressBlock),
            () => new ConcreteMaterialACI318("C40", 45, ConcreteMaterial.CompressionStressStrainDiagrams.StressBlock)),
        Case<ConcreteMaterialEN1992>(() => C30(), () => C30(), () => C30(35)),
        Case<ConcreteMaterialEuropeanCommon>(() => Mc2010(ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle),
            () => Mc2010(ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle), () => C30()),
        Case<ConcreteMaterialModelCode2010>(() => Mc2010(ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle),
            () => Mc2010(ConcreteMaterial.CompressionStressStrainDiagrams.ParabolaRectangle),
            () => Mc2010(ConcreteMaterial.CompressionStressStrainDiagrams.StressBlock)),
        Case<ResultPlateStress>(() => PlateStress(-1), () => PlateStress(-1), () => PlateStress(-1.5)),
        Case<BoltSection>(() => new BoltSection(20, SteelMaterialEN1993Data.S355, "M20"), () => new BoltSection(20, SteelMaterialEN1993Data.S355, "M20"),
            () => new BoltSection(24, SteelMaterialEN1993Data.S355, "M20")),
        Case<ConcretePlateProperty>(() => new ConcretePlateProperty(C30(), 300, 300, "slab"), () => new ConcretePlateProperty(C30(), 300, 300, "slab"),
            () => new ConcretePlateProperty(C30(), 250, 300, "slab")),
        Case<ReinforcedConcreteRebar>(() => new ReinforcedConcreteRebar(new RebarSectionCircular(20, SteelMaterialEN1992Data.B450C), new Point2d(50, 50)),
            () => new ReinforcedConcreteRebar(new RebarSectionCircular(20, SteelMaterialEN1992Data.B450C), new Point2d(50, 50)),
            () => new ReinforcedConcreteRebar(new RebarSectionCircular(20, SteelMaterialEN1992Data.B450C), new Point2d(60, 50))),
        Case<SectionC>(() => new SectionC(200, 8.5, 75, 11.5, 75, 11.5, "UPN200", 12, 0), () => new SectionC(200, 8.5, 75, 11.5, 75, 11.5, "UPN200", 12, 0),
            () => new SectionC(220, 8.5, 75, 11.5, 75, 11.5, "UPN200", 12, 0)),
        Case<SectionCircular>(() => new SectionCircular(100, "D"), () => new SectionCircular(100, "D"), () => new SectionCircular(120, "D")),
        Case<SectionRectangular>(() => new SectionRectangular(300, 400, "R"), () => new SectionRectangular(300, 400, "R"),
            () => new SectionRectangular(300, 500, "R")),
        Case<ShapeEx>(() => Shape(500), () => Shape(500), () => Shape(400)),
        Case<ThinWallSection>(() => new SectionRectangular(200, 100, "R"), () => new SectionRectangular(200, 100, "R"),
            () => new SectionRectangular(200, 120, "R")),
        Case<SteelPlateProperty>(() => new SteelPlateProperty(SteelMaterialEN1993Data.S355, 20, 20, "steel"),
            () => new SteelPlateProperty(SteelMaterialEN1993Data.S355, 20, 20, "steel"), () => new SteelPlateProperty(SteelMaterialEN1993Data.S355, 25, 20, "steel")),
        Case<ElementProperty>(() => new ConcretePlateProperty(C30(), 300, 300, "slab"), () => new ConcretePlateProperty(C30(), 300, 300, "slab"),
            () => new ConcretePlateProperty(C30(), 300, 250, "slab")),
        Case<PlateProperty>(() => new SteelPlateProperty(SteelMaterialEN1993Data.S355, 20, 20, "steel"),
            () => new SteelPlateProperty(SteelMaterialEN1993Data.S355, 20, 20, "steel"), () => new SteelPlateProperty(SteelMaterialEN1993Data.S275, 20, 20, "steel")),
        // already null-safe (see SectionCalculationTest.EqualityOperatorWithNull)
        Case<Section>(() => new SectionCircular(100, "D"), () => new SectionCircular(100, "D"), () => new SectionRectangular(100, 100, "D")),
    };

    private static object[] Case<T>(Func<T> make, Func<T> copy, Func<T> different) where T : class =>
        new object[] { typeof(T), (Func<object>)make, (Func<object>)copy, (Func<object>)different };

    public static string DisplayName(MethodInfo method, object[] data) => $"{method.Name} ({((Type)data[0]).Name})";

    private static MethodInfo? Operator(Type type, string name) =>
        type.GetMethod(name, BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly, null, new[] { type, type }, null);

    /// <summary>The == operator declared by <paramref name="type"/>, checked against its != operator</summary>
    private static bool Equal(Type type, object? left, object? right)
    {
        bool equal = (bool)Operator(type, "op_Equality")!.Invoke(null, new[] { left, right })!;
        Assert.AreEqual(!equal, (bool)Operator(type, "op_Inequality")!.Invoke(null, new[] { left, right })!, $"{type.Name}: != is not the negation of ==");
        return equal;
    }

    [DataTestMethod]
    [DynamicData(nameof(Operators), DynamicDataDisplayName = nameof(DisplayName))]
    public void NullOperandsEqualCopiesAndDifferentValues(Type type, Func<object> make, Func<object> copy, Func<object> different)
    {
        object x = make(), same = copy(), other = different();
        Assert.IsInstanceOfType(x, type);
        Assert.IsTrue(Equal(type, null, null), "null == null");
        Assert.IsFalse(Equal(type, null, x), "null == x");
        Assert.IsFalse(Equal(type, x, null), "x == null");
        Assert.IsTrue(Equal(type, x, x), "x == x");
        // non-null operands: the result of Equals, as before
        Assert.IsTrue(x.Equals(same), "the copy is not equal (Equals)");
        Assert.IsTrue(Equal(type, x, same), "x == copy");
        Assert.IsTrue(Equal(type, same, x), "copy == x");
        Assert.IsFalse(x.Equals(other), "the different value is equal (Equals)");
        Assert.IsFalse(Equal(type, x, other), "x == different");
        Assert.IsFalse(Equal(type, other, x), "different == x");
    }

    [TestMethod]
    public void EveryEqualityOperatorOfTheModelAcceptsNullOperands()
    {
        // every class of GPCModel with its own == (a generic one closed on its constraint): null == null, null == x and x == null
        // without exceptions; x is an instance without constructor (the operators must not reach Equals with a null operand), of a concrete
        // derived class for an abstract type
        var classes = typeof(ModelObject).Assembly.GetTypes().Where(t => t.IsClass).ToList();
        var failures = new List<string>();
        int operators = 0;
        foreach (Type declared in classes)
        {
            Type type = declared.IsGenericTypeDefinition
                ? declared.MakeGenericType(declared.GetGenericArguments().Select(a => a.GetGenericParameterConstraints().FirstOrDefault(c => c.IsClass) ?? typeof(object)).ToArray())
                : declared;
            if (Operator(type, "op_Equality") is null)
                continue;
            operators++;
            Type? concrete = type.IsAbstract
                ? classes.FirstOrDefault(c => !c.IsAbstract && !c.ContainsGenericParameters && type.IsAssignableFrom(c))
                : type;
            object? x = concrete is null ? null : RuntimeHelpers.GetUninitializedObject(concrete);
            foreach (var (left, right, expected, text) in new (object?, object?, bool, string)[]
                { (null, null, true, "null == null"), (null, x, false, "null == x"), (x, null, false, "x == null") })
            {
                if (text != "null == null" && x is null)
                    continue;
                try
                {
                    if (Equal(type, left, right) != expected)
                        failures.Add($"{type.FullName}: {text} is {!expected}");
                }
                catch (TargetInvocationException e)
                {
                    failures.Add($"{type.FullName}: {text} throws {e.InnerException?.GetType().Name}");
                }
            }
        }
        Assert.IsTrue(operators > 80, $"only {operators} equality operators found");
        Assert.AreEqual(0, failures.Count, string.Join(Environment.NewLine, failures));
    }

    [TestMethod]
    public void CallersThatCompareWithNullReachTheirOwnBranch()
    {
        // SectionBuiltUp.DoubleChannel: before, NullReferenceException from channel == null instead of its ArgumentNullException
        Assert.ThrowsException<ArgumentNullException>(() => SectionBuiltUp.DoubleChannel(null!, 10, true, "2C"));
        // ResultFilter.Resolve with a property filter: before, NullReferenceException from b.BeamProperty != null for a beam without
        // property; now the beam is not selected and the shells of the property are
        var plan = new ResultFilter { Properties = new[] { "Shell 300" } }
            .Resolve(MixedModelFactory.Create(m => m.BeamElements.Values.Single().BeamProperty = null));
        Assert.AreEqual(0, plan.Beams.Count());
        CollectionAssert.AreEqual(new[] { 1090, 1130 }, plan.Shells.Select(x => x.Id).ToArray());
    }
}
