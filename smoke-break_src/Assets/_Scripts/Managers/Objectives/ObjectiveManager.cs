using System;
using System.Collections;
using System.IO;
using _Scripts.Player;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts.Managers.Objectives
{
    public class ObjectiveManager : MonoBehaviour
    {
        #region Singleton
        public static ObjectiveManager Instance { get; private set; }
        #endregion

        #region Variables

        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI objectiveText;

        [Header("Settings")]
        [SerializeField] private float fadeDuration = 1.5f;
        [SerializeField] private float objectiveDisplayTime = 3f;

        private Coroutine _currentObjectiveRoutine;
        private string _currentObjective;
        private string _savePath;
        private InputControls _input;

        #endregion

        #region Initialization

        private void Awake()
        {
            if (Instance == null) Instance = this;
            _savePath = SaveManager.GetSaveFilePath();
            _input = new InputControls();
        }

        private void Start()
        {
            ObjectiveData.LoadObjectivesFromJson();
            
            if (objectiveText == null)
            {
                Debug.LogError("❌ ObjectiveManager: ObjectiveText is not assigned in the Inspector.");
                return;
            }

            objectiveText.alpha = 0;
        }
        
        private void OnEnable()
        {
            _input.UI.ViewObjective.performed += ShowLastObjective;
            _input.Enable();
        }

        private void OnDisable()
        {
            _input.UI.ViewObjective.performed -= ShowLastObjective;
            _input.Disable();
        }

        #endregion

        #region Objective System
        
        public string GetLastObjective() => _currentObjective;
        
        public void SetObjective(string objective)
        {
            _currentObjective = objective;
            SaveObjective();
        }
        
        private void SaveObjective()
        {
            if (File.Exists(_savePath))
            {
                var json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                data.lastObjective = _currentObjective;
                File.WriteAllText(_savePath, JsonUtility.ToJson(data, true));
            }
        }

        private string LoadObjective()
        {
            string objective;
            
            if (File.Exists(_savePath))
            {

                var json = File.ReadAllText(_savePath);
                var data = JsonUtility.FromJson<SaveData>(json);
                objective = data.lastObjective;
            }
            else
            {
                objective = objectiveText.text;
            }

            return objective;
        }
        
        private void ShowLastObjective(InputAction.CallbackContext context)
        {
            StartCoroutine(!string.IsNullOrEmpty(LoadObjective())
                ? DisplayObjective(LoadObjective())
                : DisplayObjective(objectiveText.text));
        }

        public void ShowObjective(string objectiveMessage)
        {
            if (_currentObjectiveRoutine != null)
            {
                StopCoroutine(_currentObjectiveRoutine);
            }

            _currentObjectiveRoutine = StartCoroutine(DisplayObjective(objectiveMessage));
        }

        private IEnumerator DisplayObjective(string objectiveMessage)
        {
            objectiveText.text = objectiveMessage;
            yield return FadeObjective(true);

            yield return new WaitForSeconds(objectiveDisplayTime);

            yield return FadeObjective(false);
        }

        private IEnumerator FadeObjective(bool fadeIn)
        {
            var elapsedTime = 0f;
            var textColor = objectiveText.color;
            var startAlpha = fadeIn ? 0f : 1f;
            var endAlpha = fadeIn ? 1f : 0f;

            while (elapsedTime < fadeDuration)
            {
                textColor.a = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
                objectiveText.color = textColor;
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            objectiveText.color = new Color(textColor.r, textColor.g, textColor.b, endAlpha);
        }

        #endregion
    }
}