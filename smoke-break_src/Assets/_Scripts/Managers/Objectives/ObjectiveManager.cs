using System.Collections;
using TMPro;
using UnityEngine;

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

        #endregion

        #region Initialization

        private void Awake()
        {
            if (Instance == null) Instance = this;
        }

        private void Start()
        {
            if (objectiveText == null)
            {
                Debug.LogError("❌ ObjectiveManager: ObjectiveText is not assigned in the Inspector.");
                return;
            }

            objectiveText.alpha = 0;
        }

        #endregion

        #region Objective System

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
            objectiveText.text = "";
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