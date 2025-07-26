using System;

namespace _Scripts.Objects
{
    [Serializable]
    public class SaveData
    {
        public string playerId;
        public string timestamp;
        public float playerX;
        public float playerY;
        public float playerZ;
        public int playerHealth;
        public int clipAmmo;
        public int storedAmmo;
        public string savedLevel;
    }
}