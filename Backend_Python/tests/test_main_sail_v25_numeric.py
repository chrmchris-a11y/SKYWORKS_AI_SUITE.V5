import pytest
from fastapi.testclient import TestClient

import importlib

app = importlib.import_module('main').app
client = TestClient(app)


def test_sail_v25_requires_numeric_arc():
    # Missing residual_arc_level should be 400 for 2.5
    resp = client.post('/api/v1/calculate/sail', json={
        'sora_version': '2.5',
        'final_grc': 4,
        'residual_arc': 'b'
    })
    assert resp.status_code == 400
    body = resp.json()
    assert 'residual_arc_level' in body['detail']


def test_sail_v25_numeric_mapping_example():
    # Given example: GRC=4, ARC=3 => SAIL III per official table
    resp = client.post('/api/v1/calculate/sail', json={
        'sora_version': '2.5',
        'final_grc': 4,
        'residual_arc_level': 3
    })
    assert resp.status_code == 200
    body = resp.json()
    assert body['sail'] == 'III'
    assert body['residual_arc_level'] == 3
    assert body['oso_count'] is None
    assert body.get('oso_count_source') == 'derived-2.5'


def test_sail_v25_additional_examples():
    # GRC=6, ARC=4 => SAIL V (per official numeric table)
    r1 = client.post('/api/v1/calculate/sail', json={
        'sora_version': '2.5',
        'final_grc': 6,
        'residual_arc_level': 4
    })
    assert r1.status_code == 200
    assert r1.json()['sail'] == 'V'

    # GRC<=2, ARC=4 => SAIL II
    r2 = client.post('/api/v1/calculate/sail', json={
        'sora_version': '2.5',
        'final_grc': 2,
        'residual_arc_level': 4
    })
    assert r2.status_code == 200
    assert r2.json()['sail'] == 'II'


def test_sail_v20_letter_and_category_c():
    # SORA 2.0 table: GRC 5 / ARC c => SAIL IV (EASA AMC/GM Annex D, Table D.1)
    r = client.post('/api/v1/calculate/sail', json={
        'sora_version': '2.0',
        'final_grc': 5,
        'residual_arc': 'c'
    })
    assert r.status_code == 200
    assert r.json()['sail'] == 'IV'
    assert r.json()['oso_count'] == 18

    # SORA 2.0: GRC > 7 => Category C
    rC = client.post('/api/v1/calculate/sail', json={
        'sora_version': '2.0',
        'final_grc': 8,
        'residual_arc': 'b'
    })
    assert rC.status_code == 200
    bodyC = rC.json()
    assert bodyC.get('category') == 'C'
    assert bodyC.get('sail') is None
