"""
Saves Controller
Handles save/load/delete business logic for player save data.
"""

import json
import os
from app.config import SAVES_DIR
from app.logger import info, error


def _save_path(player_id: str) -> str:
    return os.path.join(SAVES_DIR, f"{player_id}.json")


def upload_save(data: dict) -> dict:
    os.makedirs(SAVES_DIR, exist_ok=True)
    path = _save_path(data["player_id"])

    with open(path, "w") as f:
        json.dump(data, f, indent=2)

    info(f"Save uploaded for player: {data['player_id']}")
    return {"status": "success", "message": "Save uploaded."}


def download_save(player_id: str) -> dict | None:
    path = _save_path(player_id)

    if not os.path.exists(path):
        return None

    with open(path) as f:
        data = json.load(f)

    info(f"Save downloaded for player: {player_id}")
    return {"status": "success", "data": data}


def delete_save(player_id: str) -> bool:
    path = _save_path(player_id)

    if not os.path.exists(path):
        return False

    os.remove(path)
    info(f"Save deleted for player: {player_id}")
    return True