"""Optional integration fixture for properties, offsets, restraints and loads; requires installed/licensed Windows x64 Straus7 R3.

Usage: python generate_native_assignments_fixture.py <absolute-St7api.dll> <NEW-output-directory>
Creates and solves ONLY a new model (mm, N, MPa, tonne): two concrete columns, a steel I girder with an offset, two concrete
plates (one with element thickness and offset), fixed and pinned supports; case 1 with nodal, distributed (principal/global),
concentrated and pressure loads, case 2 gravity. The production converter neither creates nor solves models.
"""
import ctypes as c
import os
from pathlib import Path
import sys

dll_path = Path(sys.argv[1]).resolve(strict=True)
output = Path(sys.argv[2]).resolve()
output.mkdir(parents=True, exist_ok=False)  # No overwrite of existing analyses.
model = output / "assignments.st7"
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


BEAM, PLATE = 1, 2
call("St7SetLicenceOptions", 2, 0, 0)  # lmAbort: no licence dialogs.
call("St7Init")
opened = False
try:
    call("St7NewFile", 1, text(str(model)), text(str(output)))
    opened = True
    call("St7SetUnits", 1, integers(2, 0, 2, 1, 0, 0))  # mm, N, MPa, tonne, Celsius, joule.
    nodes = [(0, 0, 0), (0, 0, 3000), (6000, 0, 0), (6000, 0, 3000), (6000, 3000, 3000), (0, 3000, 3000), (6000, 6000, 3000), (0, 6000, 3000)]
    for number, xyz in enumerate(nodes, 1):
        call("St7SetNodeXYZ", 1, number, doubles(*xyz))
    # Beam property 1: concrete solid rectangle B 400 x D 600; property 2: steel mono-symmetric I.
    call("St7NewBeamProperty", 1, 1, 6, text("Column 400x600"))
    call("St7SetBeamSectionGeometry", 1, 1, 3, doubles(400, 600, 0, 0, 0, 0))
    call("St7CalculateBeamSectionProperties", 1, 1, c.c_ubyte(0))
    call("St7SetBeamMaterialData", 1, 1, doubles(32000, 13333.333333, 0.2, 2.5e-9, 1e-5, 0, 0, 0, 0))
    call("St7SetMaterialName", 1, BEAM, 1, text("Concrete C30/37"))
    call("St7NewBeamProperty", 1, 2, 6, text("Girder I"))
    call("St7SetBeamSectionGeometry", 1, 2, 7, doubles(200, 150, 400, 15, 12, 8))
    call("St7CalculateBeamSectionProperties", 1, 2, c.c_ubyte(0))
    call("St7SetBeamMaterialData", 1, 2, doubles(210000, 80769.230769, 0.3, 7.85e-9, 1.2e-5, 0, 0, 0, 0))
    call("St7SetMaterialName", 1, BEAM, 2, text("Steel S355"))
    call("St7SetElementConnection", 1, BEAM, 1, 1, integers(2, 1, 2, *([0] * 18)))
    call("St7SetElementConnection", 1, BEAM, 2, 1, integers(2, 3, 4, *([0] * 18)))
    call("St7SetElementConnection", 1, BEAM, 3, 2, integers(2, 2, 4, *([0] * 18)))
    call("St7SetBeamOffset2", 1, 3, doubles(0, 100))
    # Plate property 1: concrete shell 200 mm; plate 2 overrides the thickness and has an offset.
    call("St7NewPlateProperty", 1, 1, 4, 1, text("Slab 200"))
    call("St7SetPlateThickness", 1, 1, doubles(200, 200))
    call("St7SetPlateIsotropicMaterial", 1, 1, doubles(32000, 0.2, 2.5e-9, 1e-5, 0, 0, 0, 0))
    call("St7SetMaterialName", 1, PLATE, 1, text("Concrete C30/37"))
    call("St7SetElementConnection", 1, PLATE, 1, 1, integers(4, 2, 4, 5, 6, *([0] * 16)))
    call("St7SetElementConnection", 1, PLATE, 2, 1, integers(4, 6, 5, 7, 8, *([0] * 16)))
    call("St7SetPlateThickness2", 1, 2, doubles(250, 250))
    call("St7SetPlateOffset1", 1, 2, doubles(50))
    for getter, creator in [("St7GetNumLoadCase", "St7NewLoadCase"), ("St7GetNumFreedomCase", "St7NewFreedomCase")]:
        count = c.c_int32()
        call(getter, 1, c.byref(count))
        if count.value == 0:
            call(creator, 1, text("Loads"))
        elif count.value != 1:
            raise RuntimeError("Unexpected new-file case defaults")
    call("St7SetLoadCaseName", 1, 1, text("Loads"))
    for node in (1, 3):
        call("St7SetNodeRestraint6", 1, node, 1, 1, integers(1, 1, 1, 1, 1, 1), doubles(0, 0, 0, 0, 0, 0))
    for node in (5, 6, 7, 8):
        call("St7SetNodeRestraint6", 1, node, 1, 1, integers(1, 1, 1, 0, 0, 0), doubles(0, 0, 0, 0, 0, 0))
    # Case 1: every supported load family.
    call("St7SetNodeForce3", 1, 2, 1, doubles(1000, 2000, -3000))
    call("St7SetNodeMoment3", 1, 4, 1, doubles(0, 5e5, 0))
    call("St7SetBeamDistributedForcePrincipal6ID", 1, 3, 2, 1, 5, 1, doubles(-5, -8, -2, -3, 0.25, 0.25))  # trapezoidal, principal 2
    call("St7SetBeamDistributedForceGlobal6ID", 1, 1, 1, 0, 1, 1, 1, doubles(1, 2, 0, 0, 0.1, 0.2))  # linear, global X
    call("St7SetBeamPointForcePrincipal4ID", 1, 3, 1, 1, doubles(0, -10000, 0, 0.3))
    call("St7SetBeamPointForceGlobal4ID", 1, 3, 1, 2, doubles(0, 0, -5000, 0.7))
    call("St7SetBeamPointMomentPrincipal4ID", 1, 2, 1, 1, doubles(2e6, 0, 0, 0.5))
    call("St7SetPlateNormalPressure2", 1, 1, 1, doubles(0.002, 0.005))
    call("St7SetPlateGlobalPressure3S", 1, 2, 2, 0, 1, doubles(0, 0, -0.004))
    call("St7EnableLSALoadCase", 1, 1, 1)
    # Case 2: gravity.
    call("St7NewLoadCase", 1, text("Gravity"))
    call("St7SetLoadCaseType", 1, 2, 1)
    call("St7SetLoadCaseGravityDir", 1, 2, 3)
    call("St7SetLoadCaseGravity", 1, 2, c.c_double(9806.65))
    call("St7SetLoadCaseMassOption", 1, 2, c.c_ubyte(1), c.c_ubyte(0))
    call("St7EnableLSALoadCase", 1, 2, 1)
    call("St7SetEntityResult", 1, 21, c.c_ubyte(1))  # srElementNodeForce
    call("St7SaveFile", 1)
    call("St7RunSolver", 1, 1, 3, 1)  # Linear static, background, wait; fixture only.
    print(f"Native assignments fixture solved: {model}")
finally:
    try:
        if opened:
            call("St7CloseFile", 1)
    finally:
        call("St7Release")
