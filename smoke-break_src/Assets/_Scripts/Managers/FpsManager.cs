using UnityEngine;

namespace _Scripts.Managers
{
    public class FpsManager : MonoBehaviour
    {
        private void Start()
        {
            Application.targetFrameRate = 60;
            QualitySettings.vSyncCount = 0;
        }
    }
}