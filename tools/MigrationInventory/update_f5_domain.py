"""Reviewed F5 domain/export deltas. Does not claim completion of the remaining F2 matrix."""
import json
from pathlib import Path

folder = Path('docs/bridghe-arachne')
inventory = json.loads((folder / 'f2-inventario.json').read_text(encoding='utf-8-sig'))
path = folder / 'f5-destinazioni-parziali.json'
overlay = json.loads(path.read_text(encoding='utf-8-sig'))
rows = {(r['sourceType'], r['member']): r for r in overlay['fields']}
routes = {}

def route(source, fields, evidence, units):
    routes[source] = {name: (destination, evidence, units) for name, destination in fields.items()}

bridge_proof = 'BridgeDefinitionContractsTest: net6/net472, copy, XML archive, curved alignment and fabrication'
bridge_units = 'mm; skew rad (source degrees); slope dimensionless (source percent /100); stable definition GUID'
prefix = 'BridGHe.Common.Geometry.'
for cls, fields in {
    'BridgeDefinition': dict(Alignment='Alignment', Supports='Supports', Girders='Girders', Deck='Deck', Fabrication='Fabrication', Diaphragms='Diaphragms'),
    'BridgeSupportLayout': dict(Stations='Stations', Skew='SkewRadians'),
    'BridgeGirderLayout': dict(Offsets='Offsets', Levels='Levels'),
    'BridgeDeckLayout': dict(Widths='Widths', Thickness='Thickness', SlopeStations='SlopeStations', Slopes='Slopes', BeamLevel='BeamLevel'),
    'BridgeMeshSettings': dict(MaximumLength='MaximumLength', ChordTolerance='ChordTolerance', SlabSpacing='SlabSpacing', SlabStations='SlabStations'),
    'BridgeFabricationPlan': dict(FabricationSegments='Ranges', Length='Length', GirderCount='GirderCount', Settings='Settings', Ranges='Ranges'),
    'BridgeSegmentRange': dict(Girder='Girder', Segment='Segment', Start='Start', End='End'),
}.items():
    route(prefix + cls, {name: 'GPC.Model.Structure.Bridges.' + cls + '.' + dest for name, dest in fields.items()}, bridge_proof, bridge_units)
route(prefix + 'BridgeSegments', {n: 'BridgeSegmentSettings.' + n for n in ['MaximumLength', 'Joints']}, bridge_proof, bridge_units)
route(prefix + 'BridgeDiaphragms', {n: 'BridgeDiaphragmLayout.' + n for n in ['MaximumSpacing', 'Stations', 'AtSupports', 'TrussHeight', 'PanelsPerBay', 'Pattern', 'ConnectCrossings']}, bridge_proof, bridge_units)
route(prefix + 'BridgeDiaphragmLayout', {'Settings': 'BridgeDiaphragmLayout (flattened settings)', 'Truss': 'BridgeDiaphragmLayout.Truss'}, bridge_proof, bridge_units)
route(prefix + 'SimpleBridgeInput', {
    'Alignment': 'BridgeDefinition.Alignment', 'Supports': 'BridgeDefinition.Supports.Stations',
    'GirderOffsets': 'BridgeDefinition.Girders.Offsets', 'GirderLevels': 'BridgeDefinition.Girders.Levels',
    'DeckWidths': 'BridgeDefinition.Deck.Widths', 'DeckThickness': 'BridgeDefinition.Deck.Thickness',
    'CrossfallStations': 'BridgeDefinition.Deck.SlopeStations', 'CrossfallPercent': 'BridgeDefinition.Deck.Slopes (/100)',
    'SupportSkewDegrees': 'BridgeDefinition.Supports.SkewRadians (*pi/180)',
    'Segments': 'BridgeFabricationPlan.Create', 'Diaphragms': 'BridgeDefinition.Diaphragms',
    'MaximumElementLength': 'BridgeDefinition.Mesh.MaximumLength', 'ChordTolerance': 'BridgeDefinition.Mesh.ChordTolerance',
    'DeckBeamSpacing': 'BridgeDefinition.Mesh.SlabSpacing', 'DeckBeamStations': 'BridgeDefinition.Mesh.SlabStations',
    'DeckBeamLevel': 'BridgeDefinition.Deck.BeamLevel', 'Truss': 'BridgeDefinition.Diaphragms.Truss'}, bridge_proof, bridge_units)
route(prefix + 'BridgeTrussPattern', {n: 'BridgeTrussPattern.' + n for n in ['X', 'V', 'Warren']}, bridge_proof, 'enum; no unit')

