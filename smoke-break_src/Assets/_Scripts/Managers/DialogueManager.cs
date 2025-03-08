using System.Collections;
using System.IO;
using _Scripts.Managers.Objectives;
using _Scripts.UI.Dialogue;
using UnityEngine;

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

            if (objectiveKey != ObjectiveKey.None)
            {
                HandleObjective(objectiveKey);
            }
        }
        #endregion

        #region Objective Handling
        private void HandleObjective(ObjectiveKey objectiveKey)
        {
            if (objectiveKey == ObjectiveKey.None) return;

            var keyString = objectiveKey.ToString();
            var objective = ObjectiveData.GetObjective(keyString);
            
            if (objective != null)
            {
                ObjectiveManager.Instance.SetObjective(objective.message);
                StartCoroutine(WaitForDialogueToEnd(objective.delay, objective.key));
            }
            else
            {
                Debug.LogError($"❌ Objective '{objectiveKey}' not found in objectives.json");
            }
        }

        private IEnumerator WaitForDialogueToEnd(float delay, string objectiveKey)
        {
            yield return new WaitForSeconds(delay);

            var objective = ObjectiveData.GetObjective(objectiveKey);
            if (objective is { completed: false })
            {
                ObjectiveManager.Instance.ShowObjective(objective.message);
                ObjectiveData.CompleteObjective(objectiveKey);
            }
        }
        #endregion
    }
}
