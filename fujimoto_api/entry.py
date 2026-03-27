"""
fujimoto API
Cloudflare Workers entrypoint.
"""

import time
from workers import WorkerEntrypoint
from app.logger import error
from app.messages import INTERNAL_ERROR
from api.middleware.auth import authenticate
from api.middleware.cors import preflight, apply
from api.middleware.request_logger import log_request
from api.router import resolve
from utils.response import json_error, parse_path

_started_at = time.time()


class Default(WorkerEntrypoint):
    async def fetch(self, request):
        method = request.method
        path = parse_path(request.url)
        start = time.time()

        if method == "OPTIONS":
            return preflight()

        try:
            db = self.env.DB
            api_key = getattr(self.env, "FUJIMOTO_API_KEY", None)

            auth_error = authenticate(request, path, api_key)
            if auth_error:
                return apply(auth_error)

            response = await resolve(db, method, path, request, _started_at)
        except Exception as e:
            error(f"Unhandled exception on [{method}] {path}: {e}")
            response = json_error(INTERNAL_ERROR, 500)

        log_request(method, path, start, response.status)

        return apply(response)
