"""Build the field-by-field F5 review without treating a proposed F2 destination as implemented.

Every unmatched field remains OPEN. --check fails while any such row exists. No input
inventory, API baseline or fixture is rewritten. Rules are explicit and reviewable here.
"""
import argparse
import collections
import hashlib
import json
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[2]
DOC = ROOT / 'docs/bridghe-arachne'
inventory = json.loads((DOC / 'f2-inventario.json').read_text(encoding='utf-8-sig'))
baseline = json.loads((DOC / 'f2-destinazioni.json').read_text(encoding='utf-8-sig'))
overlay = json.loads((DOC / 'f5-destinazioni-parziali.json').read_text(encoding='utf-8-sig'))
previous = {(r['sourceType'], r['member']): r for r in baseline['fields']}
verified = {(r['sourceType'], r['member']): r for r in overlay['fields']}
routes = {}


def route(type_name, fields, destination, units, evidence, disposition='domain-contract'):
    """A route explicitly lists all source fields it reviews; no wildcard completion."""
    names = fields.split() if isinstance(fields, str) else fields
    for name in names:
        routes[(type_name, name)] = dict(destination=destination, units=units,
                                       evidence=evidence, disposition=disposition)


def common(type_name, fields, destination, units, evidence, disposition='domain-contract'):
    for plugin in ('BridGHe', 'Arachne'):
        route(plugin + '.Common.Models.' + type_name, fields, destination, units, evidence, disposition)


