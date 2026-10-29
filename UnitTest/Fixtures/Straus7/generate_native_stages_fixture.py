"""Optional integration fixture for stages, masses, temperatures and reactions; requires installed/licensed Windows x64 Straus7 R3.

Usage: python generate_native_stages_fixture.py <absolute-St7api.dll> <NEW-output-directory>
Creates and solves ONLY a new model (mm, N, MPa, tonne, Celsius):
  beam 1 (1-2) and beam 2 (2-3) along X, steel I 400 x 200 x 16/10; truss 3 (4-2) CHS 114.3 x 6.3; plate 1 (5-6-7-8), 200 mm steel shell;
  groups "Deck" (beam 1, plate 1) and "Span 2" (beam 2, truss 3) under the root group "Model";
  stage 1 "Deck only" (Span 2 disabled), stage 2 "Complete" (all groups), both unmorphed;
  nodes 1, 4, 5, 8 fixed, node 3 pinned (translations);
  masses: node 3 translational 0.5 t (X, Y, Z) and rotational 10, 20, 30 t mm2 (global UCS); case 1 non-structural masses: node 2 0.2 t
  (dynamic factor 1, offset Z 100 mm), beam 1 constant 1e-4 t/mm, plate 1 2e-7 t/mm2;
  case 1 "Dead": gravity -Z 9806.65 mm/s2 on structural and non-structural mass; case 2 "Live": node 2 Fz -1000 N;
  case 3 "Thermal": reference temperature 10 C, nodes 1-3 at 40 C, beam 1 gradient 0.01 C/mm along principal axis 1, plate 1 gradient 0.02 C/mm;
  LSA combination "ULS" = 1.35 Dead + 1.5 Live.
Solutions: stages.LSA (linear static, no active stage), stages-stage1.LSA (linear static, active stage 1) and stages.NLA (nonlinear static
staged analysis: stage 1 increment "Dead" = Dead; stage 2 increments "Live half" = Dead + 0.5 Live and "Live full" = Dead + Live).
The production converter neither creates nor solves models.
"""
import ctypes as c
import os
from pathlib import Path
import sys

dll_path = Path(sys.argv[1]).resolve(strict=True)
output = Path(sys.argv[2]).resolve()
output.mkdir(parents=True, exist_ok=False)  # No overwrite of existing analyses.
model = output / "stages.st7"
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


