"""Optional integration fixture for linear static results of several freedom cases; requires installed/licensed Windows x64 Straus7 R3.

Usage: python generate_native_freedom_cases_fixture.py <absolute-St7api.dll> <NEW-output-directory>
Creates and solves ONLY a new model (mm, N, MPa, tonne):
  beams 1 (1-2) and 2 (2-3) along +X, steel I 400 x 200 x 16/10; plate 1 (4-5-6-7), 200 mm steel shell, cantilever from x = 0;
  two freedom cases, both named "Freedom Case 1":
    freedom case 1: node 1 DX DY DZ RX, node 3 DY DZ, node 2 on a vertical node spring kz = 200 N/mm (one span of 8 m, elastic mid support);
    freedom case 2: node 1 DX DY DZ RX, nodes 2 and 3 DY DZ (two spans of 4 m), no spring;
    both: plate nodes 4 and 7 fixed;
  load cases 1 "G1" (gravity -Z), 2 "Q" (beam 1 global point force Z -10000 N at mid-span), 3 "G1" (gravity -Z, the name of case 1 again),
  4 "W" (beam 2 global Z -2 N/mm, plate node 5 Fz -500 N);
  linear static pairs saved in the model: load case 1 and 4 under freedom case 1, load cases 2 and 3 under freedom case 2;
  LSA combinations: "ULS1" = 1.35 [1, FC1] + 1.5 [4, FC1]; "ULS2" = 1.35 [3, FC2] + 1.5 [2, FC2]; "MIX" = [1, FC1] + [2, FC2];
  "UNSOLVED" = [2, FC1] (a pair the saved pairs do not solve).
Solutions (linear static):
  freedom-shared.LSA: the saved pairs and load case 2 also under freedom case 1 (one load case under both freedom cases);
  freedom-fc1.LSA: load cases 1 and 4 under freedom case 1 only;
  freedom.LSA: the saved pairs (solved last; the model keeps these pairs).
After each solution the result case names are printed, without and with the LSA combinations (kGenerateNewCombinations).
The production converter neither creates nor solves models.
"""
import ctypes as c
import os
from pathlib import Path
import sys

dll_path = Path(sys.argv[1]).resolve(strict=True)
output = Path(sys.argv[2]).resolve()
output.mkdir(parents=True, exist_ok=False)  # No overwrite of existing analyses.
model = output / "freedom.st7"
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


def pairs(enabled):
    for case in (1, 2, 3, 4):
        for freedom in (1, 2):
            if (case, freedom) in enabled:
                call("St7EnableLSALoadCase", 1, case, freedom)
            else:
                call("St7DisableLSALoadCase", 1, case, freedom)
    for case in (1, 2, 3, 4):
        for freedom in (1, 2):
            state = c.c_ubyte()
            call("St7GetLSALoadCaseState", 1, case, freedom, c.byref(state))
            if (state.value != 0) != ((case, freedom) in enabled):
                raise RuntimeError(f"LSA pair {case}/{freedom} not set")


def names(result):
    for combinations in (0, 1):  # kNoCombinations, kGenerateNewCombinations
        primary, secondary = c.c_int32(), c.c_int32()
        call("St7OpenResultFile", 1, text(str(result)), text(""), combinations, c.byref(primary), c.byref(secondary))
        try:
            label = "with combinations" if combinations else "primary"
            print(f"{result.name} ({label}): {primary.value} primary, {secondary.value} secondary")
            for number in range(1, primary.value + secondary.value + 1):
                name = c.create_string_buffer(256)
                call("St7GetResultCaseName", 1, number, name, 256)
                print(f"  {number}: {name.value.decode('mbcs')!r}")
        finally:
            call("St7CloseResultFile", 1)


