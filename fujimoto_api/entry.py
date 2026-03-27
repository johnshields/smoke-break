"""
fujimoto API
Cloudflare Workers entrypoint — routing, CORS, error handling, and request logging.
"""

import json
import time
from urllib.parse import urlparse
from workers import WorkerEntrypoint, Response
from app import config
from routes import routes, saves
_started_at = time.time()


def _json(data: dict, status: int = 200) -> Response:
    return Response(
        json.dumps(data),
        status=status,
        headers={"Content-Type": "application/json"},
    )


def _cors_headers() -> dict:
    return {
        "Access-Control-Allow-Origin": config.CORS_ORIGINS,
        "Access-Control-Allow-Methods": "GET, POST, DELETE, OPTIONS",
        "Access-Control-Allow-Headers": "Content-Type",
        "Access-Control-Allow-Credentials": "true",
    }


def _parse_path(url: str) -> str:
    return urlparse(url).path.rstrip("/") or "/"


class Default(WorkerEntrypoint):
    async def fetch(self, request):
        db = self.env.DB
        method = request.method
        path = _parse_path(request.url)
        start = time.time()

        if method == "OPTIONS":
            return Response("", status=204, headers=_cors_headers())

        try:
            response = await self._route(db, method, path, request)
        except Exception as e:
            print(f"[error]: Unhandled exception on [{method}] {path}: {e}")
            response = _json({"status": "error", "message": "Internal server error."}, 500)

        for key, value in _cors_headers().items():
            response.headers[key] = value

        duration_ms = round((time.time() - start) * 1000)
        print(f"[info]: [{method}] {path} - {response.status} - Took {duration_ms}ms")

        return response

    async def _route(self, db, method: str, path: str, request) -> Response:
        if path == "/" and method == "GET":
            return routes.health(_started_at, config.API_NAME)

        if path == "/api" and method == "GET":
            return routes.api_info(_started_at, config.API_NAME, config.VERSION)

        if path == "/api/saves" and method == "POST":
            return await saves.upload_save(db, request)

        if path.startswith("/api/saves/") and method == "GET":
            player_id = path.split("/api/saves/")[1]
            return await saves.download_save(db, player_id)

        if path.startswith("/api/saves/") and method == "DELETE":
            player_id = path.split("/api/saves/")[1]
            return await saves.delete_save(db, player_id)

        return _json({"status": "error", "message": "Not found."}, 404)
