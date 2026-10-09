using GPC.Geometry;
using GPC.Model.Data.Concrete;
using GPC.Model.Data.Steel;
using GPC.Model.Sections;
using GPC.Model.Sections.Concrete;
using GPC.Model.Sections.Steel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;

namespace UnitTest
{
    [TestClass]
    public class SectionCurveTests
    {
        public static IEnumerable<object[]> NativeCases()
        {
            // 20 section families, five independent scales: 100 discovered cases.
            foreach (double scale in new[] { .1, 1, 2, 10, 100 })
                for (int family = 0; family < 20; family++) yield return new object[] { family, scale };
        }

        private static Section Create(int family, double k)
        {
            Section result;
            switch (family)
            {
                case 0: return new SectionCircular(100 * k);
                case 1: return new SectionCHS(100 * k, 5 * k);
                case 2: return new SectionStadium(200 * k, 100 * k);
                case 3: return new SectionStadium(100 * k, 200 * k, 5 * k);
                case 4: return new SectionStadium(100 * k, 100 * k, 5 * k);
                case 5: return new SectionHollowCore(600 * k, 200 * k, 3, 100 * k, 100 * k, 180 * k);
                case 6: return new SectionHollowCore(600 * k, 200 * k, 3, 100 * k, 140 * k, 180 * k);
                case 7: return new SectionRHSRoundedCorners(200 * k, 100 * k, 5 * k, 10 * k, 5 * k, "RHS");
                case 8: return new SectionRHSRoundedCorners(200 * k, 100 * k, 5 * k, 50 * k, 45 * k, "touching corners");
                case 9: result = new SectionH(300 * k, 8 * k, 150 * k, 12 * k, 150 * k, 12 * k, "H", 10 * k); break;
                case 10: result = new SectionC(200 * k, 8 * k, 75 * k, 12 * k, 75 * k, 12 * k, "C", 10 * k, 3 * k); break;
                case 11: result = new SectionL(100 * k, 10 * k, 150 * k, 10 * k, "L", 10 * k, 3 * k); break;
                case 12: result = new SectionT(200 * k, 100 * k, 8 * k, 12 * k, "T", 10 * k); break;
                case 13: return new SectionHTaperFlange(300 * k, 8 * k, 150 * k, 12 * k, .1, 10 * k, 3 * k, 20 * k, "H taper");
                case 14: return new SectionCTaperFlange(200 * k, 8 * k, 75 * k, 12 * k, .1, 10 * k, 3 * k, 20 * k, "C taper");
                case 15: return new SectionTTaperFlange(200 * k, 100 * k, 8 * k, 12 * k, .1, 10 * k, 3 * k, 20 * k, "T taper");
                case 16: return SectionColdFormed.Channel(200 * k, 75 * k, 2 * k, 3 * k);
                case 17: return SectionColdFormed.Channel(200 * k, 75 * k, 2 * k, 0);
                case 18: return SectionColdFormed.LippedChannel(200 * k, 75 * k, 20 * k, 2 * k, 3 * k);
                default: return SectionColdFormed.Sigma(200 * k, 75 * k, 20 * k, 15 * k, 40 * k, 20 * k, 2 * k, 3 * k);
            }
            result.SetEdgeTypeFromSteelType(Section.SectionTypes.Rolled);
            result.SetMechanicalProperties();
            return result;
        }

        private static double[] Coordinates(Shape shape) => shape.GetPoints().SelectMany(p => new[] { p.X, p.Y, p.Z }).ToArray();
        private static IEnumerable<ArcCurve3d> Arcs(Curve3d curve)
        {
            if (curve is ArcCurve3d arc) yield return arc;
            if (curve is PolyCurve3d poly)
                for (int i = 0; i < poly.SegmentCount; i++)
                    foreach (var child in Arcs(poly.GetSegment(i))) yield return child;
        }

