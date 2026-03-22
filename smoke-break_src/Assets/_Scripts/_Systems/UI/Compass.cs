using _Scripts._Systems.Utils;
using UnityEngine;
using UnityEngine.UI;

namespace _Scripts._Systems.UI
{
    public class Compass : MonoBehaviour
    {
        [SerializeField] private Rect texRect = new(0, 0, 3.0f / 8.0f, 1.0f);

        private RawImage _compassImage;
        private Transform _target;
        private Transform _northPoint;

        private void Start()
        {
            _target = GameObject.FindGameObjectWithTag(GameTags.Player)?.transform;
            _northPoint = GameObject.FindGameObjectWithTag(GameTags.NorthPoint)?.transform;
            _compassImage = GetComponent<RawImage>();
        }

        private void Update()
        {
            if (!_target || !_northPoint || !_compassImage) return;

            // Calculate direction to NorthPoint
            var toNorth = _northPoint.position - _target.position;
            var northAngle = Mathf.Atan2(toNorth.x, toNorth.z) * Mathf.Rad2Deg;

            // Get player's current rotation
            var playerAngle = _target.eulerAngles.y;

            // Calculate relative angle difference (North should always be at center)
            var relativeAngle = Mathf.DeltaAngle(playerAngle, northAngle);

            // Convert angle (-180 to 180) into UV offset (0 to 1)
            texRect.x = (5.0f / 16.0f) + (relativeAngle / 720.0f);
            _compassImage.uvRect = texRect; // Update RawImage's UV mapping
        }
    }
}