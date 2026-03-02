"""
Configuration
Centralised configuration loaded from environment variables with sensible defaults.
"""

import os

try:
    from dotenv import load_dotenv
    load_dotenv()
except ImportError:
    pass

API_NAME = os.getenv("API_NAME", "fujimoto_api")
DESCRIPTION = "API for saving and loading player data."
VERSION = "1.0.0"

HOST = os.getenv("HOST", "0.0.0.0")
PORT = int(os.getenv("PORT", 5000))

CORS_ORIGINS = os.getenv("CORS_ORIGINS", "*").split(",")

DATA_DIR = os.getenv("DATA_DIR", os.path.join(os.path.dirname(os.path.dirname(__file__)), "data"))
SAVES_DIR = os.path.join(DATA_DIR, "saves")
