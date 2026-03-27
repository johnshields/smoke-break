"""
Saves Controller
Save/load/delete business logic against D1 (SQLite).
"""

import secrets
from models.save import to_db_params, from_db_row


def _gen_uid(prefix: str) -> str:
    hex_part = secrets.token_hex(4).upper()[:6]
    return f"{prefix}_{hex_part}"


async def upload_save(db, data: dict) -> dict:
    uid = _gen_uid("SAV")

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
            updated_at      = strftime('%Y-%m-%dT%H:%M:%fZ', 'now'),
            deleted_at      = NULL
    """).bind(*to_db_params(data, uid)).run()

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
    return {"status": "success", "data": from_db_row(row)}


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
