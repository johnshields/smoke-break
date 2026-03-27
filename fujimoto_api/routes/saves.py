"""
Save Routes
HTTP endpoints for player save data.
"""

from fastapi import APIRouter, HTTPException, Request
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
async def upload_save(request: Request, save_data: SaveData):
    db = request.app.state.db
    return await saves_controller.upload_save(db, save_data.model_dump())


@router.get("/{player_id}")
async def download_save(request: Request, player_id: str):
    db = request.app.state.db
    result = await saves_controller.download_save(db, player_id)
    if result is None:
        raise HTTPException(status_code=404, detail="Save not found.")
    return result


@router.delete("/{player_id}")
async def delete_save(request: Request, player_id: str):
    db = request.app.state.db
    if not await saves_controller.delete_save(db, player_id):
        raise HTTPException(status_code=404, detail="Save not found.")
    return {"status": "success", "message": "Save deleted."}
