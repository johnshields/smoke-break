"""
Save Routes
HTTP endpoints for player save data.
"""

from fastapi import APIRouter, HTTPException
from pydantic import BaseModel
from controllers import saves_controller

router = APIRouter(prefix="/api/saves", tags=["Saves"])


class SaveData(BaseModel):
    player_id: str = ""
    saved_at: str = ""
    playerX: float = 0.0
    playerY: float = 0.0
    playerZ: float = 0.0
    playerHealth: int = 0
    clipAmmo: int = 0
    storedAmmo: int = 0
    savedLevel: str = ""


@router.post("")
def upload_save(save_data: SaveData):
    return saves_controller.upload_save(save_data.model_dump())


@router.get("/{player_id}")
def download_save(player_id: str):
    result = saves_controller.download_save(player_id)
    if result is None:
        raise HTTPException(status_code=404, detail="Save not found.")
    return result


@router.delete("/{player_id}")
def delete_save(player_id: str):
    if not saves_controller.delete_save(player_id):
        raise HTTPException(status_code=404, detail="Save not found.")
    return {"status": "success", "message": "Save deleted."}