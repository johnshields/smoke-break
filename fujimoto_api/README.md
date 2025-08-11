# fujimoto API

A simple FastAPI service for saving and loading player data.

## Requirements
- Python 3.9+
- pip

## ⚙️ Setup & Run
Clone the repository and install dependencies:

```bash
$ git clone https://github.com/johnshields/smoke-break
$ cd smoke-break/fujimoto_api
$ pip install -r requirements.txt
$ python -m uvicorn main:app --host 127.0.0.1 --port 5000 --reload
```

### 📦 API Endpoints
- [`/` - Root health check](http://127.0.0.1:5000/)
- [`/docs` - API docs](http://127.0.0.1:5000/docs)
- [`api/` - API info](http://127.0.0.1:5000/api)