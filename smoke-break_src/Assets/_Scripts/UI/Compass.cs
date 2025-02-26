using UnityEngine;

namespace _Scripts.UI
{
    public class Compass : MonoBehaviour
    {
        private Transform _northPoint;
        private Transform _player;
        private Vector3 _direction;
        private const float SmoothSpeed = 5f;

        private void Start()
        {
            _northPoint = GameObject.FindGameObjectWithTag("NorthPoint")?.transform;
            _player = Camera.main?.transform;

            if (_northPoint == null)
                Debug.LogError("No GameObject found with tag 'NorthPoint'.");

            if (_player == null)
                Debug.LogError("No Camera found.");
        }

        private void LateUpdate()
        {
            if (!_northPoint || !_player) return;

            var toNorth = _northPoint.position - _player.position;
            var northAngle = Mathf.Atan2(toNorth.x, toNorth.z) * Mathf.Rad2Deg;
            var playerAngle = _player.eulerAngles.y;

            var targetRotation = playerAngle - northAngle;
            _direction.z = Mathf.LerpAngle(_direction.z, targetRotation, Time.deltaTime * SmoothSpeed);

            transform.localEulerAngles = _direction;
        }
    }
}