using GPC.Model.Geotechnics;
using GPC.Model.Standards;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;

namespace UnitTest
{
    /// <summary>Soil data (N, mm, MPa, rad) and geotechnical standards of Model.</summary>
    [TestClass]
    public class GeotechnicsTest
    {
        private static Soil Sand(double gamma = 18, double sat = 20) => new Soil("Sand", gamma * SoilUnits.KiloNewtonPerCubicMetre, sat * SoilUnits.KiloNewtonPerCubicMetre,
            32 * SoilUnits.Degree, 0, "test report");
        private static Soil Clay() => new Soil("Clay", 19 * SoilUnits.KiloNewtonPerCubicMetre, 19.5 * SoilUnits.KiloNewtonPerCubicMetre, 24 * SoilUnits.Degree,
            10 * SoilUnits.KiloPascal, "test report", undrainedShearStrength: 60 * SoilUnits.KiloPascal, constrainedModulus: 6);

        [TestMethod]
        public void SoilRequiresProvenanceAndConsistentParameters()
        {
            Assert.ThrowsException<ArgumentException>(() => new Soil("S", 1.8e-5, 2e-5, .5, 0, " "));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Soil("S", 2e-5, 1.8e-5, .5, 0, "r"), "γsat < γ");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Soil("S", 1.8e-5, 2e-5, Math.PI / 2, 0, "r"), "φ = 90°");
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Soil("S", 1.8e-5, 2e-5, .5, -1e-3, "r"));
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => new Soil("S", 1.8e-5, 2e-5, .5, 0, "r", undrainedShearStrength: 0));
            var clay = Clay();
            Assert.AreEqual(0.06, clay.UndrainedShearStrength!.Value, 1e-15); Assert.IsNull(Sand().UndrainedShearStrength);
            Assert.AreEqual(Clay(), clay); Assert.AreNotEqual(Sand(), Sand(18.5));
        }

        // Ground at 0, sand 0 to -3 m (γ 18, γsat 20 kN/m³), clay -3 to -10 m (γ 19, γsat 19.5), water table at -2 m.
        // At -6 m: σv = 18·2 + 20·1 + 19.5·3 = 114.5 kPa; u = 9.81·4 = 39.24 kPa; σ'v = 75.26 kPa.
        [TestMethod]
        public void ProfileGivesHydrostaticGeostaticStresses()
        {
            var profile = new SoilProfile("BH1", new[] { new SoilLayer(Sand(), 0, -3000), new SoilLayer(Clay(), -3000, -10000) }, "test report", -2000);
            Assert.AreEqual(114.5e-3, profile.VerticalTotalStress(-6000), 1e-12);
            Assert.AreEqual(39.24e-3, profile.PorePressure(-6000), 1e-12);
            Assert.AreEqual(75.26e-3, profile.VerticalEffectiveStress(-6000), 1e-12);
            Assert.AreEqual(36e-3, profile.VerticalEffectiveStress(-2000), 1e-12);
            Assert.AreEqual(0, profile.VerticalTotalStress(0));
            Assert.AreEqual("Clay", profile.LayerAt(-3000).Soil.Name); Assert.AreEqual("Sand", profile.LayerAt(-2999).Soil.Name);
            Assert.ThrowsException<ArgumentOutOfRangeException>(() => profile.VerticalTotalStress(-10001));
            Assert.ThrowsException<ArgumentException>(() => new SoilProfile("gap", new[] { new SoilLayer(Sand(), 0, -3000), new SoilLayer(Clay(), -3500, -10000) }, "r"));
            var dry = new SoilProfile("BH2", new[] { new SoilLayer(Sand(), 0, -3000) }, "r");
            Assert.AreEqual(54e-3, dry.VerticalEffectiveStress(-3000), 1e-12);
        }

        [TestMethod]
        public void Ntc2018CombinationsAndFactorsFollowTheTables()
        {
            var ntc = new StandardNTC2018Geotechnics();
            var wall = ntc.Combinations(GeotechnicalCheck.RetainingWallSliding, GeotechnicalSituation.PersistentTransient).Single();
            Assert.AreEqual("A1+M1+R3", wall.Name); Assert.AreEqual(1.1, wall.ResistanceFactor); Assert.AreEqual(1.3, wall.Actions.PermanentUnfavourable);
            Assert.AreEqual(1.5, wall.Actions.VariableUnfavourable); Assert.AreEqual(0, wall.Actions.VariableFavourable);
            Assert.AreEqual(1.15, ntc.Combinations(GeotechnicalCheck.RetainingWallOverturning, GeotechnicalSituation.PersistentTransient).Single().ResistanceFactor);
            Assert.AreEqual(1.4, ntc.Combinations(GeotechnicalCheck.RetainingWallBearing, GeotechnicalSituation.PersistentTransient).Single().ResistanceFactor);
            Assert.AreEqual(2.3, ntc.Combinations(GeotechnicalCheck.ShallowFoundationBearing, GeotechnicalSituation.PersistentTransient).Single().ResistanceFactor);
            var global = ntc.Combinations(GeotechnicalCheck.GlobalStability, GeotechnicalSituation.PersistentTransient).Single();
            Assert.AreEqual("A2+M2+R2", global.Name); Assert.AreEqual(1.25, global.Materials.TanFrictionAngle); Assert.AreEqual(1.4, global.Materials.UndrainedShearStrength);
            Assert.AreEqual(1.1, global.ResistanceFactor); Assert.AreEqual(1.3, global.Actions.VariableUnfavourable);
            var seismic = ntc.Combinations(GeotechnicalCheck.GlobalStability, GeotechnicalSituation.Seismic).Single();
            Assert.AreEqual(1.2, seismic.ResistanceFactor); Assert.AreEqual(1.0, seismic.Materials.TanFrictionAngle); Assert.AreEqual(1.0, seismic.Actions.PermanentUnfavourable);
            Assert.AreEqual(1.2, ntc.Combinations(GeotechnicalCheck.RetainingWallBearing, GeotechnicalSituation.Seismic).Single().ResistanceFactor);
            Assert.AreEqual(1.0, ntc.Combinations(GeotechnicalCheck.RetainingWallSliding, GeotechnicalSituation.Seismic).Single().ResistanceFactor);
            Assert.AreEqual(1.35, ntc.ResistanceFactor(GeotechnicalCheck.PileBase, GeotechnicalSituation.PersistentTransient, "R3"));
            Assert.AreEqual(1.25, ntc.ResistanceFactor(GeotechnicalCheck.PileShaftTension, GeotechnicalSituation.PersistentTransient, "R3"));
            Assert.AreEqual(1.3, ntc.ResistanceFactor(GeotechnicalCheck.PileTransverse, GeotechnicalSituation.PersistentTransient, "R3"));
            Assert.AreEqual(0, ntc.Combinations(GeotechnicalCheck.PileBase, GeotechnicalSituation.Seismic).Count, "not defined: no combination");
            Assert.AreEqual(Tuple.Create(1.70, 1.70), ntc.PileCorrelationFactors(1));
            Assert.AreEqual(Tuple.Create(1.50, 1.34), ntc.PileCorrelationFactors(6), "between columns the safe-side column applies");
            Assert.AreEqual(Tuple.Create(1.40, 1.21), ntc.PileCorrelationFactors(12));
        }

        [TestMethod]
        public void En1997ApproachesSelectTheSets()
        {
            var da1 = new StandardEN1997p1();
            var sliding = da1.Combinations(GeotechnicalCheck.ShallowFoundationSliding, GeotechnicalSituation.PersistentTransient);
            CollectionAssert.AreEqual(new[] { "A1+M1+R1", "A2+M2+R1" }, sliding.Select(c => c.Name).ToArray());
            Assert.AreEqual(1.35, sliding[0].Actions.PermanentUnfavourable); Assert.AreEqual(1.25, sliding[1].Materials.EffectiveCohesion);
            var piles = da1.Combinations(GeotechnicalCheck.PileBase, GeotechnicalSituation.PersistentTransient);
            CollectionAssert.AreEqual(new[] { "A1+M1+R1-Bored", "A2+M1+R4-Bored" }, piles.Select(c => c.Name).ToArray());
            Assert.AreEqual(1.25, piles[0].ResistanceFactor); Assert.AreEqual(1.6, piles[1].ResistanceFactor);
            var da2 = new StandardEN1997p1("EN 1997-1 DA2", GeotechnicalDesignApproach.DA2, PileExecution.Driven);
            Assert.AreEqual(1.4, da2.Combinations(GeotechnicalCheck.ShallowFoundationBearing, GeotechnicalSituation.PersistentTransient).Single().ResistanceFactor);
            Assert.AreEqual(1.1, da2.Combinations(GeotechnicalCheck.PileBase, GeotechnicalSituation.PersistentTransient).Single().ResistanceFactor);
            var da3 = new StandardEN1997p1("EN 1997-1 DA3", GeotechnicalDesignApproach.DA3);
            Assert.AreEqual("A2+M2+R3", da3.Combinations(GeotechnicalCheck.SlopeStability, GeotechnicalSituation.PersistentTransient).Single().Name);
            var overturning = da1.Combinations(GeotechnicalCheck.RetainingWallOverturning, GeotechnicalSituation.PersistentTransient).Single();
            Assert.AreEqual("EQU+M2+EQU", overturning.Name); Assert.AreEqual(.9, overturning.Actions.PermanentFavourable);
            Assert.AreEqual(0, da1.Combinations(GeotechnicalCheck.GlobalStability, GeotechnicalSituation.Seismic).Count, "EN 1998-5 not implemented");
            Assert.AreEqual(Tuple.Create(1.31, 1.20), da1.PileCorrelationFactors(4));
        }

        [TestMethod]
        public void OverridesAreExplicitAndSurviveSerialization()
        {
            var ntc = new StandardNTC2018Geotechnics();
            ntc.SetResistanceFactor(GeotechnicalCheck.RetainingWallSliding, GeotechnicalSituation.PersistentTransient, "R3", 1.2);
            ntc.SetMaterialSet(new GeotechnicalMaterialFactors("M2", 1.3, 1.3, 1.5, 1.6, 1.0));
            Assert.AreEqual(1.2, ntc.Combinations(GeotechnicalCheck.RetainingWallSliding, GeotechnicalSituation.PersistentTransient).Single().ResistanceFactor);
            Assert.AreNotEqual(new StandardNTC2018Geotechnics(), ntc);
            foreach (StandardGeotechnical original in new StandardGeotechnical[] { ntc, new StandardEN1997p1("EN DA3", GeotechnicalDesignApproach.DA3, PileExecution.ContinuousFlightAuger) })
            {
                var info = new SerializationInfo(original.GetType(), new FormatterConverter());
                original.GetObjectData(info, new StreamingContext());
                var copy = (StandardGeotechnical)original.GetType().GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null,
                    new[] { typeof(SerializationInfo), typeof(StreamingContext) }, null)!.Invoke(new object[] { info, new StreamingContext() });
                Assert.AreEqual(original, copy, original.Name);
            }
        }
    }
}
