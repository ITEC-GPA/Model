"""Optional integration fixture for element families, end releases, node springs, links, LSA combinations and plate node results;
requires installed/licensed Windows x64 Straus7 R3.

Usage: python generate_native_elements_fixture.py <absolute-St7api.dll> <NEW-output-directory>
Creates and solves ONLY a new model (mm, N, MPa, tonne):
  beams 1 (1-2) and 2 (2-3) along X, steel I; beam 1 End 2 rotation about principal axis 1 released, beam 2 End 2 partial (1e9 Nmm/rad);
  truss 3 (4-2), cable 4 (5-2), spring-damper 5 (1-6, axial 2000 N/mm); node spring kz = 500 N/mm at node 2;
  rigid link 2-7 (XYZ), master-slave link 7-8 (coincident, all DOFs); two cantilever plates 1-2 (nodes 9-14, fixed at x = 0); one brick.
  Case 1 "Dead": node 8 Fz -1000, beam 2 global Z -2 N/mm, nodes 11 and 14 Fz -500. Case 2 "Live": beam 1 principal-2 point force -3000 at 0.5.
  LSA combination "ULS" = 1.35 Dead + 1.5 Live. Element node forces enabled. The production converter neither creates nor solves models.
"""
import ctypes as c
import os
from pathlib import Path
import sys

dll_path = Path(sys.argv[1]).resolve(strict=True)
output = Path(sys.argv[2]).resolve()
output.mkdir(parents=True, exist_ok=False)  # No overwrite of existing analyses.
model = output / "elements.st7"
search = os.add_dll_directory(str(dll_path.parent))
api = c.WinDLL(str(dll_path))


def call(name, *args):
    status = getattr(api, name)(*args)
    if status:
        message = c.create_string_buffer(1024)
        api.St7GetAPIErrorString(status, message, 1024)
        raise RuntimeError(f"{name}: {status}: {message.value.decode('mbcs')}")


def doubles(*values):
    return (c.c_double * len(values))(*values)


def integers(*values):
    return (c.c_int32 * len(values))(*values)


def text(value):
    return value.encode('mbcs')


def connection(*nodes):
    return integers(len(nodes), *nodes, *([0] * (20 - len(nodes))))