BEAM, PLATE = 1, 2
STEEL = (210000, 80769.230769, 0.3, 7.85e-9, 1.2e-5, 0, 0, 0, 0)
call("St7SetLicenceOptions", 2, 0, 0)  # lmAbort: no licence dialogs.
call("St7Init")
opened = False
try:
    call("St7NewFile", 1, text(str(model)), text(str(output)))
    opened = True
    call("St7SetUnits", 1, integers(2, 0, 2, 1, 0, 0))  # mm, N, MPa, tonne, Celsius, joule.
    nodes = {1: (0, 0, 0), 2: (4000, 0, 0), 3: (8000, 0, 0), 4: (4000, 0, 3000), 5: (0, 2000, 0), 6: (1000, 2000, 0), 7: (1000, 3000, 0), 8: (0, 3000, 0)}
    for number, xyz in nodes.items():
        call("St7SetNodeXYZ", 1, number, doubles(*xyz))
    call("St7NewBeamProperty", 1, 1, 6, text("Girder I"))
    call("St7SetBeamSectionGeometry", 1, 1, 7, doubles(200, 200, 400, 16, 16, 10))
    call("St7CalculateBeamSectionProperties", 1, 1, c.c_ubyte(0))
    call("St7SetBeamMaterialData", 1, 1, doubles(*STEEL))
    call("St7SetMaterialName", 1, BEAM, 1, text("Steel S355"))
    call("St7NewBeamProperty", 1, 2, 3, text("Strut CHS"))
    call("St7SetBeamSectionGeometry", 1, 2, 2, doubles(114.3, 0, 0, 6.3, 0, 0))
    call("St7CalculateBeamSectionProperties", 1, 2, c.c_ubyte(0))
    call("St7SetBeamMaterialData", 1, 2, doubles(*STEEL))
    call("St7SetMaterialName", 1, BEAM, 2, text("Steel S355"))
    for beam, prop, i, j in ((1, 1, 1, 2), (2, 1, 2, 3), (3, 2, 4, 2)):
        call("St7SetElementConnection", 1, BEAM, beam, prop, connection(i, j))
    call("St7NewPlateProperty", 1, 1, 4, 1, text("Plate 200"))
    call("St7SetPlateThickness", 1, 1, doubles(200, 200))
    call("St7SetPlateIsotropicMaterial", 1, 1, doubles(210000, 0.3, 7.85e-9, 1.2e-5, 0, 0, 0, 0))
    call("St7SetMaterialName", 1, PLATE, 1, text("Steel S355"))
    call("St7SetElementConnection", 1, PLATE, 1, 1, connection(5, 6, 7, 8))
    # Groups and stages (stages are numbered from 1; a new stage enables every group).
    name = c.create_string_buffer(256); root = c.c_int32()
    call("St7GetGroupByIndex", 1, 1, name, 256, c.byref(root))
    deck, span = c.c_int32(), c.c_int32()
    call("St7NewChildGroup", 1, root.value, text("Deck"), c.byref(deck))
    call("St7NewChildGroup", 1, root.value, text("Span 2"), c.byref(span))
    call("St7SetEntityGroup", 1, BEAM, 1, deck.value)
    call("St7SetEntityGroup", 1, PLATE, 1, deck.value)
    for beam in (2, 3):
        call("St7SetEntityGroup", 1, BEAM, beam, span.value)
    call("St7AddStage", 1, text("Deck only"), integers(0, 0, 0, 0, 0))
    call("St7AddStage", 1, text("Complete"), integers(0, 0, 0, 0, 0))
    call("St7DisableStageGroup", 1, 1, span.value)
    # Cases.
    for getter, creator in [("St7GetNumLoadCase", "St7NewLoadCase"), ("St7GetNumFreedomCase", "St7NewFreedomCase")]:
        count = c.c_int32()
        call(getter, 1, c.byref(count))
        if count.value == 0:
            call(creator, 1, text("Dead"))
        elif count.value != 1:
            raise RuntimeError("Unexpected new-file case defaults")
    call("St7SetLoadCaseName", 1, 1, text("Dead"))
    call("St7SetLoadCaseType", 1, 1, 1)  # lcGravity
    call("St7SetLoadCaseGravityDir", 1, 1, 3)
    call("St7SetLoadCaseGravity", 1, 1, c.c_double(-9806.65))
    call("St7SetLoadCaseMassOption", 1, 1, c.c_ubyte(1), c.c_ubyte(1))
    call("St7NewLoadCase", 1, text("Live"))
    call("St7SetNodeForce3", 1, 2, 2, doubles(0, 0, -1000))
    call("St7NewLoadCase", 1, text("Thermal"))
    defaults = (c.c_double * 13)()
    call("St7GetLoadCaseDefaults", 1, 3, defaults)
    defaults[0] = 10.0
    call("St7SetLoadCaseDefaults", 1, 3, defaults)
    for node in (1, 2, 3):
        call("St7SetNodeTemperature1", 1, node, 3, doubles(40.0))
    call("St7SetBeamTempGradient2", 1, 1, 3, doubles(0.01, 0.0))
    call("St7SetPlateTempGradient1", 1, 1, 3, doubles(0.02))
    # Masses.
    call("St7SetNodeTMass3", 1, 3, doubles(0.5, 0.5, 0.5))
    call("St7SetNodeRMass3", 1, 3, 1, doubles(10.0, 20.0, 30.0))
    call("St7SetNodeNSMass5ID", 1, 2, 1, 1, doubles(0.2, 1.0, 0, 0, 100))
    call("St7SetBeamNSMass10ID", 1, 1, 1, 0, 1, doubles(1e-4, 0, 0, 0, 0, 0, 1.0, 0, 0, 0))
    call("St7SetPlateNSMass5ID", 1, 1, 1, 1, doubles(2e-7, 1.0, 0, 0, 0, 0))
    # Restraints.
    fixed = integers(1, 1, 1, 1, 1, 1)
    for node in (1, 4, 5, 8):
        call("St7SetNodeRestraint6", 1, node, 1, 1, fixed, doubles(0, 0, 0, 0, 0, 0))
    call("St7SetNodeRestraint6", 1, 3, 1, 1, integers(1, 1, 1, 0, 0, 0), doubles(0, 0, 0, 0, 0, 0))
    for case in (1, 2, 3):
        call("St7EnableLSALoadCase", 1, case, 1)
    call("St7AddLSACombination", 1, text("ULS"))
    call("St7SetLSACombinationFactor", 1, 0, 1, 1, 1, c.c_double(1.35))
    call("St7SetLSACombinationFactor", 1, 0, 1, 2, 1, c.c_double(1.5))
    # Staged nonlinear analysis: increments and load factors of each stage.
    call("St7SetNLAStagedAnalysis", 1, c.c_ubyte(1))
    for stage, increments in ((1, [("Dead", {1: 1.0, 2: 0.0})]), (2, [("Live half", {1: 1.0, 2: 0.5}), ("Live full", {1: 1.0, 2: 1.0})])):
        call("St7EnableNLAStage", 1, stage)
        for case in (1, 2):
            call("St7EnableNLALoadCase", 1, stage, case)
        call("St7EnableNLAFreedomCase", 1, stage, 1)
        for number, (label, factors) in enumerate(increments, 1):
            call("St7AddNLAIncrement", 1, stage, text(label))
            for case, factor in factors.items():
                call("St7SetNLALoadIncrementFactor", 1, stage, number, case, c.c_double(factor))
            call("St7SetNLAFreedomIncrementFactor", 1, stage, number, 1, c.c_double(1.0))
    for active, result in ((0, "stages.LSA"), (1, "stages-stage1.LSA")):
        call("St7SetSolverActiveStage", 1, active)
        call("St7SetResultFileName", 1, text(str(output / result)))
        call("St7SaveFile", 1)
        call("St7RunSolver", 1, 1, 3, 1)  # Linear static, background, wait; fixture only.
    call("St7SetSolverActiveStage", 1, 0)
    call("St7SetResultFileName", 1, text(str(output / "stages.NLA")))
    call("St7SaveFile", 1)
    call("St7RunSolver", 1, 3, 3, 1)  # Nonlinear static, staged.
    call("St7SaveFile", 1)
    print(f"Native stages fixture solved: {model}")
finally:
    try:
        if opened:
            call("St7CloseFile", 1)
    finally:
        call("St7Release")
