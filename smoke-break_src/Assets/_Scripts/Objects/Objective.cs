using System.Collections.Generic;

namespace _Scripts.Objects
{
    [System.Serializable]
    public class ObjectiveList
    {
        public List<Objective> objectives;
    }

    [System.Serializable]
    public class Objective
    {
        public string key;
        public string message;
        public float delay;
        public bool completed;
    }
}