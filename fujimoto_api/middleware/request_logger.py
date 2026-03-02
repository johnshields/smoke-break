"""
Request Logger Middleware
Logs incoming requests with method, path, status, and duration.
"""

import time
from starlette.middleware.base import BaseHTTPMiddleware
from app.logger import info


class RequestLoggerMiddleware(BaseHTTPMiddleware):
    async def dispatch(self, request, call_next):
        method = request.method
        path = request.url.path
        start = time.time()

        response = await call_next(request)

        duration_ms = round((time.time() - start) * 1000)
        info(f"[{method}] {path} - {response.status_code} - Took {duration_ms}ms")

        return response