moving_proof = 'MovingLoadContractsTest; Straus7MovingWriteTest; native readback, expanded libraries and generated-result archive'
moving_units = 'N, mm, s; force/length N/mm; heat power N mm/s (native J/s /1000); centrifugal expression units explicit'
moving = 'BridGHe.Common.MovingLoads.'
for cls, fields in {
    'MovingPointForce': dict(Mobility='Mobility', Axis='Axes', Adjacent='ApplyAdjacencyFactor', Centrifugal='ApplyCentrifugal', X='X', Y='Y', Force='Force'),
    'MovingDistributedForce': dict(Mobility='Mobility', Axis='Axes', Adjacent='ApplyAdjacencyFactor', Centrifugal='ApplyCentrifugal', X1='X1', X2='X2', Y1='Y1', Y2='Y2', Force='ForcePerLength'),
    'MovingHeatSource': dict(X='X', Y='Y', SizeX='SizeX', SizeY='SizeY', Heat='Power'),
    'MovingVehicle': dict(Name='Name', Instance='Placement', Direction='Direction', Velocity='Speed', StartTime='StartTime', VehicleSet='ExclusiveSet', Lanes='AllowedLanes', Loads='PointForces + DistributedForces + HeatSources'),
}.items():
    route(moving + cls, {name: 'GPC.Model.Actions.Moving.' + cls + '.' + dest for name, dest in fields.items()}, moving_proof, moving_units)
route(moving + 'MovingCentrifugal', {
    'K0': 'MovingCentrifugalLaw.OffsetExpression', 'K1': 'MovingCentrifugalLaw.FactorExpression',
    'LimitK1': 'MovingCentrifugalLaw.LimitFactor', 'LengthUnit': 'MovingCentrifugalLaw.LengthUnit (explicit native code -> unit symbol)',
    'ForceUnit': 'MovingCentrifugalLaw.ForceUnit (explicit native code -> unit symbol)',
    'MinimumK1': 'MovingCentrifugalLaw.MinimumFactor', 'MaximumK1': 'MovingCentrifugalLaw.MaximumFactor'}, moving_proof, moving_units)
route(moving + 'MovingTemplate', {
    'Name': 'MovingLoadTemplate.Name', 'Vehicles': 'MovingLoadTemplate.Vehicles', 'LaneFactors': 'MovingLoadTemplate.LaneFactors',
    'MultiLaneType': 'MovingLoadTemplate.LaneFactorRule', 'MinimumLaneWidth': 'MovingLoadTemplate.MinimumLaneWidth', 'Centrifugal': 'MovingLoadTemplate.Centrifugal',
    'TransitionLoad': 'Straus7MovingPathOptions.TransitionTrailingLoad', 'BeamLateralTolerance': 'Straus7MovingPathOptions.BeamLateralTolerance',
    'RelativeTolerance': 'Straus7MovingPathOptions.RelativeTolerance', 'Colour': 'Straus7MovingPathOptions.Colour',
    'LibraryName': 'Straus7MovingLibrary.Resolve -> expanded MovingLoadTemplate + provenance LibraryName',
    'LibraryItem': 'Straus7MovingLibrary.Resolve -> expanded MovingLoadTemplate + provenance ItemName'}, moving_proof, moving_units)
route(moving + 'MovingPath', {
    'LoadCaseName': 'MovingLoadPath.CaseGuid (explicit name -> registered GUID)', 'Template': 'MovingLoadPath.Template', 'Shape': 'MovingLoadPath.Shape',
    'Surface': 'MovingLoadPath.Surface', 'Target': 'MovingLoadPath.Target', 'Divisions': 'MovingLoadPath.Divisions',
    'EntitySetId': 'MovingLoadPath.TargetElementGuids; native set resolved to identities, never reused as domain ID',
    'Coordinates': 'MovingLoadPath.Start + End + Reference', 'ConvertToStaticCases': 'Straus7MovingPathOptions.ConvertToStaticCases'}, moving_proof, moving_units)

for plugin in ['BridGHe', 'Arachne']:
    route(plugin + '.Common.Models.MaterialModel', {
        'ShearModulus': 'Material.AnalysisData.BeamShearModulus (only when UsePoisson=false)',
        'UsePoisson': 'Material.AnalysisData.BeamShearModulus null -> G derived from E and nu',
        'ViscousDamping': 'Material.AnalysisData.ViscousDampingDensity', 'DampingRatio': 'Material.AnalysisData.ModalDampingRatio',
        'SpecificHeat': 'Material.AnalysisData.SpecificHeat', 'Conductivity': 'Material.AnalysisData.ThermalConductivity',
        'StressStrainTable': 'Material.AnalysisData.StressStrainPairs; explicit analysis law, not design-strength tables',
        'Mode': 'Material.AnalysisData.NonlinearResponse', 'Hardening': 'Material.AnalysisData.Hardening'},
        'ElasticExportContractsTest.MaterialAnalysisProperties; Straus7ModelWriteTest.NativeMaterialLaw',
        'G MPa; viscous damping N s/mm^4; damping ratio dimensionless; conductivity N/(s C); heat capacity N mm/(t C); strain dimensionless, stress MPa. Legacy exporter ignored Mode/Hardening: F6 must explicitly preserve effective legacy law and report conflicting requested settings.')

