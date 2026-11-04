"""Prepare the tested Port Layouts DLL for its Port Rotation Nexus listing."""
from pathlib import Path
import hashlib
import json
import tempfile
import xml.etree.ElementTree as ET
import zipfile

PROJECT = Path(__file__).resolve().parents[1]
GAME = PROJECT.parents[1]
OUT = PROJECT / 'Releases' / 'Nexus'
VERSION = ET.parse(PROJECT / 'PortLayouts.csproj').getroot().findtext('PropertyGroup/Version')


def sha(data):
    return hashlib.sha256(data).hexdigest()


def main():
    dll = (PROJECT / 'bin/Release/netstandard2.1/PortLayouts.dll').read_bytes()
    assert dll == (GAME / 'BepInEx/plugins/PortLayouts/PortLayouts.dll').read_bytes()
    reports = [json.loads((PROJECT / 'Verification' / name).read_text())
               for name in ('runtime-tests.json', 'compatibility-tests.json')]
    for report in reports:
        assert report['passed'] and not report['error'] and report['count'] >= 300
        assert report['version'] == VERSION and report['dllSha256'] == sha(dll)
    assert set(reports[1]['companions']) == {'Party', 'Ledger', 'LeylineConduit'}
    payload = {
        'BepInEx/plugins/PortLayouts/PortLayouts.dll': dll,
        'Port Rotation README.txt': (PROJECT / 'Nexus/README.txt').read_bytes(),
        'Port Rotation CHANGELOG.txt': (PROJECT / 'Nexus/CHANGELOG.txt').read_bytes(),
    }
    OUT.mkdir(parents=True, exist_ok=True)
    archive = OUT / f'KingdomInc-PortRotation-{VERSION}.zip'
    with zipfile.ZipFile(archive, 'w', compression=zipfile.ZIP_DEFLATED, compresslevel=9) as output:
        for name, data in sorted(payload.items()):
            info = zipfile.ZipInfo(name, (2026, 11, 3, 0, 0, 0))
            info.compress_type = zipfile.ZIP_DEFLATED
            info.create_system = 3
            info.external_attr = 0o100644 << 16
            output.writestr(info, data)
    with zipfile.ZipFile(archive) as check:
        assert check.testzip() is None and set(check.namelist()) == set(payload)
        with tempfile.TemporaryDirectory(prefix='NexusPackageExtract-', dir=PROJECT / 'Verification') as folder:
            check.extractall(folder)
            actual = {p.relative_to(folder).as_posix(): p.read_bytes() for p in Path(folder).rglob('*') if p.is_file()}
            assert actual == payload
    thumbnail = PROJECT / 'Assets/Thumbnails/port-rotation-thumbnail-v1.png'
    (OUT / thumbnail.name).write_bytes(thumbnail.read_bytes())
    screenshots = OUT / 'Screenshots'
    screenshots.mkdir(exist_ok=True)
    for name in ('port-layouts.png', 'top-bottom-pipe-alignment.png'):
        (screenshots / name).write_bytes((PROJECT / 'Verification' / name).read_bytes())
    for name in ('SUMMARY.txt', 'NEXUS_DESCRIPTION.bbcode.txt', 'CHANGELOG.txt'):
        (OUT / name).write_bytes((PROJECT / 'Nexus' / name).read_bytes())
    result = {
        'name': 'Port Rotation', 'internalPlugin': 'Port Layouts', 'version': VERSION,
        'archive': str(archive), 'bytes': archive.stat().st_size, 'sha256': sha(archive.read_bytes()),
        'dllSha256': sha(dll), 'gameVersion': reports[0]['gameVersion'],
        'runtimeChecks': reports[0]['count'], 'companionChecks': reports[1]['count'],
        'files': sorted(payload), 'cleanExtractionPassed': True,
        'thumbnail': str(OUT / thumbnail.name), 'thumbnailSha256': sha(thumbnail.read_bytes()),
        'publication': 'prepared; verify destination separately',
    }
    (OUT / 'RELEASE_VERIFICATION.json').write_text(json.dumps(result, indent=2) + '\n')
    print(json.dumps(result, indent=2))


if __name__ == '__main__':
    main()
