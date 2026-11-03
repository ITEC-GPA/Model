"""Update only the F5 rows supported by the model-export and workflow tests.
Unreviewed fields retain their explicit F2/F5 gaps; this does not close the matrix.
Run from the Model repository root.
"""
import json
from pathlib import Path

folder = Path('docs/bridghe-arachne')
inventory = json.loads((folder / 'f2-inventario.json').read_text(encoding='utf-8-sig'))
path = folder / 'f5-destinazioni-parziali.json'
overlay = json.loads(path.read_text(encoding='utf-8-sig'))
rows = {(r['sourceType'], r['member']): r for r in overlay['fields']}

routes = {
    'BridGHe.Common.Models.BeamStringGroupAttributeModel+AttributeData': {
        'ID': ('BeamPhysicalData.StringGroup; native positive IDs generated per export', 'persistent GUID; source string group number resolved once at adapter boundary')},
    'BridGHe.Common.Models.LoadPatchTypeAttributeModel+AttributeData': {
        'PatchType': ('LoadPatchDistribution.Kind', 'explicit enum; native ordering differs and is mapped'),
        'SelectedEdges': ('LoadPatchDistribution.SelectedEdges', 'one-based edges in connectivity order; native bit mask generated'),
        'Weights': ('LoadPatchDistribution.EdgeWeights', 'four dimensionless direct multipliers; no normalization')},
    'BridGHe.Common.Models.BridgeSectionAssignment': {
        'Behavior': ('BridgeBeamSectionState.Behavior', 'explicit action behavior'),
        'ConcreteExcluded': ('BridgeBeamSectionState.ConcreteExcluded', 'boolean; requires consistent beam selection and history method'),
        'ModularRatio': ('BridgeBeamSectionState.ModularRatio', 'positive dimensionless Es/Ec-effective'),
        'ClassificationCase': ('BridgeBeamSectionState.ClassificationCaseGuid', 'persistent model case GUID; source name resolved explicitly'),
        'PropertySignature': ('BridgeBeamSectionState.PhysicalPropertyFingerprint', 'fingerprint of GPC property/schedule/profile/taper, not the legacy string')},
    'BridGHe.Common.Checks.BridgeActionDefinition': {
        'Key': ('BridgeWorkflowActionDefinition.Key', 'unique action key'),
        'Source': ('BridgeWorkflowActionDefinition.SourceModelGuid + SourceAnalysisFingerprint', 'no recursively embedded model'),
        'Case': ('BridgeWorkflowActionDefinition.SourceCaseGuid', 'persistent source case GUID'),
        'Behavior': ('BridgeWorkflowActionDefinition.Behavior', 'explicit enum'),
        'Strain': ('BridgeWorkflowActionDefinition.Strain', 'dimensionless; contraction negative; executor converts to microstrain'),
        'BeamIds': ('BridgeWorkflowActionDefinition.BeamGuids', 'persistent beam GUIDs; empty selects all'),
        'PrimaryOnly': ('BridgeWorkflowActionDefinition.PrimaryOnly', 'boolean; explicitly excludes a secondary FEM force case')},
    'BridGHe.Common.Checks.BridgePhaseDefinition': {
        k: ('BridgeWorkflowPhaseDefinition.' + k, 'days' if k == 'Day' else 'stable phase descriptor')
        for k in ['Id', 'Order', 'Name', 'State', 'Day', 'Actions']},
    'BridGHe.Common.Checks.BridgeWorkflowAction': {
        'HasLocalSectionStates': ('BridgeWorkflowActionDefinition.SectionStates.Length != 0', 'derived from explicit per-beam section state')},
}
for declaration in inventory['types']:
    selected = routes.get(declaration['type'], {})
    for member in declaration['members']:
        if member['name'] not in selected:
            continue
        destination, units = selected[member['name']]
        workflow = '.Checks.' in declaration['type'] or 'BridgeSectionAssignment' in declaration['type']
        row = dict(source=declaration['source'], line=member['line'], sourceType=declaration['type'], member=member['name'],
                   destination=destination, units=units,
                   evidence='BridgeWorkflowDefinitionTests + BridgeWorkflowExecutionTests' if workflow else
                   'ElasticExportContractsTest + Straus7ModelWriteTest.NativeLoadPatchTransfersPressureToBeamsAndKeepsStringGroups',
                   status='contract-and-execution-verified; production-adapter-pending-F6')
        rows[(declaration['type'], member['name'])] = row
for row in rows.values():
    if row['destination'] == 'BeamPhysicalData.MinimumOutputStations':
        row['evidence'] = 'Straus7ModelWriteTest: 17 native stations; export/result/archive round trip'
    if row['sourceType'].startswith('BridGHe.Common.Checks.BridgeWorkflow'):
        row['evidence'] = 'BridgeWorkflowDefinitionTests + BridgeWorkflowExecutionTests; exact case/station/state binding'
overlay['fields'] = sorted(rows.values(), key=lambda r: (r['sourceType'], r['line']))
overlay['reviewedFieldCount'] = len(rows)
overlay['notReviewedInThisOverlay'] = overlay['baselineFieldCount'] - len(rows)
overlay['limits'] = [
    'Verified domain contracts and native/Checker execution; no source field declared migrated to production.',
    'Moving heat, centrifugal equations, library templates and native entity sets remain pending. Mechanical static conversion has native analytical evidence.',
    'Semantic and implementation coverage of the entire matrix is still incomplete; the full F5 exit gate remains open.'
]
path.write_text(json.dumps(overlay, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
print(f"{len(rows)}/{overlay['baselineFieldCount']} reviewed fields; F5 remains incomplete.")