core_proof = 'tests/MigrationCopyContractsTest.cs; UnitTest/Straus7ModelWriteTest.cs'
physical_proof = 'tests/PhysicalAssignmentsTest.cs; UnitTest/Straus7PhysicalWriteTest.cs'
result_proof = 'UnitTest/Straus7ModelWriteTest.cs; UnitTest/ResultArchiveTest.cs'
common('NodeModel', 'NodeId _nodeId Position _position', 'NodeElement: persistent Guid, assembly Id and Position; source string ID retained in import identity map', 'Position mm; string source ID is not a solver integer', core_proof)
common('CaseModel', 'Name _name', 'LoadCaseBase.Name; case Guid is persistent', 'text', core_proof)
common('LoadCaseModel', 'Id _id', 'LoadCaseBase.Guid + source-ID import map', 'source string ID; no numeric coercion', core_proof)
common('GroupModel', 'Name _name Parent _parent FullName', 'Group.Name, Parent; FullName is derived from the hierarchy', 'text; persistent group Guid', core_proof)
common('CoordinateSystemModel', 'Id _id Name _name Origin _origin Point1 _point1 Point2 _point2 Global', 'GPC.Geometry.CoordinateSystem: Origin and orthonormal V1/V2/V3; source labels remain adapter metadata', 'mm; unit vectors; reject coincident/collinear defining points', core_proof)
common('BeamModel', 'BeamId _beamId PointFrom _pointFrom PointTo _pointTo BeamProperty _beamProperty AngleDeg _angleDeg Offset _offset Length IsVertical', 'BeamElement + BeamAssignments.SectionAxes/OffsetI/OffsetJ; physical nodes are shared only after explicit assembly', 'mm; source angle degrees -> explicit frame; length/direction derived from nodes', core_proof)
common('PlateModel', 'PlateId _plateId Points _points AngleDeg _angleDeg Offset _offset PlateProperty _plateProperty', 'AreaElement connectivity; ShellGeometry preserves warped quad; ShellAssignments axes/offset and ElasticPlateProperty', 'mm; source angle degrees -> explicit section frame; topology has explicit 3/4 vertices', core_proof)
common('ElementModel', 'Groups _groups Loads _loads Attributes _attributes Results _results', 'Element groups, typed loads/assignments, AnalysisDataset result stores; generic legacy lists are dispatched by concrete type in F6', 'heterogeneous typed state; no CLR object bag in GPC', core_proof)
common('BeamPropertyBaseModel', 'Id _id Name _name Material _material', 'ElasticBeamProperty (Guid/Name/Material), material identity scoped to copied graph', 'source ID mapped explicitly; material dimensions in N-mm-t-s', 'tests/ElasticExportContractsTest.cs')
common('BeamPropertyModel', 'Section _section', 'ElasticBeamProperty.Section / SteelSection.SectionShape', 'dimensional ISectionShape; analysis modifiers remain separate', 'tests/ElasticExportContractsTest.cs; UnitTest/Straus7ModelWriteTest.cs')
common('LoadModel', 'LoadCase _loadCase', 'Load.LoadCase / typed physical assignment Case; registered case reference rebound on assembly', 'case Guid; no physical unit', physical_proof)
common('LoadModel`1', 'Value _value Type', 'Concrete typed GPC load/physical assignment; Type is derived from the descriptor, never loaded by CLR name', 'per concrete load contract', physical_proof)
common('AttributeModel`1', 'Value _value Type', 'Concrete GPC assignment; generic base has no independent physical meaning', 'per concrete assignment contract', physical_proof)
common('BeamPointLoadModel', 'PointLoadType _loadType', 'BeamLoadAssignment.Concentrated (PointLoad force/moment components)', 'N or N mm; explicit case and axis convention', 'UnitTest/Straus7ModelWriteTest.cs')
common('BeamPointLoadModel+LoadType', 'Force Moment', 'BeamLoadAssignment.Concentrated: PointLoad force/moment components', 'N / N mm', 'UnitTest/Straus7ModelWriteTest.cs')
common('BeamPointLoadModel+LoadData', 'CoordinateSystem P A', 'BeamLoadAssignment.Concentrated and Start=End=A; NodeToNode station domain', 'A is dimensionless distance ratio from node I; null frame is beam-local; force N or moment N mm', 'UnitTest/Straus7ModelWriteTest.cs; source BeamPointLoadComponent input descriptions')
common('ResultModel', 'LoadCase _loadCase StepNum _stepNum StepType _stepType', 'ResultState: case Guid, Step, semantics, cumulative/combined flags; AnalysisDataset provenance', 'native step number/label is not a new input load case', result_proof)
common('NodeReactResultModel', 'Force _force Moment _moment', 'NodeResult / NodalActions with NodalForceKind.Reaction', 'global force N, moment N mm; convention normalized at provider boundary', result_proof)
route('BridGHe.Common.Models.NodeDisplResultModel', 'Displacement _displacement Rotation _rotation', 'NodeResult displacement / NodalKinematics', 'mm, rad; explicit axes', result_proof)
common('NodeRestraintAttributeModel+AttributeData', 'Tx Ty Tz Rx Ry Rz CoordinateSystem', 'RestrainAssignment + NodeRestrain with explicit DOFs/frame and FreedomCase', 'boolean constrained DOFs; source coordinate-system geometry mm', core_proof)
route('BridGHe.Common.Models.NodeStiffnessAttributeModel+AttributeData', 'Tx Ty Tz Rx Ry Rz', 'DofRestrain.Stiffness within NodeRestrain.Restrains or NodalSpring diagonal matrix; scoped freedom case explicit', 'translation N/mm; rotation N mm/rad', core_proof)
route('BridGHe.Common.Models.BeamGradientLoadModel+LoadData', 'Axis1 Axis2', 'BeamTemperatureGradient.Axis1/Axis2', 'delta C/mm in explicit section axes', physical_proof)
common('BeamPreLoadModel', '_loadType PreLoadType', 'BeamInitialState.Kind', 'AxialForce N / AxialStrain dimensionless; positive tensile', physical_proof)
common('BeamPreLoadModel+LoadType', 'Tension Strain', 'BeamInitialStateKind.AxialForce / AxialStrain', 'N / dimensionless', physical_proof)
route('BridGHe.Common.Models.NodeTemperatureLoadModel', '_loadType', 'NodalTemperatureAssignment.Kind', 'Reference / Fixed->Prescribed / Initial; absolute C', physical_proof)
route('BridGHe.Common.Models.NodeTemperatureLoadModel+LoadType', 'Reference Fixed Initial', 'NodalTemperatureKind.Reference / Prescribed / Initial', 'absolute C; legacy exporter failed to set the role; migration must report that difference', physical_proof)
route('BridGHe.Common.Models.NodeDisplacementLoadModel+LoadData', 'Translation Rotation', 'RestrainAssignment.Case + DofRestrain.Prescribe; exporter freedom/load-case factor association still required', 'mm / rad; retained prescribed-value components, not load-force components', 'Model/Actions/Restraints/DofRestrain.cs; native writer currently rejects a restraint with Case', 'implementation-required')
common('BeamDistributedLoadModel', '_loadType DistributedLoadType', 'BeamLoadAssignment.StartIntensity/EndIntensity LineLoad force or moment components', 'N/mm or N mm/mm', 'UnitTest/Straus7ModelWriteTest.cs')
common('BeamDistributedLoadModel+LoadType', 'Undefined Force Moment', 'Force/moment LineLoad components; Undefined rejected before conversion', 'N/mm or N mm/mm', 'UnitTest/Straus7ModelWriteTest.cs')
common('BeamDistributedLoadModel+LoadData', 'LoadSchema CoordinateSystem LoadDirection P1 PA PB P2 A B', 'Piecewise BeamLoadAssignment sequence: breakpoints 0,A,1-B,1 and explicit endpoint intensities; zero-length inactive pieces omitted', 'A/B dimensionless from I/J respectively; intensities N/mm or N mm/mm; frames explicit', 'UnitTest/Straus7ModelWriteTest.cs; source BeamDistributedLoadComponent distance-ratio descriptions')
common('BeamDistributedLoadModel+LoadSchema', 'Undefined Uniform Keystone Triangular TriangularEnd1_Keystone Keystone_TriangularEnd2 TriangularEnds_Keystone', 'Piecewise linear BeamLoadAssignment sequence preserving all active end intensities; Undefined rejected', 'dimensionless schema; no resampling of a nonlinear law', 'UnitTest/Straus7ModelWriteTest.cs')
common('BeamDistributedLoadModel+LoadDirection', 'Undefined X Y Z X_Projected Y_Projected Z_Projected Gravity Gravity_Projected', 'LineLoad frame/components plus BeamLoadLengthConvention and ProjectionPlaneNormal; gravity uses explicit global direction', 'named axis mapping; projected intensity per projected length; Undefined rejected', 'UnitTest/Straus7ModelWriteTest.cs')
common('BeamTaperAttributeModel+AlignmentCodes', 'Top Center Bottom', 'BeamSectionTaper.Alignment1/2: signed fixed side / Symmetric, resolved against source section axes', 'dimensionless; explicit axis orientation required', physical_proof)
route('BridGHe.Common.Models.LoadCaseModel', 'StructuralMassAcceleration NonStructuralMassAcceleration', 'LoadCaseBase.InertiaMasses.Structural / NonStructural', 'boolean selection; stored masses unchanged; all four combinations supported', 'tests/PhysicalAssignmentsTest.cs; UnitTest/Straus7ModelWriteTest.NativeGravityMassFamiliesMatchIndependentEquilibrium')
route('BridGHe.Common.Models.LoadCaseModel', 'GravityValue GravityDirection', 'ModelGravityLoad.Acceleration / GravityVersor with global CoordinateSystem', 'mm/s2; signed axis, X/Y/Z by name', 'UnitTest/Straus7ModelWriteTest.NativeGravityMassFamiliesMatchIndependentEquilibrium')
route('BridGHe.Common.Models.LoadCaseModel+GravityDirections', 'X Y Z', 'ModelGravityLoad global unit vector', 'dimensionless direction', 'UnitTest/Straus7ModelWriteTest.NativeGravityMassFamiliesMatchIndependentEquilibrium')
route('BridGHe.Common.Models.BridgeMemberMetadata', 'PhysicalId Family Group Segment Span StationFrom StationTo SectionUp GeometryId', 'PhysicalMemberDefinition.BridgeMetadata / GPC BridgeMemberMetadata; StationFrom/To -> StationFromMm/ToMm', 'mm; SectionUp global geometric direction; IDs distinct from solver numbering', 'tests/MigrationCopyContractsTest.cs')
route('BridGHe.Common.Models.BeamForceResultModel', 'Station AxialForce ShearForce Vx Vy Torque BendingMoment Mx My NormalizedLength', 'BeamResult samples with BeamStation location; ResultBeamForces N,V1,V2,T,M1,M2; local axis/sign conversion at provider', 'N, N mm; normalized station converted to declared NodeToNode mm domain before comparison', result_proof)
common('PlatePropertyModel', 'Id Name PropertyType Material MembraneThickness BendingThickness', 'ElasticPlateProperty or MembranePlateProperty; explicit formulation and material; bending and membrane thickness independent', 'mm; membrane-per-width property uses explicit N/mm contract', 'tests/ElasticExportContractsTest.cs; UnitTest/Straus7ModelWriteTest.cs')

