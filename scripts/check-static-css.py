#!/usr/bin/env python3
"""Check generated CSS fingerprints, gzip variants, and optional live HTTP responses.

Run after dotnet build BlazorBlueprint.sln:
  python3 scripts/check-static-css.py --base-url http://localhost:7172
"""
import argparse
import base64
import gzip
import hashlib
import json
from pathlib import Path
from urllib.request import Request, urlopen

root = Path(__file__).resolve().parent.parent
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--configuration', default='Debug')
parser.add_argument('--base-url', help='Optional running demo URL; verifies actual identity and gzip responses.')
args = parser.parse_args()
files = {
    root / 'src/BlazorBlueprint.Components/wwwroot/blazorblueprint.css': '_content/BlazorBlueprint.Components/blazorblueprint.css',
    root / 'demos/BlazorBlueprint.Demo.Shared/wwwroot/css/app.css': '_content/BlazorBlueprint.Demo.Shared/css/app.css',
}
projects = [
    'demos/BlazorBlueprint.Demo.Server',
    'demos/BlazorBlueprint.Demo.Wasm',
    'demos/BlazorBlueprint.Demo.Auto',
]
checks = 0
for project in projects:
    manifest = root / project / 'obj' / args.configuration / 'net10.0/staticwebassets.build.json'
    if not manifest.exists():
        raise SystemExit(f'Missing {manifest}. Build the solution first.')
    assets = json.loads(manifest.read_text())['Assets']
    for file in files:
        data = file.read_bytes()
        assert data, f'Empty generated CSS: {file}'
        primary = next((asset for asset in assets if asset['Identity'] == str(file)), None)
        assert primary, f'{manifest}: missing {file.name}'
        expected = base64.b64encode(hashlib.sha256(data).digest()).decode()
        assert primary['Integrity'] == expected, f'{manifest}: stale fingerprint for {file.name}'
        assert primary['FileLength'] == len(data), f'{manifest}: stale size for {file.name}'
        compressed = [asset for asset in assets if asset.get('RelatedAsset') == str(file) and asset.get('AssetTraitValue') == 'gzip']
        assert compressed, f'{manifest}: missing gzip variant for {file.name}'
        for asset in compressed:
            assert gzip.decompress(Path(asset['Identity']).read_bytes()) == data, f'Stale gzip variant: {asset["Identity"]}'
        checks += 1
    print(f'{project}: CSS metadata and gzip variants match')

if args.base_url:
    for file, route in files.items():
        for encoding in ('identity', 'gzip'):
            request = Request(f'{args.base_url.rstrip("/")}/{route}', headers={'Accept-Encoding': encoding})
            with urlopen(request, timeout=20) as response:
                data = response.read()
                assert response.status == 200 and response.headers.get_content_type() == 'text/css', f'Invalid CSS response: {route} ({encoding})'
                if response.headers.get('Content-Encoding') == 'gzip':
                    data = gzip.decompress(data)
                assert data == file.read_bytes(), f'Empty/stale CSS response: {route} ({encoding})'
            checks += 1
        print(f'{route}: live identity/gzip responses match generated CSS')
print(f'Passed {checks} CSS delivery checks.')
