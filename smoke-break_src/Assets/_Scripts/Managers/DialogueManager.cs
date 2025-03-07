using _Scripts.UI.Dialogue;
using UnityEngine;

namespace _Scripts.Managers
{
    public class DialogueManager : MonoBehaviour
    {
        private static DialogueManager _instance;
        public static DialogueManager Instance 
        { 
            get 
            { 
                if (_instance == null)
                {
                    Debug.LogError("DialogueManager instance not found in the scene.");
                }
                return _instance; 
            } 
        }

        [SerializeField] private DialogueTyper typer; 
        
        private void Awake()
        {
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject); // Persist across scenes
            }
            else
            {
                Destroy(gameObject); // Prevent duplicate instances
            }
        }

        private void Start()
        {
            Invoke(nameof(OpeningDialogue), 2.5f);
        }
        
        private void OpeningDialogue()
        {
            PlayDialogue(DialogueKey.Opening);
        }
        
        // Public method to trigger dialogue anywhere in the game.
        public void PlayDialogue(DialogueKey key)
        {
            if (typer != null)
            {
                typer.WriteDialogue(key);
            }
            else
            {
                Debug.LogError("DialogueTyper reference is missing in DialogueManager.");
            }
        }
    }
}