section_proof='Model/Sections; tests/ElasticExportContractsTest.GenericSectionRetainsDeclaredElasticModuliWithoutInventingAnOutline; UnitTest/Straus7ModelWriteTest'
for source_class in ('SectionModel','BeamPropertyModel'):
    for field,target,unit in (
        ('SectionArea','ISectionShape.Area','mm2'),('Ixx','ISectionShape.Jxx','mm4'),('Iyy','ISectionShape.Jyy','mm4'),
        ('Ixy','ISectionShape.Jxy','mm4; product sign follows section coordinates'),('I11','ISectionShape.J11','mm4'),('I22','ISectionShape.J22','mm4'),
        ('J','ISectionShape.Jt','mm4; torsional constant, not polar inertia'),('W11','ISectionShape.Wel1','mm3'),('W22','ISectionShape.Wel2','mm3'),
        ('Wxx','ISectionShape.WelX','mm3'),('Wyy','ISectionShape.WelY','mm3'),('AngleX1Rad','ISectionShape.AngleX1','rad'),
        ('Centroid','ISectionShape.Centroid','mm'),('ShearA1','ElasticBeamProperty.ShearArea1','mm2'),('ShearA2','ElasticBeamProperty.ShearArea2','mm2'),
        ('ShearL1','ISectionShape.ShearCenter - Centroid projected on principal axis 1','mm'),
        ('ShearL2','ISectionShape.ShearCenter - Centroid projected on principal axis 2','mm'),
        ('Rho11','sqrt(ISectionShape.J11 / Area), NOT GPC R11 (which uses J22)','mm'),
        ('Rho22','sqrt(ISectionShape.J22 / Area), NOT GPC R22 (which uses J11)','mm'),
    ):
        common(source_class,field,target+'; for geometric sections recompute derived caches; generic input retains explicit prescribed properties',unit,section_proof)
    common(source_class,'IntegrationPoints','Straus7ModelWritePlan beamIntegrationSlices keyed by beam Guid','integer 2..10; numerical exporter option, not physical state',section_proof)
    common(source_class,'IncludeTorsionalStiffness','BeamAnalysisProfile.Modifiers.Torsion: 0 suppresses analysis GJ while physical Jt stays available','boolean -> dimensionless multiplier', 'Model/Analysis/BeamAnalysisProfile.cs; UnitTest/Straus7ModelWriteTest.cs')
    common(source_class,'D B B1 B2 L T T1 T2 T3 R SectionType','Typed GPC section dimensions or explicit polygon regions; source family selects the constructor, never integer-cast a section type','mm; dimensional geometry retained independently from equivalent analysis rigidities',section_proof)
    common(source_class,'Mirror MirrorGapA MirrorGapB','SectionBuiltUp.Part with explicit signed placement and MirrorX/MirrorY; BXS exports expanded regions','gaps mm; legacy writer always used mtLeft for any nonzero Mirror: F6 must report requested/effective discrepancy',section_proof)
    common(source_class,'GenericShapes','Section(shape) or SectionBuiltUp.FromRegions for multiple regions; no Rhino BRep in the domain','Shape2d polygon fill/holes in mm; BXS mesh keeps holes',section_proof)
    common(source_class,'IsDBSection Code SectionCode','Catalog provenance / computed display label in plugin adapter; physical section is the expanded dimensional value','text and flag, not an independent rigidity', 'Source SectionModel.Code and native-library reader; PIANO.md adapter scope','retain-plugin')
