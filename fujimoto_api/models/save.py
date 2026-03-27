"""
Save Model
Field mapping between client payload and DB row for the saves table.
"""

from utils.json_columns import pack, unpack

POSITION_KEYS = {"x": "playerX", "y": "playerY", "z": "playerZ"}
AMMO_KEYS = {"clip": "clipAmmo", "stored": "storedAmmo"}


def to_db_params(data: dict, uid: str) -> tuple:
    return (
        uid,
        data.get("player_id", ""),
        pack(data, POSITION_KEYS),
        data.get("playerHealth", 0),
        pack(data, AMMO_KEYS),
        data.get("savedLevel", ""),
        data.get("saved_at", ""),
    )


def from_db_row(row) -> dict:
    position = unpack(row, "player_position")
    ammo = unpack(row, "player_ammo")

    return {
        "player_id": row["player_id"],
        "saved_at": row["saved_at"],
        "playerX": position.get("x", 0.0),
        "playerY": position.get("y", 0.0),
        "playerZ": position.get("z", 0.0),
        "playerHealth": row["player_health"],
        "clipAmmo": ammo.get("clip", 0),
        "storedAmmo": ammo.get("stored", 0),
        "savedLevel": row["saved_level"],
    }
