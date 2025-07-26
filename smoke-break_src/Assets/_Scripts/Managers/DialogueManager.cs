using _Scripts.UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Managers
{
    public class DialogueManager : MonoBehaviour
    {
        #region Singleton

        private static DialogueManager Instance { get; set; }

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

        private void OnEnable()
        {
            skipAction.Enable();
        }

        private void OnDisable()
        {
            skipAction.Disable();
        }

        #endregion
    }
}