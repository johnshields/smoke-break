using UnityEngine;

namespace _Scripts._Gameplay.PickUps
{
    public class GlowEffect : MonoBehaviour
    {
        private static readonly int EmissionColor = Shader.PropertyToID("_EmissionColor");
        [SerializeField] private Color glowColor = Color.white;
        [SerializeField] private float glowIntensity = 1f;
        [SerializeField] private float pulseSpeed = 1f;

        private Material _material;

        private void Awake()
        {
            var component = GetComponent<Renderer>();
            _material = component.material;
            _material.EnableKeyword("_EMISSION");
        }

        private void Update()
        {
            var emission = (Mathf.Sin(Time.time * pulseSpeed) * 0.5f + 0.5f) * glowIntensity;
            _material.SetColor(EmissionColor, glowColor * emission);
        }
    }
}