        [DataTestMethod, DynamicData(nameof(NativeCases), DynamicDataSourceType.Method), TestCategory("SectionCurves100")]
        public void NativeContoursPreserveLegacyGeometryAndConverge(int family, double scale)
        {
            Section section = Create(family, scale);
            double[] before = Coordinates(section.Shape);
            double area = section.Area, jxx = section.Jxx, jyy = section.Jyy;
            SectionCurveOutline outline = section.GetCurveOutlines().Single();
            Assert.IsFalse(outline.IsApproximation);
            Curve3d boundary = outline.Boundary;
            Assert.IsTrue(boundary.IsClosed);
            Assert.AreEqual(0, boundary.StartPoint.DistanceTo(boundary.EndPoint), 1e-8 * scale);
            Assert.IsTrue(new[] { boundary }.Concat(outline.Holes).SelectMany(Arcs).Any(), "Native circular segments expected");
            double exactLegacyArea = section.GetPlasticShape().GetArea();
            Shape2d fine = outline.ToShape(.001 * scale);
            Assert.AreEqual(exactLegacyArea, fine.GetArea(), .001 * Math.Abs(exactLegacyArea), "Match independently sampled old contour");
            Assert.AreEqual(section.Shape.Holes?.Length ?? 0, outline.Holes.Count);
            foreach (var arc in new[] { boundary }.Concat(outline.Holes).SelectMany(Arcs))
            {
                double t = arc.Domain.ParameterAt(.37);
                Assert.AreEqual(arc.Radius, arc.PointAt(t).DistanceTo(arc.Center), 1e-9 * scale);
                Assert.AreEqual(0, arc.PointAt(t).Z);
                Assert.AreEqual(t, arc.ParameterAtLength(arc.LengthAt(t)), 1e-12);
            }
            CollectionAssert.AreEqual(before, Coordinates(section.Shape));
            Assert.AreEqual(area, section.Area);
            Assert.AreEqual(jxx, section.Jxx);
            Assert.AreEqual(jyy, section.Jyy);
        }

        [TestMethod]
        public void CircularAndStadiumPerimetersAreAnalytical()
        {
            Assert.AreEqual(100 * Math.PI, new SectionCircular(100).GetCurveOutlines()[0].Boundary.Length, 1e-10);
            var tube = new SectionCHS(100, 5).GetCurveOutlines()[0];
            Assert.AreEqual(90 * Math.PI, tube.Holes[0].Length, 1e-10);
            Assert.AreEqual(200 + 100 * Math.PI, new SectionStadium(200, 100).GetCurveOutlines()[0].Boundary.Length, 1e-10);
            var circle = new SectionCircular(100).GetCurveOutlines()[0];
            Assert.IsTrue(Math.Abs(circle.ToShape(.001).GetArea() - Math.PI * 2500) < Math.Abs(circle.ToShape(1).GetArea() - Math.PI * 2500));
        }

        [TestMethod]
        public void SnapshotsOwnTheirGeometryAndFollowSectionEdits()
        {
            var section = new SectionCHS(100, 5);
            var before = section.GetCurveOutlines()[0];
            before.Boundary.Move(10, 20, 30);
            before.Holes[0].Move(10, 20, 30);
            Assert.AreEqual(0, before.Boundary.StartPoint.Z);
            Assert.AreEqual(0, before.Holes[0].StartPoint.Z);
            section.Diameter = 200;
            Assert.AreEqual(100 * Math.PI, before.Boundary.Length, 1e-10);
            Assert.AreEqual(200 * Math.PI, section.GetCurveOutlines()[0].Boundary.Length, 1e-10);
            var source = new ArcCurve3d(new Point3d(0, 0, 0), new Vector3d(0, 0, 1), new Vector3d(1, 0, 0), 5, 2 * Math.PI);
            var owned = new SectionCurveOutline(source);
            source.Move(100, 0, 0);
            Assert.AreEqual(5, owned.Boundary.StartPoint.X);
        }

        [TestMethod]
        public void GenericPolygonsRetainNestedIslandsAndEmptySections()
        {
            Polygon2d Square(double from, double to) => new Polygon2d(new[] { new Point2d(from, from), new Point2d(to, from), new Point2d(to, to), new Point2d(from, to) });
            var shape = new Shape2d(Square(0, 100), new[] { Square(20, 80) }, new[] { new Shape2d(Square(40, 60)) });
            var section = new Section(shape);
            var outline = section.GetCurveOutlines()[0];
            Assert.AreEqual(1, outline.Children.Count);
            Assert.AreEqual(6800, outline.ToShape().GetArea(), 1e-9);
            Assert.IsFalse(outline.IsApproximation);
            var generic = new Section(100, 10, 5, 2, 0, new Point2d(0, 0), new Point3d(0, 0, 0), 0, "properties only");
            Assert.AreEqual(0, generic.GetCurveOutlines().Count);
            Assert.IsFalse(new SectionEllipse(100, 50).GetCurveOutlines()[0].IsApproximation);
            Assert.IsTrue(SectionCurveOutline.FromShape(shape, true).IsApproximation);
        }