BEAM, PLATE, BRICK = 1, 2, 3
STEEL = (210000, 80769.230769, 0.3, 7.85e-9, 1.2e-5, 0, 0, 0, 0)
call("St7SetLicenceOptions", 2, 0, 0)  # lmAbort: no licence dialogs.
call("St7Init")
opened = False
try:
    call("St7NewFile", 1, text(str(model)), text(str(output)))
    opened = True
    call("St7SetUnits", 1, integers(2, 0, 2, 1, 0, 0))  # mm, N, MPa, tonne, Celsius, joule.
    nodes = {1: (0, 0, 0), 2: (4000, 0, 0), 3: (8000, 0, 0), 4: (4000, 0, 3000), 5: (8000, 0, 3000), 6: (0, 0, -1000), 7: (4000, 0, -500), 8: (4000, 0, -500),
             9: (0, 2000, 0), 10: (1000, 2000, 0), 11: (2000, 2000, 0), 12: (0, 3000, 0), 13: (1000, 3000, 0), 14: (2000, 3000, 0),
             15: (0, 5000, 0), 16: (500, 5000, 0), 17: (500, 5500, 0), 18: (0, 5500, 0), 19: (0, 5000, 500), 20: (500, 5000, 500), 21: (500, 5500, 500), 22: (0, 5500, 500)}
    for number, xyz in nodes.items():
        call("St7SetNodeXYZ", 1, number, doubles(*xyz))
    # Beam properties: 1 steel I (btBeam), 2 steel tube truss (btTruss), 3 steel cable (btCable), 4 spring-damper (btSpring).
    call("St7NewBeamProperty", 1, 1, 6, text("Girder I"))
    call("St7SetBeamSectionGeometry", 1, 1, 7, doubles(200, 200, 400, 16, 16, 10))
    call("St7CalculateBeamSectionProperties", 1, 1, c.c_ubyte(0))
    call("St7SetBeamMaterialData", 1, 1, doubles(*STEEL))
    call("St7SetMaterialName", 1, BEAM, 1, text("Steel S355"))
    for prop, kind, name in ((2, 3, "Strut CHS"), (3, 2, "Cable")):
        call("St7NewBeamProperty", 1, prop, kind, text(name))
        call("St7SetBeamSectionGeometry", 1, prop, 2 if kind == 3 else 1, doubles(114.3, 0, 0, 6.3, 0, 0) if kind == 3 else doubles(30, 0, 0, 0, 0, 0))
        call("St7CalculateBeamSectionProperties", 1, prop, c.c_ubyte(0))
        call("St7SetBeamMaterialData", 1, prop, doubles(*STEEL))
        call("St7SetMaterialName", 1, BEAM, prop, text("Steel S355"))
    call("St7NewBeamProperty", 1, 4, 1, text("Spring"))
    call("St7SetSpringDamperData", 1, 4, doubles(2000, 0, 0, 0, 0, 0, 0))
    for beam, prop, i, j in ((1, 1, 1, 2), (2, 1, 2, 3), (3, 2, 4, 2), (4, 3, 5, 2), (5, 4, 1, 6)):
        call("St7SetElementConnection", 1, BEAM, beam, prop, connection(i, j))
    call("St7SetBeamRRelease3", 1, 1, 2, integers(0, 1, 1), doubles(0, 0, 0))  # End 2: free rotation about principal axis 1.
    call("St7SetBeamRRelease3", 1, 2, 2, integers(2, 1, 1), doubles(1e9, 0, 0))  # End 2: partial, 1e9 Nmm/rad about axis 1.
    call("St7SetRigidLink", 1, 1, 1, 0, integers(2, 2, 7))  # Global UCS, rlPlaneXYZ; connection = count, nodes.
    call("St7SetMasterSlaveLink", 1, 2, 1, integers(2, 7, 8), integers(1, 1, 1, 1, 1, 1))
    # Plates: 200 mm steel shells, cantilever along X.
    call("St7NewPlateProperty", 1, 1, 4, 1, text("Plate 200"))
    call("St7SetPlateThickness", 1, 1, doubles(200, 200))
    call("St7SetPlateIsotropicMaterial", 1, 1, doubles(210000, 0.3, 7.85e-9, 1.2e-5, 0, 0, 0, 0))
    call("St7SetMaterialName", 1, PLATE, 1, text("Steel S355"))
    call("St7SetElementConnection", 1, PLATE, 1, 1, connection(9, 10, 13, 12))
    call("St7SetElementConnection", 1, PLATE, 2, 1, connection(10, 11, 14, 13))
    call("St7NewBrickProperty", 1, 1, 1, text("Block"))
    call("St7SetBrickIsotropicMaterial", 1, 1, doubles(30000, 0.2, 2.5e-9, 1e-5, 0, 0, 0, 0))
    call("St7SetElementConnection", 1, BRICK, 1, 1, connection(15, 16, 17, 18, 19, 20, 21, 22))
    for getter, creator in [("St7GetNumLoadCase", "St7NewLoadCase"), ("St7GetNumFreedomCase", "St7NewFreedomCase")]:
        count = c.c_int32()
        call(getter, 1, c.byref(count))
        if count.value == 0:
            call(creator, 1, text("Dead"))
        elif count.value != 1:
            raise RuntimeError("Unexpected new-file case defaults")
    call("St7SetLoadCaseName", 1, 1, text("Dead"))
    fixed, translations = integers(1, 1, 1, 1, 1, 1), integers(1, 1, 1, 0, 0, 0)
    for node in (3, 4, 5, 6, 9, 12):
        call("St7SetNodeRestraint6", 1, node, 1, 1, fixed, doubles(0, 0, 0, 0, 0, 0))
    call("St7SetNodeRestraint6", 1, 1, 1, 1, integers(1, 1, 0, 1, 0, 0), doubles(0, 0, 0, 0, 0, 0))  # Vertical support by the spring 1-6.
    for node in (15, 16, 17, 18):
        call("St7SetNodeRestraint6", 1, node, 1, 1, fixed, doubles(0, 0, 0, 0, 0, 0))
    for node in (19, 20, 21, 22):
        call("St7SetNodeRestraint6", 1, node, 1, 1, integers(0, 0, 0, 1, 1, 1), doubles(0, 0, 0, 0, 0, 0))
    call("St7SetNodeKTranslation3F", 1, 2, 1, 1, doubles(0, 0, 500))
    call("St7SetNodeForce3", 1, 8, 1, doubles(0, 0, -1000))
    call("St7SetBeamDistributedForceGlobal6ID", 1, 2, 3, 0, 1, 0, 1, doubles(-2, 0, 0, 0, 0, 0))
    for node in (11, 14):
        call("St7SetNodeForce3", 1, node, 1, doubles(0, 0, -500))
    call("St7EnableLSALoadCase", 1, 1, 1)
    call("St7NewLoadCase", 1, text("Live"))
    call("St7SetBeamPointForcePrincipal4ID", 1, 1, 2, 1, doubles(0, -3000, 0, 0.5))
    call("St7EnableLSALoadCase", 1, 2, 1)
    call("St7AddLSACombination", 1, text("ULS"))
    call("St7SetLSACombinationFactor", 1, 0, 1, 1, 1, c.c_double(1.35))
    call("St7SetLSACombinationFactor", 1, 0, 1, 2, 1, c.c_double(1.5))
    call("St7SetEntityResult", 1, 21, c.c_ubyte(1))  # srElementNodeForce
    call("St7SaveFile", 1)
    call("St7RunSolver", 1, 1, 3, 1)  # Linear static, background, wait; fixture only.
    print(f"Native elements fixture solved: {model}")
finally:
    try:
        if opened:
            call("St7CloseFile", 1)
    finally:
        call("St7Release")
