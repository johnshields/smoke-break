using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

namespace _Scripts
{
    public class PistolProfiler : MonoBehaviour
    {
        private static readonly int ShootHash = Animator.StringToHash("Shoot");

        [Header("Pistol Settings")]
        [SerializeField] private GameObject bulletPrefab;
        [SerializeField] private Transform firePoint;
        [SerializeField] private float bulletSpeed = 20f;
        [SerializeField] private int maxAmmo = 10;
        [SerializeField] private float fireRate = 0.3f;

        [Header("Effects")]
        [SerializeField] private GameObject muzzleFlashPrefab;

        [Header("Weapon Visibility")]
        [SerializeField] private GameObject pistol;
        private const float WeaponHideTime = 5f;
        private float _lastActionTime;

        private Animator _animator;
        private InputControls _actions;
        private PlayerProfiler _player;
        private int _currentAmmo;
        private bool _canShoot = true;
        private bool _isReloading = false;

        private void Awake()
        {
            _actions = new InputControls();
            _animator = GetComponent<Animator>();
            _player = GetComponent<PlayerProfiler>();
            _currentAmmo = maxAmmo;
            pistol.SetActive(false);
        }

        private void OnEnable()
        {
            _actions.Profiler.Enable();
            _actions.Profiler.Shoot.performed += ShootAction;
        }

        private void OnDisable()
        {
            _actions.Profiler.Shoot.performed -= ShootAction;
            _actions.Profiler.Disable();
        }

        private void Update()
        {
            if (pistol.activeSelf && Time.time - _lastActionTime > WeaponHideTime)
            {
                pistol.SetActive(false);
            }
        }

        private void ShootAction(InputAction.CallbackContext context)
        {
            if (!_canShoot || _isReloading || _currentAmmo <= 0) return;

            _player.disableMovement = true; 
            _lastActionTime = Time.time;
            pistol.SetActive(true);
            _currentAmmo--;
            _canShoot = false;

            _animator.SetTrigger(ShootHash);
            StartCoroutine(ShootWithDelay());
        }

        private IEnumerator ShootWithDelay()
        {
            yield return new WaitForSeconds(0.5f); 
            
            GameObject muzzleFlash = Instantiate(muzzleFlashPrefab, firePoint.position, firePoint.rotation);
            Destroy(muzzleFlash, 0.1f);
            
            GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
            Rigidbody rb = bullet.GetComponent<Rigidbody>();
            rb.velocity = firePoint.forward * bulletSpeed;

            yield return new WaitForSeconds(fireRate);
            _canShoot = true;
            _player.disableMovement = false;
        }
    }
}
