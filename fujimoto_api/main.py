"""
fujimoto API
Main application server with CORS, request logging, error handling, and graceful shutdown.
"""

import time
from contextlib import asynccontextmanager
from datetime import datetime, timezone
from fastapi import FastAPI, HTTPException, Request
from fastapi.middleware.cors import CORSMiddleware
from fastapi.responses import JSONResponse
from app import config
from app.logger import info, error
from middleware.request_logger import RequestLoggerMiddleware
from routes import routes, saves


@asynccontextmanager
async def lifespan(application: FastAPI):
    info(f"{config.API_NAME} is live... access endpoints at:")
    info(f"[/] - Root health check")
    info(f"[/docs] - API docs")
    info(f"[/api] - API info")
    info(f"[/api/saves] - POST, GET, DELETE player saves")
    yield
    info(f"{config.API_NAME} shutting down...")


app = FastAPI(
    title=config.API_NAME,
    version=config.VERSION,
    description=config.DESCRIPTION,
    lifespan=lifespan,
)

_started_at = time.time()


app.add_middleware(
    CORSMiddleware,
    allow_origins=config.CORS_ORIGINS,
    allow_credentials=True,
    allow_methods=["GET", "POST", "DELETE"],
    allow_headers=["Content-Type"],
)

app.add_middleware(RequestLoggerMiddleware)


@app.exception_handler(HTTPException)
async def http_exception_handler(request: Request, exc: HTTPException):
    return JSONResponse(
        status_code=exc.status_code,
        content={"status": "error", "message": exc.detail},
    )


@app.exception_handler(Exception)
async def global_exception_handler(request: Request, exc: Exception):
    error(f"Unhandled exception on [{request.method}] {request.url.path}: {exc}")
    return JSONResponse(
        status_code=500,
        content={"status": "error", "message": "Internal server error."},
    )


@app.get("/", tags=["Health"])
def root():
    uptime = round(time.time() - _started_at, 1)
    return {
        "status": "healthy",
        "service": config.API_NAME,
        "message": f"{config.API_NAME} is live...",
        "uptime_seconds": uptime,
        "timestamp": datetime.now(timezone.utc).isoformat(),
    }


app.include_router(routes.router)
app.include_router(saves.router)


if __name__ == "__main__":
    import uvicorn

    uvicorn.run("main:app", host=config.HOST, port=config.PORT, reload=True)
