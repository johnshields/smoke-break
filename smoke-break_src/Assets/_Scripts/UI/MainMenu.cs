using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

namespace _Scripts.UI
{
    public class MainMenu : MonoBehaviour
    {
        [Header("UI Elements")] public GameObject controlsPanel;
        public Button startGameButton, controlsButton, returnButton, quitButton;
        public TextMeshProUGUI startButtonText, controlsButtonText, returnButtonText, quitButtonText;

        [Header("Highlight Settings")] [SerializeField]
        private Color highlightColor = Color.green;

        [SerializeField] private Color defaultColor = Color.white;

        private InputControls _actions;
        private int _currentButtonIndex;
        private Button[] _buttons;
        private Image[] _buttonImages;
        private TextMeshProUGUI[] _texts;
        private TextMeshProUGUI[] _buttonTexts;

        private void Awake()
        {
            _actions = new InputControls();
            _actions.UI.Navigate.performed += NavigateMenu;
            _actions.UI.Submit.performed += SelectButton;

            _buttons = new Button[] { startGameButton, controlsButton, returnButton, quitButton };
            _buttonImages = new Image[_buttons.Length];
            _texts = new TextMeshProUGUI[] { startButtonText, controlsButtonText, returnButtonText, quitButtonText };
            _buttonTexts = new TextMeshProUGUI[_texts.Length];

            for (int i = 0; i < _buttons.Length; i++)
            {
                _buttonImages[i] = _buttons[i].GetComponent<Image>();
                _buttonTexts[i] = _texts[i].GetComponent<TextMeshProUGUI>();
            }
        }

        private void OnEnable()
        {
            _actions.UI.Enable();
            startGameButton.onClick.AddListener(StartGame);
            controlsButton.onClick.AddListener(OpenControls);
            returnButton.onClick.AddListener(ReturnButton);
            quitButton.onClick.AddListener(QuitGame);

            _currentButtonIndex = 0; // Start with first button selected
            UpdateButtonSelection();
        }

        private void OnDisable()
        {
            _actions.UI.Navigate.performed -= NavigateMenu;
            _actions.UI.Submit.performed -= SelectButton;
            _actions.UI.Disable();
        }

        private void NavigateMenu(InputAction.CallbackContext context)
        {
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
                _buttonImages[i].color = (i == _currentButtonIndex) ? highlightColor : defaultColor;
                _buttonTexts[i].color = (i == _currentButtonIndex) ? defaultColor : highlightColor;
            }

            EventSystem.current.SetSelectedGameObject(_buttons[_currentButtonIndex].gameObject);
            _buttons[_currentButtonIndex].Select();
        }

        private void SelectButton(InputAction.CallbackContext context)
        {
            _buttons[_currentButtonIndex].onClick.Invoke();
        }

        private void StartGame()
        {
            Debug.Log("▶️ Starting Game...");
            SceneManager.LoadScene("SampleScene");
        }

        private void OpenControls()
        {
            controlsPanel.SetActive(true);
        }

        private void ReturnButton()
        {
            controlsPanel.SetActive(false);
        }

        private void QuitGame()
        {
            Debug.Log("Quiting Game");
            Application.Quit();
        }
    }
}