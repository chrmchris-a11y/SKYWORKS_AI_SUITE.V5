import json, urllib.request, urllib.error, sys

def post_json(url, data):
    req = urllib.request.Request(url, data=json.dumps(data).encode('utf-8'), headers={'Content-Type': 'application/json'})
    with urllib.request.urlopen(req, timeout=10) as resp:
        return resp.status, json.loads(resp.read().decode('utf-8'))

try:
    # .NET API info smoke
    try:
        with urllib.request.urlopen('http://localhost:5210/api/sora/info', timeout=10) as resp:
            info = resp.status, json.loads(resp.read().decode('utf-8'))
        print('DOTNET INFO:', json.dumps(info, ensure_ascii=False))
    except Exception as e:
        print('DOTNET INFO ERROR:', repr(e))
    with urllib.request.urlopen('http://localhost:8001/health', timeout=10) as resp:
        health = resp.status, json.loads(resp.read().decode('utf-8'))
    print('HEALTH:', json.dumps(health, ensure_ascii=False))

    status_20, body_20 = post_json('http://localhost:8001/api/v1/calculate/sail', {
        'sora_version': '2.0', 'final_grc': 5, 'residual_arc': 'b'
    })
    print('SORA 2.0 RESPONSE:', json.dumps({'status': status_20, 'body': body_20}, ensure_ascii=False))

    status_25, body_25 = post_json('http://localhost:8001/api/v1/calculate/sail', {
        'sora_version': '2.5', 'final_grc': 10, 'residual_arc_level': 10
    })
    print('SORA 2.5 RESPONSE:', json.dumps({'status': status_25, 'body': body_25}, ensure_ascii=False))

    # .NET API golden checks (/api/sail/calculate on 5210) with minimal DTO shape
    # SORA 2.0: Expect finalGRC=5, residualArc='c', SAIL='IV'
    try:
        status_dotnet_20, body_dotnet_20 = post_json('http://localhost:5210/api/sail/calculate', {
            'soraVersion': '2.0',
            'finalGrc': 5,
            'residualArc': 'c'
        })
        print('DOTNET 2.0 RESPONSE:', json.dumps({'status': status_dotnet_20, 'body': body_dotnet_20}, ensure_ascii=False))
    except urllib.error.HTTPError as e:
        try:
            body = e.read().decode('utf-8')
        except Exception:
            body = ''
        print('DOTNET 2.0 ERROR:', e.code, body)
    except Exception as e:
        print('DOTNET 2.0 ERROR:', repr(e))

    # SORA 2.5: Expect finalGRC=10, residualArcLevel=10, SAIL='VI'
    try:
        status_dotnet_25, body_dotnet_25 = post_json('http://localhost:5210/api/sail/calculate', {
            'soraVersion': '2.5',
            'finalGrc': 10,
            'residualArcLevel': 10
        })
        print('DOTNET 2.5 RESPONSE:', json.dumps({'status': status_dotnet_25, 'body': body_dotnet_25}, ensure_ascii=False))
    except urllib.error.HTTPError as e:
        try:
            body = e.read().decode('utf-8')
        except Exception:
            body = ''
        print('DOTNET 2.5 ERROR:', e.code, body)
    except Exception as e:
        print('DOTNET 2.5 ERROR:', repr(e))

except Exception as e:
    print('ERROR:', repr(e))
    sys.exit(1)
