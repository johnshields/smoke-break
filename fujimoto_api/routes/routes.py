"""
System Routes
API information and system status endpoints.
"""

import json
import time
from datetime import datetime, timezone
from workers import Response


def _json(data: dict) -> Response:
    return Response(json.dumps(data), headers={"Content-Type": "application/json"})


def health(started_at: float, api_name: str) -> Response:
    uptime = round(time.time() - started_at, 1)
    return _json({
        "status": "healthy",
        "service": api_name,
        "message": f"{api_name} is live...",
        "uptime_seconds": uptime,
        "timestamp": datetime.now(timezone.utc).isoformat(),
    })


def api_info(started_at: float, api_name: str, version: str) -> Response:
    uptime = round(time.time() - started_at, 1)
    return _json({
        "status": "OK",
        "service": api_name,
        "version": version,
        "uptime_seconds": uptime,
        "message": f"{api_name} is live...",
        "timestamp": datetime.now(timezone.utc).isoformat(),
    })
