from workers import WorkerEntrypoint
from main import app


class Default(WorkerEntrypoint):
    async def fetch(self, request):
        app.state.db = self.env.DB

        from workers.http import fetch as asgi_fetch
        return await asgi_fetch(app, request)
