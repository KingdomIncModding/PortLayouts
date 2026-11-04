"""Package an explicit, locally verified Port Layouts payload; no publication."""
from pathlib import Path, PurePosixPath
import hashlib
import json
import tempfile
import xml.etree.ElementTree as ET
import zipfile

PROJECT = Path(__file__).resolve().parents[1]
GAME = PROJECT.parents[1]
VERSION = ET.parse(PROJECT / 'PortLayouts.csproj').getroot().findtext('PropertyGroup/Version')


def digest(data):
    return hashlib.sha256(data).hexdigest()


def main():
    dll = (PROJECT / 'bin/Release/netstandard2.1/PortLayouts.dll').read_bytes()
    reports = [json.loads((PROJECT / 'Verification' / name).read_text())
               for name in ('runtime-tests.json', 'compatibility-tests.json')]
    for report in reports:
        assert report['passed'] and not report['error'] and report['count'] >= 300
        assert report['version'] == VERSION and report['dllSha256'] == digest(dll)
    assert set(reports[1]['companions']) == {'Party', 'Ledger', 'LeylineConduit'}
    assert dll == (GAME / 'BepInEx/plugins/PortLayouts/PortLayouts.dll').read_bytes()
    payload = {
        'BepInEx/plugins/PortLayouts/PortLayouts.dll': dll,
        'Port Layouts README.txt': (PROJECT / 'README.md').read_bytes(),
    }
    output = PROJECT / 'Releases'
    output.mkdir(exist_ok=True)
    archive = output / f'KingdomInc-PortLayouts-{VERSION}.zip'
    with zipfile.ZipFile(archive, 'w', compression=zipfile.ZIP_DEFLATED, compresslevel=9) as target:
        for name, data in sorted(payload.items()):
            path = PurePosixPath(name)
            assert not path.is_absolute() and '..' not in path.parts and ':' not in name
            target.writestr(name, data)
    with zipfile.ZipFile(archive) as check:
        assert check.testzip() is None and set(check.namelist()) == set(payload)
        with tempfile.TemporaryDirectory(prefix='PackageExtract-', dir=PROJECT / 'Verification') as folder:
            check.extractall(folder)
            actual = {p.relative_to(folder).as_posix(): p.read_bytes() for p in Path(folder).rglob('*') if p.is_file()}
            assert actual == payload
    result = {
        'name': 'Port Layouts', 'version': VERSION, 'publication': 'local only',
        'archive': str(archive), 'bytes': archive.stat().st_size, 'sha256': digest(archive.read_bytes()),
        'dllSha256': digest(dll), 'gameVersion': reports[0]['gameVersion'],
        'runtimeChecks': reports[0]['count'], 'companionChecks': reports[1]['count'],
        'files': sorted(payload), 'cleanExtractionPassed': True,
    }
    (output / 'release-verification.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result, indent=2))


if __name__ == '__main__':
    main()
