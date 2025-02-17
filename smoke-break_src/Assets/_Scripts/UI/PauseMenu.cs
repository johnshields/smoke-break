using UnityEngine;
using UnityEngine.EventSystems;
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

        [Header("Highlight Settings")] [SerializeField]
        private Color highlightColor = Color.green;

        [SerializeField] private Color defaultColor = Color.white;

        private InputControls _actions;
        private bool _isPaused;
        private int _currentButtonIndex;
        private Button[] _buttons;
        private Image[] _buttonImages;

        private void Awake()
        {
            _actions = new InputControls();
            _actions.UI.Pause.performed += TogglePause;
            _actions.UI.Navigate.performed += NavigateMenu;
            _actions.UI.Submit.performed += SelectButton;

            _buttons = new Button[] { resumeButton, settingsButton, quitButton };
            _buttonImages = new Image[_buttons.Length];

            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttonImages[i] = _buttons[i].GetComponent<Image>(); // ✅ Get Image component
            }
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

            _currentButtonIndex = 0;
            UpdateButtonSelection();
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

        private void NavigateMenu(InputAction.CallbackContext context)
        {
            if (!_isPaused) return;

            float direction = context.ReadValue<Vector2>().y;
            if (direction > 0) _currentButtonIndex--;
            else if (direction < 0) _currentButtonIndex++;

            _currentButtonIndex = Mathf.Clamp(_currentButtonIndex, 0, _buttons.Length - 1);
            UpdateButtonSelection();
        }

        private void UpdateButtonSelection()
        {
            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttonImages[i].color =
                    (i == _currentButtonIndex) ? highlightColor : defaultColor; // ✅ Change Image color
            }

            EventSystem.current.SetSelectedGameObject(_buttons[_currentButtonIndex].gameObject);
            _buttons[_currentButtonIndex].Select();
        }

        private void SelectButton(InputAction.CallbackContext context)
        {
            if (!_isPaused) return;
            _buttons[_currentButtonIndex].onClick.Invoke();
        }
    }
}