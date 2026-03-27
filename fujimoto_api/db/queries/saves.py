"""
Save Queries
SQL statements for the saves table.
"""

UPSERT = """
    INSERT INTO saves (
        uid, player_id,
        player_position, player_health, player_ammo, saved_level,
        saved_at
    )
    VALUES (?, ?, ?, ?, ?, ?, ?)
    ON CONFLICT(player_id) DO UPDATE SET
        player_position = excluded.player_position,
        player_health   = excluded.player_health,
        player_ammo     = excluded.player_ammo,
        saved_level     = excluded.saved_level,
        saved_at        = excluded.saved_at,
        updated_at      = strftime('%Y-%m-%dT%H:%M:%fZ', 'now'),
        deleted_at      = NULL
"""

FIND_BY_PLAYER = """
    SELECT * FROM saves
    WHERE player_id = ?
      AND deleted_at IS NULL
"""

EXISTS_BY_PLAYER = """
    SELECT player_id FROM saves
    WHERE player_id = ?
      AND deleted_at IS NULL
"""

SOFT_DELETE = """
    UPDATE saves
    SET deleted_at = strftime('%Y-%m-%dT%H:%M:%fZ', 'now')
    WHERE player_id = ?
"""