        [TestMethod]
        public void OpenAndNonXYContoursAreRejected()
        {
            Assert.ThrowsException<ArgumentException>(() => new SectionCurveOutline(new LineCurve3d(new Point3d(0, 0, 0), new Point3d(1, 0, 0))));
            Assert.ThrowsException<ArgumentNullException>(() => new SectionCurveOutline(null!));
            var tilted = new ArcCurve3d(new Point3d(0, 0, 0), new Vector3d(0, 1, 0), new Vector3d(1, 0, 0), 10, 2 * Math.PI);
            Assert.ThrowsException<ArgumentException>(() => new SectionCurveOutline(tilted).ToShape());
            var elevated = new SectionCircular(100).GetCurveOutlines()[0].Boundary;
            elevated.Move(0, 0, 10);
            Assert.ThrowsException<ArgumentException>(() => new SectionCurveOutline(elevated).ToShape());
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new SectionCircular(100).GetCurveOutlines()[0].ToShape(0));
        }

        [DataTestMethod, DataRow(false, false), DataRow(true, false), DataRow(false, true), DataRow(true, true)]
        public void BuiltUpPlacementPreservesArcsAndTheirOrientation(bool mirrorX, bool mirrorY)
        {
            var section = new SectionCHS(100, 5);
            var built = new SectionBuiltUp(new[] { new SectionBuiltUp.Part(section, 230, -40, mirrorX, mirrorY) }, "placed");
            var actual = built.GetCurveOutlines().Single();
            var source = section.GetCurveOutlines().Single();
            Assert.IsInstanceOfType(actual.Boundary, typeof(ArcCurve3d));
            foreach (double t in new[] { 0, .13, .5, .81, 1 })
            {
                Point3d p = source.Boundary.PointAt(t), q = actual.Boundary.PointAt(t);
                Assert.AreEqual((mirrorX ? -p.X : p.X) + 230, q.X, 1e-9);
                Assert.AreEqual((mirrorY ? -p.Y : p.Y) - 40, q.Y, 1e-9);
            }
            Assert.AreEqual(source.ToShape(.01).GetArea(), actual.ToShape(.01).GetArea(), 1e-8);
        }

        [TestMethod]
        public void WrappersExposeTheSameNativeBoundary()
        {
            var circle = new SectionCircular(100);
            ISectionShape steel = new SteelSection(circle, SteelMaterialEN1993Data.S355);
            ISectionShape concrete = new ReinforcedConcreteSection(circle, ConcreteMaterialEN1992Data.C30_37, null, null);
            Assert.IsInstanceOfType(steel.GetCurveOutlines()[0].Boundary, typeof(ArcCurve3d));
            Assert.IsInstanceOfType(concrete.GetCurveOutlines()[0].Boundary, typeof(ArcCurve3d));
        }

        [TestMethod]
        public void SerializationRetainsNativeCurvesAndExistingSectionParameters()
        {
#pragma warning disable SYSLIB0011
            object RoundTrip(object source)
            {
                using var stream = new MemoryStream();
                var formatter = new BinaryFormatter();
                formatter.Serialize(stream, source);
                stream.Position = 0;
                return formatter.Deserialize(stream);
            }
#pragma warning restore SYSLIB0011
            var source = Create(18, 1);
            var restored = (Section)RoundTrip(source);
            Assert.AreEqual(source.GetCurveOutlines()[0].Boundary.Length, restored.GetCurveOutlines()[0].Boundary.Length, 1e-9);
            var outline = (SectionCurveOutline)RoundTrip(new SectionCHS(100, 5).GetCurveOutlines()[0]);
            Assert.IsInstanceOfType(outline.Boundary, typeof(ArcCurve3d));
            Assert.AreEqual(1, outline.Holes.Count);
            var archived = GPC.Model.Persistence.ModelArchive.CopyValue(source.GetCurveOutlines()[0]);
            Assert.AreEqual(source.GetCurveOutlines()[0].Boundary.Length, archived.Boundary.Length, 1e-9);
            Assert.IsTrue(Arcs(archived.Boundary).Any());
        }

