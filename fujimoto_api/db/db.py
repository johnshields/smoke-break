"""
DB Client
Thin wrapper around D1 prepared statements.
"""


async def execute(db, sql: str, *params):
    return await db.prepare(sql).bind(*params).run()


async def fetch_one(db, sql: str, *params):
    return await db.prepare(sql).bind(*params).first()
