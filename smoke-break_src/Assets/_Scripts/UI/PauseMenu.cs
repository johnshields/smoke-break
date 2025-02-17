using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace _Scripts.UI
{
    public class PauseMenu : MonoBehaviour
    {
        [Header("UI Elements")] [SerializeField]
        private GameObject pauseMenuUI;

        [SerializeField] private Button resumeButton;
        [SerializeField] private Button settingsButton;
        [SerializeField] private Button quitButton;

        private InputControls _actions;
        private bool _isPaused;

        private void Awake()
        {
            _actions = new InputControls();
            _actions.UI.Pause.performed += TogglePause;
        }

        private void OnEnable()
        {
            _actions.UI.Enable();
            resumeButton.onClick.AddListener(ResumeGame);
            settingsButton.onClick.AddListener(OpenSettings);
            quitButton.onClick.AddListener(QuitGame);
        }

        private void OnDisable()
        {
            _actions.UI.Pause.performed -= TogglePause;
            _actions.UI.Disable();
        }

        private void TogglePause(InputAction.CallbackContext context)
        {
            if (_isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        private void PauseGame()
        {
            _isPaused = true;
            pauseMenuUI.SetActive(true);
            Time.timeScale = 0f;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void ResumeGame()
        {
            _isPaused = false;
            pauseMenuUI.SetActive(false);
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        private void OpenSettings()
        {
            Debug.Log("⚙️ Open Settings (Not Implemented Yet)");
        }

        private void QuitGame()
        {
            Debug.Log("🚪 Quit Game (Not Implemented Yet)");
        }
    }
}