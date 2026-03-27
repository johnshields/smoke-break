-- fujimoto API — D1 (SQLite) Schema

CREATE TABLE IF NOT EXISTS saves (
    id           INTEGER PRIMARY KEY AUTOINCREMENT,
    uid          TEXT    NOT NULL UNIQUE,
    player_id    TEXT    NOT NULL UNIQUE,

    player_health   INTEGER NOT NULL DEFAULT 30,
    player_position TEXT    NOT NULL DEFAULT '{"x":0.0,"y":0.0,"z":0.0}',
    player_ammo     TEXT    NOT NULL DEFAULT '{"clip":0,"stored":0}',
    saved_level     TEXT,

    saved_at     TEXT,
    created_at   TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')),
    updated_at   TEXT    NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')),
    deleted_at   TEXT             DEFAULT NULL
);

CREATE INDEX IF NOT EXISTS idx_saves_uid        ON saves(uid);
CREATE INDEX IF NOT EXISTS idx_saves_player_id  ON saves(player_id);
CREATE INDEX IF NOT EXISTS idx_saves_deleted_at ON saves(deleted_at);

CREATE TRIGGER IF NOT EXISTS trg_saves_updated_at
AFTER UPDATE ON saves
FOR EACH ROW
BEGIN
    UPDATE saves
    SET updated_at = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')
    WHERE id = OLD.id;
END;