common('SectionModel','Id Name','Section Guid/Name plus source-ID import map','text; source catalog ID is not a solver property number',section_proof)
common('SectionModel+SectionTypes','Undefined SolidCircle HollowCircle SolidRectangle HollowRectangle I T C Omega Angle Z Generic GenericShapes Composite','Named dispatch to GPC section types, Section/SectionBuiltUp, or ReinforcedConcreteSection; Undefined diagnosed','enum; no ordinal casting',section_proof)
common('SectionModel+MirrorTypes','None Top Bottom Left Right LeftAndTop LeftAndBottom RightAndTop RightAndBottom LeftTopOnly LeftBottomOnly RightTopOnly RightBottomOnly','Explicit SectionBuiltUp.Part geometry; legacy mtLeft discrepancy must be reported before changing effective physics','axis reflection and signed placements; gaps mm',section_proof)
common('CutoffType','Undefined Brittle Ductile','BeamCutoffLaw.Failure; Undefined is invalid for a CutoffBar','enum; nonnegative force limits N', 'UnitTest/Straus7ModelWriteTest.NativeCutoffLawReproducesBrittleFailureAndDuctileForceCap')
common('BeamPropertyType','Undefined Cable Truss CutoffBar Beam','BeamAssignments.Formulation by named mapping: Unknown, Cable, Truss, CutoffBar, StraightTwoNode','enum; Unknown rejected by exporter', 'UnitTest/Straus7ModelWriteTest.cs')
route('BridGHe.Common.Models.PlatePropertyType','Undefined ShellThin ShellThick PlateThin PlateThick Membrane LoadPatch ShearPanel','ElasticPlateFormulation; legacy thin/thick and shell/plate labels all exported as native ptPlateShell; Membrane/LoadPatch/ShearPanel explicit','enum; preserve effective legacy formulation, diagnose requested thin/thick distinction', 'BridGHe StrausR3Helper plate-property switch; UnitTest/Straus7ModelWriteTest.cs')
route('BridGHe.Common.Models.BeamSupportAttributeModel+AttributeData','Stiffness Gap SimpleSpringType LocalAxisDirection','BeamDistributedSupport stiffness/gap/behavior/direction and explicit FreedomCase','N/mm2, mm; signed axes 1/2, tension-only reverses native compression direction',physical_proof)
route('BridGHe.Common.Models.BeamSupportAttributeModel+SimpleSpringTypes','TensionCompression CompressionOnly TensionOnly','DistributedSpringBehavior.Bilateral / CompressionOnly / TensionOnly','dimensionless; bilateral gap must be zero',physical_proof)
route('BridGHe.Common.Models.BeamSupportAttributeModel+LocalAxisDirection','negative3Axis negative2Axis negative1Axis positive3Axis positive2Axis positive1Axis','BeamDistributedSupport.Direction in explicit section frame; native longitudinal support rejected as in source BeamSupportExport','signed unit vector; native supports +/-1 and +/-2',physical_proof)
route('BridGHe.Common.Models.BeamSupportAttributeModel+SpringType','Simple LinkProperty','Simple -> BeamDistributedSupport; LinkProperty is explicitly unsupported by BOTH source and new Straus writer','enum; no constitutive link law inferred', 'BridGHe/src/BridGHe.Common/Straus/BeamSupportExport.cs','reviewed-legacy-limit')
route('BridGHe.Common.Models.BeamSupportAttributeModel+SpringTensionOrientation','BeamLocalAxis BeamObject12Plane CustomVector','BeamLocalAxis -> signed section direction; other modes rejected by source BeamSupportExport, preserved as unsupported input in migration','enum; no approximation to nearest axis', 'BridGHe/src/BridGHe.Common/Straus/BeamSupportExport.cs','reviewed-legacy-limit')

