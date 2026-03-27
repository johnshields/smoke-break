"""
Response Helpers
Shared JSON response builder and request parsing for Workers.
"""

import json
from urllib.parse import urlparse
from workers import Response


def json_response(data: dict, status: int = 200) -> Response:
    return Response(
        json.dumps(data),
        status=status,
        headers={"Content-Type": "application/json"},
    )


def json_error(message: str, status: int) -> Response:
    return json_response({"status": "error", "message": message}, status)


def parse_path(url: str) -> str:
    return urlparse(url).path.rstrip("/") or "/"
