using System.Collections.Generic;

namespace _Scripts._Systems.Objects
{
    [System.Serializable]
    public class DialogueEntry
    {
        public string key;
        public List<DialogueLine> lines;
    }
}