# Additional reviewed physical/geometry deltas, verified by native mechanical tests.
route(prefix + 'BridgeSectionRule', {n: 'GPC.Model.Structure.Bridges.BridgeSectionRule.' + n for n in ['Family','Group','Segment','Start','End','Property','ScaleFrom','ScaleTo']},
    'BridgeDefinitionContractsTest.SectionRulesKeepIndependentPropertiesSelectorsAndDimensionalScales', 'Stations mm; scale dimensionless; group/segment zero wildcard; property shared by identity within each independent copy')
route(prefix + 'BridgeFamilyProperty', {'Family':'BridgeFamilyProperty.Family (source -1 -> null all-family fallback)','Property':'BridgeFamilyProperty.Property'},
    'BridgeDefinitionContractsTest.SectionRulesKeepIndependentPropertiesSelectorsAndDimensionalScales', 'Family enum, nullable fallback; dimensional elastic property')
for plugin in ['BridGHe','Arachne']:
    route(plugin + '.Common.Models.BeamPropertyModel', {'MaxTension':'ElasticBeamProperty.CutoffLaw.TensionLimit','MaxCompression':'ElasticBeamProperty.CutoffLaw.CompressionLimit',
        'CutoffType':'BeamCutoffLaw.Failure','KeepMass':'BeamCutoffLaw.KeepMass'},
        'Straus7ModelWriteTest.NativeCutoffLawReproducesBrittleFailureAndDuctileForceCap', 'N, nonnegative tensile/compressive force magnitudes; mass retention boolean; explicit CutoffBar formulation')
    route(plugin + '.Common.Models.SectionModel', {'ShearL1':'ISectionShape.ShearCenter minus Centroid, projected onto principal axis 1','ShearL2':'ISectionShape.ShearCenter minus Centroid, projected onto principal axis 2',
        'IntegrationPoints':'Straus7ModelWritePlan.Prepare beamIntegrationSlices by stable beam GUID'},
        'Straus7ModelWriteTest.NativeUnsymmetricSectionRetainsShearCenterAndTorsionalResponse; NativeTaperedBarMatchesIntegratedAxialCompliance', 'Shear-center offsets mm; lengthwise integration count numerical exporter option, not physical material state')
route('BridGHe.Common.Models.PlatePressureModel+LoadData', {'Face':'ShellSurfacePressure.Face','CoordinateSystem':'ShellSurfacePressure.GlobalPressure transformed to global; null selects explicit NormalPressure',
    'P':'ShellSurfacePressure.GlobalPressure or NormalPressure','Projected':'ShellSurfacePressure.Projection: false=None, true=Resultant (native ppProjResultant=1)'},
    'Straus7ModelWriteTest.NativeSurfacePressuresPreserveFacesProjectionAndForceEquilibrium', 'N/mm2; normal pressure positive inward per selected face; global components fixed. Legacy normal writer passed an undersized one-double array to a two-double API: F6 must diagnose this defect, not reproduce an out-of-bounds read.')
route('BridGHe.Common.Models.RigidConnectionAttributeModel', {'Value':'Model.Costrains / RigidLink with resolved node identities'},
    'Straus7ModelWriteTest.NativeRigidLinkTransfersEccentricLoadAndPreservesRigidBodyKinematics', 'Rigid translations and rotations; lever arms mm; resolve source target point against tolerance before creating node links; preserve actual stored equations')

for declaration in inventory['types']:
    selected = routes.get(declaration['type'], {})
    for member in declaration['members']:
        name = member['name']
        # Backing fields are the same source state as their public properties, not extra independent quantities.
        public = name[1:2].upper() + name[2:] if name.startswith('_') else name
        if public not in selected:
            continue
        destination, evidence, units = selected[public]
        rows[(declaration['type'], name)] = dict(source=declaration['source'], line=member['line'], sourceType=declaration['type'], member=name,
            destination=destination, units=units, evidence=evidence, status='contract-and-execution-verified; production-adapter-pending-F6')

overlay['fields'] = sorted(rows.values(), key=lambda r: (r['sourceType'], r['line']))
overlay['reviewedFieldCount'] = len(rows)
overlay['notReviewedInThisOverlay'] = overlay['baselineFieldCount'] - len(rows)
overlay['limits'] = [
    'F5 domain/export deltas verified; no production component migration is claimed.',
    'Remaining F2 rows need semantic review and implementation where required. This overlay does not close F5.',
    'Centrifugal equations retain the explicit Straus7-R3 dialect; no cross-solver expression equivalence is inferred.',
    'Native thermal and damping properties are read back; no full thermal/dynamic analysis equivalence is claimed.',
    'Staged normalized results use small-displacement kinematics; native generated cases remain discrete samples, not a continuous influence envelope.'
]
path.write_text(json.dumps(overlay, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(f"{len(rows)}/{overlay['baselineFieldCount']} explicit F5 delta rows; full-matrix gate remains open.")
