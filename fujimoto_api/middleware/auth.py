"""
Auth Middleware
API key validation for protected endpoints.
"""

import hmac
from workers import Response
from utils.response import json_error

PUBLIC_PATHS = {"/", "/api", "/api/healthz"}


def authenticate(request, path: str, api_key: str) -> Response | None:
    if path in PUBLIC_PATHS:
        return None

    token = request.headers.get("Authorization", "").removeprefix("Bearer ").strip()

    if not token:
        token = request.headers.get("X-API-Key", "").strip()

    if not token:
        return json_error("Authentication required.", 401)

    if not hmac.compare_digest(token, api_key):
        return json_error("Invalid credentials.", 401)

    return None
