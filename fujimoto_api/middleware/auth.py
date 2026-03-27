"""
Auth Middleware
API key validation for protected endpoints.
"""

import json
import hmac
from workers import Response

PUBLIC_PATHS = {"/", "/api"}


def _json_error(message: str, status: int) -> Response:
    return Response(
        json.dumps({"status": "error", "message": message}),
        status=status,
        headers={"Content-Type": "application/json"},
    )


def authenticate(request, path: str, api_key: str) -> Response | None:
    if path in PUBLIC_PATHS:
        return None

    token = request.headers.get("Authorization", "").removeprefix("Bearer ").strip()

    if not token:
        token = request.headers.get("X-API-Key", "").strip()

    if not token:
        return _json_error("Authentication required.", 401)

    if not hmac.compare_digest(token, api_key):
        return _json_error("Invalid credentials.", 401)

    return None
