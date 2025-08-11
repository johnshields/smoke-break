from fastapi import APIRouter
from datetime import datetime

router = APIRouter()


@router.get("/api", tags=["Info"])
def api_info():
    return {
        "name": "fujimoto API",
        "version": "1.0.0",
        "description": "API for saving and loading player data.",
        "timestamp": datetime.utcnow().isoformat()
    }
