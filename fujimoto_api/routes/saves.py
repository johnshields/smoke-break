"""
Save Routes
HTTP endpoints for player save data.
"""

from controllers import saves_controller
from utils.response import json_response, json_error


async def upload_save(db, request):
    body = await request.json()
    result = await saves_controller.upload_save(db, body)
    return json_response(result)


async def download_save(db, player_id: str):
    result = await saves_controller.download_save(db, player_id)
    if result is None:
        return json_error("Save not found.", 404)
    return json_response(result)


async def delete_save(db, player_id: str):
    if not await saves_controller.delete_save(db, player_id):
        return json_error("Save not found.", 404)
    return json_response({"status": "success", "message": "Save deleted."})
