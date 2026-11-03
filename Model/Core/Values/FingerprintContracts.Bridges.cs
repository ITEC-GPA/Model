namespace GPC.Model.Core
{
    internal static partial class FingerprintContracts
    {
        private static void RegisterBridgeContracts()
        {
            Register("GPC.Model.LoadCases.InertiaMassSelection", "GPC.Model.LoadCases.InertiaMassSelection",
                Field("GPC.Model.LoadCases.InertiaMassSelection", "<Structural>k__BackingField", "Structural", false),
                Field("GPC.Model.LoadCases.InertiaMassSelection", "<NonStructural>k__BackingField", "NonStructural", false));
            Register("GPC.Model.Structure.Bridges.BridgeGeometry", "GPC.Model.Structure.Bridges.BridgeGeometry",
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "schema", "schema", false),
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "<Id>k__BackingField", "Id", false),
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "<Nodes>k__BackingField", "Nodes", false),
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "<Members>k__BackingField", "Members", false),
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "<Supports>k__BackingField", "Supports", false),
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "<Joints>k__BackingField", "Joints", false),
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "<Segments>k__BackingField", "Segments", false),
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "<Frames>k__BackingField", "Frames", false),
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "<Stations>k__BackingField", "Stations", false),
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "<Deck>k__BackingField", "Deck", false),
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "<DeckThickness>k__BackingField", "DeckThickness", false),
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "<LeftEdge>k__BackingField", "LeftEdge", false),
                Field("GPC.Model.Structure.Bridges.BridgeGeometry", "<RightEdge>k__BackingField", "RightEdge", false));
            Register("GPC.Model.Structure.Bridges.BridgeAxisMember", "GPC.Model.Structure.Bridges.BridgeAxisMember",
                Field("GPC.Model.Structure.Bridges.BridgeAxisMember", "<Guid>k__BackingField", "Guid", false),
                Field("GPC.Model.Structure.Bridges.BridgeAxisMember", "<Id>k__BackingField", "Id", false),
                Field("GPC.Model.Structure.Bridges.BridgeAxisMember", "<Family>k__BackingField", "Family", false),
                Field("GPC.Model.Structure.Bridges.BridgeAxisMember", "<Group>k__BackingField", "Group", false),
                Field("GPC.Model.Structure.Bridges.BridgeAxisMember", "<Segment>k__BackingField", "Segment", false),
                Field("GPC.Model.Structure.Bridges.BridgeAxisMember", "<Span>k__BackingField", "Span", false),
                Field("GPC.Model.Structure.Bridges.BridgeAxisMember", "<NodeFrom>k__BackingField", "NodeFrom", false),
                Field("GPC.Model.Structure.Bridges.BridgeAxisMember", "<NodeTo>k__BackingField", "NodeTo", false),
                Field("GPC.Model.Structure.Bridges.BridgeAxisMember", "<StationFrom>k__BackingField", "StationFrom", false),
                Field("GPC.Model.Structure.Bridges.BridgeAxisMember", "<StationTo>k__BackingField", "StationTo", false));
            Register("GPC.Model.Structure.Bridges.BridgeSupportPoint", "GPC.Model.Structure.Bridges.BridgeSupportPoint",
                Field("GPC.Model.Structure.Bridges.BridgeSupportPoint", "<Support>k__BackingField", "Support", false),
                Field("GPC.Model.Structure.Bridges.BridgeSupportPoint", "<Girder>k__BackingField", "Girder", false),
                Field("GPC.Model.Structure.Bridges.BridgeSupportPoint", "<Station>k__BackingField", "Station", false),
                Field("GPC.Model.Structure.Bridges.BridgeSupportPoint", "<Point>k__BackingField", "Point", false));
            Register("GPC.Model.Structure.Bridges.BridgeJointPoint", "GPC.Model.Structure.Bridges.BridgeJointPoint",
                Field("GPC.Model.Structure.Bridges.BridgeJointPoint", "<Girder>k__BackingField", "Girder", false),
                Field("GPC.Model.Structure.Bridges.BridgeJointPoint", "<Joint>k__BackingField", "Joint", false),
                Field("GPC.Model.Structure.Bridges.BridgeJointPoint", "<Station>k__BackingField", "Station", false),
                Field("GPC.Model.Structure.Bridges.BridgeJointPoint", "<Point>k__BackingField", "Point", false));
            Register("GPC.Model.Structure.Bridges.BridgeDeckMesh", "GPC.Model.Structure.Bridges.BridgeDeckMesh",
                Field("GPC.Model.Structure.Bridges.BridgeDeckMesh", "<Vertices>k__BackingField", "Vertices", false),
                Field("GPC.Model.Structure.Bridges.BridgeDeckMesh", "<Faces>k__BackingField", "Faces", false));
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
