"""Prepare the F2 field-routing matrix. Architectural rules are proposals, not adapters.
Run after MigrationInventory; explicit unresolved units/gaps must be closed by F4/F5.
"""
import json
import re
import sys
from pathlib import Path

source, destination = map(Path, sys.argv[1:])
inventory = json.loads(source.read_text(encoding="utf-8"))

routes = {
    "NodeModel": ("GPC.Model.Elements.NodeElement", "existing", "Position: mm; NodeId: SourceIdentity.OriginalId"),
    "BeamModel": ("GPC.Model.Elements.BeamElement + BeamAssignments", "existing-partial", "end points/offset: mm; AngleDeg: convert to rad; explicit connectivity"),
    "PlateModel": ("GPC.Model.Elements.AreaElement + ShellGeometry + ShellAssignments", "existing-partial", "points/offset: mm; AngleDeg: rad; higher-order plates require F5 contract"),
    "ModelModel": ("GPC.Model.Models.Model + ModelAssembly", "existing-partial", "Units: explicit input conversion; results externalized in F3"),
    "ElementModel": ("GPC.Model.Elements.Element", "existing-partial", "collections: explicit load/assignment/result contracts; no generic attribute copying"),
    "GroupModel": ("GPC.Model.Attributes.Group", "existing", "labels/hierarchy; independent of physical membership"),
    "CoordinateSystemModel": ("GPC.Geometry.CoordinateSystem", "existing", "origin mm; right-handed orthonormal basis; original ID in adapter map"),
    "ElementIdAttributeModel": ("SourceIdentity + AssemblyIdentityMap + SolverIdentityMap", "existing", "source identifier is not the GPC GUID, model ID or solver ID"),
    "BridgeMemberMetadata": ("GPC.Model.Structure.Members.BridgeMemberMetadata", "existing", "stations mm; direction in global axes; indices retained"),
    "BridgeMemberAttributeModel": ("PhysicalMemberDefinition.BridgeMetadata", "existing-partial", "explicit physical path; do not infer membership from Group"),
    "BridgeSectionAssignment": ("Checker.CompositeBridge workflow / GPC physical section assignment", "split-required-F4-F5", "separate physical stiffness state from verification/classification provenance"),
    "BridgeSectionStateAttributeModel": ("Checker.CompositeBridge workflow / GPC physical section assignment", "split-required-F4-F5", "separate physical stiffness state from verification/classification provenance"),
    "NodeForceLoadModel": ("GPC.Model.Loads.PointLoad", "existing", "N; declared load case and coordinate system"),
    "NodeMomentLoadModel": ("GPC.Model.Loads.PointLoad", "existing", "N mm; declared load case and coordinate system"),
    "BeamPointLoadModel": ("GPC.Model.Loads.Assignments.BeamLoadAssignment", "existing-partial", "force N / moment N mm; explicit station domain and eccentricity"),
    "BeamDistributedLoadModel": ("GPC.Model.Loads.Assignments.BeamLoadAssignment", "existing-partial", "N/mm or N mm/mm; projected/actual length and segmented law explicit"),
    "NodeRestraintAttributeModel": ("GPC.Model.Structure.Assignments.RestrainAssignment + NodeRestrain", "existing", "translation mm; rotation rad; local axes explicit; nullable availability"),
    "NodeStiffnessAttributeModel": ("GPC.Model.Structure.Assignments.SpringMatrix", "existing", "36 coefficients: [N,N mm] / [mm,rad]; no single scalar unit"),
    "NodeDisplacementLoadModel": ("GPC.Model.Structure.Assignments / imposed displacements", "verify-adapter-F5", "translation mm; rotation rad; scope/case and axes retained"),
    "LoadCombinationModel": ("GPC.Model.Combinations.Combination", "existing-partial", "dimensionless coefficients; stable case references"),
    "LoadCaseModel": ("GPC.Model.LoadCases + ModelGravityLoad + ModelAccelerationLoad + Stages", "existing-partial", "temperature C vs delta-C; acceleration mm/s2; angular velocity rad/s; phase map separate"),
    "CaseModel": ("GPC.Model.LoadCases.ILoadCase", "existing-partial", "case names/identity; distinguish cases from combinations"),
    "BeamPropertyBaseModel": ("GPC.Model.ElementProperties.BeamProperty", "existing-partial", "material/section references; original integer ID stays in adapter"),
    "BeamPropertyModel": ("GPC.Model.ElementProperties.BeamProperty + BeamAssignments.Formulation", "existing-partial", "area mm2, inertia mm4, section modulus mm3, material MPa; unsupported spring/cutoff laws require F5"),
    "SectionModel": ("GPC.Model.Sections + section catalog / section laws", "existing-partial", "mm, mm2, mm3, mm4; angles rad; derived caches recomputed; geometry gaps explicit"),
    "MaterialModel": ("GPC.Model.Materials; membrane/orthotropy extension F4-F5", "existing-partial", "stress MPa, mass density t/mm3; membrane stiffness/strength N/mm and mass t/mm2; validate adapter columns"),
    "PlatePropertyModel": ("GPC.Model.ElementProperties.PlateProperty / IFemPlateProperty", "existing-partial", "thickness mm; formulation/orthotropy/load-patch gaps require F5"),
    "BeamReleasesAttributeModel": ("GPC.Model.Attributes.BeamReleasesAttribute / BeamDofConnection", "existing-partial", "translations/rotations explicit; finite stiffness uses mixed units"),
    "BeamTaperAttributeModel": ("GPC.Model.Structure.Assignments.BeamSectionAssignment", "existing-partial", "dimensionless factors; preserve alignment/eccentricity and section law"),
    "BeamNonPrismaticPropertyModel": ("GPC.Model.Structure.Assignments.BeamSectionAssignment", "existing-partial", "station domain and interpolation law explicit; no two-end approximation of arbitrary law"),
    "StagedConstructionModel": ("GPC.Model.Stages; Checker phase-to-case mapping", "existing-partial", "activation is physical; verification stage identity remains Checker"),
    "CompositeBeamModel": ("BeamElement + BeamSectionAssignment + PhysicalMemberDefinition", "existing-partial", "mm; retain start/end properties, offsets, member stations"),
    "CompositeBeamPropertyModel": ("GPC composite section/property + Checker.CompositeBridge workflow", "split-required-F4-F5", "physical section/materials/offset in GPC; verification modular-ratio provenance in Checker"),
    "SectionConcreteSlabModel": ("GPC.Model.Sections.Concrete / composite section", "existing-partial", "dimensions mm; reinforcement area mm2; explicit physical section state"),
    "ForceDensityModel": ("GPC form-finding objectives (extension); TensileLib adapter", "extension-F4-F5", "q=N/L -> N/mm; membrane/curvature modes retain their discriminant; penalty weights require explicit normalization"),
    "FixedLengthModel": ("GPC form-finding fixed-length objective (extension)", "extension-F4-F5", "target mm; penalty weight is solver metadata, not a load"),
    "FixedAxialForceModel": ("GPC form-finding axial-force objective (extension)", "extension-F4-F5", "target N; penalty weight is solver metadata, not a load"),
    "SolverOptionsModel": ("analysis-run configuration + TensileLib adapter", "extension-F4-F5", "separate outer/inner iteration tolerances; gravity acceleration; follower pressure; case selection"),
    "NodeWeightAttributeModel": ("GPC form-finding coordinate objective (extension)", "extension-F4-F5", "coordinate penalty weights; do not confuse with gravitational weight"),
    "NodePressureLoadModel": ("GPC form-finding derived pressure-force result (extension)", "extension-F4-F5", "nodal force N generated on the found shape; do not reapply as input in the next form-finding; pressure N/mm2 belongs to SolverOptions"),
    "NodeTemperatureLoadModel": ("GPC nodal temperature assignment (extension)", "extension-F5", "C vs delta-C; preserve Reference/Fixed/Initial modes"),
    "BeamGradientLoadModel": ("GPC beam thermal gradient assignment (extension)", "extension-F5", "delta-C/mm on the declared local directions"),
    "BeamPreLoadModel": ("GPC beam preload/initial strain assignment (extension)", "extension-F5", "force N or dimensionless strain; explicit mode and sign"),
    "PlatePreStressModel": ("GPC shell initial stress assignment (extension)", "extension-F5", "local stress N/mm2; distinguish membrane resultant N/mm"),
    "PlateNSMassModel": ("GPC shell nonstructural mass assignment (extension)", "extension-F5", "mass per area t/mm2, offset mm, dynamic factor dimensionless"),
    "PlatePressureModel": ("GPC.Model.Loads.NormalAreaLoad / NonUniformPlatePressure", "existing-partial", "N/mm2; top/bottom face, local/global axes, projection and follower semantics explicit"),
    "BeamSupportAttributeModel": ("GPC support/link constitutive assignment (extension)", "extension-F5", "stiffness and gap mm; tension/compression unilateral law and orientation retained"),
    "PlateSupportAttributeModel": ("GPC shell support assignment (extension)", "extension-F5", "distributed stiffness is force/(area*displacement); verify exporter convention"),
    "RigidConnectionAttributeModel": ("GPC.Model.Constraints", "existing-partial", "explicit node references and rigid lever arms mm"),
    "LoadPatchTypeAttributeModel": ("GPC physical load-patch descriptor + Converter Straus7", "extension-F5", "edge indices and dimensionless distribution weights; preserve split algorithm"),
    "BeamStringGroupAttributeModel": ("Converter Straus7 export descriptor", "extension-F5", "solver grouping identifier distinct from organizational Group"),
    "BeamOutputStationsModel": ("analysis/export sampling descriptor", "extension-F5", "integer sample count and explicit station domain"),
    "SuperElementAttributeModel": ("Converter Straus7 export descriptor", "extension-F5", "opaque software reference until typed supported export contract exists"),
}

