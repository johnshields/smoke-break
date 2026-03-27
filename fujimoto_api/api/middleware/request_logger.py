"""
Request Logger Middleware
Logs method, path, status, and duration for each request.
"""

import time
from app.logger import info


def log_request(method: str, path: str, start: float, status: int):
    duration_ms = round((time.time() - start) * 1000)
    info(f"[{method}] {path} - {status} - Took {duration_ms}ms")
