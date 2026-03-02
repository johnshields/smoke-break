using System;

namespace _Scripts._Systems.Objects
{
    [Serializable]
    public class SaveData
    {
        public string player_id = "";
        public string saved_at = "";
        public float playerX, playerY, playerZ;
        public int playerHealth;
        public int clipAmmo;
        public int storedAmmo;
        public string savedLevel = "";
    }
}
