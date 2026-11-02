"""Freeze F1 evidence metadata. Run from any directory; never overwrites an evidence manifest."""
import csv
from datetime import datetime, timezone
import gzip
import hashlib
import json
from pathlib import Path
import statistics
import xml.etree.ElementTree as ET

workspace = Path(__file__).resolve().parents[3]
artifacts = workspace / 'artifacts/f1'
docs = workspace / 'Model/docs/bridghe-arachne'

def digest(path):
    sha = hashlib.sha256()
    with path.open('rb') as stream:
        for data in iter(lambda: stream.read(1048576), b''): sha.update(data)
    return sha.hexdigest().upper()

def file_record(path):
    return {'path': str(path.relative_to(workspace)).replace('\\', '/'), 'bytes': path.stat().st_size, 'sha256': digest(path)}

scales = {}
for name in ('scale-net8-baseline.csv', 'scale-net48-baseline.csv', 'bridghe-scale.csv', 'arachne-scale-complete.csv'):
    rows = list(csv.DictReader((artifacts / name).open(encoding='utf-8-sig')))
    large = [r for r in rows if r['nodes'] == '10000']
    warm = [r for r in large if r['operation'] == 'copy-warm']
    assert len(warm) == 5, name
    scales[name] = {'copyWarmSeconds': [float(r['seconds']) for r in warm],
                   'medianCopySeconds': statistics.median(float(r['seconds']) for r in warm),
                   'allocationsBytes': [int(r['threadAllocatedBytes']) for r in warm],
                   'measurements': rows}

cases = {}
for file in sorted((artifacts / 'arachne-tensile-sequential').glob('*-input.xml.gz')):
    result = file.with_name(file.name.replace('-input', '-result'))
    assert result.exists(), file
    root = ET.fromstring(gzip.decompress(file.read_bytes()))
    cases.setdefault(root.attrib['case'], []).append(file.stem.split('-')[0])

checker = json.loads((artifacts / 'checker-tests/summary.json').read_text(encoding='utf-8-sig'))
anthea = sorted((workspace / 'ANTHEA/supporto/artefatti/ci').glob('*-gpc-f1-complete'))[-1]
report = {'schemaVersion': 1, 'capturedUtc': datetime.now(timezone.utc).isoformat(), 'phase': 'F1',
          'phaseCompleteForDeclaredScope': True, 'report': 'F1-BASELINE.md',
          'checker': checker, 'scales': scales, 'tensileCases': cases,
          'straus': {key: json.loads((artifacts / ('native-' + key + '-traced/input-summary.json')).read_text()) for key in ('50', '99')},
          'anthea': {'commit': 'c4f68c5e69a88b4b0a13891fe2bd02db6afe7537',
                     'summary': json.loads((anthea / 'summary.json').read_text(encoding='utf-8-sig'))},
          'exclusions': ['ReBear project not located', 'Full ANTHEA WPF suite', 'Large multi-dataset result models',
                         'Independent persisted-file readback of every Straus7 attribute; transcript covers actual accepted API writes'],
          'files': [file_record(p) for p in sorted(artifacts.rglob('*')) if p.is_file()] +
                   [file_record(p) for p in sorted(anthea.rglob('*')) if p.is_file()]}
destination = docs / 'f1-evidenze.json'
with destination.open('x', encoding='utf-8') as stream: json.dump(report, stream, indent=2, ensure_ascii=False)
print(f"Frozen {len(report['files'])} files; {sum(r['passed'] for r in checker['results'])} Checker tests; {len(cases)} Tensile cases.")
