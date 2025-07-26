using _Scripts.enums;
using _Scripts.Player;
using UnityEngine;

namespace _Scripts.PickUps
{
    public class ItemPickup : MonoBehaviour
    {
        [Header("Item Settings")] [SerializeField]
        private ItemType itemType;

        [SerializeField] private int itemValue = 10;

        [Header("Pickup Sounds")] [SerializeField]
        private AudioClip pickupSound;

        [Header("Effects")] [SerializeField] private float rotationSpeed = 50f;
        [SerializeField] private float magnetRange = 5f;
        [SerializeField] private float magnetSpeed = 5f;
        [SerializeField] private float bounceSpeed = 2f;
        [SerializeField] private float bounceHeight = 0.2f;

        private Transform _player;
        private Vector3 _startPosition;
        private PlayerHealth _playerHealth;
        private PistolProfiler _pistol;

        private void Start()
        {
            _player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (_player is not null)
            {
                _pistol = _player.GetComponentInParent<PistolProfiler>();
                _playerHealth = _player.GetComponentInParent<PlayerHealth>();
            }

            _startPosition = transform.position;
        }

        private void Update()
        {
            transform.Rotate(Vector3.up, rotationSpeed * Time.deltaTime);

            float newY = _startPosition.y + Mathf.Sin(Time.time * bounceSpeed) * bounceHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);

            if (_player is not null &&
                Vector3.Distance(transform.position, _player.position) <= magnetRange &&
                ShouldAttract())
            {
                transform.position =
                    Vector3.MoveTowards(transform.position, _player.position, magnetSpeed * Time.deltaTime);
            }
        }

        private bool ShouldAttract()
        {
            switch (itemType)
            {
                case ItemType.Health:
                    if (_playerHealth is not null)
                    {
                        return _playerHealth.GetCurrentHealth() < _playerHealth.GetMaxHealth();
                    }

                    break;

                case ItemType.Ammo:
                    if (_pistol is not null)
                    {
                        return _pistol.storedAmmo < _pistol.maxStoredAmmo;
                    }

                    break;

                default:
                    return true;
            }

            return true;
        }

        private void OnTriggerEnter(Collider other)
        {
            switch (itemType)
            {
                case ItemType.Health:
                    if (other.TryGetComponent(out PlayerHealth playerHealth) &&
                        playerHealth.GetCurrentHealth() < playerHealth.GetMaxHealth())
                    {
                        playerHealth.RestoreHealth(itemValue);
                        PlayPickupSound();
                        Destroy(gameObject);
                    }

                    break;

                case ItemType.Ammo:
                    if (other.TryGetComponent(out PistolProfiler pistol) &&
                        pistol.storedAmmo < pistol.maxStoredAmmo)
                    {
                        pistol.RefillAmmo(itemValue);
                        PlayPickupSound();
                        Destroy(gameObject);
                    }

                    break;

                case ItemType.Coin:
                    if (other.TryGetComponent(out PlayerProfiler player))
                    {
                        //player.AddCoins(itemValue);
                        PlayPickupSound();
                        Destroy(gameObject);
                    }

                    break;

                case ItemType.Other:
                    Debug.Log($"Collected {gameObject.name}, but no effect implemented yet!");
                    PlayPickupSound();
                    Destroy(gameObject);
                    break;
            }
        }

        private void PlayPickupSound()
        {
            if (pickupSound != null)
                AudioSource.PlayClipAtPoint(pickupSound, transform.position);
        }
    }
}