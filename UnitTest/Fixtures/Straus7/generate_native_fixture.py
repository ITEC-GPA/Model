"""Optional integration fixture; requires installed/licensed Windows x64 Straus7 R3.

Usage: python generate_native_fixture.py <absolute-St7api.dll> <NEW-output-directory>
Creates and solves ONLY a new benchmark model, never opens an existing user model.
The production converter neither creates nor solves models.
"""
import ctypes as c
import os
from pathlib import Path
import sys

dll_path = Path(sys.argv[1]).resolve(strict=True)
output = Path(sys.argv[2]).resolve()
output.mkdir(parents=True, exist_ok=False)  # No overwrite of existing analyses.
model = output / "benchmark.st7"
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


call("St7SetLicenceOptions", 2, 0, 0)  # lmAbort: no licence dialogs.
call("St7Init")
opened = False
try:
    call("St7NewFile", 1, str(model).encode('mbcs'), str(output).encode('mbcs'))
    opened = True
    call("St7SetUnits", 1, integers(2, 0, 2, 0, 0, 0))  # mm, N, MPa, kg, Celsius, joule.
    for number, xyz in enumerate([(0, 0, 0), (0, 0, 2000),
                                 (3000, 0, 0), (4000, 0, 0), (4000, 1000, 0), (3000, 1000, 0)], 1):
        call("St7SetNodeXYZ", 1, number, doubles(*xyz))
    call("St7NewBeamProperty", 1, 1, 6, b"Rectangular benchmark")
    call("St7SetBeamSectionGeometry", 1, 1, 3, doubles(200, 300, 0, 0, 0, 0))
    call("St7SetBeamMaterialData", 1, 1, doubles(30000, 15000, 0, 0, 0, 0, 0, 0, 0))
    call("St7SetElementConnection", 1, 1, 1, 1, integers(2, 1, 2, *([0] * 18)))
    call("St7NewPlateProperty", 1, 1, 4, 1, b"Membrane tension benchmark")
    call("St7SetPlateThickness", 1, 1, doubles(100, 100))
    call("St7SetPlateIsotropicMaterial", 1, 1, doubles(30000, 0, 0, 0, 0, 0, 0, 0))
    call("St7SetElementConnection", 1, 2, 1, 1, integers(4, 3, 4, 5, 6, *([0] * 16)))
    for getter, creator in [("St7GetNumLoadCase", "St7NewLoadCase"), ("St7GetNumFreedomCase", "St7NewFreedomCase")]:
        count = c.c_int32()
        call(getter, 1, c.byref(count))
        if count.value == 0:
            call(creator, 1, b"Benchmark")
        elif count.value != 1:
            raise RuntimeError("Unexpected new-file case defaults")
    call("St7SetLoadCaseName", 1, 1, b"Benchmark")
    for node in (1, 3, 6):
        call("St7SetNodeRestraint6", 1, node, 1, 1, integers(1, 1, 1, 1, 1, 1), doubles(0, 0, 0, 0, 0, 0))
    call("St7SetNodeForce3", 1, 2, 1, doubles(100, 200, 300))
    call("St7SetNodeMoment3", 1, 2, 1, doubles(10, 20, 30))
    for node in (4, 5):
        call("St7SetNodeForce3", 1, node, 1, doubles(500, 0, 0))
    call("St7EnableLSALoadCase", 1, 1, 1)
    call("St7NewLoadCase", 1, b"Bending")
    for node in (4, 5):
        call("St7SetNodeMoment3", 1, node, 2, doubles(0, 500, 0))
    call("St7EnableLSALoadCase", 1, 2, 1)
    call("St7SetEntityResult", 1, 21, c.c_ubyte(1))  # srElementNodeForce: store per-element nodal actions.
    call("St7SaveFile", 1)
    call("St7RunSolver", 1, 1, 3, 1)  # Linear static, background, wait; fixture only.
    print(f"Native fixture solved: {model}")
finally:
    try:
        if opened:
            call("St7CloseFile", 1)
    finally:
        call("St7Release")
        search.close()
