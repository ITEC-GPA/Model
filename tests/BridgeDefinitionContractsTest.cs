using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using GPC.Geometry;
using GPC.Model.Core;
using GPC.Model.Structure.Bridges;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]public class BridgeDefinitionContractsTest
    {
        [TestMethod]public void SectionRulesKeepIndependentPropertiesSelectorsAndDimensionalScales()
        {
            var rule=new BridgeSectionRule {Family=GPC.Model.Structure.Members.BridgeMemberFamily.Girder,Group=2,Start=1000,End=3000,
                Property=new GPC.Model.ElementProperties.ElasticBeamProperty("test",new GPC.Model.Materials.Material("steel",210000,.3,0,0),new GPC.Model.Sections.SectionRectangular(100,200)),
                ScaleFrom=new Vector2d(1,2),ScaleTo=new Vector2d(3,4)};
            rule.Validate();var copy=ModelValues.CopyValue(rule);Assert.AreEqual(ModelValues.Fingerprint(new object[]{rule}),ModelValues.Fingerprint(new object[]{copy}));
            Assert.AreNotSame(rule.Property,copy.Property);Assert.AreEqual(2,copy.Scale(2000).X);Assert.AreEqual(3,copy.Scale(2000).Y);
            Assert.AreEqual(1,copy.Scale(0).X);Assert.AreEqual(3,copy.Scale(4000).X);
            var member=new GPC.Model.Structure.Members.BridgeMemberMetadata("girder",rule.Family,2,3,1,1000,3000,new Vector3d(0,0,1));
            Assert.IsTrue(copy.Selects(member));copy.Segment=4;Assert.IsFalse(copy.Selects(member));
            copy.ScaleFrom.X=2;Assert.AreEqual(1,rule.ScaleFrom.X);copy.Family=GPC.Model.Structure.Members.BridgeMemberFamily.Post;
            Assert.ThrowsException<ArgumentException>(()=>copy.Validate());rule.End=rule.Start;Assert.ThrowsException<ArgumentException>(()=>rule.Validate());
            var fallback=new BridgeFamilyProperty {Property=copy.Property};fallback.Validate();Assert.IsNull(ModelValues.CopyValue(fallback).Family);
        }
        private static BridgeDefinition Sample(bool curved=false)
        {
            Curve3d alignment=curved?(Curve3d)new ArcCurve3d(new Point3d(0,0,0),new Vector3d(0,0,1),new Vector3d(1,0,0),30000,1)
                :new LineCurve3d(new Point3d(0,0,0),new Point3d(30000,0,0));
            return new BridgeDefinition {Id=Guid.NewGuid(),Alignment=alignment,Supports=new BridgeSupportLayout {Stations=new[]{0d,15000,30000}},
                Girders=new BridgeGirderLayout {Offsets=new[]{-3000d,3000}},Fabrication=BridgeFabricationPlan.Create(30000,2,new BridgeSegmentSettings())};
        }
        [DataTestMethod][DataRow(false)][DataRow(true)]
        public void IndependentDefinitionCopyAndXmlKeepGeometryAndFingerprint(bool curved)
        {
            var definition=Sample(curved);string fp=definition.Fingerprint();var copy=ModelValues.CopyValue(definition);
            Assert.AreEqual(fp,copy.Fingerprint());Assert.AreNotSame(definition.Alignment,copy.Alignment);
            for(int i=0;i<=10;i++)Assert.AreEqual(0,GPC.Model.Core.Coordinates.Axes.Length(definition.Alignment.PointAtLength(i*3000)-copy.Alignment.PointAtLength(i*3000)),1e-8);
            var serializer=new DataContractSerializer(typeof(BridgeDefinition),ModelValues.DataContracts);
            using(var stream=new MemoryStream())
            {serializer.WriteObject(stream,definition);stream.Position=0;var restored=(BridgeDefinition)serializer.ReadObject(stream);Assert.AreEqual(fp,restored.Fingerprint());}
            copy.Deck.Slopes[0]=.02;Assert.AreNotEqual(fp,copy.Fingerprint());Assert.AreEqual(fp,definition.Fingerprint());
            copy.Alignment.Move(100,0,0);Assert.AreEqual(curved?30000:0,definition.Alignment.StartPoint.X,1e-8);
        }
        [TestMethod]public void FabricationPrecedesMeshAndExplicitJointsTakePrecedence()
        {
            var settings=new BridgeSegmentSettings {MaximumLength=12000};settings.Joints.Add(1,new[]{8000d});
            var plan=BridgeFabricationPlan.Create(30000,2,settings);plan.Validate();
            CollectionAssert.AreEqual(new[]{0d,8000},plan.Ranges.Where(r=>r.Girder==1).Select(r=>r.Start).ToArray());
            CollectionAssert.AreEqual(new[]{8000d,30000},plan.Ranges.Where(r=>r.Girder==1).Select(r=>r.End).ToArray());
            CollectionAssert.AreEqual(new[]{0d,10000,20000},plan.Ranges.Where(r=>r.Girder==2).Select(r=>r.Start).ToArray());
            settings.Joints[1][0]=9000;Assert.AreEqual(8000,plan.Settings.Joints[1][0]);
            plan.Settings.Joints[1][0]=10000;StringAssert.Contains(Assert.ThrowsException<ArgumentException>(()=>plan.Validate()).Message,"StaleBridgeFabricationRanges");
        }
        [TestMethod]public void CommonAndLocalJointsProduceDeterministicOrderedSegments()
        {
            var a=new BridgeSegmentSettings();a.Joints.Add(2,new[]{20000d,10000});a.Joints.Add(0,new[]{10000d});
            var b=new BridgeSegmentSettings();b.Joints.Add(0,new[]{10000d});b.Joints.Add(2,new[]{20000d,10000});
            var pa=BridgeFabricationPlan.Create(30000,2,a);var pb=BridgeFabricationPlan.Create(30000,2,b);
            Assert.AreEqual(ModelValues.Fingerprint(new object[]{pa}),ModelValues.Fingerprint(new object[]{pb}));
            Assert.AreEqual(5,pa.Ranges.Length);Assert.IsTrue(pa.Ranges.All(r=>r.End>r.Start));
        }
        [TestMethod]public void InvalidLayoutAndUnmatchedFabricationAreRejected()
        {
            var definition=Sample();definition.Supports.Stations=new[]{0d,0,30000};Assert.ThrowsException<ArgumentException>(()=>definition.Validate());
            definition=Sample();definition.Deck.Slopes=new[]{.01,.02};definition.Deck.SlopeStations=new[]{1000d,30000};Assert.ThrowsException<ArgumentException>(()=>definition.Validate());
            definition=Sample();definition.Fabrication=BridgeFabricationPlan.Create(20000,2,new BridgeSegmentSettings());Assert.ThrowsException<ArgumentException>(()=>definition.Validate());
            definition=Sample();definition.Supports.SkewRadians[0]=Math.PI/2;Assert.ThrowsException<ArgumentException>(()=>definition.Validate());
            definition=Sample();definition.Diaphragms.Truss=true;Assert.ThrowsException<ArgumentException>(()=>definition.Validate());
        }
        [TestMethod]public void FutureSchemaAndUnboundedSubdivisionAreRejected()
        {
            var serializer=new DataContractSerializer(typeof(BridgeDefinition),ModelValues.DataContracts);string xml;
            using(var bytes=new MemoryStream()){serializer.WriteObject(bytes,Sample());xml=Encoding.UTF8.GetString(bytes.ToArray());}
            Assert.IsTrue(xml.Contains("<schema>1</schema>"));
            using(var bytes=new MemoryStream(Encoding.UTF8.GetBytes(xml.Replace("<schema>1</schema>","<schema>2</schema>"))))
                Assert.ThrowsException<SerializationException>(()=>serializer.ReadObject(bytes));
            Assert.ThrowsException<ArgumentException>(()=>BridgeFabricationPlan.Create(30000,2,new BridgeSegmentSettings {MaximumLength=.001}));
        }
    }
}
