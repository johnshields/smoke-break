"""
System Routes
API information, health, and readiness endpoints.
"""

from utils.response import json_response
from utils.system import get_timestamp, get_uptime, check_d1


def api_info(started_at: float, api_name: str, version: str):
    return json_response({
        "status": "OK",
        "service": api_name,
        "version": version,
        "uptime_seconds": get_uptime(started_at),
        "message": f"{api_name} is live...",
        "timestamp": get_timestamp(),
    })


def health(started_at: float, api_name: str):
    return json_response({
        "status": "healthy",
        "service": api_name,
        "uptime_seconds": get_uptime(started_at),
        "timestamp": get_timestamp(),
    })


async def readiness(db, started_at: float):
    d1 = await check_d1(db)
    status = "ready" if d1["healthy"] else "degraded"

    data = {
        "status": status,
        "d1": d1["healthy"],
        "uptime_seconds": get_uptime(started_at),
        "timestamp": get_timestamp(),
    }

    if not d1["healthy"]:
        data["errors"] = {"d1": d1.get("error")}

    return json_response(data)
