"""
Save Routes
HTTP endpoints for player save data.
"""

import json
from workers import Response
from controllers import saves_controller


def _json(data: dict, status: int = 200) -> Response:
    return Response(
        json.dumps(data),
        status=status,
        headers={"Content-Type": "application/json"},
    )


async def upload_save(db, request) -> Response:
    body = await request.json()
    result = await saves_controller.upload_save(db, body)
    return _json(result)


async def download_save(db, player_id: str) -> Response:
    result = await saves_controller.download_save(db, player_id)
    if result is None:
        return _json({"status": "error", "message": "Save not found."}, 404)
    return _json(result)


async def delete_save(db, player_id: str) -> Response:
    if not await saves_controller.delete_save(db, player_id):
        return _json({"status": "error", "message": "Save not found."}, 404)
    return _json({"status": "success", "message": "Save deleted."})
