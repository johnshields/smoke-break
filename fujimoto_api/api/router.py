"""
Router
Route table mapping (method, path) to handlers.
"""

import re
from app import config
from app.messages import NOT_FOUND
from api.routes import routes, saves
from utils.response import json_error


_routes = []


def route(method: str, pattern: str):
    regex = re.compile("^" + re.sub(r":(\w+)", r"(?P<\1>[^/]+)", pattern) + "$")

    def decorator(fn):
        _routes.append((method, regex, fn))
        return fn

    return decorator


@route("GET", "/")
@route("GET", "/api")
async def _api_info(db, request, started_at, **kwargs):
    return routes.api_info(started_at, config.API_NAME, config.VERSION)


@route("GET", "/api/healthz")
async def _health(db, request, started_at, **kwargs):
    return routes.health(started_at, config.API_NAME)


@route("GET", "/api/readyz")
async def _readiness(db, request, started_at, **kwargs):
    return await routes.readiness(db, started_at)


@route("POST", "/api/saves")
async def _upload_save(db, request, started_at, **kwargs):
    return await saves.upload_save(db, request)


@route("GET", "/api/saves/:player_id")
async def _download_save(db, request, started_at, player_id, **kwargs):
    return await saves.download_save(db, player_id)


@route("DELETE", "/api/saves/:player_id")
async def _delete_save(db, request, started_at, player_id, **kwargs):
    return await saves.delete_save(db, player_id)


async def resolve(db, method: str, path: str, request, started_at: float):
    for route_method, regex, handler in _routes:
        if method != route_method:
            continue
        match = regex.match(path)
        if match:
            return await handler(db, request, started_at, **match.groupdict())

    return json_error(NOT_FOUND, 404)
