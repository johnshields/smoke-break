using System.Collections.Generic;

namespace _Scripts.Objects
{
    [System.Serializable]
    public class DialogueEntry
    {
        public string key;
        public List<DialogueLine> lines;
    }
}