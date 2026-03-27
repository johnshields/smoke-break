"""
Saves Controller
Save/load/delete business logic against D1 (SQLite).
"""

import json
import secrets


def _gen_uid(prefix: str) -> str:
    hex_part = secrets.token_hex(4).upper()[:6]
    return f"{prefix}_{hex_part}"


async def upload_save(db, data: dict) -> dict:
    uid = _gen_uid("SAV")
    position = json.dumps({
        "x": data.get("playerX", 0.0),
        "y": data.get("playerY", 0.0),
        "z": data.get("playerZ", 0.0),
    })

    await db.prepare("""
        INSERT INTO saves (
            uid, player_id,
            player_position, player_health, clip_ammo, stored_ammo, saved_level,
            saved_at
        )
        VALUES (?, ?, ?, ?, ?, ?, ?, ?)
        ON CONFLICT(player_id) DO UPDATE SET
            player_position = excluded.player_position,
            player_health   = excluded.player_health,
            clip_ammo       = excluded.clip_ammo,
            stored_ammo     = excluded.stored_ammo,
            saved_level     = excluded.saved_level,
            saved_at        = excluded.saved_at,
            deleted_at      = NULL
    """).bind(
        uid,
        data.get("player_id", ""),
        position, data.get("playerHealth", 0), data.get("clipAmmo", 0),
        data.get("storedAmmo", 0), data.get("savedLevel", ""),
        data.get("saved_at", "")
    ).run()

    print(f"[info]: Save uploaded for player: {data.get('player_id')} [{uid}]")
    return {"status": "success", "message": "Save uploaded.", "uid": uid}


async def download_save(db, player_id: str) -> dict | None:
    row = await db.prepare("""
        SELECT * FROM saves
        WHERE player_id = ?
          AND deleted_at IS NULL
    """).bind(player_id).first()

    if not row:
        return None

    print(f"[info]: Save downloaded for player: {player_id}")
    return {"status": "success", "data": dict(row)}


async def delete_save(db, player_id: str) -> bool:
    row = await db.prepare("""
        SELECT player_id FROM saves
        WHERE player_id = ?
          AND deleted_at IS NULL
    """).bind(player_id).first()

    if not row:
        return False

    await db.prepare("""
        UPDATE saves
        SET deleted_at = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')
        WHERE player_id = ?
    """).bind(player_id).run()

    print(f"[info]: Save soft-deleted for player: {player_id}")
    return True
