"""
Logging
Structured logging functions matching boilerplate-api format.
"""


def info(msg: str):
    print(f"[info]: {msg}")


def warn(msg: str):
    print(f"[warn]: {msg}")


def error(msg: str):
    print(f"[error]: {msg}")