# Explicit getters are aliases, not additional physical values. Only an exact getter ->
# field binding is followed; an arbitrary private field is never marked by name similarity.
def propagate_backing_fields():
    for declaration in inventory['types']:
        code=(ROOT.parent/declaration['source']).read_text(encoding='utf-8-sig')
        names={m['name'] for m in declaration['members']}
        for public, backing in re.findall(r'\bpublic\s+[\w.<>,?\[\]]+\s+(\w+)\s*\{\s*get\s*=>\s*(_\w+)\s*;', code):
            key=(declaration['type'],public)
            if public in names and backing in names and key in routes:
                routes[(declaration['type'],backing)]=dict(routes[key], aliasOf=public)

# Rhino algorithms, transient editing/search state and preview products remain in the plugin.
# List source fields explicitly: a future physical field cannot silently inherit this disposition.
geometry = 'BridGHe.Common.Geometry.'
for name, fields in {
    'SimpleBridgeBuilder': 'JoinTolerance MaximumStations',
    'SimpleBridgeBuilder+Builder': 'p alignment length levels widths skews slopeStations slopes frames output nodeBuckets edges counters joints supports',
    'BridgeGeometryEditor': 'Geometry cells memberIndex Tol',
    'BridgeSegmentSections': 'Header StationTolerance Inv',
    'BridgeSegmentSections+Row': 'Id Start End Dimensions',
    'BridgeSegmentTableUpdate': 'Table Changes',
    'BridgeSpanAlternative': 'Stations Points Lengths Score',
    'BridgeSpanAdvice': 'Alternatives CandidateCount Diagnostics',
}.items():
    route(geometry + name, fields, 'Plugin geometric algorithm / editing or recommendation result; replace contained legacy domain references by the mapped GPC values in F6',
          'geometry mm; tolerances mm; algorithm scores dimensionless; table culture invariant',
          'PIANO.md F5/F6 separation; original source listed on each row', 'retain-plugin-algorithm')

