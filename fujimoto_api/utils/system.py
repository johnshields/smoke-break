"""
System Utilities
Uptime, timestamps, and dependency health checks.
"""

import time
from datetime import datetime, timezone


def get_timestamp() -> str:
    return datetime.now(timezone.utc).isoformat()


def get_uptime(started_at: float) -> float:
    return round(time.time() - started_at, 1)


async def check_d1(db) -> dict:
    try:
        row = await db.prepare("SELECT 1 AS ok").first()
        if row and row["ok"] == 1:
            return {"healthy": True}
        return {"healthy": False, "error": "Unexpected query result."}
    except Exception as e:
        return {"healthy": False, "error": str(e)[:180]}
