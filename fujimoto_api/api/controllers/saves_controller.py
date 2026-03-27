"""
Saves Controller
Save/load/delete business logic against D1 (SQLite).
"""

from app.logger import info
from app.messages import SAVE_UPLOADED
from db.queries import saves as queries
from db.db import execute, fetch_one
from models.save import to_db_params, from_db_row
from utils.uid import gen_uid


async def upload_save(db, data: dict) -> dict:
    uid = gen_uid("SAV")

    await execute(db, queries.UPSERT, *to_db_params(data, uid))

    info(f"Save uploaded for player: {data.get('player_id')} [{uid}]")
    return {"status": "success", "message": SAVE_UPLOADED, "uid": uid}


async def download_save(db, player_id: str) -> dict | None:
    row = await fetch_one(db, queries.FIND_BY_PLAYER, player_id)

    if not row:
        return None

    info(f"Save downloaded for player: {player_id}")
    return {"status": "success", "data": from_db_row(row)}


async def delete_save(db, player_id: str) -> bool:
    row = await fetch_one(db, queries.EXISTS_BY_PLAYER, player_id)

    if not row:
        return False

    await execute(db, queries.SOFT_DELETE, player_id)

    info(f"Save soft-deleted for player: {player_id}")
    return True