        [DataTestMethod, DataRow(0), DataRow(1), DataRow(2)]
        public void CurvedContoursCanBeMeshedByExistingDelaunay(int kind)
        {
            Section source = kind == 2 ? new SectionEllipse(100, 60, 10) : kind == 1 ? new SectionCHS(100, 10) : new SectionCircular(100);
            var polygon = source.GetCurveOutlines()[0].ToShape(.1, 10);
            var mesh = SectionHelper.GenerateMesh(polygon, 15, true);
            Assert.IsTrue(mesh.FacesCount > 0);
            Assert.AreEqual(polygon.GetArea(), mesh.GetFaces().Sum(f => mesh.GetFaceArea(f)), 1e-7);
        }

        [DataTestMethod, DataRow(100.0, 50.0, 0.0), DataRow(100.0, 50.0, 5.0), DataRow(50.0, 100.0, 5.0), DataRow(100.0, 100.0, 5.0)]
        public void EllipticalSectionsRetainNativeAxesThroughPlacementAndPersistence(double width, double height, double thickness)
        {
            var section = new SectionEllipse(width, height, thickness);
            var outline = section.GetCurveOutlines().Single();
            Assert.IsInstanceOfType(outline.Boundary, typeof(EllipseCurve3d));
            Assert.IsFalse(outline.IsApproximation);
            double expected = Math.PI * (width * height / 4 - (thickness == 0 ? 0 : (width / 2 - thickness) * (height / 2 - thickness)));
            Assert.AreEqual(expected, outline.ToShape(.0001).GetArea(), expected * 1e-5);
            var archived = GPC.Model.Persistence.ModelArchive.CopyValue(outline);
            Assert.IsInstanceOfType(archived.Boundary, typeof(EllipseCurve3d));
            Assert.AreEqual(outline.Boundary.Length, archived.Boundary.Length, 1e-9);
            var placed = new SectionBuiltUp(new[] { new SectionBuiltUp.Part(section, 200, -30, true, false) }, "ellipse").GetCurveOutlines()[0];
            Assert.IsInstanceOfType(placed.Boundary, typeof(EllipseCurve3d));
            foreach (double t in new[] { .0, .17, .4, .89, 1 })
            {
                Point3d p = outline.Boundary.PointAt(t), q = placed.Boundary.PointAt(t);
                Assert.AreEqual(200 - p.X, q.X, 1e-9);
                Assert.AreEqual(p.Y - 30, q.Y, 1e-9);
            }
        }

        [TestMethod]
        public void WeldsStayStraightAndDoubleBottomFlangeGetsFourFillets()
        {
            var section = Create(9, 1);
            section.SetEdgeTypeFromSteelType(Section.SectionTypes.Welded);
            Assert.AreEqual(0, Arcs(section.GetCurveOutlines()[0].Boundary).Count());
            var doubleBottom = new SectionHDoubleBottomFlange(1895, 14, 500, 25, 800, 40, 600, 30, "H", 10);
            doubleBottom.SetEdgeTypeFromSteelType(Section.SectionTypes.Rolled);
            doubleBottom.SetMechanicalProperties();
            Assert.AreEqual(4, Arcs(doubleBottom.GetCurveOutlines()[0].Boundary).Count());
            Assert.AreEqual(doubleBottom.Area, doubleBottom.GetCurveOutlines()[0].ToShape(.01).GetArea(), 1);
        }

        [DataTestMethod, DataRow(false), DataRow(true)]
        public void ColdFormedBendMayConsumeTheWholeInsideBoundary(bool reversed)
        {
            var points = new[] { new Point2d(-1, 0), new Point2d(0, 0), new Point2d(0, 1) };
            var section = new SectionColdFormed(reversed ? points.Reverse() : points, 2, 0);
            var outline = section.GetCurveOutlines().Single();
            Assert.IsTrue(outline.Boundary.IsClosed);
            Assert.AreEqual(Math.PI + 4, outline.Boundary.Length, 1e-10);
            Assert.AreEqual(Math.PI, outline.ToShape(1e-5).GetArea(), 3e-5);
            Assert.AreEqual(1, Arcs(outline.Boundary).Count());
        }

        [TestMethod]
        public void UnbentColdFormedStripHasFourStraightEdges()
        {
            var section = new SectionColdFormed(new[] { new Point2d(0, 0), new Point2d(10, 0) }, 2, 0);
            var outline = section.GetCurveOutlines()[0];
            Assert.AreEqual(0, Arcs(outline.Boundary).Count());
            Assert.AreEqual(24, outline.Boundary.Length, 1e-10);
            Assert.AreEqual(20, outline.ToShape().GetArea(), 1e-10);
        }
    }
}
