#nullable disable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Elements;
using GPC.Model.FreedomCases;
using GPC.Model.LoadCases;
using GPC.Model.Models;
using GPC.Model.Persistence;
using GPC.Model.Restraints;
using GPC.Model.Structure.Assignments;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    /// <summary>
    /// 18/11/2026 (GPCModel 5.53): restraints, springs to ground and load cases of several freedom cases. The restraints of a node are validated
    /// per freedom case, a spring to ground and a load case may name their freedom case (<see cref="SpringMatrix.FreedomCase"/>,
    /// <see cref="LoadCaseBase.FreedomCase"/>). Without a freedom case nothing is written: archives and fingerprints are those of before.
    /// UnitTest runs these tests on .NET 6, Model.NetFramework.Tests on .NET Framework 4.7.2.
    /// </summary>
    [TestClass]
    public class FreedomCaseScopesTest
    {
        private static RestrainAssignment Fixed(NodeElement node, FreedomCase freedomCase, params GeometryRestrain.DOF[] dofs) => new RestrainAssignment
        { FreedomCase = freedomCase, Restrain = new NodeRestrain(node, CoordinateSystem.Global, dofs.Select(d => new DofRestrain(d)).ToList()), SourceRecord = "test" };

        private static SpringMatrix Spring(double kz, FreedomCase freedomCase = null)
        {
            var k = new double[36]; k[14] = kz;
            return new SpringMatrix(k, CoordinateSystem.Global) { SourceRecord = "spring", FreedomCase = freedomCase };
        }

        /// <summary>A node restrained in DZ under freedom case A and DX, DZ under B, with a spring in DZ under B, and two load cases solved
        /// under A and B; a second node with a spring without a freedom case.</summary>
        private static (Model Model, FreedomCase A, FreedomCase B) TwoFreedomCases()
        {
            var model = new Model(); var a = new FreedomCase("A"); var b = new FreedomCase("B");
            model.FreedomCases.Add(a); model.FreedomCases.Add(b);
            var node = new NodeElement(new Point3d(0, 0, 0)); var other = new NodeElement(new Point3d(1000, 0, 0));
            model.NodesElements.Add(node); model.NodesElements.Add(other);
            node.Assignments.AssignRestrain(Fixed(node, a, GeometryRestrain.DOF.DZ), AssignmentMode.Add);
            node.Assignments.AssignRestrain(Fixed(node, b, GeometryRestrain.DOF.DX, GeometryRestrain.DOF.DZ), AssignmentMode.Add);
            node.Assignments.GroundSprings.Add(Spring(500, b));
            other.Assignments.GroundSprings.Add(Spring(100));
            model.LoadCases.Add(new LoadCaseBase("G") { FreedomCase = a }); model.LoadCases.Add(new LoadCaseBase("Q") { FreedomCase = b });
            model.LoadCases.Add(new LoadCaseBase("W"));
            return (model, a, b);
        }

        [TestMethod]
        public void RestraintsAreValidatedPerFreedomCase()
        {
            var (model, a, b) = TwoFreedomCases();
            var codes = model.ValidateAssignments().Select(d => d.Code).ToArray();
            Assert.IsFalse(codes.Contains("ConflictingRestrainScope"), "DZ under A and under B: two freedom cases, no conflict (until GPCModel 5.52 an error).");
            Assert.IsFalse(codes.Contains("UnregisteredFreedomCase"), string.Join(", ", codes));
            // In one freedom case, or without freedom cases, two restraints of one DOF still conflict.
            var node = model.NodesElements.Values.First();
            node.Assignments.Restrains.Add(Fixed(node, a, GeometryRestrain.DOF.DZ));
            Assert.IsTrue(model.ValidateAssignments().Any(d => d.Code == "ConflictingRestrainScope"));
            var plain = new Model(); var n = new NodeElement(new Point3d(0, 0, 0)); plain.NodesElements.Add(n);
            n.Assignments.Restrains.Add(Fixed(n, null, GeometryRestrain.DOF.DZ)); n.Assignments.Restrains.Add(Fixed(n, null, GeometryRestrain.DOF.DZ));
            Assert.IsTrue(plain.ValidateAssignments().Any(d => d.Code == "ConflictingRestrainScope"), "Without freedom cases the rule of before.");
        }

        [TestMethod]
        public void NodeAssignmentsGiveTheRestraintsAndSpringsOfAFreedomCase()
        {
            var (model, a, b) = TwoFreedomCases();
            var node = model.NodesElements.Values.First(); var other = model.NodesElements.Values.Last();
            CollectionAssert.AreEqual(new[] { GeometryRestrain.DOF.DZ }, node.Assignments.RestrainsOf(a).SelectMany(r => r.Restrain.Restrains).Select(d => d.Dof).ToArray());
            CollectionAssert.AreEqual(new[] { GeometryRestrain.DOF.DX, GeometryRestrain.DOF.DZ }, node.Assignments.RestrainsOf(b).SelectMany(r => r.Restrain.Restrains).Select(d => d.Dof).ToArray());
            Assert.AreEqual(0, node.Assignments.RestrainsOf(null).Count());
            Assert.AreEqual(500, node.Assignments.GroundSpringsOf(model.LoadCases["Q"].FreedomCase).Single()[2, 2]);
            Assert.AreEqual(0, node.Assignments.GroundSpringsOf(a).Count());
            Assert.AreEqual(100, other.Assignments.GroundSpringsOf(null).Single()[2, 2], "A spring without a freedom case is in the default set.");
            // By Guid: a copy of the freedom case selects the same assignments.
            Assert.AreEqual(1, node.Assignments.GroundSpringsOf(ModelValues.CopyValue(b)).Count());
        }

        [TestMethod]
        public void UnregisteredFreedomCasesAndLinkSpringsWithAFreedomCaseAreRefused()
        {
            var (model, a, b) = TwoFreedomCases();
            var stranger = new FreedomCase("A"); // the name of A, another Guid
            model.NodesElements.Values.Last().Assignments.GroundSprings.Single().FreedomCase = stranger;
            model.LoadCases["W"].FreedomCase = new FreedomCase("C");
            var refused = model.ValidateAssignments().Where(d => d.Code == "UnregisteredFreedomCase").ToArray();
            Assert.AreEqual(2, refused.Length, string.Join("; ", refused.Select(d => d.Message)));
            var node = model.NodesElements.Values.First(); var link = new NodalLink { OtherNodeId = model.NodesElements.Values.Last().Id, Spring = Spring(1, a), SourceRecord = "link" };
            node.Assignments.Links.Add(link);
            Assert.IsTrue(model.ValidateAssignments().Any(d => d.Code == "FreedomCaseOnLinkSpring"));
        }

        [TestMethod]
        public void AbsentFreedomCasesLeaveFingerprintsAndArchivesAsBefore()
        {
            var spring = Spring(500); string springBefore = ModelValues.Fingerprint(new object[] { spring });
            var loadCase = new LoadCaseBase("G"); string caseBefore = ModelValues.Fingerprint(new object[] { loadCase });
            var masses = new LoadCaseBase("M") { InertiaMasses = new InertiaMassSelection { Structural = true, NonStructural = false } };
            string massesBefore = ModelValues.Fingerprint(new object[] { masses });
            var fc = new FreedomCase("B");
            spring.FreedomCase = fc; loadCase.FreedomCase = fc; masses.FreedomCase = fc;
            Assert.AreNotEqual(springBefore, ModelValues.Fingerprint(new object[] { spring }));
            Assert.AreNotEqual(caseBefore, ModelValues.Fingerprint(new object[] { loadCase }));
            Assert.AreNotEqual(massesBefore, ModelValues.Fingerprint(new object[] { masses }));
            spring.FreedomCase = null; loadCase.FreedomCase = null; masses.FreedomCase = null;
            Assert.AreEqual(springBefore, ModelValues.Fingerprint(new object[] { spring }));
            Assert.AreEqual(caseBefore, ModelValues.Fingerprint(new object[] { loadCase }));
            Assert.AreEqual(massesBefore, ModelValues.Fingerprint(new object[] { masses }));

            // A model without them gives the same archive, byte for byte, before and after they are set and removed again.
            var (model, a, b) = TwoFreedomCases();
            void Scopes(bool on)
            {
                model.LoadCases["G"].FreedomCase = on ? a : null; model.LoadCases["Q"].FreedomCase = on ? b : null;
                model.NodesElements.Values.First().Assignments.GroundSprings.Single().FreedomCase = on ? b : null;
            }
            byte[] Save(bool document) { using var stream = new MemoryStream(); if (document) ModelArchive.SaveDocument(model, stream); else ModelArchive.Save(model, stream); return stream.ToArray(); }
            Scopes(false); var without = new[] { Save(false), Save(true) }; string fingerprint = model.AnalysisFingerprint();
            Scopes(true); var with = new[] { Save(false), Save(true) };
            Assert.AreNotEqual(fingerprint, model.AnalysisFingerprint());
            CollectionAssert.AreNotEqual(without[0], with[0]); CollectionAssert.AreNotEqual(without[1], with[1]);
            Scopes(false);
            CollectionAssert.AreEqual(without[0], Save(false)); CollectionAssert.AreEqual(without[1], Save(true));
            Assert.AreEqual(fingerprint, model.AnalysisFingerprint());
        }

        [TestMethod]
        public void FreedomCasesOfSpringsAndLoadCasesRoundTrip()
        {
            var (model, a, b) = TwoFreedomCases();
            model.LoadCases["G"].InertiaMasses = new InertiaMassSelection { Structural = true, NonStructural = false };
            string fingerprint = model.AnalysisFingerprint();
            var spring = model.NodesElements.Values.First().Assignments.GroundSprings.Single();
            Assert.AreSame(b, spring.ToCoordinateSystem(new CoordinateSystem(spring.CoordinateSystem.Origin, new Vector3d(0, 1, 0), new Vector3d(-1, 0, 0))).FreedomCase);
            foreach (bool document in new[] { false, true })
                using (var stream = new MemoryStream())
                {
                    if (document) ModelArchive.SaveDocument(model, stream); else ModelArchive.Save(model, stream);
                    stream.Position = 0; var restored = ModelArchive.Load(stream);
                    Assert.AreEqual(fingerprint, restored.AnalysisFingerprint(), document ? "document" : "model");
                    var fa = restored.FreedomCases["A"]; var fb = restored.FreedomCases["B"];
                    Assert.AreSame(fa, restored.LoadCases["G"].FreedomCase); Assert.AreSame(fb, restored.LoadCases["Q"].FreedomCase);
                    Assert.IsNull(restored.LoadCases["W"].FreedomCase); Assert.IsTrue(restored.LoadCases["G"].InertiaMasses.Structural);
                    Assert.AreSame(fb, restored.NodesElements.Values.First().Assignments.GroundSprings.Single().FreedomCase);
                    Assert.IsNull(restored.NodesElements.Values.Last().Assignments.GroundSprings.Single().FreedomCase);
                    Assert.AreSame(fa, restored.NodesElements.Values.First().Assignments.Restrains[0].FreedomCase);
                }
            var copy = ModelValues.Copy(model);
            Assert.AreEqual(fingerprint, copy.AnalysisFingerprint());
            Assert.AreSame(copy.FreedomCases["B"], copy.LoadCases["Q"].FreedomCase);
            Assert.AreSame(copy.FreedomCases["B"], copy.NodesElements.Values.First().Assignments.GroundSprings.Single().FreedomCase);
            // The freedom case of a load case is an analysis input.
            copy.LoadCases["Q"].FreedomCase = copy.FreedomCases["A"];
            Assert.AreNotEqual(fingerprint, copy.AnalysisFingerprint()); Assert.AreEqual(fingerprint, model.AnalysisFingerprint());
            using (var one = new MemoryStream())
            {
                ModelValueArchive.Save(model.LoadCases["Q"], one); one.Position = 0;
                Assert.AreEqual("B", ModelValueArchive.Load<LoadCaseBase>(one).FreedomCase.Name);
            }
        }

        /// <summary>The model of the archive fixture: <see cref="TwoFreedomCases"/>, the inertia masses of G and a spring of a third of a N/mm.</summary>
        internal static Model FixtureModel()
        {
            var (model, a, b) = TwoFreedomCases();
            model.LoadCases["G"].InertiaMasses = new InertiaMassSelection { Structural = true, NonStructural = false };
            model.NodesElements.Values.Last().Assignments.GroundSprings.Add(Spring(1.0 / 3, a));
            return model;
        }

        /// <summary>The fingerprint of <c>tests/Fixtures/freedom-cases-20261118.xml</c> (<see cref="FixtureModel"/> saved with <see cref="ModelArchive.Save(Model, Stream)"/>
        /// by this change of GPCModel on .NET 6, then loaded there).</summary>
        private const string FixtureFingerprint = "7ADEEA95A27DE59F04576EF081F7DFEA65C082EABAFEB09344C0B053ED4B031E";

        [TestMethod]
        public void ArchiveWrittenOnNetLoadsWithItsFreedomCasesOnEveryRuntime()
        {
            using var file = File.OpenRead(Path.Combine(AppContext.BaseDirectory, "Fixtures", "freedom-cases-20261118.xml"));
            var model = ModelArchive.Load(file);
            Assert.AreEqual(FixtureFingerprint, model.AnalysisFingerprint());
            Assert.AreSame(model.FreedomCases["A"], model.LoadCases["G"].FreedomCase); Assert.AreSame(model.FreedomCases["B"], model.LoadCases["Q"].FreedomCase);
            Assert.IsNull(model.LoadCases["W"].FreedomCase); Assert.IsTrue(model.LoadCases["G"].InertiaMasses.Structural);
            var springs = model.NodesElements.Values.SelectMany(n => n.Assignments.GroundSprings).ToArray();
            Assert.AreEqual(3, springs.Length);
            Assert.AreSame(model.FreedomCases["B"], springs[0].FreedomCase); Assert.IsNull(springs[1].FreedomCase); Assert.AreSame(model.FreedomCases["A"], springs[2].FreedomCase);
            Assert.AreEqual(BitConverter.DoubleToInt64Bits(1.0 / 3), BitConverter.DoubleToInt64Bits(springs[2][2, 2]));
            Assert.IsFalse(model.ValidateAssignments().Any(d => d.Severity == GPC.Model.Core.Diagnostics.DiagnosticSeverity.Error));
        }

        [TestMethod]
        public void AssemblyKeepsOneFreedomCaseOfEachName()
        {
            var a = new FreedomCase("A"); var node = new NodeElement(new Point3d(0, 0, 0)); var g = new LoadCaseBase("G") { FreedomCase = a };
            node.Assignments.GroundSprings.Add(Spring(5, new FreedomCase(a.Guid, "A")));
            node.Loads.Add(new GPC.Model.Loads.PointLoad(0, 0, -1, 0, 0, 0, node.Position, g));
            var model = ModelAssembly.Assemble(new Element[] { node }, Guid.NewGuid());
            Assert.AreEqual(1, model.FreedomCases.Count);
            Assert.AreSame(model.FreedomCases["A"], model.LoadCases["G"].FreedomCase);
            Assert.AreSame(model.FreedomCases["A"], model.NodesElements.Values.Single().Assignments.GroundSprings.Single().FreedomCase);
            Assert.IsFalse(model.ValidateAssignments().Any(d => d.Code == "UnregisteredFreedomCase"));
        }
    }
}
