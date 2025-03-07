using _Scripts.UI.Dialogue;
using UnityEngine;

namespace _Scripts.Managers
{
    public class DialogueManager : MonoBehaviour
    {
        [SerializeField] private DialogueTyper typer;
        
        private void Start()
        {
            if (typer != null)
            {
                typer.WriteDialogue(DialogueKey.Opening);
            }
        }
    }
}
