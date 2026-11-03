"""Freeze F3 evidence after the complete suites and the two-runtime probe. Never overwrite an existing manifest."""
from pathlib import Path
import csv
import hashlib
import json
import statistics
import xml.etree.ElementTree as ET

model = Path(__file__).resolve().parents[2]
workspace = model.parent
artifacts = workspace / "artifacts/f3"
destination = model / "docs/bridghe-arachne/f3-evidenze.json"
if destination.exists():
    raise SystemExit("F3 evidence already frozen; do not replace it.")


def digest(path):
    h = hashlib.sha256()
    with path.open("rb") as stream:
        for block in iter(lambda: stream.read(1024 * 1024), b""):
            h.update(block)
    return h.hexdigest().upper()


tests = []
for path in sorted((artifacts / "model-tests-final").glob("*.trx")):
    root = ET.parse(path).getroot()
    outcomes = [e.attrib["outcome"] for e in root.iter() if e.tag.endswith("}UnitTestResult")]
    assert outcomes and all(o in ["Passed", "NotExecuted"] for o in outcomes), path
    tests.append(dict(suite=path.stem, passed=outcomes.count("Passed"), skipped=outcomes.count("NotExecuted")))
assert len(tests) == 3
checker = json.loads((artifacts / "checker-tests-final/summary.json").read_text(encoding="utf-8-sig"))
assert len(checker["results"]) == 11 and all(s["failed"] == 0 for s in checker["results"])
anthea = []
for tag in ["gpc-f3-before", "gpc-f3-shared-final", "gpc-f3-default-final"]:
    matches = list((workspace / "ANTHEA/supporto/artefatti/ci").glob("*-" + tag + "/summary.json"))
    assert len(matches) == 1, (tag, matches)
    summary = json.loads(matches[0].read_text(encoding="utf-8-sig"))
    assert len(summary["results"]) == 4 and all(r["Status"] == "PASS" for r in summary["results"])
    anthea.append(dict(tag=tag, summary=str(matches[0].relative_to(workspace)), sha256=digest(matches[0]), snapshot=summary["libManifestSha256"], results=[dict(suite=r["Suite"], status=r["Status"]) for r in summary["results"]]))
assert len(set(r["snapshot"] for r in anthea)) == 1

probes = []
for runtime in ["net8", "net48"]:
    path = artifacts / ("probe-" + runtime + "-final/metrics.csv")
    values = dict(list(csv.reader(path.read_text(encoding="utf-8-sig").splitlines()))[1:])
    assert values["status"] == "PASS"
    warm = [float(values["unchangedSave" + str(i) + "Seconds"]) for i in range(5)]
    probes.append(dict(runtime=runtime, values=values, unchangedSaveMedianSeconds=statistics.median(warm)))
for name in ["read-net8-on-framework-final.log", "read-framework-on-net8-final.log"]:
    assert "status,PASS" in (artifacts / name).read_text(encoding="utf-8-sig")

sources = list((model / "GPC.Archive").glob("*.cs")) + list((model / "GPC.Archive").glob("*.csproj"))
sources += [model / p for p in ["GPC.Model.Persistence/ExternalModelArchive.cs", "GPC.Model.Persistence/ModelDocument.cs", "GPC.Model.Persistence/GPC.Model.Persistence.csproj", "tests/ExternalArchiveTest.cs", "tests/Fixtures/anthea-blocks-v1-20261103.bin", "tests/Fixtures/anthea-blocks-v1-20261103.json", "tools/ExternalArchiveProbe/Program.cs", "tools/ExternalArchiveProbe/ExternalArchiveProbe.csproj"]]
sources += [workspace / p for p in ["ANTHEA/src/ANTHEA.ModelWorkspace/ModelResultArchive.Shared.cs", "ANTHEA/src/ANTHEA.ModelWorkspace/ANTHEA.ModelWorkspace.csproj", "Checker/build/Release-Set.ps1"]]
files = sorted(set(p for p in artifacts.rglob("*") if p.is_file()) | set(sources))
files += sorted((workspace / "ANTHEA/supporto/artefatti/gpc-f3-shared-binaries").glob("*.dll"))
payload = dict(phase="F3", date="2026-11-03", branch="codex/gpc-unification-20261102", status="development-verified", modelTests=tests,
               checkerPassed=sum(s["passed"] for s in checker["results"]), checkerExecutions=len(checker["results"]),
               anthea=anthea, probes=probes, crossRuntime="PASS both directions, binary numeric values + input fingerprint + solver map",
               exclusions=["ANTHEA production snapshot and full WPF remain F7/F8", "GH component and native solver pilots remain F4", "256 MiB limit per block and index", "F1/F2 historical evidence unchanged"],
               files=[dict(path=str(p.relative_to(workspace)).replace("\\", "/"), bytes=p.stat().st_size, sha256=digest(p)) for p in files])
with destination.open("x", encoding="utf-8", newline="\r\n") as stream:
    json.dump(payload, stream, indent=2, ensure_ascii=False)
    stream.write("\n")
print(json.dumps(dict(files=len(payload["files"]), modelPassed=sum(t["passed"] for t in tests), modelSkipped=sum(t["skipped"] for t in tests), checkerPassed=payload["checkerPassed"], probes=[dict(runtime=p["runtime"], median=p["unchangedSaveMedianSeconds"]) for p in probes]), indent=2))
