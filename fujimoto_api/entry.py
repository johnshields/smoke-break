"""
fujimoto API
Cloudflare Workers entrypoint.
"""

import time
from workers import WorkerEntrypoint
from app import config
from app.logger import info, error
from middleware.auth import authenticate
from middleware.cors import preflight, apply
from routes import routes, saves
from utils.response import json_error, parse_path

_started_at = time.time()


class Default(WorkerEntrypoint):
    async def fetch(self, request):
        db = self.env.DB
        api_key = self.env.API_KEY
        method = request.method
        path = parse_path(request.url)
        start = time.time()

        if method == "OPTIONS":
            return preflight()

        auth_error = authenticate(request, path, api_key)
        if auth_error:
            return apply(auth_error)

        try:
            response = await self._route(db, method, path, request)
        except Exception as e:
            error(f"Unhandled exception on [{method}] {path}: {e}")
            response = json_error("Internal server error.", 500)

        duration_ms = round((time.time() - start) * 1000)
        info(f"[{method}] {path} - {response.status} - Took {duration_ms}ms")

        return apply(response)

    async def _route(self, db, method: str, path: str, request):
        if path in ("/", "/api") and method == "GET":
            return routes.api_info(_started_at, config.API_NAME, config.VERSION)

        if path == "/api/healthz" and method == "GET":
            return routes.health(_started_at, config.API_NAME)

        if path == "/api/readyz" and method == "GET":
            return await routes.readiness(db, _started_at)

        if path == "/api/saves" and method == "POST":
            return await saves.upload_save(db, request)

        if path.startswith("/api/saves/") and method == "GET":
            player_id = path.split("/api/saves/")[1]
            return await saves.download_save(db, player_id)

        if path.startswith("/api/saves/") and method == "DELETE":
            player_id = path.split("/api/saves/")[1]
            return await saves.delete_save(db, player_id)

        return json_error("Not found.", 404)
