"""
JSON Helpers
Pack and unpack JSON columns for D1 storage.
"""

import json


def pack(data: dict, keys: dict) -> str:
    return json.dumps({k: data.get(v, 0) for k, v in keys.items()})


def unpack(row, column: str) -> dict:
    return json.loads(row[column]) if row[column] else {}
