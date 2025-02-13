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
        [SerializeField] private GameObject worldCrosshairPrefab;

        [Header("Weapon Visibility")]
        [SerializeField] private GameObject pistol;
        private const float WeaponHideTime = 5f;
        private float _lastActionTime;

        [Header("Aiming Settings")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private float aimFOV = 40f;
        [SerializeField] private float normalFOV = 60f;
        [SerializeField] private float aimSpeed = 10f;
        [SerializeField] private float aimSnapSpeed = 10f;
        [SerializeField] private float crosshairHeightOffset = 0.2f;
        [SerializeField] private LayerMask aimableLayers;

        private GameObject _worldCrosshair;
        private InputControls _actions;
        private PlayerProfiler _player;
        private Animator _animator;
        private int _currentAmmo;
        private bool _canShoot = true;
        private bool _isReloading = false;
        private bool _isAiming = false;
        private Vector3 _aimTarget;

        private void Awake()
        {
            _actions = new InputControls();
            _player = GetComponent<PlayerProfiler>();
            _animator = GetComponent<Animator>();
            _currentAmmo = maxAmmo;
            pistol.SetActive(false);
            
            _worldCrosshair = Instantiate(worldCrosshairPrefab);
            _worldCrosshair.SetActive(false);
        }

        private void OnEnable()
        {
            _actions.Profiler.Enable();
            _actions.Profiler.Shoot.performed += ShootAction;
            _actions.Profiler.Aim.performed += StartAiming;
            _actions.Profiler.Aim.canceled += StopAiming;
        }

        private void OnDisable()
        {
            _actions.Profiler.Shoot.performed -= ShootAction;
            _actions.Profiler.Aim.performed -= StartAiming;
            _actions.Profiler.Aim.canceled -= StopAiming;
            _actions.Profiler.Disable();
        }

        private void Update()
        {
            if (pistol.activeSelf && Time.time - _lastActionTime > WeaponHideTime)
            {
                pistol.SetActive(false);
            }
            
            playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, 
                _isAiming ? aimFOV : normalFOV, Time.deltaTime * aimSpeed);
            
            if (_isAiming)
            {
                UpdateCrosshairPosition();
                SnapToTarget();
            }
        }

        private void StartAiming(InputAction.CallbackContext context)
        {
            _isAiming = true;
            _worldCrosshair.SetActive(true); 
        }

        private void StopAiming(InputAction.CallbackContext context)
        {
            _isAiming = false;
            _worldCrosshair.SetActive(false);
        }

        private void UpdateCrosshairPosition()
        {
            Ray ray = playerCamera.ScreenPointToRay(new Vector2(Screen.width / 2, Screen.height / 2));

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, aimableLayers)) 
            {
                _aimTarget = hit.point + hit.normal * 0.5f; 
                _aimTarget.y += crosshairHeightOffset; 
                
                _worldCrosshair.transform.position = _aimTarget;
                _worldCrosshair.transform.LookAt(playerCamera.transform);
                
                if (!_worldCrosshair.activeSelf)
                    _worldCrosshair.SetActive(true);
            }
            else
            {
                _worldCrosshair.SetActive(false);
            }
        }
        
        private void SnapToTarget()
        {
            if (_aimTarget == Vector3.zero) return;

            Vector3 lookDirection = (_aimTarget - transform.position).normalized;
            lookDirection.y = 0;

            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * aimSnapSpeed);
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