def route(t):
    name = t["type"].split(".")[-1].split("+")[0].split("`")[0]
    if t["type"].startswith("TensileLib."):
        return ("TensileLib (solver internals retained); GPC adapter boundary", "retain-solver", "preserve F1 input/output transcript; explicit unit adapter, IDs from SolverIdentityMap")
    if ".Checks." in t["type"]:
        return ("Checker.CompositeBridge; Converter for result extraction/conventions", "review-split-F4-F5", "verification workflow and reports; physical input references are stable GPC identities")
    if ".MovingLoads." in t["type"]:
        return ("GPC physical moving-load descriptors; Converter Straus7; Checker workflow", "extension-F5", "path stations/axles mm, forces N; generated cases preserve source-position identity")
    if ".Geometry." in t["type"]:
        return ("GPC bridge descriptors; Rhino geometry construction/preview remains plugin", "review-split-F5", "dimensions/stations mm, section orientation rad; unsupported curves require explicit representation")
    if "Result" in name:
        return ("GPC.Model.Results + AnalysisDataset + external result archive", "existing-partial-F3-F5", "forces N, moments N mm, displacements mm, rotations rad; shell resultants N/mm and N mm/mm; adapter confirms sign/axes")
    return routes.get(name, ("GPC typed domain descriptor or plugin adapter (decision still open)", "review-required-F5", "not inferred from name; resolve against serialization and native exporter before conversion"))

