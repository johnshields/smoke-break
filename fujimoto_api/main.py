import uvicorn
from fastapi import FastAPI
from datetime import datetime
from routes import routes

app = FastAPI(
    title="fujimoto API",
    version="1.0.0",
    description="API for saving and loading player data"
)


# Root health check
@app.get("/", tags=["Health"])
def root():
    return {
        "message": "fujimoto API is live...",
        "status": 200,
        "timestamp": datetime.utcnow().isoformat()
    }


# Include other routes
app.include_router(routes.router)

if __name__ == "__main__":
    uvicorn.run("main:app", host="0.0.0.0", port=5000, reload=True)
