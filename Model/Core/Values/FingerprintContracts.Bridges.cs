namespace GPC.Model.Core
{
    internal static partial class FingerprintContracts
    {
        private static void RegisterBridgeContracts()
        {
            Register("GPC.Model.Structure.Bridges.BridgeSectionRule", "GPC.Model.Structure.Bridges.BridgeSectionRule",
                Field("GPC.Model.Structure.Bridges.BridgeSectionRule", "<Family>k__BackingField", "Family", false),
                Field("GPC.Model.Structure.Bridges.BridgeSectionRule", "<Group>k__BackingField", "Group", false),
                Field("GPC.Model.Structure.Bridges.BridgeSectionRule", "<Segment>k__BackingField", "Segment", false),
                Field("GPC.Model.Structure.Bridges.BridgeSectionRule", "<Start>k__BackingField", "Start", false),
                Field("GPC.Model.Structure.Bridges.BridgeSectionRule", "<End>k__BackingField", "End", false),
                Field("GPC.Model.Structure.Bridges.BridgeSectionRule", "<Property>k__BackingField", "Property", false),
                Field("GPC.Model.Structure.Bridges.BridgeSectionRule", "<ScaleFrom>k__BackingField", "ScaleFrom", false),
                Field("GPC.Model.Structure.Bridges.BridgeSectionRule", "<ScaleTo>k__BackingField", "ScaleTo", false));
            Register("GPC.Model.Structure.Bridges.BridgeFamilyProperty", "GPC.Model.Structure.Bridges.BridgeFamilyProperty",
                Field("GPC.Model.Structure.Bridges.BridgeFamilyProperty", "<Family>k__BackingField", "Family", false),
                Field("GPC.Model.Structure.Bridges.BridgeFamilyProperty", "<Property>k__BackingField", "Property", false));
            Register("GPC.Model.Structure.Bridges.BridgeDefinition", "GPC.Model.Structure.Bridges.BridgeDefinition",
                Field("GPC.Model.Structure.Bridges.BridgeDefinition", "schema", "Schema", false),
                Field("GPC.Model.Structure.Bridges.BridgeDefinition", "<Id>k__BackingField", "Id", false),
                Field("GPC.Model.Structure.Bridges.BridgeDefinition", "<Alignment>k__BackingField", "Alignment", false),
                Field("GPC.Model.Structure.Bridges.BridgeDefinition", "<Supports>k__BackingField", "Supports", false),
                Field("GPC.Model.Structure.Bridges.BridgeDefinition", "<Girders>k__BackingField", "Girders", false),
                Field("GPC.Model.Structure.Bridges.BridgeDefinition", "<Deck>k__BackingField", "Deck", false),
                Field("GPC.Model.Structure.Bridges.BridgeDefinition", "<Fabrication>k__BackingField", "Fabrication", false),
                Field("GPC.Model.Structure.Bridges.BridgeDefinition", "<Diaphragms>k__BackingField", "Diaphragms", false),
                Field("GPC.Model.Structure.Bridges.BridgeDefinition", "<Mesh>k__BackingField", "Mesh", false));
            Register("GPC.Model.Structure.Bridges.BridgeSupportLayout", "GPC.Model.Structure.Bridges.BridgeSupportLayout",
                Field("GPC.Model.Structure.Bridges.BridgeSupportLayout", "<Stations>k__BackingField", "Stations", false),
                Field("GPC.Model.Structure.Bridges.BridgeSupportLayout", "<SkewRadians>k__BackingField", "SkewRadians", false));
            Register("GPC.Model.Structure.Bridges.BridgeGirderLayout", "GPC.Model.Structure.Bridges.BridgeGirderLayout",
                Field("GPC.Model.Structure.Bridges.BridgeGirderLayout", "<Offsets>k__BackingField", "Offsets", false),
                Field("GPC.Model.Structure.Bridges.BridgeGirderLayout", "<Levels>k__BackingField", "Levels", false));
            Register("GPC.Model.Structure.Bridges.BridgeDeckLayout", "GPC.Model.Structure.Bridges.BridgeDeckLayout",
                Field("GPC.Model.Structure.Bridges.BridgeDeckLayout", "<Widths>k__BackingField", "Widths", false),
                Field("GPC.Model.Structure.Bridges.BridgeDeckLayout", "<Thickness>k__BackingField", "Thickness", false),
                Field("GPC.Model.Structure.Bridges.BridgeDeckLayout", "<SlopeStations>k__BackingField", "SlopeStations", false),
                Field("GPC.Model.Structure.Bridges.BridgeDeckLayout", "<Slopes>k__BackingField", "Slopes", false),
                Field("GPC.Model.Structure.Bridges.BridgeDeckLayout", "<BeamLevel>k__BackingField", "BeamLevel", false));
            Register("GPC.Model.Structure.Bridges.BridgeMeshSettings", "GPC.Model.Structure.Bridges.BridgeMeshSettings",
                Field("GPC.Model.Structure.Bridges.BridgeMeshSettings", "<MaximumLength>k__BackingField", "MaximumLength", false),
                Field("GPC.Model.Structure.Bridges.BridgeMeshSettings", "<ChordTolerance>k__BackingField", "ChordTolerance", false),
                Field("GPC.Model.Structure.Bridges.BridgeMeshSettings", "<SlabSpacing>k__BackingField", "SlabSpacing", false),
                Field("GPC.Model.Structure.Bridges.BridgeMeshSettings", "<SlabStations>k__BackingField", "SlabStations", false));
            Register("GPC.Model.Structure.Bridges.BridgeDiaphragmLayout", "GPC.Model.Structure.Bridges.BridgeDiaphragmLayout",
                Field("GPC.Model.Structure.Bridges.BridgeDiaphragmLayout", "<MaximumSpacing>k__BackingField", "MaximumSpacing", false),
                Field("GPC.Model.Structure.Bridges.BridgeDiaphragmLayout", "<Stations>k__BackingField", "Stations", false),
                Field("GPC.Model.Structure.Bridges.BridgeDiaphragmLayout", "<AtSupports>k__BackingField", "AtSupports", false),
                Field("GPC.Model.Structure.Bridges.BridgeDiaphragmLayout", "<TrussHeight>k__BackingField", "TrussHeight", false),
                Field("GPC.Model.Structure.Bridges.BridgeDiaphragmLayout", "<PanelsPerBay>k__BackingField", "PanelsPerBay", false),
                Field("GPC.Model.Structure.Bridges.BridgeDiaphragmLayout", "<Pattern>k__BackingField", "Pattern", false),
                Field("GPC.Model.Structure.Bridges.BridgeDiaphragmLayout", "<ConnectCrossings>k__BackingField", "ConnectCrossings", false),
                Field("GPC.Model.Structure.Bridges.BridgeDiaphragmLayout", "<Truss>k__BackingField", "Truss", false));
            Register("GPC.Model.Structure.Bridges.BridgeSegmentSettings", "GPC.Model.Structure.Bridges.BridgeSegmentSettings",
                Field("GPC.Model.Structure.Bridges.BridgeSegmentSettings", "<MaximumLength>k__BackingField", "MaximumLength", false),
                Field("GPC.Model.Structure.Bridges.BridgeSegmentSettings", "<Joints>k__BackingField", "Joints", false));
            Register("GPC.Model.Structure.Bridges.BridgeSegmentRange", "GPC.Model.Structure.Bridges.BridgeSegmentRange",
                Field("GPC.Model.Structure.Bridges.BridgeSegmentRange", "<Girder>k__BackingField", "Girder", false),
                Field("GPC.Model.Structure.Bridges.BridgeSegmentRange", "<Segment>k__BackingField", "Segment", false),
                Field("GPC.Model.Structure.Bridges.BridgeSegmentRange", "<Start>k__BackingField", "Start", false),
                Field("GPC.Model.Structure.Bridges.BridgeSegmentRange", "<End>k__BackingField", "End", false));
            Register("GPC.Model.Structure.Bridges.BridgeFabricationPlan", "GPC.Model.Structure.Bridges.BridgeFabricationPlan",
                Field("GPC.Model.Structure.Bridges.BridgeFabricationPlan", "<Length>k__BackingField", "Length", false),
                Field("GPC.Model.Structure.Bridges.BridgeFabricationPlan", "<GirderCount>k__BackingField", "GirderCount", false),
                Field("GPC.Model.Structure.Bridges.BridgeFabricationPlan", "<Settings>k__BackingField", "Settings", false),
                Field("GPC.Model.Structure.Bridges.BridgeFabricationPlan", "<Ranges>k__BackingField", "Ranges", false));
            Register("GPC.Model.Structure.Bridges.BridgeTrussPattern", "GPC.Model.Structure.Bridges.BridgeTrussPattern", Field("GPC.Model.Structure.Bridges.BridgeTrussPattern", "value__", "value__", false));
        }
    }
}