bridge_proof = 'tests/BridgeDefinitionContractsTest.cs (net6 and net472)'
route(geometry+'SimpleBridgeGeometry', 'FabricationSegments Members Nodes Supports Joints Segments Frames Stations Deck DeckThickness LeftEdge RightEdge',
      'GPC.Model.Structure.Bridges.BridgeGeometry; FabricationSegments aliases Segments', 'mm; frames orthonormal; deck mesh retains non-planar vertices', bridge_proof)
route(geometry+'SimpleBridgeGeometry', 'Diagnostics', 'Plugin runtime diagnostic messages; not structural input state', 'text', 'PIANO.md F6', 'retain-plugin')
route(geometry+'BridgeGeometryMember', 'Id Family Group Segment Span NodeFrom NodeTo StationFrom StationTo PhysicalId Axis',
      'BridgeAxisMember; Guid added persistently; PhysicalId derived, Axis resolved by BridgeGeometry.Axis',
      'mm; node indices 1-based within sampled geometry, not solver IDs', bridge_proof)
route(geometry+'BridgeSupportPoint', 'Support Girder Station Point', 'GPC.Model.Structure.Bridges.BridgeSupportPoint', 'positive layout indices; station and point mm', bridge_proof)
route(geometry+'BridgeJointPoint', 'Girder Joint Station Point', 'GPC.Model.Structure.Bridges.BridgeJointPoint', 'positive layout indices; station and point mm', bridge_proof)
route(geometry+'BridgeMemberFamily', 'Girder SolidDiaphragm TopChord BottomChord Post Diagonal LongitudinalBracing BracingPost DeckTransverse',
      'GPC.Model.Structure.Members.BridgeMemberFamily by named value', 'enum; no ordinal inference', bridge_proof)


def build():
    propagate_backing_fields()
    rows=[]
    for declaration in inventory['types']:
        for member in declaration['members']:
            key=(declaration['type'],member['name']); old=previous[key]
            row=dict(old)
            row['reviewStatus']='OPEN'
            row['disposition']='unresolved'
            if old['status'] in ('retain-solver','retain-plugin','retain-compatibility-F6'):
                row.update(reviewStatus='reviewed-retained',disposition=old['status'],evidence='PIANO.md scope + source declaration; production replacement remains F6')
            if key in verified:
                delta=verified[key]
                row.update(destination=delta['destination'],units=delta['units'],evidence=delta['evidence'],reviewStatus='reviewed-domain',disposition='domain-contract')
            if key in routes:
                row.update(routes[key]);row['reviewStatus']='OPEN' if row['disposition']=='implementation-required' else 'reviewed'
            # Source content is pinned, not inferred from a possibly shifted line number.
            source=ROOT.parent/declaration['source']
            actual=hashlib.sha256(source.read_bytes()).hexdigest().upper()
            row['currentSourceSha256']=actual
            row['sourceChangedSinceF2']=actual!=old['sourceSha256'].upper()
            rows.append(row)
    counts=collections.Counter(r['reviewStatus'] for r in rows)
    report=dict(schema=1,sourceInventory='f2-inventario.json',typeCount=len(inventory['types']),fieldCount=len(rows),
                status='F5-OPEN' if counts['OPEN'] else 'field-review-complete; execution-gates-separate',
                counts=dict(counts),fields=rows)
    return report


if __name__=='__main__':
    parser=argparse.ArgumentParser();parser.add_argument('--check',action='store_true');args=parser.parse_args()
    report=build();output=DOC/'f5-matrice-revisione.json'
    output.write_text(json.dumps(report,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    print(json.dumps(report['counts']))
    pending=collections.defaultdict(list)
    for row in report['fields']:
        if row['reviewStatus']=='OPEN':pending[row['sourceType']].append(row['member'])
    (DOC/'f5-campi-da-risolvere.json').write_text(json.dumps(pending,ensure_ascii=False,indent=2)+'\n',encoding='utf-8')
    if args.check and pending:raise SystemExit(1)
