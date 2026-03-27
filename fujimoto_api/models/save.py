"""
Save Model
Maps between the client payload, DB row, and API response for the saves table.
"""

import json


def to_db_params(data: dict, uid: str) -> tuple:
    position = json.dumps({
        "x": data.get("playerX", 0.0),
        "y": data.get("playerY", 0.0),
        "z": data.get("playerZ", 0.0),
    })

    return (
        uid,
        data.get("player_id", ""),
        position,
        data.get("playerHealth", 0),
        data.get("clipAmmo", 0),
        data.get("storedAmmo", 0),
        data.get("savedLevel", ""),
        data.get("saved_at", ""),
    )


def from_db_row(row) -> dict:
    position = json.loads(row["player_position"]) if row["player_position"] else {}

    return {
        "player_id": row["player_id"],
        "saved_at": row["saved_at"],
        "playerX": position.get("x", 0.0),
        "playerY": position.get("y", 0.0),
        "playerZ": position.get("z", 0.0),
        "playerHealth": row["player_health"],
        "clipAmmo": row["clip_ammo"],
        "storedAmmo": row["stored_ammo"],
        "savedLevel": row["saved_level"],
    }
