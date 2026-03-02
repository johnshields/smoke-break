"""
System Routes
API information and system status endpoints.
"""

from datetime import datetime, timezone
from fastapi import APIRouter
from app import config

router = APIRouter()

_started_at = datetime.now(timezone.utc)


def _uptime_seconds() -> float:
    return round((datetime.now(timezone.utc) - _started_at).total_seconds(), 1)


@router.get("/api", tags=["System"])
def api_info():
    return {
        "status": "OK",
        "service": config.API_NAME,
        "description": config.DESCRIPTION,
        "version": config.VERSION,
        "uptime_seconds": _uptime_seconds(),
        "message": f"{config.API_NAME} is live...",
        "timestamp": datetime.now(timezone.utc).isoformat()
    }
