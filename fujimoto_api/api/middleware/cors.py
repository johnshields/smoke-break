"""
CORS Middleware
Headers and preflight handling.
"""

from workers import Response
from app import config

CORS_HEADERS = {
    "Access-Control-Allow-Origin": config.CORS_ORIGINS,
    "Access-Control-Allow-Methods": "GET, POST, DELETE, OPTIONS",
    "Access-Control-Allow-Headers": "Content-Type, Authorization, X-API-Key",
    "Access-Control-Allow-Credentials": "true",
}


def preflight() -> Response:
    return Response("", status=204, headers=CORS_HEADERS)


def apply(response: Response) -> Response:
    for key, value in CORS_HEADERS.items():
        response.headers[key] = value
    return response
