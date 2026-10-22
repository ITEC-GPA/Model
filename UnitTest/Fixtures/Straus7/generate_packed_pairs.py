"""Native geometry and six-component action benchmark for eight mirrored C/L pairs.
Usage: python generate_packed_pairs.py <absolute-St7api.dll> <NEW-output-directory>
Requires licensed Straus7 R3 x64. Creates/solves only a new disposable model.
This is an import/axis benchmark, not a validation of the structural connection.
"""
import ctypes as c
import json
import os
from pathlib import Path
import sys

dll_path = Path(sys.argv[1]).resolve(strict=True)
output = Path(sys.argv[2]).resolve()
output.mkdir(parents=True, exist_ok=False)
search = os.add_dll_directory(str(dll_path.parent))
api = c.WinDLL(str(dll_path))

def call(name, *args):
    code = getattr(api, name)(*args)
    if code:
        message = c.create_string_buffer(1024)
        api.St7GetAPIErrorString(code, message, 1024)
        raise RuntimeError(f"{name}: {code}: {message.value.decode('mbcs')}")

def doubles(*values):
    return (c.c_double * len(values))(*values)

def integers(*values):
    return (c.c_int32 * len(values))(*values)

call("St7SetLicenceOptions", 2, 0, 0)
call("St7Init")
opened = False
try:
    call("St7NewFile", 1, str(output / "packed-pairs.st7").encode('mbcs'), str(output).encode('mbcs'))
    opened = True
    call("St7SetUnits", 1, integers(2, 0, 2, 0, 0, 0))  # mm, N, MPa, kg.
    for getter, creator in [("St7GetNumLoadCase", "St7NewLoadCase"), ("St7GetNumFreedomCase", "St7NewFreedomCase")]:
        count = c.c_int32()
        call(getter, 1, c.byref(count))
        if count.value == 0:
            call(creator, 1, b"Pairs")
        elif count.value != 1:
            raise RuntimeError("Unexpected case defaults")
    call("St7SetLoadCaseName", 1, 1, b"Pairs")
    rows = []
    for number, (kind, mirror) in enumerate([(k, m) for k in ['L', 'C-standard'] for m in [1, 2, 3, 4]], 1):
        i, j = 2 * number - 1, 2 * number
        call("St7SetNodeXYZ", 1, i, doubles(0, 1000 * number, 0))
        call("St7SetNodeXYZ", 1, j, doubles(4000, 1000 * number, 0))
        call("St7NewBeamProperty", 1, number, 6, (kind + str(mirror)).encode())
        geometry = [100, 150, 0, 12, 10, 0] if kind == 'L' else [75, 200, 0, 12, 10, 0]
        call("St7SetBeamSectionGeometry", 1, number, 9 if kind == 'L' else 5, doubles(*geometry))
        call("St7SetBeamMirrorOption", 1, number, mirror, 1, doubles(10, 0))
        call("St7CalculateBeamSectionProperties", 1, number, c.c_ubyte(0))
        call("St7SetBeamMaterialData", 1, number, doubles(210000, 80769.23076923, .3, 0, 1.2e-5, 0, 0, 0, 0))
        call("St7SetMaterialName", 1, 1, number, b"Steel S355")
        call("St7SetElementConnection", 1, 1, number, number, integers(2, i, j, *([0] * 18)))
        call("St7SetNodeRestraint6", 1, i, 1, 1, integers(1, 1, 1, 1, 1, 1), doubles(0, 0, 0, 0, 0, 0))
        call("St7SetNodeForce3", 1, j, 1, doubles(-10000, 200, 300))
        call("St7SetNodeMoment3", 1, j, 1, doubles(7000, 4000, -6000))
        properties, axes = doubles(*([0] * 11)), doubles(*([0] * 9))
        call("St7GetBeamSectionPropertyData", 1, number, integers(0), properties)
        call("St7GetBeamAxisSystemInitial", 1, number, axes)
        rows.append(dict(kind=kind, mirror=mirror, gap=10, geometry=geometry, properties=list(properties), axes=list(axes)))
    (output / "properties.json").write_text(json.dumps(rows, indent=2) + '\n')
    call("St7EnableLSALoadCase", 1, 1, 1)
    call("St7SetEntityResult", 1, 21, c.c_ubyte(1))
    call("St7SaveFile", 1)
    call("St7RunSolver", 1, 1, 3, 1)
    print(f"Native pair fixture solved: {output}")
finally:
    try:
        if opened:
            call("St7CloseFile", 1)
    finally:
        call("St7Release")
        search.close()
