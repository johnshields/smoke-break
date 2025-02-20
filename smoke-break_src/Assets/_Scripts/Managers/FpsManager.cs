using TMPro;
using UnityEngine;

namespace _Scripts.Managers
{
    public class FpsManager : MonoBehaviour
    {
        public TextMeshProUGUI fpsText;
        private float _deltaTime;

        private void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }

        private void Update()
        {
            _deltaTime += (Time.deltaTime - _deltaTime) * 0.1f;
            float fps = 1.0f / _deltaTime;

            if (fpsText is not null)
                fpsText.text = $"FPS: {Mathf.Ceil(fps)}";
        }
    }
}