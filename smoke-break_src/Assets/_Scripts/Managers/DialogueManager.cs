using System.Collections;
using System.IO;
using _Scripts.enums;
using _Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Managers
{
    public class DialogueManager : MonoBehaviour
    {
        #region Singleton

        public static DialogueManager Instance { get; private set; }

        #endregion

        #region Fields

        private string _savePath;
        private bool _hasPlayedOpeningDialogue;
        public InputAction skipAction;
        [SerializeField] private DialogueTyper typer;

        #endregion

        #region Unity Methods

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _savePath = SaveManager.GetSaveFilePath();
        }

        private void Start()
        {
            if (!File.Exists(_savePath) || new FileInfo(_savePath).Length == 0)
            {
                Invoke(nameof(OpeningDialogue), 2.5f);
            }
        }

        private void OnEnable()
        {
            skipAction.Enable();
        }

        private void OnDisable()
        {
            skipAction.Disable();
        }

        #endregion

        #region Dialogue Handling

        private void OpeningDialogue()
        {
            PlayDialogue(DialogueKey.Opening, ObjectiveKey.FindIcarus);
        }

        public void PlayDialogue(DialogueKey key, ObjectiveKey objectiveKey = ObjectiveKey.None)
        {
            Debug.Log($"🎬 Playing Dialogue: {key} | Objective: {objectiveKey}");

            if (typer == null)
            {
                Debug.LogError("❌ DialogueTyper reference is missing in DialogueManager.");
                return;
            }

            typer.InitDialogue(key);
        }

        #endregion
    }
}