using System;
using System.Collections.Generic;

namespace _Scripts._Systems.Objects
{
    [Serializable]
    public class SaveData
    {
        // Main
        public string player_id = "";
        public string saved_at = "";
        public float playerX;
        public float playerY;
        public float playerZ;
        public int playerHealth;
        public int clipAmmo;
        public int storedAmmo;
        public string savedLevel = "";
        public string saveType = "";

        // Objectives and Checkpoints
        public string newObjective = "";
        public string lastObjective = "";
        public string newCheckpoint = "";
        public string lastCheckpoint = "";
        public string checkpointId = "";
        public bool? IsCheckpoint = false;
        public string lastDialogueNode = "";

        // Stats tracking
        public int? coins;
        public int? score;
        public int? death_count = 0;
        public int? enemies_defeated;
        public int? playtime;
        public int? collectables_found;

        // Tracking & progression
        public List<TrophyEntry> trophies = new();
        public List<InventoryItem> inventory = new();
        public string[] unlocked_areas = Array.Empty<string>();
        public string[] visited_zones = Array.Empty<string>();
        public string[] logs_collected = Array.Empty<string>();

        // Misc
        public string notes = "";
        public string version = "";
        public int? session_id;
        public string crash_log = "";
        public string updated_at = "";
        public string synced_at = "";
    }

    [Serializable]
    public class TrophyEntry
    {
        public string key;
        public string value;
    }

    [Serializable]
    public class InventoryItem
    {
        public string item_id;
        public int quantity;
    }
}