rows, decisions = [], []
for declaration in inventory["types"]:
    target, status, dimensions = route(declaration)
    decisions.append(dict(sourceType=declaration["type"], destination=target, status=status, unitContract=dimensions))
    for member in declaration["members"]:
        key = member["name"].lstrip("_").lower()
        dest, state, units = target, status, dimensions
        archive = "Explicit GPC DataContract/ISerializable registration when migrated; F3 for result blocks"
        fingerprint = "Explicit fingerprint contract for physical inputs; verify sensitivity field by field in F4/F5"
        identity = "Entity GUID persistent; model integer ID per assembly; solver ID per run; values owned by containing entity"
        if "SERIALIZATION_RELEASE" in member["name"]:
            dest, state, units = "Legacy plugin reader/version gate", "retain-compatibility-F6", "version integer/key"
            archive, fingerprint = "Read legacy only; not a structural input", "excluded (serialization bookkeeping)"
        elif key in {"isvisible", "scale", "color", "isaxisvisible", "axisscale", "shapes", "shape", "geometryrevision", "genericpreview", "description", "isvalid", "isvertical"}:
            # Shape is UI only for element preview; section shape is authoritative geometry.
            if key != "shape" or "PlateModel" in declaration["type"]:
                dest, state, units = "Plugin wrapper / preview / computed validation", "retain-plugin", "view units or derived value; not independent physical state"
                archive, fingerprint = "Plugin document where required; cache recomputed", "excluded from structural input fingerprint"
        elif member["kind"] == "event":
            dest, state, units = "Runtime callback in owning plugin/solver", "runtime-only", "not applicable"
            archive, fingerprint = "not serialized", "excluded"
        elif member["kind"] == "enumValue":
            units = "discriminant; preserve numeric value and reject unknown values"
        if key in {"position", "pointfrom", "pointto", "points", "origin", "offset", "thickness", "stationfrom", "stationto", "length", "maximumlength", "chordtolerance"}:
            units = "mm in GPC; resolve source unit context explicitly at adapter boundary"
        elif key == "angledeg":
            units = "source degrees -> GPC radians; orientation sign requires converter test"
        elif key == "anglex1rad":
            units = "rad; retain section-axis convention"
        elif key in {"nodeid", "beamid", "plateid", "id"}:
            identity = "OriginalId/source label; do not cast to persistent GUID or reuse as solver ID"
        rows.append(dict(source=declaration["source"], line=member["line"], sourceType=declaration["type"], member=member["name"],
            declaredType=member["type"], kind=member["kind"], meaning=member["documentation"] or "Declared source member; detailed semantics require adapter verification",
            destination=dest, status=state, units=units, identity=identity, serialization=archive, fingerprint=fingerprint,
            consumers="BridGHe/Converter/Straus7/Checker" if declaration["type"].startswith("BridGHe.") else "Arachne/TensileLib adapter",
            sourceSha256=declaration["sha256"]))

assert len(rows) == inventory["memberCount"]
assert len({(r["sourceType"], r["member"]) for r in rows}) == len(rows), "Non-unique type/member identity"
result = dict(schema="GPC-migration-field-matrix-v1", typeCount=len(decisions), memberCount=len(rows),
    status="F2 architecture routing; partial existing contracts and explicit F4/F5 gaps. No adapter equivalence claimed.",
    types=decisions, fields=rows)
with destination.open("w", encoding="utf-8", newline="\r\n") as output:
    output.write(json.dumps(result, ensure_ascii=False, indent=2) + "\n")
print(f"{len(decisions)} types, {len(rows)} members; complete routing coverage, unresolved details explicitly marked.")