BEAM, PLATE = 1, 2
STEEL = (210000, 80769.230769, 0.3, 7.85e-9, 1.2e-5, 0, 0, 0, 0)
call("St7SetLicenceOptions", 2, 0, 0)  # lmAbort: no licence dialogs.
call("St7Init")
opened = False
try:
    call("St7NewFile", 1, text(str(model)), text(str(output)))
    opened = True
    call("St7SetUnits", 1, integers(2, 0, 2, 1, 0, 0))  # mm, N, MPa, tonne, Celsius, joule.
    nodes = {1: (0, 0, 0), 2: (4000, 0, 0), 3: (8000, 0, 0), 4: (0, 2000, 0), 5: (1000, 2000, 0), 6: (1000, 3000, 0), 7: (0, 3000, 0)}
    for number, xyz in nodes.items():
        call("St7SetNodeXYZ", 1, number, doubles(*xyz))
    call("St7NewBeamProperty", 1, 1, 6, text("Girder I"))
    call("St7SetBeamSectionGeometry", 1, 1, 7, doubles(200, 200, 400, 16, 16, 10))
    call("St7CalculateBeamSectionProperties", 1, 1, c.c_ubyte(0))
    call("St7SetBeamMaterialData", 1, 1, doubles(*STEEL))
    call("St7SetMaterialName", 1, BEAM, 1, text("Steel S355"))
    for beam, i, j in ((1, 1, 2), (2, 2, 3)):
        call("St7SetElementConnection", 1, BEAM, beam, 1, connection(i, j))
    call("St7NewPlateProperty", 1, 1, 4, 1, text("Plate 200"))
    call("St7SetPlateThickness", 1, 1, doubles(200, 200))
    call("St7SetPlateIsotropicMaterial", 1, 1, doubles(210000, 0.3, 7.85e-9, 1.2e-5, 0, 0, 0, 0))
    call("St7SetMaterialName", 1, PLATE, 1, text("Steel S355"))
    call("St7SetElementConnection", 1, PLATE, 1, 1, connection(4, 5, 6, 7))
    # Cases: the new file has one load case and one freedom case.
    for getter in ("St7GetNumLoadCase", "St7GetNumFreedomCase"):
        count = c.c_int32()
        call(getter, 1, c.byref(count))
        if count.value != 1:
            raise RuntimeError("Unexpected new-file case defaults")
    call("St7SetFreedomCaseName", 1, 1, text("Freedom Case 1"))
    # St7NewFreedomCase refuses an existing name (error 35); the second freedom case is renamed after its creation.
    call("St7NewFreedomCase", 1, text("Freedom Case 2"))
    call("St7SetFreedomCaseName", 1, 2, text("Freedom Case 1"))
    call("St7SetLoadCaseName", 1, 1, text("G1"))
    for name in ("Q", "G1 (3)", "W"):
        call("St7NewLoadCase", 1, text(name))
    call("St7SetLoadCaseName", 1, 3, text("G1"))  # As for the freedom cases: the repeated name only by renaming.
    for case in (1, 3):
        call("St7SetLoadCaseType", 1, case, 1)  # lcGravity
        call("St7SetLoadCaseGravityDir", 1, case, 3)
        call("St7SetLoadCaseGravity", 1, case, c.c_double(-9806.65))
        call("St7SetLoadCaseMassOption", 1, case, c.c_ubyte(1), c.c_ubyte(1))
    call("St7SetBeamPointForceGlobal4ID", 1, 1, 2, 1, doubles(0, 0, -10000, 0.5))  # Mid-span of beam 1: a free DOF in both freedom cases.
    call("St7SetBeamDistributedForceGlobal6ID", 1, 2, 3, 0, 4, 0, 1, doubles(-2, 0, 0, 0, 0, 0))
    call("St7SetNodeForce3", 1, 5, 4, doubles(0, 0, -500))
    # Freedom cases.
    zero = doubles(0, 0, 0, 0, 0, 0)
    for freedom in (1, 2):
        call("St7SetNodeRestraint6", 1, 1, freedom, 1, integers(1, 1, 1, 1, 0, 0), zero)
        call("St7SetNodeRestraint6", 1, 3, freedom, 1, integers(0, 1, 1, 0, 0, 0), zero)
        for node in (4, 7):
            call("St7SetNodeRestraint6", 1, node, freedom, 1, integers(1, 1, 1, 1, 1, 1), zero)
    call("St7SetNodeKTranslation3F", 1, 2, 1, 1, doubles(0, 0, 200))
    call("St7SetNodeRestraint6", 1, 2, 2, 1, integers(0, 1, 1, 0, 0, 0), zero)
    # LSA combinations: factors by (load case, freedom case).
    for name, factors in (("ULS1", {(1, 1): 1.35, (4, 1): 1.5}), ("ULS2", {(3, 2): 1.35, (2, 2): 1.5}),
                          ("MIX", {(1, 1): 1.0, (2, 2): 1.0}), ("UNSOLVED", {(2, 1): 1.0})):
        call("St7AddLSACombination", 1, text(name))
        count = c.c_int32()
        call("St7GetNumLSACombinations", 1, c.byref(count))
        for (case, freedom), factor in factors.items():
            call("St7SetLSACombinationFactor", 1, 0, count.value, case, freedom, c.c_double(factor))
    saved = {(1, 1), (4, 1), (2, 2), (3, 2)}
    for result, enabled in (("freedom-shared.LSA", saved | {(2, 1)}), ("freedom-fc1.LSA", {(1, 1), (4, 1)}), ("freedom.LSA", saved)):
        pairs(enabled)
        call("St7SetResultFileName", 1, text(str(output / result)))
        call("St7SaveFile", 1)
        call("St7RunSolver", 1, 1, 3, 1)  # Linear static, background, wait; fixture only.
        names(output / result)
    call("St7SaveFile", 1)
    print(f"Native freedom-case fixture solved: {model}")
finally:
    try:
        if opened:
            call("St7CloseFile", 1)
    finally:
        call("St7Release")
