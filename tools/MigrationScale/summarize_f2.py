"""Freeze the final F2 evidence; keep failed intermediate attempts identifiable."""
import csv
from datetime import datetime, timezone
import hashlib
import json
from pathlib import Path
import statistics
import xml.etree.ElementTree as ET

workspace = Path(__file__).resolve().parents[3]
artifacts = workspace / "artifacts/f2"
docs = workspace / "Model/docs/bridghe-arachne"
ns = {"t": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}

def record(path):
    digest = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1048576), b""):
            digest.update(block)
    return dict(path=str(path.relative_to(workspace)).replace("\\", "/"), bytes=path.stat().st_size, sha256=digest.hexdigest().upper())

tests = {}
for path in sorted((artifacts / "model-tests-final").glob("*.trx")):
    rows = ET.parse(path).findall(".//t:UnitTestResult", ns)
    tests[path.stem] = dict(passed=sum(r.attrib["outcome"] == "Passed" for r in rows),
        failed=[r.attrib["testName"] for r in rows if r.attrib["outcome"] == "Failed"],
        skipped=[r.attrib["testName"] for r in rows if r.attrib["outcome"] not in ("Passed", "Failed")])
assert len(tests) == 3 and all(not t["failed"] for t in tests.values())
checker = json.loads((artifacts / "checker-tests-final/summary.json").read_text(encoding="utf-8-sig"))
assert len(checker["results"]) == 11 and all(r["failed"] == 0 for r in checker["results"])
scales = {}
for runtime in ("net8", "net48"):
    rows = list(csv.DictReader((artifacts / ("scale-" + runtime + "-final.csv")).open(encoding="utf-8-sig")))
    large = [r for r in rows if r["nodes"] == "10000"]
    warm = [r for r in large if r["operation"] == "copy-warm"]
    assert len(warm) == 5 and len(rows) == 30
    times = [float(r["seconds"]) for r in warm]
    allocations = [int(r["threadAllocatedBytes"]) for r in warm]
    first = float(next(r["seconds"] for r in large if r["operation"] == "copy-first"))
    scales[runtime] = dict(runtime=warm[0]["runtime"], medianCopySeconds=statistics.median(times), warmCopySeconds=times,
        firstCopySeconds=first, medianAllocatedBytes=statistics.median(allocations), allMeasurements=rows,
        medianTargetPass=statistics.median(times) <= 0.3, everyWarmCopyWithinTarget=max(times) <= 0.3,
        firstCopyWithinTarget=first <= 0.3)
assert all(s["medianTargetPass"] for s in scales.values()), "The F1 median protocol target is not met. Do not silently relax it."
binaries = [workspace / "Model/tools/MigrationScale/bin/Release" / runtime / "GPCModel.dll" for runtime in ("net8.0", "net48")]
assert record(binaries[0])["sha256"] == record(binaries[1])["sha256"]
matrix = json.loads((docs / "f2-destinazioni.json").read_text(encoding="utf-8"))
report = dict(schemaVersion=1, phase="F2", capturedUtc=datetime.now(timezone.utc).isoformat(), modelVersion="5.39.0.0",
    phaseCompleteForDeclaredScope=True, performanceCriterion="Median of five warm copies, identical F1 protocol; first-copy and worst-case are reported separately and are NOT bounded by 0.3 s.",
    model=tests, checker=checker, scale=scales, inventory=dict(types=matrix["typeCount"], members=matrix["memberCount"]),
    limits=["Production plugins not yet migrated", "Field routing includes explicit F4/F5 contract and unit gaps", "No universal <=0.3 s latency bound, especially first copy on net48",
        "ModelAssembly supports explicit nodes/two-node beams/linear shells; other input kinds rejected", "Warped shell support must be declared per solver formulation", "F3/F4 not implemented in this checkpoint"],
    intermediateAttempts="All intermediate logs/TRX retained. Only model-tests-final, checker-tests-final and scale-*-final are release-checkpoint evidence. Earlier failures include SDK sandbox access, development compile/test failures and the equality audit corrected before the final run.",
    files=[record(p) for p in sorted(artifacts.rglob("*")) if p.is_file()] + [record(p) for p in binaries] + [record(docs / name) for name in ("f2-inventario.json", "f2-destinazioni.json")])
with (docs / "f2-evidenze.json").open("x", encoding="utf-8", newline="\r\n") as stream:
    json.dump(report, stream, ensure_ascii=False, indent=2)
    stream.write("\n")
print(json.dumps(dict(modelPassed=sum(t["passed"] for t in tests.values()), modelSkipped=sum(len(t["skipped"]) for t in tests.values()),
    checkerPassed=sum(r["passed"] for r in checker["results"]), copies={key: {k:v for k,v in value.items() if k != "allMeasurements"} for key,value in scales.items()}), indent=2))
